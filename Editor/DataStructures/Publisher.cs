using System;

namespace NGPublisher
{
	[Serializable]
	public class Publisher
	{
		[Serializable]
		public class Overview
		{
			[Serializable]
			public class Keyimage
			{
				//public string	small; // Unused
				//public string	big; // Unused
				public string small_v2;
				//public string	big_v2; // Unused
			}

			//public string	organization_id; // Unused
			//public STRUCT	latest; // Unused
			//public bool		service; // Unused
			public string name;
			public Keyimage keyimage;
			//public string	support_email; // Unused
			//public string	description; // Unused
			//public STRUCT	rating; // Unused
			//public string	auth; // Unused
			//public float	payout_cut; // Unused
			//public string	updated_at; // Unused
			//public string	url; // Unused
			public int id;
			//public string	short_url; // Unused
			//public string	support_url; // Unused
		}

		public Overview overview;
	}
}