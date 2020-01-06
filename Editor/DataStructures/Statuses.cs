namespace NGPublisher
{
	public enum Statuses
	{
		Draft = 1 << 0,
		Published = 1 << 1,
		Deprecated = 1 << 2,
		Accepted = 1 << 3,
		Pending = 1 << 4,
		All = Draft | Published | Deprecated | Accepted | Pending,
	}
}