using System;

namespace NGPublisher
{
	public class Status
	{
		public const string	Published = "published";
		public const string	Draft = "draft";
		public const string	Deprecated = "deprecated";
		public const string	Accepted = "accepted";
		public const string	Pending = "pending";

		public static Statuses	Get(string status)
		{
			if (status[0] == 'p')
			{
				if (status[1] == 'u')
					return Statuses.Published;
				return Statuses.Pending;
			}

			if (status[0] == 'd')
			{
				if (status[1] == 'r')
					return Statuses.Draft;
				return Statuses.Deprecated;
			}

			if (status[0] == 'a')
				return Statuses.Accepted;

			throw new Exception("Status \"" + status + "\" not implemented.");
		}
	}
}