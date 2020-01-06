using System;

namespace NGPublisher
{
	[Serializable]
	public class Vets
	{
		[Serializable]
		public class Option
		{
			public string	label;
			public string	code;
		}

		public Option[]	platforms;
		public Option[]	srps;
		public Option[]	unity_versions;
	}
}