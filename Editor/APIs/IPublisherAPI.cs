using System;
using System.Collections.Generic;
using System.Net;

namespace NGPublisher
{
	public interface IPublisherAPI
	{
		bool	IsConnected { get; }
		Session	Session { get; }

		bool	IsConnecting();
		void	Connect(string username, string password, Action<HttpWebResponse, string> onCompleted);
		void	Disconnect();

		bool	IsGettingAllPackages();
		void	GetAllPackages(Action<HttpWebResponse, string> onCompleted);

		bool	IsGettingLanguages();
		void	GetLanguages(Action<HttpWebResponse, string> onCompleted);

		bool	IsGettingVettingConfig();
		void	GetVettingConfig(Action<HttpWebResponse, string> onCompleted);

		bool	IsGettingCategories(int versionId);
		void	GetCategories(int versionId, Action<HttpWebResponse, string> onCompleted = null);

		bool	IsGettingPackageVersion(int versionId);
		void	GetPackageVersion(int versionId, Action<HttpWebResponse, string> onCompleted);

		bool	IsCreatingDraft(int versionId);
		void	CreateDraft(int versionId, Action<HttpWebResponse, string> onCompleted);

		bool	IsDeletingingDraft(int versionId);
		void	DeleteDraft(int versionId, Action<HttpWebResponse, string> onCompleted);

		bool	IsDeletingingUnityPackage(int versionId, VersionDetailed.Package.Version.UnityPackage unityPackage);
		void	DeleteUnityPackage(int versionId, VersionDetailed.Package.Version.UnityPackage unityPackage, Action<HttpWebResponse, string> onCompleted);

		bool	IsVettingVersion(int versionId, int uploadID);
		void	VetVersion(int versionId, int uploadID, IEnumerable<string> platforms, IEnumerable<string> unityVersions, IEnumerable<string> srpTypes, IEnumerable<string> dependencies, Action<HttpWebResponse, string> onCompleted);

		bool	IsAddingLanguage(int versionId, string languageCode);
		void	AddLanguage(int versionId, string languageCode, string versionName, Action<HttpWebResponse, string> onCompleted);

		bool	IsSettingKeyImage(int versionId, string language, string type);
		void	SetKeyImage(int versionId, string language, string imagePath, string type, Action<HttpWebResponse, string> onCompleted);

		bool	IsSettingPackageMetadata(int versionId);
		void	SetPackageMetadata(int versionId, string language, string name, string description, string keywords, Action<HttpWebResponse, string> onCompleted);

		bool	IsSettingPackage(int versionId);
		void	SetPackage(int versionId, string versionName, string publishnotes, int categoryID, float price, Action<HttpWebResponse, string> onCompleted);
	}
}