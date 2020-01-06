using System;
using UnityEngine;

namespace NGPublisher
{
	[Serializable]
	public class Package : ISerializationCallbackReceiver
	{
		public int			id;
		public string		name;
		public int			category_id;
		public int			average_rating;
		public int			count_ratings;
		public string		short_url;
		public Version[]	versions;
		//public string		management_flags; // Unused

		[SerializeField]
		private bool		hasRatings;
		[SerializeField]
		private Rating[]	ratings;
		public Rating[]		Ratings
		{
			get
			{
				return this.ratings;
			}
			set
			{
				this.hasRatings = true;
				this.ratings = value;
			}
		}

		public string	Category
		{
			get
			{
				for (int i = 0, max = this.versions.Length; i < max; ++i)
				{
					Version	version = this.versions[i];
					string	label = version.GetCategory(this.versions[i].category_id);

					if (label != null)
						return label;
				}

				return string.Empty;
			}
		}

		void	ISerializationCallbackReceiver.OnBeforeSerialize()
		{
		}

		void	ISerializationCallbackReceiver.OnAfterDeserialize()
		{
			if (this.hasRatings == false)
				this.ratings = null;

			if (this.versions != null)
			{
				for (int i = 0, max = this.versions.Length; i < max; ++i)
					this.versions[i].package = this;
			}
		}
	}
}