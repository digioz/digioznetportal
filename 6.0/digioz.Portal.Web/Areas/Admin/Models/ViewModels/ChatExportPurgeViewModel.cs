using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace digioz.Portal.Web.Areas.Admin.Models.ViewModels
{
	public class ChatExportPurgeViewModel
	{
        [DisplayName("Start Date")]
        public string StartDate { get; set; }
        [DisplayName("End Date")]
        public string EndDate { get; set; }
        [DisplayName("Process All?")]
        public bool ProcessAll { get; set; }
    }
}
