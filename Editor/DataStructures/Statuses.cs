namespace NGPublisher
{
	public enum Statuses
	{
		Draft = 1 << 0,
		Published = 1 << 1,
		Deprecated = 1 << 2,
		Accepted = 1 << 3,
		Declined = 1 << 4,
		Pending = 1 << 5,
		All = Draft | Published | Deprecated | Accepted | Declined | Pending,
	}
}