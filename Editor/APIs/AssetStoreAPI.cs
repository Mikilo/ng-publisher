using NGToolsStandalone_For_NGPublisherEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using UnityEditor;

namespace NGPublisher
{
	public class AssetStoreAPI : IAssetStoreAPI
	{
		private HashSet<int>	runningRequests = new HashSet<int>();

		public	AssetStoreAPI()
		{
		}

		public bool	IsGettingRatings(int packageId)
		{
			lock (this.runningRequests)
			{
				return this.runningRequests.Contains("https://assetstore.unity.com/api/graphql/batch".GetHashCode() + packageId.GetHashCode() + "GetRatings".GetHashCode());
			}
		}

		public void	GetRatings(int packageId, Action<HttpWebResponse, string> onCompleted)
		{
			string			endpoint = "https://assetstore.unity.com/api/graphql/batch";
			HttpWebRequest	request = (HttpWebRequest)WebRequest.Create(endpoint);
			StringBuilder	buffer = Utility.GetBuffer(@"[{""query"":""query ProductRatingStar {\n rating(id: $id) {\n count\n value\n  }\n}\n"",""variables"":{""id"":""" + packageId + @"""}}]");
			byte[]			content = Encoding.ASCII.GetBytes(Utility.ReturnBuffer(buffer));

			request.ServerCertificateValidationCallback = (a, b, c, d) => true;
			request.Headers = new WebHeaderCollection();
			request.Method = "POST";
			request.ContentLength = content.Length;
			request.Accept = "application/json";
			request.Host = "assetstore.unity.com";
			request.Headers.Add("X-Requested-With", "XMLHttpRequest");
			request.Headers.Add("X-Csrf-Token", "180a6c954bba5a102b3be40522449382");
			request.CookieContainer = new CookieContainer();
			request.CookieContainer.Add(new Cookie("_csrf", "180a6c954bba5a102b3be40522449382", "/", ".assetstore.unity.com"));

			using (Stream stream = request.GetRequestStream())
			{
				stream.Write(content, 0, content.Length);
			}

			int	requestHash = endpoint.GetHashCode() + packageId.GetHashCode() + "GetRatings".GetHashCode();

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
						Utility.SafeDelayCall(() => onCompleted(response, response.StatusDescription));
					else
						Utility.SafeDelayCall(() => onCompleted(null, ex.Message));
				}
				catch (Exception ex)
				{
					//Debug.LogException(ex);
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