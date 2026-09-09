using System.Collections.Generic;
using digioz.Portal.Bo;

namespace digioz.Portal.Bo.ViewModels
{
    public class RssFeedPreviewViewModel
    {
        public Rss Rss { get; set; } = new Rss();
        public List<RssItemViewModel> Items { get; set; } = new List<RssItemViewModel>();
    }
}
