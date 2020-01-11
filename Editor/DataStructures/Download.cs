using System;

namespace NGPublisher
{
	[Serializable]
	public class Download
	{
		public string	asset;
		public int		quantity;
		public string	first;
		public string	last;
		public string short_url;
	}
}