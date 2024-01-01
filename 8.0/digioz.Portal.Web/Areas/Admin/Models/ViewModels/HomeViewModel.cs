using digioz.Portal.Bo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace digioz.Portal.Web.Areas.Admin.Models.ViewModels
{
	public class HomeViewModel
	{
		public List<VisitorInfo> Visitors { get; set; }

		public Dictionary<string, int> VisitorYearlyHits { get; set; }

		public Dictionary<string, int> VisitorMonthlyHits { get; set; }

		public Dictionary<string, int> GetLogCounts { get; set; }
	}
}
