using System;
using System.Collections.Generic;

namespace NGPublisher
{
	[Serializable]
	public class VersionDetailed
	{
		[Serializable]
		public class Package
		{
			[Serializable]
			public class Version
			{
				[Serializable]
				public class Language
				{
					[Serializable]
					public class KeyImages
					{
						public string	icon;
						public string	small;
						public string	small_v2;
						public string	big;
						public string	big_v2;
						public string	facebook;
					}

					[Serializable]
					public class Artwork
					{
						public int		id;
						public string	reference;
						public string	scaled;
						public string	type; // youtube|screenshot
						public string	uri;
					}

					public string		languageCode; // Converted by the API.
					public KeyImages	key_images;
					public string		keywords;
					public Artwork[]	artwork;
					public string		name;
					public string		description;
					public string		release_notes;
				}

				public Language	this[string languageCode]
				{
					get
					{
						for (int i = 0, max = this.languages.Length; i < max; ++i)
						{
							Language	language = this.languages[i];

							if (language.languageCode == languageCode)
								return language;
						}

						return null;
					}
				}

				public IEnumerable<Language>	EachLanguage
				{
					get
					{
						for (int i = 0, max = this.languages.Length; i < max; ++i)
							yield return this.languages[i];
					}
				}

				[Serializable]
				public class UnityPackage
				{
					[Serializable]
					public class Vetting
					{
						// Default value required, because Unity can discard it.
						public int		id = 0;
						public string	status = string.Empty;
						public bool		allow_edit = true;
						//public string	genesis_vetting_id; // Unused
						public string	platforms = string.Empty;
						public string	unity_versions = string.Empty;
						public string	srp_type = string.Empty;
						public string[]	dependencies = new string[0];

						public override int	GetHashCode()
						{
							return this.platforms.GetHashCode()
								//+ this.allow_edit.GetHashCode()
								//+ this.status.GetHashCode()
								+ this.dependencies.GetHashCode()
								//+ this.genesis_vetting_id.GetHashCode()
								+ this.unity_versions.GetHashCode()
								//+ this.id.GetHashCode()
								+ this.srp_type.GetHashCode();
						}

						public override bool	Equals(object obj)
						{
							Vetting		b = obj as Vetting;

							if (b == null)
								return false;

							string[]	aParts = this.platforms.Split(',');
							string[]	bParts = b.platforms.Split(',');

							if (aParts.Length != bParts.Length)
								return false;

							int	hash = 0;

							for (int i = 0, max = aParts.Length; i < max; ++i)
								hash += aParts[i].GetHashCode() - bParts[i].GetHashCode();

							if (hash != 0)
								return false;

							aParts = this.unity_versions.Split(',');
							bParts = b.unity_versions.Split(',');

							if (aParts.Length != bParts.Length)
								return false;

							for (int i = 0, max = aParts.Length; i < max; ++i)
								hash += aParts[i].GetHashCode() - bParts[i].GetHashCode();

							if (hash != 0)
								return false;

							aParts = this.srp_type.Split(',');
							bParts = b.srp_type.Split(',');

							if (aParts.Length != bParts.Length)
								return false;

							for (int i = 0, max = aParts.Length; i < max; ++i)
								hash += aParts[i].GetHashCode() - bParts[i].GetHashCode();

							if (hash != 0)
								return false;

							aParts = this.dependencies;
							bParts = b.dependencies;

							if (aParts.Length != bParts.Length)
								return false;

							for (int i = 0, max = aParts.Length; i < max; ++i)
								hash += aParts[i].GetHashCode() - bParts[i].GetHashCode();

							if (hash != 0)
								return false;

							return true;
						}
					}

					public int		package_upload_id;
					public string	unity_version;
					public string	timestamp;
					public int		size;
					public Vetting	vetting;
					public string	asset_store_tools_version;
					public string	path;
				}

				public int				id;
				//public int			package_version_id; // I think it's an alias of "id". // Unused
				//public int				package_id; // Unused
				//public string			status; // Unused
				public string			name;
				//public string			version_name; // Unused
				//public int				category_id; // Unused
				//public float			price; // Unused
				//public string			publishnotes; // Unused
				//public int				size; // Unused
				public Language[]		languages; // Converted to array instead of object.
				public UnityPackage[]	unitypackages;
				//public string			modified; // Unused
				//public string			published; // Unused
				//public string			created; // Unused
				//public string[]			packmanpackages; // Unused
			}

			//public int	id; // Unused
			//public string	name; // Unused
			//public int	category_id; // Unused
			//public int	average_rating; // Unused
			//public int	count_ratings; // Unused
			//public string	short_url; // Unused
			public Version	version;
			//public string	management_flags; // Unused
		}

		public Package	package;
		//public string	allow_submit; // Unused
		//public string	allow_publish; // Unused
		//public int	terms_current; // Unused
		//public int	terms_accepted; // Unused
		//public string	draft; // Unused
		//public string	publisher_name; // Unused
		//public int	publisher_id; // Unused
	}
}