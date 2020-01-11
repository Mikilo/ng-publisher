using System;

namespace NGPublisher
{
	[Serializable]
	public class Sale
	{
		public string	asset;
		public string	price;
		public int		quantity;
		public int		refunds;
		public int		chargebacks;
		public string	gross;
		public string	first;
		public string	last;
		public string	net;
		public string	short_url;
	}
}