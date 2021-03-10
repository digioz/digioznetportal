using digioz.Portal.Bo.ViewModels;
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

		public Dictionary<string, int> GetLogCounts()
		{
			var models = new Dictionary<string, int>();

			using (var context = new digiozPortalContext())
			using (var command = context.Database.GetDbConnection().CreateCommand())
			{
				var query = @"SELECT [Level], COUNT(*) As [Count] FROM [Log] GROUP BY [Level];";

				command.CommandText = query;
				context.Database.OpenConnection();

				using (var result = command.ExecuteReader())
				{
					int count = result.FieldCount;
					int total = 0;

					var dt = new DataTable();
					dt.Load(result);

					foreach (DataRow dr in dt.Rows)
					{
						models.Add(dr["Level"].ToString(), Convert.ToInt32(dr["Count"]));
						total += Convert.ToInt32(dr["Count"]);
					}

					models.Add("All", total);
				}

				if (!models.ContainsKey("Error"))
				{
					models.Add("Error", 0);
				}

				if (!models.ContainsKey("Warning"))
				{
					models.Add("Warning", 0);
				}

				if (!models.ContainsKey("Information"))
				{
					models.Add("Information", 0);
				}

				context.Database.CloseConnection();
			}

			return models;
		}

		public List<PollDisplayChartViewModel> GetPollResults(string id)
		{
			var models = new List<PollDisplayChartViewModel>();

			using (var context = new digiozPortalContext())
			using (var command = context.Database.GetDbConnection().CreateCommand())
			{
				var query = @"select pa.Answer, count(pv.Id) CountOf from PollVote pv, PollAnswer pa 
                             where pa.Id = pv.PollAnswerId and pa.PollId = '" + id + "' group by pa.Answer;";

				command.CommandText = query;
				context.Database.OpenConnection();

				using (var result = command.ExecuteReader())
				{
					int count = result.FieldCount;

					var dt = new DataTable();
					dt.Load(result);

					foreach (DataRow dr in dt.Rows)
					{
						var pollDisplayChart = new PollDisplayChartViewModel()
						{
							Answer = dr["Answer"].ToString(),
							CountOf = Convert.ToInt32(dr["CountOf"])
						};

						models.Add(pollDisplayChart);
					}
				}

				context.Database.CloseConnection();
			}

			return models;
		}
	}
}
