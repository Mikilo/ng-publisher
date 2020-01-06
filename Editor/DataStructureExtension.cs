using NGToolsStandalone_For_NGPublisher;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using UnityEngine;

namespace NGPublisher
{
	public static class DataStructureExtension
	{
#pragma warning disable 649
		[Serializable]
		private class Packages
		{
			public Package[]	packages;
			// public string		publisher_name; // Unused
			// public string		terms_current; // Unused
			// public string		terms_accepted; // Unused
			// public string		publisher_id; // Unused
		}

		[Serializable]
		private class LanguagesContainer
		{
			public Language[]	languages;
		}

		[Serializable]
		private class CategoriesContainer
		{
			public Category[]	categories;
		}

		[Serializable]
		private class PackageRatings
		{
			public Rating[]	rating;
		}
#pragma warning restore 649

		public static void	RequestAllPackages(this PublisherDatabase database, IPublisherAPI api, Action<RequestResponse<Package[]>> onCompleted = null)
		{
			api.GetAllPackages((r, result) =>
			{
				RequestResponse<Package[]>	requestResponse = new RequestResponse<Package[]>()
				{
					context = database,
					ok = false,
					error = null,
					result = null
				};

				if (Conf.DebugMode == Conf.DebugState.Verbose)
					Debug.Log(result);

				if (DataStructureExtension.CheckRequest(r, result, requestResponse) == true)
				{
					Response	response = DataStructureExtension.CheckResponse(result, requestResponse);

					if (Conf.DebugMode == Conf.DebugState.Verbose)
						InternalNGDebug.Snapshot(response);

					if (response == null || response.status == "ok")
					{
						Packages		packagesContainer = JsonUtility.FromJson<Packages>(result);
						List<Package>	packages = new List<Package>(packagesContainer.packages);

						packages.Sort((a, b) => a.name.CompareTo(b.name));

						requestResponse.ok = true;
						requestResponse.result = packages.ToArray();
						database.Packages = requestResponse.result;
					}
				}

				if (onCompleted != null)
					onCompleted(requestResponse);
			});
		}
		
		public static void	RequestLanguages(this PublisherDatabase database, IPublisherAPI api, Action<RequestResponse<Language[]>> onCompleted = null)
		{
			api.GetLanguages((r, result) =>
			{
				RequestResponse<Language[]>	requestResponse = new RequestResponse<Language[]>()
				{
					context = database,
					ok = false,
					error = null,
					result = null
				};

				if (DataStructureExtension.CheckRequest(r, result, requestResponse) == true)
				{
					LanguagesContainer	languages = JsonUtility.FromJson<LanguagesContainer>("{\"languages\":" + result + '}');

					if (Conf.DebugMode == Conf.DebugState.Verbose)
						Debug.Log(result);

					requestResponse.ok = true;
					requestResponse.result = languages.languages;
					database.Languages = requestResponse.result;
				}

				if (onCompleted != null)
					onCompleted(requestResponse);
			});
		}
		
		public static void	RequestVettingConfig(this PublisherDatabase database, IPublisherAPI api, Action<RequestResponse<Vets>> onCompleted = null)
		{
			api.GetVettingConfig((r, result) =>
			{
				RequestResponse<Vets>	requestResponse = new RequestResponse<Vets>()
				{
					context = database,
					ok = false,
					error = null,
					result = null
				};

				if (DataStructureExtension.CheckRequest(r, result, requestResponse) == true)
				{
					if (Conf.DebugMode == Conf.DebugState.Verbose)
						Debug.Log(result);

					requestResponse.ok = true;
					requestResponse.result = JsonUtility.FromJson<Vets>(result);
					database.Vets = requestResponse.result;
				}

				if (onCompleted != null)
					onCompleted(requestResponse);
			});
		}

		public static void	RequestRatings(this Package package, IAssetStoreAPI api, Action<RequestResponse<Rating[]>> onCompleted = null)
		{
			api.GetRatings(package.id, (r, result) =>
			{
				RequestResponse<Rating[]>	requestResponse = new RequestResponse<Rating[]>()
				{
					context = package,
					ok = false,
					error = null,
					result = null
				};

				if (DataStructureExtension.CheckRequest(r, result, requestResponse) == true)
				{
					PackageRatings	packageRatings = JsonUtility.FromJson<PackageRatings>(result.Substring("[{\"data\":".Length - 2, result.Length - "[{\"data\":".Length - 1));

					if (Conf.DebugMode == Conf.DebugState.Verbose)
						Debug.Log(result);

					requestResponse.ok = true;
					requestResponse.result = packageRatings.rating;
					package.Ratings = requestResponse.result;
				}
				else
					package.Ratings = new Rating[0];

				if (onCompleted != null)
					onCompleted(requestResponse);
			});
		}

		public static void	RequestCategories(this Version version, IPublisherAPI api, Action<RequestResponse<Category[]>> onCompleted = null)
		{
			api.GetCategories(version.id, (r, result) =>
			{
				RequestResponse<Category[]>	requestResponse = new RequestResponse<Category[]>()
				{
					context = version,
					ok = false,
					error = null,
					result = null
				};

				if (DataStructureExtension.CheckRequest(r, result, requestResponse) == true)
				{
					CategoriesContainer	categories = JsonUtility.FromJson<CategoriesContainer>("{\"categories\":" + result + "}");

					if (Conf.DebugMode == Conf.DebugState.Verbose)
						Debug.Log(result);

					requestResponse.ok = true;
					requestResponse.result = categories.categories;
					version.Categories = requestResponse.result;
				}

				if (onCompleted != null)
					onCompleted(requestResponse);
			});
		}

		public static void	RequestCreateDraft(this Version version, IPublisherAPI api, Action<RequestResponse<int>> onCompleted = null)
		{
			api.CreateDraft(version.id, (r, result) =>
			{
				RequestResponse<int>	requestResponse = new RequestResponse<int>()
				{
					context = version,
					ok = false,
					error = null,
					result = 0
				};

				if (DataStructureExtension.CheckRequest(r, result, requestResponse) == true)
				{
					Response	response = DataStructureExtension.CheckResponse(result, requestResponse);

					if (Conf.DebugMode == Conf.DebugState.Verbose)
						InternalNGDebug.Snapshot(response);

					if (response != null)
					{
						if (response.status == "ok")
						{
							Package	package = version.package;
							Version	model = null;

							// Pick a model, published if available. Even without, Unity might do it automatically on its side. Not sure though.
							for (int i = 0, max = package.versions.Length; i < max; ++i)
							{
								Version	v = package.versions[i];

								if (model == null || v.status == Status.Published)
									model = v;
							}

							string	modelJson = JsonUtility.ToJson(model);

							model = new Version();

							JsonUtility.FromJsonOverwrite(modelJson, model);

							model.id = response.id;
							model.status = Status.Draft;

							DateTime	now = DateTime.Now;

							model.created = now.Year.ToString("0000") + "-" + now.Month.ToString("00") + "-" + now.Day.ToString("00") + " " + now.Hour.ToString("00") + ":" + now.Minute.ToString("00") + ":" + now.Second.ToString("00");
							model.modified = model.created;
							model.published = string.Empty;

							Array.Resize(ref package.versions, package.versions.Length + 1);
							package.versions[package.versions.Length - 1] = model;

							requestResponse.ok = true;
							requestResponse.result = response.id;
						}
					}
				}

				if (onCompleted != null)
					onCompleted(requestResponse);
			});
		}

		public static void	RequestDeleteDraft(this Version version, IPublisherAPI api, Action<RequestResponse<bool>> onCompleted = null)
		{
			Debug.Assert(version.status == Status.Draft);

			api.DeleteDraft(version.id, (r, result) =>
			{
				RequestResponse<bool>	requestResponse = new RequestResponse<bool>()
				{
					context = version,
					ok = false,
					error = null,
					result = false
				};

				if (DataStructureExtension.CheckRequest(r, result, requestResponse) == true)
				{
					Response	response = DataStructureExtension.CheckResponse(result, requestResponse);

					if (Conf.DebugMode == Conf.DebugState.Verbose)
						InternalNGDebug.Snapshot(response);

					if (response != null && response.status == "ok")
					{
						Package			package = version.package;
						List<Version>	list = new List<Version>(package.versions);

						for (int j = 0, max2 = list.Count; j < max2; ++j)
						{
							if (list[j].id == version.id)
							{
								list.RemoveAt(j);
								break;
							}
						}

						package.versions = list.ToArray();

						requestResponse.ok = true;
						requestResponse.result = true;
					}
				}

				if (onCompleted != null)
					onCompleted(requestResponse);
			});
		}

		public static void	RequestSetPackage(this Version version, IPublisherAPI api, string versionName, string publishnotes, int categoryID, float price, Action<RequestResponse<bool>> onCompleted = null)
		{
			api.SetPackage(version.id,
						   versionName,
						   publishnotes,
						   categoryID,
						   price,
						   (r, result) =>
			{
				RequestResponse<bool>	requestResponse = new RequestResponse<bool>()
				{
					context = version,
					ok = false,
					error = null,
					result = false
				};

				if (DataStructureExtension.CheckRequest(r, result, requestResponse) == true)
				{
					Response	response = DataStructureExtension.CheckResponse(result, requestResponse);

					if (Conf.DebugMode == Conf.DebugState.Verbose)
						InternalNGDebug.Snapshot(response);

					if (response != null && response.status == "ok")
					{
						requestResponse.ok = true;
						requestResponse.result = true;
					}
				}

				if (onCompleted != null)
					onCompleted(requestResponse);
			});
		}

		public static void	RequestGetPackageVersion(this PublisherDatabase database, IPublisherAPI api, Version version, Action<RequestResponse<VersionDetailed>> onCompleted = null)
		{
			api.GetPackageVersion(version.id, (r, result) =>
			{
				RequestResponse<VersionDetailed>	requestResponse = new RequestResponse<VersionDetailed>()
				{
					context = version,
					ok = false,
					error = null,
					result = null
				};

				if (DataStructureExtension.CheckRequest(r, result, requestResponse) == true)
				{
					try
					{
						StringBuilder	buffer = Utility.GetBuffer(result);

						// Convert object "languages" to a JSON array.
						int	start = result.IndexOf("\"languages\":{");
						Debug.AssertFormat(start != -1, "Token \"languages\" not found when requesting version \"{0}\".", version);

						start += "\"languages\":{".Length - 1;

						// Rely on brackets instead of sibling, in case Unity server changes the order.
						int	end = DataStructureExtension.DigestBracketScope(buffer, start) - 1;
						Debug.AssertFormat(end < buffer.Length, "Closing token of \"languages\" not found when requesting version \"{0}\".", version);

						buffer[start] = '[';
						buffer[end] = ']';

						int	i = start;

						while (i < buffer.Length && buffer[i] != ']')
						{
							++i;

							int	startBracket = i;

							i = DataStructureExtension.DigestString(buffer, i);
							buffer[i + 1] = ',';
							buffer.Remove(i + 2, 1);

							buffer.Insert(startBracket, "{\"languageCode\":");

							i = DataStructureExtension.DigestBracketScope(buffer, startBracket);
						}

						result = Utility.ReturnBuffer(buffer);
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
					}

					VersionDetailed	versionDetail = JsonUtility.FromJson<VersionDetailed>(result);
					var				languages = new List<VersionDetailed.Package.Version.Language>(versionDetail.package.version.languages);

					languages.Sort((a, b) => a.languageCode.CompareTo(b.languageCode));

					versionDetail.package.version.languages = languages.ToArray();
					version.detailed = versionDetail;

					int	index = database.versions.FindIndex(v => v.package.version.id == versionDetail.package.version.id);
					if (index != -1)
						database.versions[index] = versionDetail;
					else
						database.versions.Add(versionDetail);

					requestResponse.ok = true;
					requestResponse.result = versionDetail;
				}

				if (onCompleted != null)
					onCompleted(requestResponse);
			});
		}

		public static void	RequestAddLanguage(this Version version, IPublisherAPI api, string languageCode, Action<RequestResponse<bool>> onCompleted = null)
		{
			api.AddLanguage(version.id, languageCode, version.name, (r, result) =>
			{
				RequestResponse<bool>	requestResponse = new RequestResponse<bool>()
				{
					context = version,
					ok = false,
					error = null,
					result = false
				};

				if (DataStructureExtension.CheckRequest(r, result, requestResponse) == true)
				{
					Response	response = DataStructureExtension.CheckResponse(result, requestResponse);

					if (Conf.DebugMode == Conf.DebugState.Verbose)
						InternalNGDebug.Snapshot(response);

					if (response != null && response.status == "ok")
					{
						requestResponse.ok = true;
						requestResponse.result = true;
					}
				}

				if (onCompleted != null)
					onCompleted(requestResponse);
			});
		}

		public static void	RequestVetVersion(this Version version, IPublisherAPI api, VersionDetailed.Package.Version.UnityPackage unityPackage, IEnumerable<string> platforms, IEnumerable<string> unityVersions, IEnumerable<string> srpTypes, IEnumerable<string> dependencies, Action<RequestResponse<bool>> onCompleted = null)
		{
			api.VetVersion(version.id,
						   unityPackage.package_upload_id,
						   platforms,
						   unityVersions,
						   srpTypes,
						   dependencies,
						   (r, result) =>
			{
				RequestResponse<bool>	requestResponse = new RequestResponse<bool>()
				{
					context = version,
					ok = false,
					error = null,
					result = false
				};

				if (DataStructureExtension.CheckRequest(r, result, requestResponse) == true)
				{
					Response	response = DataStructureExtension.CheckResponse(result, requestResponse);

					if (Conf.DebugMode == Conf.DebugState.Verbose)
						InternalNGDebug.Snapshot(response);

					if (response != null && response.status == "ok")
					{
						requestResponse.ok = true;
						requestResponse.result = true;
					}
				}

				if (onCompleted != null)
					onCompleted(requestResponse);
			});
		}

		public static void	RequestDeleteUnityPackage(this Version version, IPublisherAPI api, VersionDetailed.Package.Version.UnityPackage unityPackage, Action<RequestResponse<bool>> onCompleted = null)
		{
			api.DeleteUnityPackage(version.id, unityPackage, (r, result) =>
			{
				RequestResponse<bool>	requestResponse = new RequestResponse<bool>()
				{
					context = version,
					ok = false,
					error = null,
					result = false
				};

				if (DataStructureExtension.CheckRequest(r, result, requestResponse) == true)
				{
					Response	response = DataStructureExtension.CheckResponse(result, requestResponse);

					if (Conf.DebugMode == Conf.DebugState.Verbose)
						InternalNGDebug.Snapshot(response);

					if (response != null && response.status == "ok")
					{
						var unityPackages = new List<VersionDetailed.Package.Version.UnityPackage>(version.detailed.package.version.unitypackages);

						unityPackages.Remove(unityPackage);

						version.detailed.package.version.unitypackages = unityPackages.ToArray();

						requestResponse.ok = true;
						requestResponse.result = true;
					}
				}

				if (onCompleted != null)
					onCompleted(requestResponse);
			});
		}

		public static void	RequestSetPackageMetadata(this Version version, IPublisherAPI api, VersionDetailed.Package.Version.Language language, string name, string description, string keywords, Action<RequestResponse<bool>> onCompleted = null)
		{
			api.SetPackageMetadata(version.id, language.languageCode,
								   name,
								   description,
								   keywords,
								   (r, result) =>
			{
				RequestResponse<bool>	requestResponse = new RequestResponse<bool>()
				{
					context = version,
					ok = false,
					error = null,
					result = false
				};

				if (DataStructureExtension.CheckRequest(r, result, requestResponse) == true)
				{
					Response	response = DataStructureExtension.CheckResponse(result, requestResponse);

					if (Conf.DebugMode == Conf.DebugState.Verbose)
						InternalNGDebug.Snapshot(response);

					if (response != null && response.status == "ok")
					{
						requestResponse.ok = true;
						requestResponse.result = true;
					}
				}

				if (onCompleted != null)
					onCompleted(requestResponse);
			});
		}

		public static void	RequestSetKeyImage(this Version version, IPublisherAPI api, VersionDetailed.Package.Version.Language language, string path, string type, Action<RequestResponse<string>> onCompleted = null)
		{
			api.SetKeyImage(version.id, language.languageCode, path, type,
							(r, result) =>
			{
				RequestResponse<string>	requestResponse = new RequestResponse<string>()
				{
					context = version,
					ok = false,
					error = null,
					result = null
				};

				if (DataStructureExtension.CheckRequest(r, result, requestResponse) == true)
				{
					Response	response = DataStructureExtension.CheckResponse(result, requestResponse);

					if (Conf.DebugMode == Conf.DebugState.Verbose)
						InternalNGDebug.Snapshot(response);

					if (response != null && response.status == "ok")
					{
						requestResponse.ok = true;
						requestResponse.result = response.url;
					}
				}

				if (onCompleted != null)
					onCompleted(requestResponse);
			});
		}

		public static int	DigestBracketScope(StringBuilder raw, int i, int max = -1)
		{
			if (raw[i] == '{')
			{
				bool	backslashed = false;
				bool	inString = false;
				int		level = 1;

				++i;

				if (max < i)
					max = raw.Length;

				for (; i < max; i++)
				{
					if (raw[i] == '{')
					{
						if (inString == false && backslashed == false)
							++level;
					}
					else if (raw[i] == '}')
					{
						if (inString == false && backslashed == false)
						{
							--level;

							if (level == 0)
							{
								++i;
								break;
							}
						}
					}
					else if (raw[i] == '"')
					{
						if (backslashed == false)
							inString = !inString;
					}
					else if (raw[i] == '\\')
						backslashed = !backslashed;
					else
						backslashed = false;
				}
			}

			return i;
		}

		public static int	DigestString(StringBuilder raw, int i, int max = -1)
		{
			if (raw[i] == '"')
			{
				bool	backslashed = false;

				++i;

				if (max < i)
					max = raw.Length;

				for (; i < max; i++)
				{
					if (raw[i] == '"' && backslashed == false)
					{
						++i;
						break;
					}

					if (raw[i] == '\\')
						backslashed = !backslashed;
					else
						backslashed = false;
				}

				// Make sure even if the string is not properly ended with a quote.
				--i;
			}

			return i;
		}

		///// <summary>Checks if the response's content is usable. Returns false if something went wrong.</summary>
		/// <param name="request"></param>
		/// <param name="result"></param>
		/// <returns></returns>
		private static bool	CheckRequest<T>(HttpWebResponse request, string result, RequestResponse<T> requestResponse)
		{
			if (Conf.DebugMode == Conf.DebugState.Verbose)
				InternalNGDebug.Snapshot(request);

			if (request == null || request.StatusCode != HttpStatusCode.OK)
			{
				requestResponse.error = result;
				return false;
			}

			if (Conf.DebugMode == Conf.DebugState.Verbose)
			{
				//if (result[0] == '{' || result[0] == '[')
				//	NGDebug.LogJSON(result);
				//else
					Debug.Log(result);
			}

			return true;
		}

		private static Response	CheckResponse<T>(string result, RequestResponse<T> requestResponse)
		{
			if (result[0] == '{') // Let's consider it is JSON and try to get a response if available.
			{
				result = Response.MergeErrors(result);

				try
				{
					Response	response = JsonUtility.FromJson<Response>(result);

					if (response.status == "error")
					{
						if (Conf.DebugMode == Conf.DebugState.Verbose)
							Debug.LogError(result);

						requestResponse.error = response.errors.error.Replace("<br>", "\n");
						return response;
					}
					else if (response.status == "ok")
						return response;
				}
				catch (Exception ex)
				{
					// Is not really suppose to happen.
					Debug.LogError(result);
					Debug.LogException(ex);
				}
			}

			return null;
		}
	}
}