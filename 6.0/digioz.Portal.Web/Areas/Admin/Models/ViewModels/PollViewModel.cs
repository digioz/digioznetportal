using digioz.Portal.Bo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace digioz.Portal.Web.Areas.Admin.Models.ViewModels
{
    public class PollViewModel
    {
        public Poll Poll { get; set; }
        public List<PollAnswer> PollAnswers { get; set; }
    }
}
