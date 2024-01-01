using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace digioz.Portal.Web.Models
{
    public class SettingKeys
    {
        public string ConnectionString { get; set; }
        public string EncryptionKey { get; set; }
        public string SMTPServer { get; set; }
        public string SMTPUsername { get; set; }
        public string SMTPPassword { get; set; }
        public string SiteURL { get; set; }
        public string DownloadPath { get; set; }
        public string FileSystemPath { get; set; }
    }
}