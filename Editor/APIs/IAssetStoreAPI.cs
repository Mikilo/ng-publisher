using System;
using System.Net;

namespace NGPublisher
{
	public interface IAssetStoreAPI
	{
		//bool	IsConnected { get; }

		//bool	IsWritingReview(int packageId);
		//void	WriteReview(int packageId, string subject, string review, int rating, Action<HttpWebResponse, string> onCompleted);

		bool	IsGettingRatings(int packageId);
		void	GetRatings(int packageId, Action<HttpWebResponse, string> onCompleted);
	}
}