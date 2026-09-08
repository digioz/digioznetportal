using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace digioz.Portal.Bo.ViewModels
{
    public class ChatExportViewModel
    {
        public int ID { get; set; }
        public string Username { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
    }
}