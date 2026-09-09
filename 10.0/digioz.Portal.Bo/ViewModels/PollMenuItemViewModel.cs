using System.Collections.Generic;

namespace digioz.Portal.Bo.ViewModels
{
    public class PollMenuItemViewModel
    {
        public Poll Poll { get; set; } = new();
        public List<PollAnswer> Answers { get; set; } = new();
        public bool HasVoted { get; set; }
        public string ResultsChartBase64 { get; set; } = string.Empty;
    }
}
