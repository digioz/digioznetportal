using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CsvHelper.Configuration;

namespace digioz.Portal.Web.Models.ViewModels
{
    //public class FooterLinkMap : CsvClassMap<ChatExportViewModel>
    //{
    //    public FooterLinkMap()
    //    {
    //        Map(m => m.ID);
    //        Map(m => m.Username);
    //        Map(m => m.Message);
    //        Map(m => m.Timestamp);
    //    }
    //}

    public class ChatExportViewModel
    {
        public int ID { get; set; }
        public string Username { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }

        public virtual void CreateMap()
        {
            
        }
    }
}