using NGToolsStandalone_For_NGPublisherEditor;
using System;
using System.Text;

namespace NGPublisher
{
	public class RequestResponse<T>
	{
		public object	context;
		public bool		ok;
		public string	error;
		public T		result;
	}

	[Serializable]
	public class Response
	{
		[Serializable]
		public class Errors
		{
			public string	error;
		}

		public bool	Succeeded { get { return this.success == true || this.status == "ok"; } }

		public bool		success;
		public string	status;
		public Errors	errors;
		public string	url;
		public int		id; // Create draft
		public string	voucher_code; // Create voucher

		public static string	MergeErrors(string input)
		{
			int	i = input.IndexOf("\"errors\":{");

			if (i != -1)
			{
				if (i != -1)
				{
					StringBuilder	buffer = Utility.GetBuffer(input);
					i += "\"errors\":{".Length - 1;
					int				endBracket = DataStructureExtension.DigestBracketScope(buffer, i);
					bool			first = true;

					while (i + 1 < endBracket && buffer[i] != ']')
					{
						++i;

						int	startBracket = i;

						i = DataStructureExtension.DigestString(buffer, i);

						if (i != startBracket)
						{
							string	key = buffer.ToString().Substring(startBracket + 1, i - (startBracket + 1));

							if (first == true)
							{
								first = false;
								input = input.Replace("\"" + key + "\":\"", "\"error\":\"[" + key + "] ");
							}
							else
								input = input.Replace("\",\"" + key + "\":\"", "<br><br>[" + key + "] ");
							++i;
						}
					}

					Utility.RestoreBuffer(buffer);
				}
			}

			return input;
		}
	}
}