using digioz.Portal.Dal;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace digioz.Portal.Bll
{
	public class ChartLogic
	{
		public Dictionary<string, int> GetVisitorYearlyHits()
		{
			var models = new Dictionary<string, int>();

			using (var context = new digiozPortalContext())
			using (var command = context.Database.GetDbConnection().CreateCommand())
			{
				var query = @"SELECT LEFT(DateName(MONTH , DateAdd(MONTH,MONTH(Timestamp),-1)), 3) AS [Year], COUNT(*) AS [Count] FROM VisitorInfo
							WHERE YEAR(Timestamp) = YEAR(dateadd(dd, -1, GetDate()))
							GROUP BY MONTH(Timestamp)
							ORDER BY MONTH(Timestamp)";

				command.CommandText = query;
				context.Database.OpenConnection();

				using (var result = command.ExecuteReader())
				{
					int count = result.FieldCount;

					var dt = new DataTable();
					dt.Load(result);

					foreach (DataRow dr in dt.Rows)
					{
						models.Add(dr["Year"].ToString(), Convert.ToInt32(dr["Count"]));
					}
				}

				context.Database.CloseConnection();
			}

			return models;
		}

		public Dictionary<string, int> GetVisitorMonthlyHits()
		{
			var models = new Dictionary<string, int>();

			using (var context = new digiozPortalContext())
			using (var command = context.Database.GetDbConnection().CreateCommand())
			{
				var query = @"SELECT DAY(Timestamp) AS [Day], COUNT(*) AS [Count] FROM VisitorInfo
							WHERE YEAR(Timestamp) = YEAR(dateadd(dd, -1, GetDate()))
							AND MONTH(Timestamp) = MONTH(GetDate())
							GROUP BY DAY(Timestamp)
							ORDER BY DAY(Timestamp)";

				command.CommandText = query;
				context.Database.OpenConnection();

				using (var result = command.ExecuteReader())
				{
					int count = result.FieldCount;

					var dt = new DataTable();
					dt.Load(result);

					foreach (DataRow dr in dt.Rows)
					{
						models.Add(dr["Day"].ToString(), Convert.ToInt32(dr["Count"]));
					}
				}

				context.Database.CloseConnection();
			}

			return models;
		}
	}
}
