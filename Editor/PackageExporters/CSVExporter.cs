using System;
using System.Collections.Generic;
using System.Text;

namespace NGPublisher
{
	public class CSVExporter : IPackageExporter
	{
		public string	Name { get { return "CSV"; } }

		public void	ExportPeriod(StringBuilder buffer, IEnumerable<Period> periods, string filterAsset)
		{
			foreach (Period period in periods)
			{
				buffer.Append(period.value);
				buffer.Append(',');
				buffer.AppendLine(period.name);

				for (int i = 0, max = period.Sales.Length; i < max; ++i)
				{
					Sale	sale = period.Sales[i];

					if (string.IsNullOrEmpty(filterAsset) == false && sale.asset != filterAsset)
						continue;

					buffer.Append(sale.asset);
					buffer.Append(',');
					buffer.Append(sale.price);
					buffer.Append(',');
					buffer.Append(sale.quantity);
					buffer.Append(',');
					buffer.Append(sale.refunds);
					buffer.Append(',');
					buffer.Append(sale.chargebacks);
					buffer.Append(',');
					buffer.Append(sale.gross);
					buffer.Append(',');
					buffer.Append(sale.first);
					buffer.Append(',');
					buffer.AppendLine(sale.last);
				}

				buffer.AppendLine();

				for (int i = 0, max = period.Downloads.Length; i < max; ++i)
				{
					Download	download = period.Downloads[i];

					if (string.IsNullOrEmpty(filterAsset) == false && download.asset != filterAsset)
						continue;

					buffer.Append(download.asset);
					buffer.Append(',');
					buffer.Append(download.quantity);
					buffer.Append(',');
					buffer.Append(download.first);
					buffer.Append(',');
					buffer.AppendLine(download.last);
				}

				buffer.AppendLine();
			}

			buffer.Length -= Environment.NewLine.Length << 1;
		}
	}
}