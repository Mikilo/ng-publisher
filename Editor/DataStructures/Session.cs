using System;

namespace NGPublisher
{
	[Serializable]
	public class Session
	{
		//[Serializable]
		//public class Role
		//{
		//	public int	v2user;
		//}

		//[Serializable]
		//public class Country
		//{
		//	public string	name;
		//	public string	code;
		//}

		[Serializable]
		public class Keyimage
		{
			public string	icon;
			//public string	icon24; // Unused
		}

		//public string	unity_version; // Unused
		//public string	language_code; // Unused
		//public string	vat_percent; // Unused
		//public string	uuid; // Unused
		//public string	nps; // Unused
		//public string	current; // Unused
		//public bool		v2editor_allowed; // Unused
		public string	xunitysession;
		//public string	kharma_version; // Unused
		//public string	language_url_code; // Unused
		//public string	id; // Unused
		public int		publisher;
		//public Role		role; // Unused
		//public Country	country; // Unused
		//public bool		is_anonymous; // Unused
		//public bool		show_intro; // Unused
		public string	name;
		public Keyimage	keyimage;
		public string	username;
		//public string	rounding; // Unused
		//public bool		v2editor_preferred; // Unused
		//public bool		v2_preferred; // Unused
	}
}