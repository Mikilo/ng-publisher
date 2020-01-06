using System;
using UnityEngine;

namespace NGPublisher
{
	[Serializable]
	public class Version : ISerializationCallbackReceiver
	{
		public int		id;
		//public int		package_version_id; // Unused
		//public int		package_id; // Unused
		public string	status;
		public string	name;
		public string	version_name;
		public int		category_id;
		public float	price;
		public string	publishnotes;
		public int		size;
		public string	modified;
		public string	created;
		public string	published;

		public Statuses	Status { get { return NGPublisher.Status.Get(this.status); } }

		[NonSerialized]
		private string	categoryName;
		public string	Category { get { return this.GetCategory(); } }

		[NonSerialized]
		public Package	package; // Set by Package itself.

		[NonSerialized]
		public VersionDetailed	detailed; // Set by PublisherDatabase when available.

		[SerializeField]
		private bool		hasCategories;
		[SerializeField]
		private Category[]	categories; // Very strange that Unity keeps a list of categories per version instead of a global list...
		public Category[]	Categories
		{
			get
			{
				return this.categories;
			}
			set
			{
				this.hasCategories = true;
				this.categories = value;
			}
		}

		public string	GetCategory(int categoryId)
		{
			if (this.categories != null)
			{
				for (int i = 0, max = this.categories.Length; i < max; ++i)
				{
					Category	category = this.categories[i];

					if (category.id == categoryId)
						return category.name;
				}
			}

			return null;
		}

		private string	GetCategory()
		{
			if (this.categoryName != null)
				return this.categoryName;

			if (this.categories != null)
			{
				this.categoryName = this.GetCategory(this.category_id);
				return this.categoryName;
			}

			return string.Empty;
		}

		public override string	ToString()
		{
			return "[" + this.status + "] " + this.name + " (" + this.id + ")";
		}

		void	ISerializationCallbackReceiver.OnBeforeSerialize()
		{
		}

		void	ISerializationCallbackReceiver.OnAfterDeserialize()
		{
			if (this.hasCategories == false)
				this.categories = null;
		}
	}
}