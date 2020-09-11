﻿using NGToolsStandalone_For_NGPublisher;
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
		private class VoucherPackagesContainer
		{
			public string[]	aaData;
		}

		[Serializable]
		private class VouchersContainer
		{
			public string[]	aaData;
		}

		[Serializable]
		private class FreeDownloadsContainer
		{
			public string[]	aaData;
			public Result[]	result;
		}

		[Serializable]
		private class SaleCountsContainer
		{
			public string[]	aaData;
			public Result[]	result;
		}

		[Serializable]
		private class PeriodsContainer
		{
			public Period[]	periods;
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

				if (DataStructureExtension.CheckRequest(r, result, requestResponse) == true)
				{
					Response	response = DataStructureExtension.CheckResponse(result, requestResponse);

					if (Conf.DebugMode == Conf.DebugState.Verbose)
						InternalNGDebug.Snapshot(response);

					if (response == null || response.Succeeded == true)
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
						InternalNGDebug.Snapshot(languages.languages);

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
					requestResponse.ok = true;
					requestResponse.result = JsonUtility.FromJson<Vets>(result);
					database.Vets = requestResponse.result;
				}

				if (onCompleted != null)
					onCompleted(requestResponse);
			});
		}

		public static void	RequestVoucherPackages(this PublisherDatabase database, IPublisherAPI api, Action<RequestResponse<VoucherPackage[]>> onCompleted = null)
		{
			api.GetVoucherPackages(api.Session.publisher, (r, result) =>
			{
				RequestResponse<VoucherPackage[]>	requestResponse = new RequestResponse<VoucherPackage[]>()
				{
					context = database,
					ok = false,
					error = null,
					result = null
				};

				if (DataStructureExtension.CheckRequest(r, result, requestResponse) == true)
				{
					VoucherPackagesContainer	container = JsonUtility.FromJson<VoucherPackagesContainer>(result.Replace("],[", ",").Replace("[[", "[").Replace("]]", "]"));

					if (Conf.DebugMode == Conf.DebugState.Verbose)
						InternalNGDebug.Snapshot(container);

					VoucherPackage[]	packages = new VoucherPackage[container.aaData.Length >> 1];

					for (int i = 0, max = container.aaData.Length; i < max; i += 2)
					{
						packages[i >> 1] = new VoucherPackage()
						{
							packageId = int.Parse(container.aaData[i]),
							packageName = container.aaData[i + 1],
						};
					}

					requestResponse.ok = true;
					requestResponse.result = packages;
					database.VoucherPackages = requestResponse.result;
				}

				if (onCompleted != null)
					onCompleted(requestResponse);
			});
		}

		public static void	RequestVouchers(this PublisherDatabase database, IPublisherAPI api, Action<RequestResponse<Voucher[]>> onCompleted = null)
		{
			api.GetVouchers(api.Session.publisher, (r, result) =>
			{
				RequestResponse<Voucher[]>	requestResponse = new RequestResponse<Voucher[]>()
				{
					context = database,
					ok = false,
					error = null,
					result = null
				};

				if (DataStructureExtension.CheckRequest(r, result, requestResponse) == true)
				{
					VouchersContainer	container = JsonUtility.FromJson<VouchersContainer>(result.Replace("],[", ",").Replace("[[", "[").Replace("]]", "]"));

					if (Conf.DebugMode == Conf.DebugState.Verbose)
						InternalNGDebug.Snapshot(container);

					Voucher[]	vouchers = new Voucher[container.aaData.Length / 6];

					for (int i = 0, j = 0, max = container.aaData.Length; i < max; i += 6, ++j)
					{
						vouchers[j] = new Voucher()
						{
							voucherCode = container.aaData[i + 0],
							package = container.aaData[i + 1],
							issuedBy = container.aaData[i + 2],
							issuedDate = container.aaData[i + 3],
							invoice = container.aaData[i + 4],
							redeemedDate = container.aaData[i + 5]
						};
					}

					requestResponse.ok = true;
					requestResponse.result = vouchers;
					database.Vouchers = requestResponse.result;
				}

				if (onCompleted != null)
					onCompleted(requestResponse);
			});
		}

		public static void	CreateVoucher(this PublisherDatabase database, IPublisherAPI api, Package package, Action<RequestResponse<Voucher>> onCompleted = null)
		{
			api.CreateVoucher(api.Session.publisher, package.id, (r, result) =>
			{
				RequestResponse<Voucher>	requestResponse = new RequestResponse<Voucher>()
				{
					context = package,
					ok = false,
					error = null,
					result = null
				};

				if (DataStructureExtension.CheckRequest(r, result, requestResponse) == true)
				{
					Response	response = DataStructureExtension.CheckResponse(result, requestResponse);

					if (Conf.DebugMode == Conf.DebugState.Verbose)
						InternalNGDebug.Snapshot(response);

						InternalNGDebug.Snapshot(response.errors);

					if (response != null && response.Succeeded == true)
					{
						Voucher	voucher = new Voucher()
						{
							voucherCode = response.voucher_code,
							package = package.name,
							issuedBy = api.Session.name,
							issuedDate = DateTime.Now.ToString("yyyy-MM-dd"),
							invoice = string.Empty,
							redeemedDate = string.Empty
						};

						requestResponse.ok = true;
						requestResponse.result = voucher;

						Voucher[]	array = database.Vouchers;
						Array.Resize(ref array, database.Vouchers.Length + 1);
						array[array.Length - 1] = voucher;

						database.Vouchers = array;
					}
				}

				if (onCompleted != null)
					onCompleted(requestResponse);
			});
		}

		public static void	RequestPeriods(this PublisherDatabase database, IPublisherAPI api, Action<RequestResponse<Period[]>> onCompleted = null)
		{
			api.GetPeriods(api.Session.publisher, (r, result) =>
			{
				RequestResponse<Period[]>	requestResponse = new RequestResponse<Period[]>()
				{
					context = database,
					ok = false,
					error = null,
					result = null
				};

				if (DataStructureExtension.CheckRequest(r, result, requestResponse) == true)
				{
					PeriodsContainer	container = JsonUtility.FromJson<PeriodsContainer>(result);

					if (Conf.DebugMode == Conf.DebugState.Verbose)
						InternalNGDebug.Snapshot(container);

					requestResponse.ok = true;
					requestResponse.result = container.periods;
					database.Periods = requestResponse.result;
				}

				if (onCompleted != null)
					onCompleted(requestResponse);
			});
		}

		public static void	RequestSaleCounts(this PublisherDatabase database, IPublisherAPI api, Period period, Action<RequestResponse<Sale[]>> onCompleted = null)
		{
			api.GetSaleCounts(api.Session.publisher, period.value, (r, result) =>
			{
				RequestResponse<Sale[]>	requestResponse = new RequestResponse<Sale[]>()
				{
					context = period.value,
					ok = false,
					error = null,
					result = null
				};

				if (DataStructureExtension.CheckRequest(r, result, requestResponse) == true)
				{
					SaleCountsContainer	container = JsonUtility.FromJson<SaleCountsContainer>(result.Replace("],[", ",").Replace("[[", "[").Replace("]]", "]"));

					if (Conf.DebugMode == Conf.DebugState.Verbose)
						InternalNGDebug.Snapshot(container);

					Sale[]	sales = new Sale[container.aaData.Length / 8];

					for (int i = 0, j = 0, max = container.aaData.Length; i < max; i += 8, ++j)
					{
						sales[j] = new Sale()
						{
							asset = container.aaData[i + 0],
							price = container.aaData[i + 1],
							quantity = int.Parse(container.aaData[i + 2]),
							refunds = int.Parse(container.aaData[i + 3]),
							chargebacks = int.Parse(container.aaData[i + 4]),
							gross = container.aaData[i + 5],
							first = container.aaData[i + 6],
							last = container.aaData[i + 7],
							net = container.result[j].net,
							short_url = container.result[j].short_url,
						};
					}

					requestResponse.ok = true;
					requestResponse.result = sales;
					period.Sales = sales;
				}

				if (onCompleted != null)
					onCompleted(requestResponse);
			});
		}

		public static void	RequestFreeDownloads(this PublisherDatabase database, IPublisherAPI api, Period period, Action<RequestResponse<Download[]>> onCompleted = null)
		{
			api.GetFreeDownloads(api.Session.publisher, period.value, (r, result) =>
			{
				RequestResponse<Download[]>	requestResponse = new RequestResponse<Download[]>()
				{
					context = period.value,
					ok = false,
					error = null,
					result = null
				};

				if (DataStructureExtension.CheckRequest(r, result, requestResponse) == true)
				{
					FreeDownloadsContainer	container = JsonUtility.FromJson<FreeDownloadsContainer>(result.Replace("],[", ",").Replace("[[", "[").Replace("]]", "]"));

					if (Conf.DebugMode == Conf.DebugState.Verbose)
						InternalNGDebug.Snapshot(container);

					Download[]	downloads = new Download[container.aaData.Length / 4];

					for (int i = 0, j = 0, max = container.aaData.Length; i < max; i += 4, ++j)
					{
						downloads[j] = new Download()
						{
							asset = container.aaData[i + 0],
							quantity = int.Parse(container.aaData[i + 1]),
							first = container.aaData[i + 2],
							last = container.aaData[i + 3],
							short_url = container.result[j].short_url,
						};
					}

					requestResponse.ok = true;
					requestResponse.result = downloads;
					period.Downloads = downloads;
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
						InternalNGDebug.Snapshot(packageRatings.rating);

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
						InternalNGDebug.Snapshot(categories.categories);

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
						if (response.Succeeded == true)
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
							model.package = package;

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

					if (response != null && response.Succeeded == true)
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

					if (response != null && response.Succeeded == true)
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

					if (response != null && response.Succeeded == true)
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

					if (response != null && response.Succeeded == true)
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

					if (response != null && response.Succeeded == true)
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

					if (response != null && response.Succeeded == true)
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

					if (response != null && response.Succeeded == true)
					{
						requestResponse.ok = true;
						requestResponse.result = response.url;
					}
				}

				if (onCompleted != null)
					onCompleted(requestResponse);
			});
		}

		public static void	RequestCanSubmit(this Version version, IPublisherAPI api, Action<RequestResponse<string>> onCompleted = null)
		{
			api.CanSubmit(version.id, version.version_name, version.publishnotes, version.category_id, version.price, (r, result) =>
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

					if (response != null && response.Succeeded == true)
					{
						requestResponse.ok = true;
						requestResponse.result = response.url;
					}
				}

				if (onCompleted != null)
					onCompleted(requestResponse);
			});
		}

		public static void	RequestSubmit(this Version version, IPublisherAPI api, bool autoPublish, string comments, Action<RequestResponse<string>> onCompleted = null)
		{
			api.Submit(version.id, autoPublish, comments, (r, result) =>
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

					if (response != null && response.Succeeded == true)
					{
						requestResponse.ok = true;
						requestResponse.result = response.url;
					}
				}

				if (onCompleted != null)
					onCompleted(requestResponse);
			});
		}
		
		public static void	RequestPublish(this Version version, IPublisherAPI api, string comments, Action<RequestResponse<string>> onCompleted = null)
		{
			api.Publish(version.id, comments, (r, result) =>
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

					if (response != null && response.Succeeded == true)
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
			if (result[0] == '{' && (result.StartsWith("{\"status\":") == true || result.StartsWith("{\"success\":") == true || result.Contains("\"errors\":{") == true)) // Let's consider it is JSON and try to get a response if available.
			{
				result = Response.MergeErrors(result);

				try
				{
					Response	response = JsonUtility.FromJson<Response>(result);

					if (response.Succeeded == true)
						return response;

					if (Conf.DebugMode == Conf.DebugState.Verbose)
						Debug.LogError(result);

					requestResponse.error = response.errors.error.Replace("<br>", "\n");
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