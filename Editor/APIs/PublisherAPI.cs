﻿using NGToolsStandalone_For_NGPublisher;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Networking;

namespace NGPublisher
{
	using NGToolsStandalone_For_NGPublisherEditor;

	[AssemblyVerifier]
	public class PublisherAPI : IPublisherAPI
	{
		private class TwoFactorAuthParameters
		{
			public string	url;
			public string	authenticity_token;
		}

		public const string	Host = "publisher.assetstore.unity3d.com";
		public const string	EndPoint = "https://" + PublisherAPI.Host + "/";

		private static Type			UnityConnectType = AssemblyVerifier.TryGetType(typeof(Editor).Assembly, "UnityEditor.Connect.UnityConnect");
		private static MethodInfo	GetAccessTokenMethod = AssemblyVerifier.TryGetMethod(PublisherAPI.UnityConnectType, "GetAccessToken", BindingFlags.Instance | BindingFlags.Public);
		private static PropertyInfo	instanceProperty = AssemblyVerifier.TryGetProperty(PublisherAPI.UnityConnectType, "instance", BindingFlags.Static | BindingFlags.Public);

		public bool		IsConnected { get { return this.session != null && string.IsNullOrEmpty(this.session.xunitysession) == false && string.IsNullOrEmpty(this.session.kharma_token) == false; } }
		public Session	Session { get { return this.session; } }
		public bool		WaitingTFACode { get { return this.TFAParameters != null; } }

		private TwoFactorAuthParameters	TFAParameters;

		private Session		session;
		private List<int>	runningRequests = new List<int>();

		public	PublisherAPI(Session session)
		{
			if (session != null && session.xunitysession != null)
				this.session = session;
		}

		public bool	IsConnecting()
		{
			lock (this.runningRequests)
			{
				return this.runningRequests.Contains("https://kharma.unity3d.com/login".GetHashCode());
			}
		}

		public void	ConnectViaUnity(Action<HttpWebResponse, string> onCompleted)
		{
			if (PublisherAPI.instanceProperty != null &&
				PublisherAPI.GetAccessTokenMethod != null)
			{
				string			endpoint = "https://kharma.unity3d.com/login";
				HttpWebRequest	request = this.CreateRequest(endpoint);
				StringBuilder	buffer = Utility.GetBuffer();

				buffer.Append("user_access_token=");
				buffer.Append((string)GetAccessTokenMethod.Invoke(instanceProperty.GetValue(null), null));
				buffer.Append("&unityversion=");
				buffer.Append(PublisherAPI.UrlEncode(Application.unityVersion));
				buffer.Append("&toolversion=");
				buffer.Append(PublisherAPI.UrlEncode("V5.0.2"));
				buffer.Append("&license_hash=");
				buffer.Append(PublisherAPI.UrlEncode(this.GetLicenseHash()));
				buffer.Append("&hardware_hash=");
				buffer.Append(PublisherAPI.UrlEncode(this.GetHardwareHash()));

				byte[]	content = Encoding.UTF8.GetBytes(Utility.ReturnBuffer(buffer));

				request.Headers = new WebHeaderCollection();
				request.Method = "POST";
				request.ContentType = "application/x-www-form-urlencoded";
				request.ContentLength = content.Length;
				request.Accept = "application/json";
				request.KeepAlive = true;
				request.Host = "kharma.unity3d.com";
				request.CookieContainer = new CookieContainer();

				using (Stream stream = request.GetRequestStream())
				{
					stream.Write(content, 0, content.Length);
				}

				this.HandleRequest(request, endpoint.GetHashCode(), (r, s) =>
				{
					if (r == null)
						onCompleted(r, s);
					else
					{
						if (Conf.DebugMode == Conf.DebugState.Verbose)
							Debug.Log(s);

						if (s.Length > 0 && s[0] == '{')
						{
							this.session = JsonUtility.FromJson<Session>(s);
							InternalNGDebug.Snapshot(this.session);
							//Utility.StartBackgroundTask(this.FetchSessionAndToken(username, password), () => onCompleted(r, s));
						}
						else
							onCompleted(r, s);
					}
				});
			}
		}

		public void	Connect(string username, string password, Action<HttpWebResponse, string> onCompleted)
		{
			string			endpoint = "https://kharma.unity3d.com/login";
			HttpWebRequest	request = this.CreateRequest(endpoint);
			StringBuilder	buffer = Utility.GetBuffer();

			buffer.Append("user=");
			buffer.Append(PublisherAPI.UrlEncode(username));
			buffer.Append("&pass=");
			buffer.Append(PublisherAPI.UrlEncode(password));
			buffer.Append("&unityversion=");
			buffer.Append(PublisherAPI.UrlEncode(Application.unityVersion));
			buffer.Append("&toolversion=");
			buffer.Append(PublisherAPI.UrlEncode("V5.0.2"));
			buffer.Append("&license_hash=");
			buffer.Append(PublisherAPI.UrlEncode(this.GetLicenseHash()));
			buffer.Append("&hardware_hash=");
			buffer.Append(PublisherAPI.UrlEncode(this.GetHardwareHash()));

			byte[]	content = Encoding.UTF8.GetBytes(Utility.ReturnBuffer(buffer));

			request.Headers = new WebHeaderCollection();
			request.Method = "POST";
			request.ContentType = "application/x-www-form-urlencoded";
			request.ContentLength = content.Length;
			request.Accept = "application/json";
			request.KeepAlive = true;
			request.Host = "kharma.unity3d.com";
			request.CookieContainer = new CookieContainer();

			using (Stream stream = request.GetRequestStream())
			{
				stream.Write(content, 0, content.Length);
			}

			this.HandleRequest(request, endpoint.GetHashCode(), (r, s) =>
			{
				if (r == null)
					onCompleted(r, s);
				else
				{
					if (Conf.DebugMode == Conf.DebugState.Verbose)
						Debug.Log(s);

					if (s.Length > 0 && s[0] == '{')
					{
						this.session = JsonUtility.FromJson<Session>(s);
						Utility.StartBackgroundTask(this.FetchSessionAndToken(username, password), () => onCompleted(r, s));
					}
					else
						onCompleted(r, s);
				}
			});
		}

		public void	ValidateTFACode(string code, Action<HttpWebResponse, string> onCompleted)
		{
			Dictionary<string, string>	post = new Dictionary<string, string>()
			{
				{ "utf8", "✓" },
				{ "_method", "put" },
				{ "authenticity_token", this.TFAParameters.authenticity_token },
				{ "conversations_tfa_required_form[verify_code]", code },
				{ "conversations_tfa_required_form[submit_verify_code]", "Verify" },
				{ "commit", "Sign in" }
			};

			// Validate TFA code.
			using (UnityWebRequest w = UnityWebRequest.Post(this.TFAParameters.url, post))
			{
				//w.SetRequestHeader("Cookie", "_genesis_auth_frontend_session=" + _genesis_auth_frontend_session + "; path=/; secure; HttpOnly");

				//w.redirectLimit = 0;

				w.SendWebRequest();

				while (w.isDone == false);

				string html = w.downloadHandler.text;
				Debug.Log(html);

				foreach (var item in w.GetResponseHeaders())
					Debug.Log(item.Key + "=" + item.Value);
				//string	cookies = w.GetResponseHeader("Set-Cookie");
				//string	cookieName = "kharma_token=";
				//int		start = cookies.IndexOf(cookieName);
				//Debug.Assert(start != -1, cookieName + " was not found.");

				//start += cookieName.Length;
				//int		end = cookies.IndexOf(';', start);
				//string	kharma_token = cookies.Substring(start, end - start);

				//cookieName = "kharma_session=";
				//start = cookies.IndexOf(cookieName);
				//Debug.Assert(start != -1, cookieName + " was not found.");

				//start += cookieName.Length;
				//end = cookies.IndexOf(';', start);
				//string	kharma_session = cookies.Substring(start, end - start);

				//this.session.xunitysession = kharma_session;
				//this.session.kharma_token = kharma_token;

				//onCompleted(null, null);
			}

			this.TFAParameters = null;
		}

		public void	Use(string kharma_session, string kharma_token, Action<HttpWebResponse, string> onCompleted)
		{
			using (var request = UnityWebRequest.Get(PublisherAPI.EndPoint + "api/publisher/overview.json"))
			{
				request.SetRequestHeader("X-Requested-With", "XMLHttpRequest");
				request.SetRequestHeader("X-Kharma-Token", kharma_token);
				request.SetRequestHeader("Cookie", "kharma_session=" + kharma_session + "; kharma_token=" + kharma_token);

				lock (this.runningRequests)
				{
					this.runningRequests.Add("https://kharma.unity3d.com/login".GetHashCode());
				}

				request.SendWebRequest();

				while (request.isDone == false);

				if (request.HasError() == true)
					Debug.LogError(request.error);
				else
				{
					Publisher	publisher = JsonUtility.FromJson<Publisher>(request.downloadHandler.text);

					this.session = new Session()
					{
						name = publisher.overview.name,
						publisher = publisher.overview.id,
						xunitysession = kharma_session,
						kharma_token = Uri.UnescapeDataString(kharma_token),
						keyimage = new Session.Keyimage()
						{
							icon = publisher.overview.keyimage.small_v2
						}
					};
				}

				lock (this.runningRequests)
				{
					this.runningRequests.Remove("https://kharma.unity3d.com/login".GetHashCode());
				}

				if (onCompleted != null)
					onCompleted(null, request.downloadHandler.text);
			}
		}

		public void	Disconnect()
		{
			this.session = null;
		}

		public bool	IsGettingAllPackages()
		{
			lock (this.runningRequests)
			{
				return this.runningRequests.Contains((PublisherAPI.EndPoint + "api/management/packages.json").GetHashCode());
			}
		}

		public void	GetAllPackages(Action<HttpWebResponse, string> onCompleted)
		{
			string			endpoint = PublisherAPI.EndPoint + "api/management/packages.json";
			HttpWebRequest	request = this.CreateRequest(endpoint);

			this.HandleRequest(request, endpoint.GetHashCode(), onCompleted);
		}

		public bool	IsGettingLanguages()
		{
			lock (this.runningRequests)
			{
				return this.runningRequests.Contains((PublisherAPI.EndPoint + "api/management/languages.json").GetHashCode());
			}
		}

		public void	GetLanguages(Action<HttpWebResponse, string> onCompleted)
		{
			string			endpoint = PublisherAPI.EndPoint + "api/management/languages.json";
			HttpWebRequest	request = this.CreateRequest(endpoint);

			this.HandleRequest(request, endpoint.GetHashCode(), onCompleted);
		}

		public bool	IsGettingVettingConfig()
		{
			lock (this.runningRequests)
			{
				return this.runningRequests.Contains((PublisherAPI.EndPoint + "api/management/vetting-config.json").GetHashCode());
			}
		}

		public void	GetVettingConfig(Action<HttpWebResponse, string> onCompleted)
		{
			string			endpoint = PublisherAPI.EndPoint + "api/management/vetting-config.json";
			HttpWebRequest	request = this.CreateRequest(endpoint);

			this.HandleRequest(request, endpoint.GetHashCode(), onCompleted);
		}

		public bool	IsGettingCategories(int versionId)
		{
			lock (this.runningRequests)
			{
				string	endpoint = PublisherAPI.EndPoint + "api/management/categories/" + versionId + ".json";
				return this.runningRequests.Contains(endpoint.GetHashCode());
			}
		}

		public void	GetCategories(int versionId, Action<HttpWebResponse, string> onCompleted = null)
		{
			string			endpoint = PublisherAPI.EndPoint + "api/management/categories/" + versionId + ".json";
			HttpWebRequest	request = this.CreateRequest(endpoint);

			this.HandleRequest(request, endpoint.GetHashCode(), onCompleted);
		}

		public bool	IsGettingPackageVersion(int versionId)
		{
			lock (this.runningRequests)
			{
				return this.runningRequests.Contains((PublisherAPI.EndPoint + "api/management/package-version/" + versionId + ".json").GetHashCode());
			}
		}

		public void	GetPackageVersion(int versionId, Action<HttpWebResponse, string> onCompleted)
		{
			string			endpoint = PublisherAPI.EndPoint + "api/management/package-version/" + versionId + ".json";
			HttpWebRequest	request = this.CreateRequest(endpoint);

			this.HandleRequest(request, endpoint.GetHashCode(), onCompleted);
		}

		public bool	IsCreatingDraft(int versionId)
		{
			lock (this.runningRequests)
			{
				return this.runningRequests.Contains((PublisherAPI.EndPoint + "api/management/draft/" + versionId + ".json").GetHashCode());
			}
		}

		public void	CreateDraft(int versionId, Action<HttpWebResponse, string> onCompleted)
		{
			string			endpoint = PublisherAPI.EndPoint + "api/management/draft/" + versionId + ".json";
			HttpWebRequest	request = this.CreateRequest(endpoint);

			request.Method = "POST";

			this.HandleRequest(request, endpoint.GetHashCode(), onCompleted);
		}

		public bool	IsDeletingingDraft(int versionId)
		{
			lock (this.runningRequests)
			{
				return this.runningRequests.Contains((PublisherAPI.EndPoint + "api/management/draft/" + versionId + ".json").GetHashCode() + "DELETE".GetHashCode());
			}
		}

		public void	DeleteDraft(int versionId, Action<HttpWebResponse, string> onCompleted)
		{
			string			endpoint = PublisherAPI.EndPoint + "api/management/draft/" + versionId + ".json";
			HttpWebRequest	request = this.CreateRequest(endpoint);

			request.Method = "DELETE";

			this.HandleRequest(request, endpoint.GetHashCode() + "DELETE".GetHashCode(), onCompleted);
		}

		public bool	IsDeletingingUnityPackage(int versionId, VersionDetailed.Package.Version.UnityPackage unityPackage)
		{
			lock (this.runningRequests)
			{
				string	endpoint = PublisherAPI.EndPoint + "api/management/unitypackage/" + versionId + "/" + unityPackage.package_upload_id + ".json";
				return this.runningRequests.Contains(endpoint.GetHashCode() + "DELETE".GetHashCode());
			}
		}

		public void	DeleteUnityPackage(int versionId, VersionDetailed.Package.Version.UnityPackage unityPackage, Action<HttpWebResponse, string> onCompleted)
		{
			string			endpoint = PublisherAPI.EndPoint + "api/management/unitypackage/" + versionId + "/" + unityPackage.package_upload_id + ".json";
			HttpWebRequest	request = this.CreateRequest(endpoint);

			request.Method = "DELETE";

			this.HandleRequest(request, endpoint.GetHashCode() + "DELETE".GetHashCode(), onCompleted);
		}

		public bool	IsVettingVersion(int versionId, int uploadID)
		{
			lock (this.runningRequests)
			{
				return this.runningRequests.Contains((PublisherAPI.EndPoint + "api/management/vetting/" + versionId + ".json").GetHashCode() + uploadID);
			}
		}

		public void	VetVersion(int versionId, int uploadID, IEnumerable<string> platforms, IEnumerable<string> unityVersions, IEnumerable<string> srpTypes, IEnumerable<string> dependencies, Action<HttpWebResponse, string> onCompleted)
		{
			string			endpoint = PublisherAPI.EndPoint + "api/management/vetting/" + versionId + ".json";
			HttpWebRequest	request = this.CreateRequest(endpoint);
			StringBuilder	buffer = Utility.GetBuffer("{\"package_upload_id\":\"");
			bool			first = true;

			buffer.Append(uploadID);
			buffer.Append("\",\"platforms\":[");

			if (platforms != null)
			{
				foreach (string item in platforms)
				{
					if (first == false)
						buffer.Append(',');

					first = false;
					buffer.Append('"');
					buffer.Append(item);
					buffer.Append('"');
				}
			}

			buffer.Append("],\"unity_versions\":[");

			first = true;

			if (unityVersions != null)
			{
				foreach (string item in unityVersions)
				{
					if (first == false)
						buffer.Append(',');

					first = false;
					buffer.Append('"');
					buffer.Append(item);
					buffer.Append('"');
				}
			}

			buffer.Append("],\"srp_type\":[");

			first = true;

			if (srpTypes != null)
			{
				foreach (string item in srpTypes)
				{
					if (first == false)
						buffer.Append(',');

					first = false;
					buffer.Append('"');
					buffer.Append(item);
					buffer.Append('"');
				}
			}

			buffer.Append("],\"dependencies\":[");

			first = true;

			if (dependencies != null)
			{
				foreach (string item in dependencies)
				{
					if (first == false)
						buffer.Append(',');

					first = false;
					buffer.Append('"');
					buffer.Append(item);
					buffer.Append('"');
				}
			}

			buffer.Append("]}");

			byte[]	content = Encoding.ASCII.GetBytes(Utility.ReturnBuffer(buffer));

			request.Method = "POST";
			request.ContentLength = content.Length;
			request.Accept = "application/json";

			using (Stream stream = request.GetRequestStream())
			{
				stream.Write(content, 0, content.Length);
			}

			this.HandleRequest(request, endpoint.GetHashCode() + uploadID, onCompleted);
		}

		public bool	IsAddingLanguage(int versionId, string languageCode)
		{
			lock (this.runningRequests)
			{
				string	endpoint = PublisherAPI.EndPoint + "api/management/package-version/" + versionId + ".json";
				return this.runningRequests.Contains(endpoint.GetHashCode() + languageCode.GetHashCode());
			}
		}

		public void	AddLanguage(int versionId, string languageCode, string versionName, Action<HttpWebResponse, string> onCompleted)
		{
			string			endpoint = PublisherAPI.EndPoint + "api/management/package-version/" + versionId + ".json";
			HttpWebRequest	request = this.CreateRequest(endpoint);

			byte[]	content = Encoding.UTF8.GetBytes("{\"metadata\":[{\"language\":\"" + languageCode + "\",\"name\":\"" + versionName + "\",\"description\":\"\"}]}");

			request.Method = "POST";
			request.ContentType = "application/json";
			request.ContentLength = content.Length;

			using (Stream stream = request.GetRequestStream())
			{
				stream.Write(content, 0, content.Length);
			}

			this.HandleRequest(request, endpoint.GetHashCode() + languageCode.GetHashCode(), onCompleted);
		}

		public bool	IsSettingKeyImage(int versionId, string language, string type)
		{
			lock (this.runningRequests)
			{
				string	endpoint = PublisherAPI.EndPoint + "api/management/keyimage/" + versionId + ".json";
				return this.runningRequests.Contains(endpoint.GetHashCode() + language.GetHashCode() + type.GetHashCode());
			}
		}

		public void	SetKeyImage(int versionId, string language, string imagePath, string type, Action<HttpWebResponse, string> onCompleted)
		{
			string			endpoint = PublisherAPI.EndPoint + "api/management/keyimage/" + versionId + ".json";
			HttpWebRequest	request = this.CreateRequest(endpoint);
			string			boundary = "--------" + DateTime.Now.Ticks.ToString();
			byte[]			boundaryBytes = Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

			request.Method = "POST";
			request.ContentType = "multipart/form-data; boundary=" + boundary;
			request.Accept = "*/*";
			request.Credentials = CredentialCache.DefaultCredentials;

			using (Stream stream = request.GetRequestStream())
			{
				stream.Write(boundaryBytes, 0, boundaryBytes.Length);
				this.Write(stream, "Content-Disposition: form-data; name=\"file\"; filename=\"");
				this.Write(stream, Path.GetFileName(imagePath));
				this.Write(stream, "\"\r\nContent-Type: image/png\r\n\r\n");

				byte[]	content = File.ReadAllBytes(imagePath);
				stream.Write(content, 0, content.Length);

				stream.Write(boundaryBytes, 0, boundaryBytes.Length);
				this.Write(stream, "Content-Disposition: form-data; name=\"language\"\r\n\r\n");
				this.Write(stream, language);

				stream.Write(boundaryBytes, 0, boundaryBytes.Length);
				this.Write(stream, "Content-Disposition: form-data; name=\"type\"\r\n\r\n");
				this.Write(stream, type);
				this.Write(stream, "\r\n--");

				this.Write(stream, boundary);
				this.Write(stream, "--\r\n");
			}

			this.HandleRequest(request, endpoint.GetHashCode() + language.GetHashCode() + type.GetHashCode(), onCompleted);
		}
	
		public bool	IsSettingPackageMetadata(int versionId)
		{
			lock (this.runningRequests)
			{
				return this.runningRequests.Contains((PublisherAPI.EndPoint + "api/management/package-version/" + versionId + ".json").GetHashCode());
			}
		}

		public void	SetPackageMetadata(int versionId, string languageCode, string name, string description, string keywords, Action<HttpWebResponse, string> onCompleted)
		{
			string			endpoint = PublisherAPI.EndPoint + "api/management/package-version/" + versionId + ".json";
			HttpWebRequest	request = this.CreateRequest(endpoint);
			StringBuilder	buffer = Utility.GetBuffer();

			buffer.Append("{\"metadata\":[{\"language\":\"");
			buffer.Append(this.Escape(languageCode));
			buffer.Append("\",\"name\":\"");
			buffer.Append(this.Escape(name));
			buffer.Append("\",\"description\":\"");
			buffer.Append(this.Escape(description));
			buffer.Append("\",\"keywords\":\"");
			buffer.Append(this.Escape(keywords));
			buffer.Append("\"}]}");

			byte[]	content = Encoding.UTF8.GetBytes(Utility.ReturnBuffer(buffer));

			request.Method = "POST";
			request.ContentType = "application/json";
			request.ContentLength = content.Length;

			using (Stream stream = request.GetRequestStream())
			{
				stream.Write(content, 0, content.Length);
			}

			this.HandleRequest(request, endpoint.GetHashCode(), onCompleted);
		}

		public bool	IsSettingPackage(int versionId)
		{
			lock (this.runningRequests)
			{
				return this.runningRequests.Contains((PublisherAPI.EndPoint + "api/management/package-version/" + versionId + ".json").GetHashCode());
			}
		}

		public void	SetPackage(int versionId, string versionName, string publishnotes, int categoryID, float price, Action<HttpWebResponse, string> onCompleted)
		{
			string			endpoint = PublisherAPI.EndPoint + "api/management/package-version/" + versionId + ".json";
			HttpWebRequest	request = this.CreateRequest(endpoint);
			StringBuilder	buffer = Utility.GetBuffer();

			buffer.Append("{\"version_name\":\"");
			buffer.Append(this.Escape(versionName));
			buffer.Append("\",\"publishnotes\":\"");
			buffer.Append(this.Escape(publishnotes));
			buffer.Append("\",\"category_id\":\"");
			buffer.Append(categoryID);
			buffer.Append("\",\"price\":\"");
			buffer.Append(price.ToString(CultureInfo.InvariantCulture));
			buffer.Append("\"}");

			byte[]	content = Encoding.UTF8.GetBytes(Utility.ReturnBuffer(buffer));

			request.Method = "POST";
			request.ContentType = "application/json";
			request.ContentLength = content.Length;

			using (Stream stream = request.GetRequestStream())
			{
				stream.Write(content, 0, content.Length);
			}

			this.HandleRequest(request, endpoint.GetHashCode(), onCompleted);
		}

		public bool	IsGettingVoucherPackages(int publisherId)
		{
			lock (this.runningRequests)
			{
				return this.runningRequests.Contains((PublisherAPI.EndPoint + "api/publisher-info/voucher-packages/" + publisherId + ".json").GetHashCode());
			}
		}

		public void	GetVoucherPackages(int publisherId, Action<HttpWebResponse, string> onCompleted)
		{
			string			endpoint = PublisherAPI.EndPoint + "api/publisher-info/voucher-packages/" + publisherId + ".json";
			HttpWebRequest	request = this.CreateRequest(endpoint);

			this.HandleRequest(request, endpoint.GetHashCode(), onCompleted);
		}

		public bool	IsGettingVouchers(int publisherId)
		{
			lock (this.runningRequests)
			{
				return this.runningRequests.Contains((PublisherAPI.EndPoint + "api/publisher-info/vouchers/" + publisherId + ".json").GetHashCode());
			}
		}

		public void	GetVouchers(int publisherId, Action<HttpWebResponse, string> onCompleted)
		{
			string			endpoint = PublisherAPI.EndPoint + "api/publisher-info/vouchers/" + publisherId + ".json";
			HttpWebRequest	request = this.CreateRequest(endpoint);

			this.HandleRequest(request, endpoint.GetHashCode(), onCompleted);
		}

		public bool	IsCreatingVoucher(int publisherId, int packageId)
		{
			lock (this.runningRequests)
			{
				return this.runningRequests.Contains((PublisherAPI.EndPoint + "api/publisher-info/voucher-freepackage/" + publisherId + ".json").GetHashCode() + packageId);
			}
		}

		public void	CreateVoucher(int publisherId, int packageId, Action<HttpWebResponse, string> onCompleted)
		{
			string			endpoint = PublisherAPI.EndPoint + "api/publisher-info/voucher-freepackage/" + publisherId + ".json";
			HttpWebRequest	request = this.CreateRequest(endpoint);
			StringBuilder	buffer = Utility.GetBuffer();

			buffer.Append("package_id=");
			buffer.Append(packageId);

			byte[]	content = Encoding.UTF8.GetBytes(Utility.ReturnBuffer(buffer));

			request.Method = "POST";
			request.ContentType = " application/x-www-form-urlencoded; charset=UTF-8";
			request.ContentLength = content.Length;

			using (Stream stream = request.GetRequestStream())
			{
				stream.Write(content, 0, content.Length);
			}

			this.HandleRequest(request, endpoint.GetHashCode() + packageId, onCompleted);
		}

		public bool	IsCanSubmit(int versionId)
		{
			lock (this.runningRequests)
			{
				return this.runningRequests.Contains((PublisherAPI.EndPoint + "api/management/package-version/" + versionId + ".json").GetHashCode());
			}
		}

		public void	CanSubmit(int versionId, string version, string publishnotes, int category_id, float price, Action<HttpWebResponse, string> onCompleted)
		{
			string			endpoint = PublisherAPI.EndPoint + "api/management/package-version/" + versionId + ".json";
			HttpWebRequest	request = this.CreateRequest(endpoint);
			StringBuilder	buffer = Utility.GetBuffer();

			buffer.Append("{\"version_name\":\"");
			buffer.Append(version);
			buffer.Append("\",\"publishnotes\":\"");
			buffer.Append(publishnotes.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n"));
			buffer.Append("\",\"category_id\":\"");
			buffer.Append(category_id);
			buffer.Append("\",\"price\":\"");
			buffer.Append(price.ToString("0.00", CultureInfo.InvariantCulture));
			buffer.Append("\"}");

			byte[]	content = Encoding.UTF8.GetBytes(Utility.ReturnBuffer(buffer));

			request.Method = "POST";
			request.ContentType = " application/json";
			request.ContentLength = content.Length;

			using (Stream stream = request.GetRequestStream())
			{
				stream.Write(content, 0, content.Length);
			}

			this.HandleRequest(request, endpoint.GetHashCode(), onCompleted);
		}

		public bool	IsSubmitting(int versionId)
		{
			lock (this.runningRequests)
			{
				return this.runningRequests.Contains((PublisherAPI.EndPoint + "api/management/submit/" + versionId + ".json").GetHashCode());
			}
		}

		public void	Submit(int versionId, bool autoPublish, string comments, Action<HttpWebResponse, string> onCompleted)
		{
			string			endpoint = PublisherAPI.EndPoint + "api/management/submit/" + versionId + ".json";
			HttpWebRequest	request = this.CreateRequest(endpoint);
			StringBuilder	buffer = Utility.GetBuffer();

			buffer.Append("submit_message=");
			buffer.Append(PublisherAPI.UrlEncode(comments));

			if (autoPublish == true)
				buffer.Append("&auto_publish=Y");

			byte[]	content = Encoding.UTF8.GetBytes(Utility.ReturnBuffer(buffer));

			request.Method = "POST";
			request.ContentType = " application/x-www-form-urlencoded; charset=UTF-8";
			request.ContentLength = content.Length;

			using (Stream stream = request.GetRequestStream())
			{
				stream.Write(content, 0, content.Length);
			}

			this.HandleRequest(request, endpoint.GetHashCode(), onCompleted);
		}

		public bool	IsPublishing(int versionId)
		{
			lock (this.runningRequests)
			{
				return this.runningRequests.Contains((PublisherAPI.EndPoint + "api/management/publish/" + versionId + ".json").GetHashCode());
			}
		}

		public void	Publish(int versionId, string comments, Action<HttpWebResponse, string> onCompleted)
		{
			string			endpoint = PublisherAPI.EndPoint + "api/management/publish/" + versionId + ".json";
			HttpWebRequest	request = this.CreateRequest(endpoint);
			StringBuilder	buffer = Utility.GetBuffer();

			buffer.Append("submit_message=");
			buffer.Append(PublisherAPI.UrlEncode(comments));

			byte[]	content = Encoding.UTF8.GetBytes(Utility.ReturnBuffer(buffer));

			request.Method = "POST";
			request.ContentType = " application/x-www-form-urlencoded; charset=UTF-8";
			request.ContentLength = content.Length;

			using (Stream stream = request.GetRequestStream())
			{
				stream.Write(content, 0, content.Length);
			}

			this.HandleRequest(request, endpoint.GetHashCode(), onCompleted);
		}

		public bool	IsGettingPeriods(int publisherId)
		{
			lock (this.runningRequests)
			{
				return this.runningRequests.Contains((PublisherAPI.EndPoint + "api/publisher-info/months/" + publisherId + ".json").GetHashCode());
			}
		}

		public void	GetPeriods(int publisherId, Action<HttpWebResponse, string> onCompleted)
		{
			string			endpoint = PublisherAPI.EndPoint + "api/publisher-info/months/" + publisherId + ".json";
			HttpWebRequest	request = this.CreateRequest(endpoint);

			this.HandleRequest(request, endpoint.GetHashCode(), onCompleted);
		}

		public bool	IsGettingSaleCounts(int publisherId, int periodId)
		{
			lock (this.runningRequests)
			{
				return this.runningRequests.Contains((PublisherAPI.EndPoint + "api/publisher-info/sales/" + publisherId + "/" + periodId + ".json").GetHashCode());
			}
		}

		public void	GetSaleCounts(int publisherId, int periodId, Action<HttpWebResponse, string> onCompleted)
		{
			string			endpoint = PublisherAPI.EndPoint + "api/publisher-info/sales/" + publisherId + "/" + periodId + ".json";
			HttpWebRequest	request = this.CreateRequest(endpoint);

			this.HandleRequest(request, endpoint.GetHashCode(), onCompleted);
		}

		public bool	IsGettingFreeDownloads(int publisherId, int periodId)
		{
			lock (this.runningRequests)
			{
				return this.runningRequests.Contains((PublisherAPI.EndPoint + "api/publisher-info/downloads/" + publisherId + "/" + periodId + ".json?package_filter=all").GetHashCode());
			}
		}

		public void	GetFreeDownloads(int publisherId, int periodId, Action<HttpWebResponse, string> onCompleted)
		{
			string			endpoint = PublisherAPI.EndPoint + "api/publisher-info/downloads/" + publisherId + "/" + periodId + ".json?package_filter=all";
			HttpWebRequest	request = this.CreateRequest(endpoint);

			this.HandleRequest(request, endpoint.GetHashCode(), onCompleted);
		}

		public void	GetAPIKey(int publisherId, Action<HttpWebResponse, string> onCompleted)
		{
			string			endpoint = PublisherAPI.EndPoint + "api/publisher-info/api-key/" + publisherId + ".json";
			HttpWebRequest	request = this.CreateRequest(endpoint);

			this.HandleRequest(request, endpoint.GetHashCode(), onCompleted);
		}

		//public void	InsertArtwork(int versionID, string language, string image_path, Action<HttpWebResponse, string> onCompleted)
		//{
		//	string	endpoint = PublisherAPI.EndPoint + "api/management/screenshot/";
		//	HttpWebRequest	ch = curl_init(endpoint + versionID + ".json");

		//	var post = new [] {
		//		"file" = new CurlFile(image_path, "image/png", "temp.png"),
		//		"language" = language
		//	);

		//	curl_setopt(ch, CURLOPT_POSTFIELDS, post);

		//	this.HandleRequest(ch, onCompleted);
		//}

		//public void	ReplaceArtwork(int versionID, int artworkID, string language, string image_path, Action<HttpWebResponse, string> onCompleted)
		//{
		//	string	endpoint = PublisherAPI.EndPoint + "api/management/screenshot/";
		//	HttpWebRequest	ch = curl_init(endpoint + versionID + "/" + artworkID + ".json");

		//	var post = array(
		//		"file" => new CurlFile(image_path, "image/png", "temp.png"),
		//		"language" => language
		//	);

		//	curl_setopt(ch, CURLOPT_POSTFIELDS, post);

		//	this.HandleRequest(ch, onCompleted);
		//	if (package != null)
		//		return json_decode(package);

		//	return null;
		//}

		//public void	DeleteArtwork(int versionID, int artworkID, Action<HttpWebResponse, string> onCompleted)
		//{
		//	string	endpoint = PublisherAPI.EndPoint + "api/management/screenshot/";
		//	HttpWebRequest	ch = curl_init(endpoint + versionID + "/" + artworkID + ".json");
		//	curl_setopt(ch, CURLOPT_CUSTOMREQUEST, "DELETE");

		//	this.HandleRequest(ch, onCompleted);
		//}

		//public void	OrderArtworks(int versionID, int[] artworks, Action<HttpWebResponse, string> onCompleted)
		//{
		//	string	endpoint = PublisherAPI.EndPoint + "api/management/screenshot-order/";
		//	HttpWebRequest	ch = curl_init(endpoint + versionID + ".json");

		//	var post = array(
		//		"order" => implode(",", artworks)
		//	);

		//	curl_setopt(ch, CURLOPT_POSTFIELDS, post);

		//	this.HandleRequest(ch, onCompleted);
		//}

		// Database utilities
		//public void	PopulateArtworks(int languageID, int[] artworks)
		//{
		//	global $pdo;

		//	$sql = "DELETE FROM assetstore_artworks
		//		WHERE language_id = " + languageID;
		//	$pdo->exec($sql);

		//	$sql = "INSERT INTO assetstore_artworks (id, language_id, artwork_id, type, reference, uri, scaled) VALUES ";

		//	for ($i = 0; $i < count(artworks); ++$i)
		//	{
		//		$artwork = artworks[$i];

		//		if ($i > 0)
		//			$sql .= ",";

		//		$sql .= "(" + i + ", " + languageID + ", " . (int)$artwork->id + ", " + pdo->quote($artwork->type) + ", " + pdo->quote($artwork->reference) + ", " + pdo->quote($artwork->uri) + ", " + pdo->quote(isset($artwork->scaled) ? $artwork->scaled : "") + ")";
		//	}

		//	if (count(artworks) == 0)
		//		return 0;
		//	return $pdo->exec($sql);
		//}

		private string	Escape(string input)
		{
			return input.Replace("\\", "\\\\").Replace("\r", "\\r").Replace("\\r\n", "\\n").Replace("\n", "\\n").Replace("\r\n", "\\n").Replace("\"", "\\\"");
		}

		private void	Write(Stream stream, string a)
		{
			byte[]	boundaryRNB = Encoding.UTF8.GetBytes(a);
			stream.Write(boundaryRNB, 0, boundaryRNB.Length);
		}

		private static string	UrlEncode(string str)
		{
			if (str == null)
				return null;
			return PublisherAPI.UrlEncode(str, Encoding.UTF8);
		}

		private static string	UrlEncode(string str, Encoding e)
		{
			if (str == null)
				return null;
			return Encoding.ASCII.GetString(PublisherAPI.UrlEncodeToBytes(str, e));
		}

		private static byte[]	UrlEncodeToBytes(string str, Encoding e)
		{
			if (str == null)
				return null;

			byte[]	bytes = e.GetBytes(str);
			return PublisherAPI.UrlEncodeBytesToBytesInternal(bytes, 0, bytes.Length, false);
		}

		private static byte[]	UrlEncodeBytesToBytesInternal(byte[] bytes, int offset, int count, bool alwaysCreateReturnValue)
		{
			int	num = 0;
			int	num2 = 0;

			for (int i = 0; i < count; i++)
			{
				char c = (char)bytes[offset + i];
				if (c == ' ')
					num++;
				else if (!PublisherAPI.IsSafe(c))
					num2++;
			}

			if (!alwaysCreateReturnValue && num == 0 && num2 == 0)
				return bytes;

			byte[]	array = new byte[count + num2 * 2];
			int		num3 = 0;

			for (int j = 0; j < count; j++)
			{
				byte	b = bytes[offset + j];
				char	c2 = (char)b;

				if (PublisherAPI.IsSafe(c2))
					array[num3++] = b;
				else if (c2 == ' ')
					array[num3++] = 43;
				else
				{
					array[num3++] = 37;
					array[num3++] = (byte)PublisherAPI.IntToHex(b >> 4 & 15);
					array[num3++] = (byte)PublisherAPI.IntToHex((int)(b & 15));
				}
			}

			return array;
		}

		private static char	IntToHex(int n)
		{
			if (n <= 9)
				return (char)(n + 48);
			return (char)(n - 10 + 97);
		}

		private static bool	IsSafe(char ch)
		{
			if ((ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z') || (ch >= '0' && ch <= '9'))
				return true;

			if (ch != '!')
			{
				switch (ch)
				{
					case '\'':
					case '(':
					case ')':
					case '*':
					case '-':
					case '.':
						return true;
					case '+':
					case ',':
						break;
					default:
						if (ch == '_')
						{
							return true;
						}
						break;
				}
				return false;
			}
			return true;
		}

		private string	GetLicenseHash()
		{
			return InternalEditorUtility.GetAuthToken().Substring(0, 40);
		}

		private string	GetHardwareHash()
		{
			return InternalEditorUtility.GetAuthToken().Substring(40, 40);
		}

		private HttpWebRequest	CreateRequest(string endpoint)
		{
			HttpWebRequest	request = (HttpWebRequest)WebRequest.Create(endpoint);

			ServicePointManager.ServerCertificateValidationCallback = (a, b, c, d) => true;

			request.CookieContainer = new CookieContainer();

			if (this.IsConnected == true)
			{
				try
				{
					request.CookieContainer.Add(new Cookie("kharma_session", this.session.xunitysession, "/", ".assetstore.unity3d.com"));
					request.CookieContainer.Add(new Cookie("kharma_token", this.session.kharma_token, "/", ".assetstore.unity3d.com"));
					request.Headers.Add("X-Kharma-Token", this.session.kharma_token);
				}
				catch (Exception ex)
				{
					Debug.LogException(ex);
					this.session = null;
				}
			}

			request.Headers.Add(HttpRequestHeader.AcceptLanguage, "fr,fr-FR;q=0.8,en-US;q=0.5,en;q=0.3");
			request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate, br");
			request.KeepAlive = true;
			request.Headers.Add("DNT", "1");
			request.Host = PublisherAPI.Host;
			request.Referer = PublisherAPI.EndPoint;
			request.Headers.Add("Origin", PublisherAPI.EndPoint);
			request.Headers.Add(HttpRequestHeader.Te, "Trailers");
			request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:71.0) Gecko/20100101 Firefox/71.0";
			request.Headers.Add("X-Requested-With", "XMLHttpRequest");

			return request;
		}

		private IEnumerator	FetchSessionAndToken(string username, string password)
		{
			lock (this.runningRequests)
			{
				this.runningRequests.Add("https://kharma.unity3d.com/login".GetHashCode());
			}

			try
			{
				ServicePointManager.ServerCertificateValidationCallback = (a, b, c, d) => true;

				UnityWebRequest.ClearCookieCache(new Uri("https://publisher.assetstore.unity3d.com/sales.html"));

				string	endpoint = "https://publisher.assetstore.unity3d.com/sales.html";
				string	authenticity_token;
				string	_genesis_auth_frontend_session;
				int		start;
				int		end;
				string	html;

				// Request the authenticity_token from any page.
				using (UnityWebRequest w = UnityWebRequest.Get(endpoint))
				{
					w.SendWebRequest();

					while (w.isDone == false)
						yield return null;

					//Debug.Log(w.responseCode);
					//Debug.Log(w.isHttpError);
					//Debug.Log(w.isNetworkError);
					//Debug.Log(w.error);

					//foreach (var item in w.GetResponseHeaders())
					//	Debug.Log(item.Key + "=" + item.Value);

					html = w.downloadHandler.text;
					//Debug.Log(html);

					//start = html.IndexOf("<input type=\"hidden\" name=\"authenticity_token\" value=\"");
					//Debug.Assert(start != -1, "Input \"authenticity_token\" not found.");

					//start += "<input type=\"hidden\" name=\"authenticity_token\" value=\"".Length;
					//end = html.IndexOf('"', start);
					//authenticity_token = html.Substring(start, end - start);
					//Debug.Log("authenticity_token="+ authenticity_token);

					const string	refreshHeader = "http-equiv=\"refresh\" content=\"1; url=";

					start = html.IndexOf(refreshHeader);

					if (start != -1)
					{
						start += refreshHeader.Length;
						end = html.IndexOf("\"", start + 1);

						endpoint = html.Substring(start, end);
						Debug.Log(endpoint);

						using (UnityWebRequest w2 = UnityWebRequest.Get(endpoint))
						{
							w2.SendWebRequest();

							while (w2.isDone == false)
								yield return null;

							Debug.Log(w2.responseCode);
							Debug.Log(w2.isHttpError);
							Debug.Log(w2.isNetworkError);
							Debug.Log(w2.error);

							foreach (var item in w2.GetResponseHeaders())
								Debug.Log(item.Key + "=" + item.Value);

							html = w2.downloadHandler.text;
							Debug.Log(html);
						}
					}

					start = html.IndexOf("<meta name=\"csrf-token\" content=\"");
					Debug.Assert(start != -1, "Input \"authenticity_token\" not found.");

					start += "<meta name=\"csrf-token\" content=\"".Length;
					end = html.IndexOf('"', start);
					authenticity_token = html.Substring(start, end - start);
					//Debug.Log("authenticity_token="+ authenticity_token);

					string	cookies = w.GetResponseHeader("Set-Cookie");
					start = "_genesis_auth_frontend_session=".Length;
					end = cookies.IndexOf("; path=");
					_genesis_auth_frontend_session = cookies.Substring(start, end - start);
					//Debug.Log("cookies=" + cookies);
					//Debug.Log("_genesis_auth_frontend_session=" + _genesis_auth_frontend_session);

					endpoint = w.url;
				}

				ServicePointManager.ServerCertificateValidationCallback = (a, b, c, d) => true;

				WebRequest	rq = WebRequest.Create(endpoint);

				rq.Method = "POST";

				string	post = string.Format("utf8=%E2%9C%93&_method=put&authenticity_token=" + System.Net.WebUtility.UrlEncode(authenticity_token) + "&conversations_create_session_form%5Bemail%5D=mikil0%40yahoo.fr&conversations_create_session_form%5Bpassword%5D=k3RbsodwU&conversations_create_session_form%5Bremember_me%5D=false&commit=Sign+in");
				byte[]	data = Encoding.ASCII.GetBytes(post);

				rq.ContentType = "application/x-www-form-urlencoded";
				rq.ContentLength = data.Length;

				rq.Headers.Add("Cookie", "_genesis_auth_frontend_session=" + _genesis_auth_frontend_session);

				using (Stream stream = rq.GetRequestStream())
				{
					stream.Write(data, 0, data.Length);
				}

				try
				{
					using (HttpWebResponse response = (HttpWebResponse)rq.GetResponse())
					using (StreamReader readStream = new StreamReader(response.GetResponseStream(), Encoding.ASCII))
					{
						html = readStream.ReadToEnd();
					}
				}
				catch (Exception ex)
				{
					Debug.LogException(ex);
				}

				if (html.Contains("tfa_required") == true)
				{
					//Debug.Log("_genesis_auth_frontend_session="+_genesis_auth_frontend_session);
					//Debug.Log(html);

					start = html.IndexOf("<input type=\"hidden\" name=\"authenticity_token\" value=\"");
					Debug.Assert(start != -1, "Input \"authenticity_token\" not found.");

					start += "<input type=\"hidden\" name=\"authenticity_token\" value=\"".Length;
					end = html.IndexOf('"', start);
					authenticity_token = html.Substring(start, end - start);

					//Debug.Log("endpoint=" + endpoint);
					//Debug.Log("authenticity_token="+authenticity_token);
					this.TFAParameters = new TwoFactorAuthParameters()
					{
						url = endpoint,
						authenticity_token = authenticity_token
					};

					lock (this.runningRequests)
					{
						Debug.Log("Remove FetchSessionAndToken " + "https://kharma.unity3d.com/login".GetHashCode());
						this.runningRequests.Remove("https://kharma.unity3d.com/login".GetHashCode());
					}

					yield break;
				}

				start = html.IndexOf("window.location.href = \"");
				Debug.Assert(start != -1, "\"window.location.href\" was not found.");
				//Debug.Log(html);

				start += "window.location.href = \"".Length;
				end = html.IndexOf('"', start);
				endpoint = html.Substring(start, end - start);
				//Debug.Log("endpoint=" + endpoint);

				// Extract cookies kharma_session & kharma_token.
				using (UnityWebRequest w = UnityWebRequest.Get(endpoint))
				{
					w.redirectLimit = 0;

					w.SendWebRequest();

					while (w.isDone == false)
						yield return null;

					string	cookies = w.GetResponseHeader("Set-Cookie");
					string	cookieName = "kharma_token=";
					start = cookies.IndexOf(cookieName);
					Debug.Assert(start != -1, cookieName + " was not found.");

					start += cookieName.Length;
					end = cookies.IndexOf(';', start);
					string	kharma_token = cookies.Substring(start, end - start);

					cookieName = "kharma_session=";
					start = cookies.IndexOf(cookieName);
					Debug.Assert(start != -1, cookieName + " was not found.");

					start += cookieName.Length;
					end = cookies.IndexOf(';', start);
					string	kharma_session = cookies.Substring(start, end - start);

					// Ensure main thread.
					EditorApplication.delayCall += () =>
					{
						this.session.xunitysession = kharma_session;
						this.session.kharma_token = Uri.UnescapeDataString(kharma_token);
					};
				}
			}
			finally
			{
				lock (this.runningRequests)
				{
					this.runningRequests.Remove("https://kharma.unity3d.com/login".GetHashCode());
				}
			}
		}

		private void	HandleRequest(HttpWebRequest request, int requestHash, Action<HttpWebResponse, string> onCompleted)
		{
			lock (this.runningRequests)
			{
				this.runningRequests.Add(requestHash);
			}

			Action	asyncWebRequestInvoke = () =>
			{
				try
				{
					using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
					using (StreamReader readStream = new StreamReader(response.GetResponseStream(), Encoding.ASCII))
					{
						string	result = readStream.ReadToEnd();
						Utility.SafeDelayCall(() => onCompleted(response, result));
					}
				}
				catch (WebException ex)
				{
					HttpWebResponse	response = ex.Response as HttpWebResponse;

					if (response != null)
					{
						if (response.StatusCode == HttpStatusCode.Unauthorized)
							this.session = null;
						Utility.SafeDelayCall(() => onCompleted(response, response.StatusDescription));
					}
					else
						Utility.SafeDelayCall(() => onCompleted(null, ex.Message));
				}
				catch (Exception ex)
				{
					Utility.SafeDelayCall(() => onCompleted(null, ex.Message));
				}
				finally
				{
					EditorApplication.delayCall += () =>
					{
						lock (this.runningRequests)
						{
							this.runningRequests.Remove(requestHash);
						}
					};
				}
			};

			asyncWebRequestInvoke.BeginInvoke(iar => ((Action)iar.AsyncState).EndInvoke(iar), asyncWebRequestInvoke);
		}
	}
}