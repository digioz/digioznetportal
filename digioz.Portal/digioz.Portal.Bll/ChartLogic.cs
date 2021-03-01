using digioz.Portal.Dal;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
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
				var query = @"SELECT	
							(SELECT COUNT(*) FROM VisitorInfo WHERE MONTH(Timestamp) = 1 AND YEAR(Timestamp) = YEAR(dateadd(dd, -1, GetDate()))) AS 'Jan',
							(SELECT COUNT(*) FROM VisitorInfo WHERE MONTH(Timestamp) = 2 AND YEAR(Timestamp) = YEAR(dateadd(dd, -1, GetDate()))) AS 'Feb',
							(SELECT COUNT(*) FROM VisitorInfo WHERE MONTH(Timestamp) = 3 AND YEAR(Timestamp) = YEAR(dateadd(dd, -1, GetDate()))) AS 'Mar',
							(SELECT COUNT(*) FROM VisitorInfo WHERE MONTH(Timestamp) = 4 AND YEAR(Timestamp) = YEAR(dateadd(dd, -1, GetDate()))) AS 'Apr',
							(SELECT COUNT(*) FROM VisitorInfo WHERE MONTH(Timestamp) = 5 AND YEAR(Timestamp) = YEAR(dateadd(dd, -1, GetDate()))) AS 'May',
							(SELECT COUNT(*) FROM VisitorInfo WHERE MONTH(Timestamp) = 6 AND YEAR(Timestamp) = YEAR(dateadd(dd, -1, GetDate()))) AS 'Jun',
							(SELECT COUNT(*) FROM VisitorInfo WHERE MONTH(Timestamp) = 7 AND YEAR(Timestamp) = YEAR(dateadd(dd, -1, GetDate()))) AS 'Jul',
							(SELECT COUNT(*) FROM VisitorInfo WHERE MONTH(Timestamp) = 8 AND YEAR(Timestamp) = YEAR(dateadd(dd, -1, GetDate()))) AS 'Aug',
							(SELECT COUNT(*) FROM VisitorInfo WHERE MONTH(Timestamp) = 9 AND YEAR(Timestamp) = YEAR(dateadd(dd, -1, GetDate()))) AS 'Sep',
							(SELECT COUNT(*) FROM VisitorInfo WHERE MONTH(Timestamp) = 10 AND YEAR(Timestamp) = YEAR(dateadd(dd, -1, GetDate()))) AS 'Oct',
							(SELECT COUNT(*) FROM VisitorInfo WHERE MONTH(Timestamp) = 11 AND YEAR(Timestamp) = YEAR(dateadd(dd, -1, GetDate()))) AS 'Nov',
							(SELECT COUNT(*) FROM VisitorInfo WHERE MONTH(Timestamp) = 12 AND YEAR(Timestamp) = YEAR(dateadd(dd, -1, GetDate()))) AS 'Dec';";

				command.CommandText = query;
				context.Database.OpenConnection();

				using (var result = command.ExecuteReader())
				{
					while (result.Read())
					{
						models.Add("Jan", Convert.ToInt32(result["Jan"]));
						models.Add("Feb", Convert.ToInt32(result["Feb"]));
						models.Add("Mar", Convert.ToInt32(result["Mar"]));
						models.Add("Apr", Convert.ToInt32(result["Apr"]));
						models.Add("May", Convert.ToInt32(result["May"]));
						models.Add("Jun", Convert.ToInt32(result["Jun"]));
						models.Add("Jul", Convert.ToInt32(result["Jul"]));
						models.Add("Aug", Convert.ToInt32(result["Aug"]));
						models.Add("Sep", Convert.ToInt32(result["Sep"]));
						models.Add("Oct", Convert.ToInt32(result["Oct"]));
						models.Add("Nov", Convert.ToInt32(result["Nov"]));
						models.Add("Dec", Convert.ToInt32(result["Dec"]));
					}
				}
			}

			return models;
		}
	}
}
