using System;
using System.Collections.Generic;
using UnityEngine;

namespace NGPublisher
{
	[Serializable]
	public class PublisherDatabase : ScriptableObject, ISerializationCallbackReceiver
	{
		[SerializeField]
		private bool		hasPackages;
		public bool			HasPackages { get { return this.hasPackages; } }
		[SerializeField]
		private Package[]	packages;
		public Package[]	Packages
		{
			get
			{
				return this.packages;
			}
			set
			{
				this.hasPackages = true;
				this.packages = value;
				this.versions.Clear();
			}
		}

		public List<VersionDetailed>	versions = new List<VersionDetailed>();

		[SerializeField]
		private bool		hasLanguages;
		public bool			HasLanguages { get { return this.hasLanguages; } }
		[SerializeField]
		private Language[]	languages;
		public Language[]	Languages
		{
			get
			{
				return this.languages;
			}
			set
			{
				this.hasLanguages = true;
				this.languages = value;
			}
		}

		[SerializeField]
		private bool	hasVets;
		public bool		HasVets { get { return this.hasVets; } }
		[SerializeField]
		private Vets	vets;
		public Vets		Vets
		{
			get
			{
				return this.vets;
			}
			set
			{
				this.hasVets = true;
				this.vets = value;
			}
		}

		public string	GetLanguage(string languageCode)
		{
			if (this.Languages != null)
			{
				for (int i = 0, max = this.Languages.Length; i < max; ++i)
				{
					Language	language = this.Languages[i];

					if (language.code == languageCode)
						return language.name;
				}
			}

			return languageCode;
		}

		void	ISerializationCallbackReceiver.OnBeforeSerialize()
		{
		}

		void	ISerializationCallbackReceiver.OnAfterDeserialize()
		{
			if (this.hasPackages == false)
				this.packages = null;
			
			if (this.hasLanguages == false)
				this.languages = null;

			if (this.hasVets == false)
				this.vets = null;

			// Link detailed versions to their version in packages.
			if (this.Packages != null && this.versions != null)
			{
				for (int i = 0, max = this.versions.Count; i < max; ++i)
				{
					VersionDetailed	versionDetail = this.versions[i];

					for (int j = 0, max2 = this.Packages.Length; j < max2; ++j)
					{
						Package	package = this.Packages[j];

						for (int k = 0, max3 = package.versions.Length; k < max3; ++k)
						{
							Version	version = package.versions[k];

							if (version.id == versionDetail.package.version.id)
								version.detailed = versionDetail;
						}
					}
				}
			}
		}
	}
}