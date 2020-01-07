using System;
using UnityEngine;

namespace NGPublisher
{
	public class Status
	{
		public const string	Published = "published";
		public const string	Draft = "draft";
		public const string	Deprecated = "deprecated";
		public const string	Accepted = "accepted";
		public const string	Pending = "pending";

		public static Color	DraftColor { get { return Color.blue; } }
		public static Color	PublishedColor { get { return Color.green; } }
		public static Color	DeprecatedColor { get { return Color.black; } }
		public static Color	PendingColor { get { return Color.gray; } }
		public static Color	AcceptedColor { get { return Color.yellow; } }

		public static Color	GetColor(string status)
		{
			if (status[0] == 'p' || status[0] == 'P')
			{
				if (status[1] == 'u')
					return Status.PublishedColor;
				return Status.PendingColor;
			}

			if (status[0] == 'd' || status[0] == 'D')
			{
				if (status[1] == 'r')
					return Status.DraftColor;
				return Status.DeprecatedColor;
			}

			if (status[0] == 'a' || status[0] == 'A')
				return Status.AcceptedColor;

			throw new Exception("Status \"" + status + "\" not implemented.");
		}

		public static int	GetEnumMaxIndex()
		{
			return 5;
		}

		public static int	GetEnumIndex(string status)
		{
			if (status[0] == 'p')
			{
				if (status[1] == 'u')
					return 1;
				return 4;
			}

			if (status[0] == 'd')
			{
				if (status[1] == 'r')
					return 0;
				return 2;
			}

			if (status[0] == 'a')
				return 3;

			throw new Exception("Status \"" + status + "\" not implemented.");
		}

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