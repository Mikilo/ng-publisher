using System;

namespace NGPublisher
{
	[Serializable]
	public class Voucher
	{
		public string	voucherCode;
		public string	package;
		public string	issuedBy;
		public string	issuedDate;
		public string	invoice;
		public string	redeemedDate;
	}
}