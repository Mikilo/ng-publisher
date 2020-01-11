using System.Collections.Generic;
using System.Text;

namespace NGPublisher
{
	public interface IPackageExporter
	{
		string	Name { get; }

		void	ExportPeriod(StringBuilder buffer, IEnumerable<Period> periods, string filterAsset);
	}
}