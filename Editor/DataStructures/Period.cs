using System;
using UnityEngine;

namespace NGPublisher
{
	[Serializable]
	public class Period : ISerializationCallbackReceiver
	{
		public int		value;
		public string	name;

		[SerializeField]
		private bool	hasSales;
		public bool		HasSales { get { return this.hasSales; } }
		[SerializeField]
		private Sale[]	sales;
		public Sale[]	Sales
		{
			get
			{
				return this.sales;
			}
			set
			{
				this.hasSales = true;
				this.sales = value;
			}
		}

		[SerializeField]
		private bool	hasDownloads;
		public bool		HasDownloads { get { return this.hasDownloads; } }
		[SerializeField]
		private Download[]	downloads;
		public Download[]	Downloads
		{
			get
			{
				return this.downloads;
			}
			set
			{
				this.hasDownloads = true;
				this.downloads = value;
			}
		}

		void	ISerializationCallbackReceiver.OnAfterDeserialize()
		{
		}

		void	ISerializationCallbackReceiver.OnBeforeSerialize()
		{
			if (this.hasDownloads == false)
				this.downloads = null;

			if (this.hasSales == false)
				this.sales = null;
		}
	}
}