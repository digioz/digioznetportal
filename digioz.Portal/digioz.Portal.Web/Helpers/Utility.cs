using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using digioz.Portal.Web.Models.ViewModels;
using System.ServiceModel.Syndication;
using System.Xml;
using Microsoft.AspNetCore.Mvc.Rendering;
using digioz.Portal.Bo;
using digioz.Portal.Bll;
using Microsoft.AspNetCore.Http;
using digioz.Portal.Web.Models;
using digioz.Portal.Payment;

namespace digioz.Portal.Web.Helpers
{
    public static class Utility
    {
        //public static bool CreateUserProfileCustomRecord(string userID, string email)
        //{
        //    Profile profile = new Profile
        //    {
        //        UserID = userID,
        //        Email = email
        //    };

        //    // Create Profile
        //    var logic = new ProfileLogic();
        //    logic.Add(profile);

        //    return true;
        //}

        public static bool IsImage(IFormFile postedFile)
        {
            //-------------------------------------------
            //  Check the image mime types
            //-------------------------------------------
            if (postedFile.ContentType.ToLower() != "image/jpg" &&
                        postedFile.ContentType.ToLower() != "image/jpeg" &&
                        postedFile.ContentType.ToLower() != "image/pjpeg" &&
                        postedFile.ContentType.ToLower() != "image/gif" &&
                        postedFile.ContentType.ToLower() != "image/x-png" &&
                        postedFile.ContentType.ToLower() != "image/png")
            {
                return false;
            }

            //-------------------------------------------
            //  Check the image extension
            //-------------------------------------------
            if (Path.GetExtension(postedFile.FileName).ToLower() != ".jpg"
                && Path.GetExtension(postedFile.FileName).ToLower() != ".png"
                && Path.GetExtension(postedFile.FileName).ToLower() != ".gif"
                && Path.GetExtension(postedFile.FileName).ToLower() != ".jpeg")
            {
                return false;
            }

            return true;
        }

        //public static bool SubmitMail(EmailModel email)
        //{

        //    bool result = false;

        //    try
        //    {
        //        SmtpClient smtpClient = null;
        //        MailMessage message = null;
        //        System.Net.Mail.Attachment attachment = null;
        //        smtpClient = new SmtpClient(email.SMTPServer);
        //        smtpClient.Credentials = new NetworkCredential(email.SMTPUsername, email.SMTPPassword);

        //        if (email.SMTPPort > 0)
        //        {
        //            smtpClient.Port = Convert.ToInt32(email.SMTPPort);
        //        }

        //        message = new MailMessage(email.FromEmail, email.ToEmail, email.Subject, email.Message);

        //        message.BodyEncoding = Encoding.UTF8;
        //        message.IsBodyHtml = true;
        //        message.Priority = MailPriority.Normal;

        //        if (!string.IsNullOrEmpty(email.Attachment))
        //        {
        //            attachment = new System.Net.Mail.Attachment(email.Attachment);
        //            message.Attachments.Add(attachment);
        //        }

        //        smtpClient.Send(message);

        //        result = true;
        //    }
        //    catch (Exception ex)
        //    {
        //        AddLogEntry(ex.Message + email.ToEmail + "|" + email.Message);
        //        string lsException = ex.Message;
        //        result = false;
        //    }

        //    return result;
        //}

        //public static bool AddLogEntry(string message)
        //{
        //    try
        //    {
        //        Log log = new Log();
        //        log.Message = message;
        //        log.Timestamp = DateTime.Now;

        //        var logic = new LogLogic();
        //        logic.Add(log);
        //    }
        //    catch
        //    {
        //        // Do nothing
        //    }

        //    return true;
        //}

        //public static bool ForgotPasswordEmail(ForgotPasswordModel forgotPassword)
        //{
        //    bool result = false;
        //    var logic = new ProfileLogic();
        //    Profile profile = logic.GetProfileByEmail(forgotPassword.Email);
        //    EmailModel loEmail = new EmailModel();

        //    var logicConfig = new ConfigLogic();
        //    var config = logicConfig.GetConfig();

        //    if (profile != null)
        //    {
        //        try
        //        {
        //            loEmail.SMTPServer = config["SMTPServer"];
        //            loEmail.SMTPUsername = config["SMTPUsername"];
        //            loEmail.SMTPPassword = config["SMTPPassword"];
        //            loEmail.FromEmail = config["WebmasterEmail"];

        //            loEmail.ToEmail = profile.Email;
        //            loEmail.Subject = config["SiteName"] + " Account Password Reset Request";
        //            loEmail.Message = "Dear User,<br /><br />"
        //                                + "We have received a password reset request from someone specifying your email account.<br />"
        //                                + "If you requested this password reset, you can now reset your password by clicking on the following link: <br /><br />"
        //                                + config["SiteURL"] + "/Account/ResetPassword/" + profile.UserID + "<br /><br />"
        //                                + "Thanks,<br />"
        //                                + "The " + config["SiteName"] + " Management Team";

        //            bool resultEmailSubmit = SubmitMail(loEmail);

        //            if (resultEmailSubmit)
        //            {
        //                result = true;
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            string msg = "Send Email Error: " + ex.Message + " Stack Trace: " + ex.StackTrace.ToString();
        //            AddLogEntry(msg);
        //            result = false;
        //        }
        //    }

        //    return result;
        //}

        //public static bool DoesUserExist(string userID)
        //{
        //    var logic = new ProfileLogic();
        //    var profile = logic.GetProfileByUserId(userID);
        //    bool result = profile != null && profile.UserID == userID;

        //    return result;
        //}

        public static string GetCurrentTimestamp()
        {
            string result = string.Empty;
            result = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString();
            
            return result;
        }

        //public static List<string> GetDatabases()
        //{
        //    string sql = "EXEC sp_databases;";
        //    MSSQL db = new MSSQL();
        //    List<string> databases = new List<string>();

        //    db.openConnection();

        //    try
        //    {
        //        DataTable loDT = db.QueryDBDataset(sql);

        //        if (loDT != null && loDT.Rows.Count > 0)
        //        {
        //            foreach (DataRow loDr in loDT.Rows)
        //            {
        //                databases.Add(Convert.ToString(loDr["DATABASE_NAME"]));
        //            }
        //        }
        //    }
        //    catch 
        //    { }
            
        //    db.closeConnection();

        //    return databases;
        //}

        //public static bool ExecuteDBCommand(string command)
        //{
        //    string sql = command;
        //    MSSQL db = new MSSQL();
        //    bool result = false;

        //    db.openConnection();

        //    try
        //    {
        //        db.ExecDB(sql);

        //        result = true;
        //    }
        //    catch
        //    {
        //        result = false;
        //    }

        //    db.closeConnection();

        //    return result;
        //}

        public static bool IsFileAnImage(string psFilename)
        {
            string extension = Path.GetExtension(psFilename);

            switch (extension)
            {
                case ".jpg":
                    return true;
                case ".png":
                    return true;
                case ".gif":
                    return true;
                case ".tiff":
                    return true;
                case ".bmp":
                    return true;
                default:
                    return false;
            }

        }

        public static bool IsFileAVideo(string psFilename)
        {
            string extension = Path.GetExtension(psFilename);

            switch (extension)
            {
                case ".mp4":
                    return true;
                case ".wmv":
                    return true;
                case ".avi":
                    return true;
                case ".mpg":
                    return true;
                case ".mpeg":
                    return true;
                case ".mov":
                    return true;
                default:
                    return false;
            }

        }

        public static string GetMimeType(string psFilename)
        {
            string lsFileExtension = Path.GetExtension(psFilename);

            switch (lsFileExtension)
            {
                case ".jpg":
                    return "image/jpeg";
                case ".png":
                    return "image/png";
                case ".gif":
                    return "image/gif";
                case ".tiff":
                    return "image/tiff";
                case ".bmp":
                    return "image/bmp";
                case ".mp4":
                    return "video/mp4";
                case ".wmv":
                    return "video/x-ms-wmv";
                case ".avi":
                    return "video/x-msvideo";
                case ".mpg":
                    return "video/mpeg";
                case ".mpeg":
                    return "video/mpeg";
                case ".mov":
                    return "video/quicktime";
                case ".exe":
                    return "application/octet-stream";
                case ".zip":
                    return "application/zip";
                default:
                    return "";
            }
        }

        public static Size SaveImageWithResize(Image image, int maxWidth, int maxHeight, string filePath)
        {
            double resizeWidth = image.Width;
            double resizeHeight = image.Height;

            double aspect = resizeWidth / resizeHeight;

            if (resizeWidth > maxWidth)
            {
                resizeWidth = maxWidth;
                resizeHeight = resizeWidth / aspect;
            }
            if (resizeHeight > maxHeight)
            {
                aspect = resizeWidth / resizeHeight;
                resizeHeight = maxHeight;
                resizeWidth = Convert.ToInt32(resizeHeight * aspect);
            }

            return SaveImageWithCrop(image, Convert.ToInt32(resizeWidth), Convert.ToInt32(resizeHeight), filePath);
        }

        public static Size SaveImageWithCrop(Image image, int maxWidth, int maxHeight, string filePath)
        {
            //ImageCodecInfo jpgInfo = ImageCodecInfo.GetImageEncoders().Where(codecInfo => codecInfo.MimeType == "image/jpeg").First();
            Image finalImage = image;
            System.Drawing.Bitmap bitmap = null;
            try
            {
                int left = 0;
                int top = 0;
                int srcWidth = maxWidth;
                int srcHeight = maxHeight;
                bitmap = new System.Drawing.Bitmap(maxWidth, maxHeight);
                double croppedHeightToWidth = (double)maxHeight / maxWidth;
                double croppedWidthToHeight = (double)maxWidth / maxHeight;

                if (image.Width > image.Height)
                {
                    srcWidth = (int)(Math.Round(image.Height * croppedWidthToHeight));
                    if (srcWidth < image.Width)
                    {
                        srcHeight = image.Height;
                        left = (image.Width - srcWidth) / 2;
                    }
                    else
                    {
                        srcHeight = (int)Math.Round(image.Height * ((double)image.Width / srcWidth));
                        srcWidth = image.Width;
                        top = (image.Height - srcHeight) / 2;
                    }
                }
                else
                {
                    srcHeight = (int)(Math.Round(image.Width * croppedHeightToWidth));
                    if (srcHeight < image.Height)
                    {
                        srcWidth = image.Width;
                        top = (image.Height - srcHeight) / 2;
                    }
                    else
                    {
                        srcWidth = (int)Math.Round(image.Width * ((double)image.Height / srcHeight));
                        srcHeight = image.Height;
                        left = (image.Width - srcWidth) / 2;
                    }
                }
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.CompositingQuality = CompositingQuality.HighQuality;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.DrawImage(image, new Rectangle(0, 0, bitmap.Width, bitmap.Height), new Rectangle(left, top, srcWidth, srcHeight), GraphicsUnit.Pixel);
                }
                finalImage = bitmap;
            }
            catch { }
            try
            {
                using (EncoderParameters encParams = new EncoderParameters(1))
                {
                    //encParams.Param[0] = new EncoderParameter(Encoder.Quality, (long)100);
                    //quality should be in the range [0..100] .. 100 for max, 0 for min (0 best compression)
                    finalImage.Save(filePath);
                    return new Size(finalImage.Width, finalImage.Height);
                }
            }
            catch { }
            if (bitmap != null)
            {
                bitmap.Dispose();
            }
            return Size.Empty;
        }
        public static List<SelectListItem> GetPaymentGateways()
        {
            List<SelectListItem> gateways = new List<SelectListItem>();
            List<string> gatewayList = Enum.GetNames(typeof(PaymentType)).ToList();
            foreach (var item in gatewayList)
            {
                gateways.Add(new SelectListItem { Text = item, Value = item });
            }

            return gateways;
        }

        public static List<string> GetCountryList()
        {
            List<string> countries = new List<string>();

            countries.Add("Afghanistan");
            countries.Add("Albania");
            countries.Add("Algeria");
            countries.Add("American Samoa");
            countries.Add("Andorra");
            countries.Add("Angola");
            countries.Add("Anguilla");
            countries.Add("Antarctica");
            countries.Add("Antigua and Barbuda");
            countries.Add("Argentina");
            countries.Add("Armenia");
            countries.Add("Aruba");
            countries.Add("Australia");
            countries.Add("Austria");
            countries.Add("Azerbaijan");
            countries.Add("Bahamas");
            countries.Add("Bahrain");
            countries.Add("Bangladesh");
            countries.Add("Barbados");
            countries.Add("Belarus");
            countries.Add("Belgium");
            countries.Add("Belize");
            countries.Add("Benin");
            countries.Add("Bermuda");
            countries.Add("Bhutan");
            countries.Add("Bolivia");
            countries.Add("Bosnia and Herzegovina");
            countries.Add("Botswana");
            countries.Add("Brazil");
            countries.Add("British Virgin Islands");
            countries.Add("Brunei");
            countries.Add("Bulgaria");
            countries.Add("Burkina Faso");
            countries.Add("Burma");
            countries.Add("Burundi");
            countries.Add("Cambodia");
            countries.Add("Cameroon");
            countries.Add("Canada");
            countries.Add("Cape Verde");
            countries.Add("Cayman Islands");
            countries.Add("Central African Republic");
            countries.Add("Chad");
            countries.Add("Chile");
            countries.Add("China");
            countries.Add("Christmas Island");
            countries.Add("Cocos Islands");
            countries.Add("Colombia");
            countries.Add("Comoros");
            countries.Add("Democratic Republic of Congo");
            countries.Add("Republic of the Congo");
            countries.Add("Cook Islands");
            countries.Add("Costa Rica");
            countries.Add("Cote d'Ivoire");
            countries.Add("Croatia");
            countries.Add("Cuba");
            countries.Add("Cyprus");
            countries.Add("Czech Republic");
            countries.Add("Denmark");
            countries.Add("Dominica");
            countries.Add("Dominican Republic");
            countries.Add("East Timor");
            countries.Add("Ecuador");
            countries.Add("Egypt");
            countries.Add("El Salvador");
            countries.Add("Equatorial Guinea");
            countries.Add("Eritrea");
            countries.Add("Estonia");
            countries.Add("Ethiopia");
            countries.Add("Falkland Islands");
            countries.Add("Faroe Islands");
            countries.Add("Fiji");
            countries.Add("Finland");
            countries.Add("France");
            countries.Add("French Guiana");
            countries.Add("French Polynesia");
            countries.Add("Gabon");
            countries.Add("Gambia");
            countries.Add("Georgia");
            countries.Add("Germany");
            countries.Add("Ghana");
            countries.Add("Gibraltar");
            countries.Add("Greece");
            countries.Add("Greenland");
            countries.Add("Grenada");
            countries.Add("Guadeloupe");
            countries.Add("Guam");
            countries.Add("Guatemala");
            countries.Add("Guernsey");
            countries.Add("Guinea");
            countries.Add("Guinea-Bissau");
            countries.Add("Guyana");
            countries.Add("Haiti");
            countries.Add("Honduras");
            countries.Add("Hong Kong");
            countries.Add("Howland Island");
            countries.Add("Hungary");
            countries.Add("Iceland");
            countries.Add("India");
            countries.Add("Indonesia");
            countries.Add("Iran");
            countries.Add("Iraq");
            countries.Add("Ireland");
            countries.Add("Israel");
            countries.Add("Italy");
            countries.Add("Jamaica");
            countries.Add("Japan");
            countries.Add("Jersey");
            countries.Add("Jordan");
            countries.Add("Kazakhstan");
            countries.Add("Kenya");
            countries.Add("Kiribati");
            countries.Add("Korea");
            countries.Add("Kuwait");
            countries.Add("Kyrgyzstan");
            countries.Add("Laos");
            countries.Add("Latvia");
            countries.Add("Lebanon");
            countries.Add("Lesotho");
            countries.Add("Liberia");
            countries.Add("Libya");
            countries.Add("Liechtenstein");
            countries.Add("Lithuania");
            countries.Add("Luxembourg");
            countries.Add("Macau");
            countries.Add("Macedonia");
            countries.Add("Madagascar");
            countries.Add("Malawi");
            countries.Add("Malaysia");
            countries.Add("Maldives");
            countries.Add("Mali");
            countries.Add("Malta");
            countries.Add("Isle of Man");
            countries.Add("Marshall Islands");
            countries.Add("Martinique");
            countries.Add("Mauritania");
            countries.Add("Mauritius");
            countries.Add("Mayotte");
            countries.Add("Mexico");
            countries.Add("Federated States of Micronesia");
            countries.Add("Midway Islands");
            countries.Add("Moldova");
            countries.Add("Monaco");
            countries.Add("Mongolia");
            countries.Add("Montserrat");
            countries.Add("Morocco");
            countries.Add("Mozambique");
            countries.Add("Namibia");
            countries.Add("Nauru");
            countries.Add("Navassa Island");
            countries.Add("Netherlands");
            countries.Add("Netherlands Antilles");
            countries.Add("New Caledonia");
            countries.Add("New Zealand");
            countries.Add("Nicaragua");
            countries.Add("Niger");
            countries.Add("Nigeria");
            countries.Add("Niue");
            countries.Add("Norfolk Island");
            countries.Add("Northern Mariana Islands");
            countries.Add("Norway");
            countries.Add("Oman");
            countries.Add("Pakistan");
            countries.Add("Palau");
            countries.Add("Panama");
            countries.Add("Papua New Guinea");
            countries.Add("Paraguay");
            countries.Add("Peru");
            countries.Add("Philippines");
            countries.Add("Poland");
            countries.Add("Portugal");
            countries.Add("Puerto Rico");
            countries.Add("Qatar");
            countries.Add("Reunion");
            countries.Add("Romania");
            countries.Add("Russia");
            countries.Add("Rwanda");
            countries.Add("Saint Helena");
            countries.Add("Saint Kitts and Nevis");
            countries.Add("Saint Lucia");
            countries.Add("Saint Pierre and Miquelon");
            countries.Add("Saint Vincent and the Grenadines");
            countries.Add("Samoa");
            countries.Add("San Marino");
            countries.Add("Sao Tome and Principe");
            countries.Add("Saudi Arabia");
            countries.Add("Senegal");
            countries.Add("Serbia and Montenegro");
            countries.Add("Seychelles");
            countries.Add("Sierra Leone");
            countries.Add("Singapore");
            countries.Add("Slovakia");
            countries.Add("Slovenia");
            countries.Add("Solomon Islands");
            countries.Add("Somalia");
            countries.Add("South Africa");
            countries.Add("Spain");
            countries.Add("Sri Lanka");
            countries.Add("Sudan");
            countries.Add("Suriname");
            countries.Add("Svalbard");
            countries.Add("Swaziland");
            countries.Add("Sweden");
            countries.Add("Switzerland");
            countries.Add("Syria");
            countries.Add("Taiwan");
            countries.Add("Tajikistan");
            countries.Add("Tanzania");
            countries.Add("Thailand");
            countries.Add("Togo");
            countries.Add("Tokelau");
            countries.Add("Tonga");
            countries.Add("Trinidad and Tobago");
            countries.Add("Tunisia");
            countries.Add("Turkey");
            countries.Add("Turkmenistan");
            countries.Add("Turks and Caicos Islands");
            countries.Add("Tuvalu");
            countries.Add("Uganda");
            countries.Add("Ukraine");
            countries.Add("United Arab Emirates");
            countries.Add("United Kingdom");
            countries.Add("United States");
            countries.Add("United States Virgin Islands");
            countries.Add("Uruguay");
            countries.Add("Uzbekistan");
            countries.Add("Vanuatu");
            countries.Add("Venezuela");
            countries.Add("Vatican City");
            countries.Add("Vietnam");
            countries.Add("Virgin Islands");
            countries.Add("Wake Island");
            countries.Add("Wallis and Futuna");
            countries.Add("Western Sahara");
            countries.Add("Yemen");
            countries.Add("Zambia");
            countries.Add("Zimbabwe");

            return countries;
        }

        public static List<SelectListItem> CCGetCountryList()
        {
            List<SelectListItem> countries = new List<SelectListItem>();

            var countryList = GetCountryList();

            foreach (var item in countryList)
            {
                countries.Add(new SelectListItem { Text = item, Value = item });
            }

            return countries;
        }

        public static List<SelectListItem> CCGetMonthList()
        {
            List<SelectListItem> monthList = new List<SelectListItem>();

            for (int i = 1; i < 13; i++)
            {
                char pad = '0';
                string val = i.ToString().PadLeft(2, pad);

                monthList.Add(new SelectListItem { Text = val, Value = val });
            }

            return monthList;
        }

        public static List<SelectListItem> CCGetYearList()
        {
            List<SelectListItem> yearsList = new List<SelectListItem>();

            int startYear = DateTime.Now.Year;
            int endYear = startYear + 20;

            for (int j = startYear; j < endYear; j++)
            {
                yearsList.Add(new SelectListItem { Text = j.ToString(), Value = j.ToString() });
            }

            return yearsList;
        }

        public static DataTable ToDataTable<T>(IList<T> data)
        {
            PropertyDescriptorCollection properties =
                TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            foreach (PropertyDescriptor prop in properties)
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            foreach (T item in data)
            {
                DataRow row = table.NewRow();
                foreach (PropertyDescriptor prop in properties)
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                table.Rows.Add(row);
            }
            return table;
        }

        public static CultureInfo ResolveCulture(string[] languages)
        {
            if (languages == null || languages.Length == 0)
            {
                return null;
            }

            try
            {
                string language = languages[0].ToLowerInvariant().Trim();
                return CultureInfo.CreateSpecificCulture(language);
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        public static RegionInfo ResolveCountry(string[] languages)
        {
            CultureInfo culture = ResolveCulture(languages);
            if (culture != null)
            {
                return new RegionInfo(culture.LCID);
            }

            return null;
        }

        public static List<SearchEngineViewModel> GetSearchEngines()
        {
            List<SearchEngineViewModel> searchEngines = new List<SearchEngineViewModel>();

            searchEngines.Add(new SearchEngineViewModel() { Name = "Alehea", URL = "alhea.com" });
            searchEngines.Add(new SearchEngineViewModel() { Name = "Alexa", URL = "alexa.com" });
            searchEngines.Add(new SearchEngineViewModel() { Name = "Ask", URL = "ask.com" });
            searchEngines.Add(new SearchEngineViewModel() { Name = "Baidu", URL = "baidu.com" });
            searchEngines.Add(new SearchEngineViewModel() { Name = "Bing", URL = "bing.com" });
            searchEngines.Add(new SearchEngineViewModel() { Name = "Dogpile", URL = "dogpile.com" });
            searchEngines.Add(new SearchEngineViewModel() { Name = "DuckDuckGo", URL = "duckduckgo.com" });
            searchEngines.Add(new SearchEngineViewModel() { Name = "Entireweb", URL = "entireweb.com" });
            searchEngines.Add(new SearchEngineViewModel() { Name = "Exalead", URL = "exalead.com" });
            searchEngines.Add(new SearchEngineViewModel() { Name = "Excite", URL = "excite.com" });
            searchEngines.Add(new SearchEngineViewModel() { Name = "Faroo", URL = "faroo.com" });
            searchEngines.Add(new SearchEngineViewModel() { Name = "Gigablast", URL = "gigablast.com" });
            searchEngines.Add(new SearchEngineViewModel() { Name = "Goodsearch", URL = "goodsearch.com" });
            searchEngines.Add(new SearchEngineViewModel() { Name = "Google", URL = "google.com" });
            searchEngines.Add(new SearchEngineViewModel() { Name = "Hotbot", URL = "hotbot.com" });
            searchEngines.Add(new SearchEngineViewModel() { Name = "Info.com", URL = "info.com" });
            searchEngines.Add(new SearchEngineViewModel() { Name = "Infospace", URL = "infospace.com" });
            searchEngines.Add(new SearchEngineViewModel() { Name = "IXQuick", URL = "ixquick.com" });
            searchEngines.Add(new SearchEngineViewModel() { Name = "Lycos", URL = "lycos.com" });
            searchEngines.Add(new SearchEngineViewModel() { Name = "Mamma", URL = "mamma.com" });
            searchEngines.Add(new SearchEngineViewModel() { Name = "Qwant", URL = "qwant.com" });
            searchEngines.Add(new SearchEngineViewModel() { Name = "Sogou", URL = "sogou.com" });
            searchEngines.Add(new SearchEngineViewModel() { Name = "WebCrawler", URL = "webcrawler.com" });
            searchEngines.Add(new SearchEngineViewModel() { Name = "Wow", URL = "wow.com" });
            searchEngines.Add(new SearchEngineViewModel() { Name = "Yahoo", URL = "yahoo.com" });
            searchEngines.Add(new SearchEngineViewModel() { Name = "Yandex", URL = "yandex.com" });
            searchEngines.Add(new SearchEngineViewModel() { Name = "Yippy", URL = "yippy.com" });
            searchEngines.Add(new SearchEngineViewModel() { Name = "Youdao", URL = "youdao.com" });
            searchEngines.Add(new SearchEngineViewModel() { Name = "Zoo", URL = "zoo.com" });

            return searchEngines;
        }

        public static string GetSearchEngineName(string referringURL)
        {
            string searchEngineName = string.Empty;
            List<SearchEngineViewModel> searchEngines = GetSearchEngines();

            if (referringURL != null)
            {
                foreach (var searchEngine in searchEngines)
                {
                    if (referringURL.Contains(searchEngine.URL))
                    {
                        searchEngineName = searchEngine.Name;
                        return searchEngineName;
                    }
                }
            }

            return searchEngineName;
        }

        public static string GetWebContent(string url)
        {
            var client = new WebClient();
            var text = client.DownloadString(url);

            return text.ToString();
        }
        public static string GetCountryCode(string country)
        {
            Dictionary<string, string> countryCodes = new Dictionary<string, string>();
            countryCodes.Add("Afghanistan", "AF");
            countryCodes.Add("Albania", "AL");
            countryCodes.Add("Algeria", "DZ");
            countryCodes.Add("American Samoa", "AS");
            countryCodes.Add("Andorra", "AD");
            countryCodes.Add("Angola", "AO");
            countryCodes.Add("Anguilla", "AI");
            countryCodes.Add("Antarctica", "AQ");
            countryCodes.Add("Antigua and Barbuda", "AG");
            countryCodes.Add("Argentina", "AR");
            countryCodes.Add("Armenia", "AM");
            countryCodes.Add("Aruba", "AW");
            countryCodes.Add("Australia", "AU");
            countryCodes.Add("Austria", "AT");
            countryCodes.Add("Azerbaijan", "AZ");
            countryCodes.Add("Bahamas", "BS");
            countryCodes.Add("Bahrain", "BH");
            countryCodes.Add("Bangladesh", "BD");
            countryCodes.Add("Barbados", "BB");
            countryCodes.Add("Belarus", "BY");
            countryCodes.Add("Belgium", "BE");
            countryCodes.Add("Belize", "BZ");
            countryCodes.Add("Benin", "BJ");
            countryCodes.Add("Bermuda", "BM");
            countryCodes.Add("Bhutan", "BT");
            countryCodes.Add("Bolivia", "BO");
            countryCodes.Add("Bonaire", "BQ");
            countryCodes.Add("Bosnia and Herzegovina", "BA");
            countryCodes.Add("Botswana", "BW");
            countryCodes.Add("Bouvet Island", "BV");
            countryCodes.Add("Brazil", "BR");
            countryCodes.Add("British Indian Ocean Territory", "IO");
            countryCodes.Add("Brunei Darussalam", "BN");
            countryCodes.Add("Bulgaria", "BG");
            countryCodes.Add("Burkina Faso", "BF");
            countryCodes.Add("Burundi", "BI");
            countryCodes.Add("Cambodia", "KH");
            countryCodes.Add("Cameroon", "CM");
            countryCodes.Add("Canada", "CA");
            countryCodes.Add("Cape Verde", "CV");
            countryCodes.Add("Cayman Islands", "KY");
            countryCodes.Add("Central African Republic", "CF");
            countryCodes.Add("Chad", "TD");
            countryCodes.Add("Chile", "CL");
            countryCodes.Add("China", "CN");
            countryCodes.Add("Christmas Island", "CX");
            countryCodes.Add("Cocos (Keeling) Islands", "CC");
            countryCodes.Add("Colombia", "CO");
            countryCodes.Add("Comoros", "KM");
            countryCodes.Add("Congo", "CG");
            countryCodes.Add("Democratic Republic of the Congo", "CD");
            countryCodes.Add("Cook Islands", "CK");
            countryCodes.Add("Costa Rica", "CR");
            countryCodes.Add("Croatia", "HR");
            countryCodes.Add("Cuba", "CU");
            countryCodes.Add("CuraÃ§ao", "CW");
            countryCodes.Add("Cyprus", "CY");
            countryCodes.Add("Czech Republic", "CZ");
            countryCodes.Add("CÃ´te d'Ivoire", "CI");
            countryCodes.Add("Denmark", "DK");
            countryCodes.Add("Djibouti", "DJ");
            countryCodes.Add("Dominica", "DM");
            countryCodes.Add("Dominican Republic", "DO");
            countryCodes.Add("Ecuador", "EC");
            countryCodes.Add("Egypt", "EG");
            countryCodes.Add("El Salvador", "SV");
            countryCodes.Add("Equatorial Guinea", "GQ");
            countryCodes.Add("Eritrea", "ER");
            countryCodes.Add("Estonia", "EE");
            countryCodes.Add("Ethiopia", "ET");
            countryCodes.Add("Falkland Islands (Malvinas)", "FK");
            countryCodes.Add("Faroe Islands", "FO");
            countryCodes.Add("Fiji", "FJ");
            countryCodes.Add("Finland", "FI");
            countryCodes.Add("France", "FR");
            countryCodes.Add("French Guiana", "GF");
            countryCodes.Add("French Polynesia", "PF");
            countryCodes.Add("French Southern Territories", "TF");
            countryCodes.Add("Gabon", "GA");
            countryCodes.Add("Gambia", "GM");
            countryCodes.Add("Georgia", "GE");
            countryCodes.Add("Germany", "DE");
            countryCodes.Add("Ghana", "GH");
            countryCodes.Add("Gibraltar", "GI");
            countryCodes.Add("Greece", "GR");
            countryCodes.Add("Greenland", "GL");
            countryCodes.Add("Grenada", "GD");
            countryCodes.Add("Guadeloupe", "GP");
            countryCodes.Add("Guam", "GU");
            countryCodes.Add("Guatemala", "GT");
            countryCodes.Add("Guernsey", "GG");
            countryCodes.Add("Guinea", "GN");
            countryCodes.Add("Guinea-Bissau", "GW");
            countryCodes.Add("Guyana", "GY");
            countryCodes.Add("Haiti", "HT");
            countryCodes.Add("Heard Island and McDonald Mcdonald Islands", "HM");
            countryCodes.Add("Holy See (Vatican City State)", "VA");
            countryCodes.Add("Honduras", "HN");
            countryCodes.Add("Hong Kong", "HK");
            countryCodes.Add("Hungary", "HU");
            countryCodes.Add("Iceland", "IS");
            countryCodes.Add("India", "IN");
            countryCodes.Add("Indonesia", "ID");
            countryCodes.Add("Iran, Islamic Republic of", "IR");
            countryCodes.Add("Iraq", "IQ");
            countryCodes.Add("Ireland", "IE");
            countryCodes.Add("Isle of Man", "IM");
            countryCodes.Add("Israel", "IL");
            countryCodes.Add("Italy", "IT");
            countryCodes.Add("Jamaica", "JM");
            countryCodes.Add("Japan", "JP");
            countryCodes.Add("Jersey", "JE");
            countryCodes.Add("Jordan", "JO");
            countryCodes.Add("Kazakhstan", "KZ");
            countryCodes.Add("Kenya", "KE");
            countryCodes.Add("Kiribati", "KI");
            countryCodes.Add("Korea, Democratic People's Republic of", "KP");
            countryCodes.Add("Korea, Republic of", "KR");
            countryCodes.Add("Kuwait", "KW");
            countryCodes.Add("Kyrgyzstan", "KG");
            countryCodes.Add("Lao People's Democratic Republic", "LA");
            countryCodes.Add("Latvia", "LV");
            countryCodes.Add("Lebanon", "LB");
            countryCodes.Add("Lesotho", "LS");
            countryCodes.Add("Liberia", "LR");
            countryCodes.Add("Libya", "LY");
            countryCodes.Add("Liechtenstein", "LI");
            countryCodes.Add("Lithuania", "LT");
            countryCodes.Add("Luxembourg", "LU");
            countryCodes.Add("Macao", "MO");
            countryCodes.Add("Macedonia, the Former Yugoslav Republic of", "MK");
            countryCodes.Add("Madagascar", "MG");
            countryCodes.Add("Malawi", "MW");
            countryCodes.Add("Malaysia", "MY");
            countryCodes.Add("Maldives", "MV");
            countryCodes.Add("Mali", "ML");
            countryCodes.Add("Malta", "MT");
            countryCodes.Add("Marshall Islands", "MH");
            countryCodes.Add("Martinique", "MQ");
            countryCodes.Add("Mauritania", "MR");
            countryCodes.Add("Mauritius", "MU");
            countryCodes.Add("Mayotte", "YT");
            countryCodes.Add("Mexico", "MX");
            countryCodes.Add("Micronesia, Federated States of", "FM");
            countryCodes.Add("Moldova, Republic of", "MD");
            countryCodes.Add("Monaco", "MC");
            countryCodes.Add("Mongolia", "MN");
            countryCodes.Add("Montenegro", "ME");
            countryCodes.Add("Montserrat", "MS");
            countryCodes.Add("Morocco", "MA");
            countryCodes.Add("Mozambique", "MZ");
            countryCodes.Add("Myanmar", "MM");
            countryCodes.Add("Namibia", "NA");
            countryCodes.Add("Nauru", "NR");
            countryCodes.Add("Nepal", "NP");
            countryCodes.Add("Netherlands", "NL");
            countryCodes.Add("New Caledonia", "NC");
            countryCodes.Add("New Zealand", "NZ");
            countryCodes.Add("Nicaragua", "NI");
            countryCodes.Add("Niger", "NE");
            countryCodes.Add("Nigeria", "NG");
            countryCodes.Add("Niue", "NU");
            countryCodes.Add("Norfolk Island", "NF");
            countryCodes.Add("Northern Mariana Islands", "MP");
            countryCodes.Add("Norway", "NO");
            countryCodes.Add("Oman", "OM");
            countryCodes.Add("Pakistan", "PK");
            countryCodes.Add("Palau", "PW");
            countryCodes.Add("Palestine, State of", "PS");
            countryCodes.Add("Panama", "PA");
            countryCodes.Add("Papua New Guinea", "PG");
            countryCodes.Add("Paraguay", "PY");
            countryCodes.Add("Peru", "PE");
            countryCodes.Add("Philippines", "PH");
            countryCodes.Add("Pitcairn", "PN");
            countryCodes.Add("Poland", "PL");
            countryCodes.Add("Portugal", "PT");
            countryCodes.Add("Puerto Rico", "PR");
            countryCodes.Add("Qatar", "QA");
            countryCodes.Add("Romania", "RO");
            countryCodes.Add("Russian Federation", "RU");
            countryCodes.Add("Rwanda", "RW");
            countryCodes.Add("Reunion", "RE");
            countryCodes.Add("Saint Barthelemy", "BL");
            countryCodes.Add("Saint Helena", "SH");
            countryCodes.Add("Saint Kitts and Nevis", "KN");
            countryCodes.Add("Saint Lucia", "LC");
            countryCodes.Add("Saint Martin (French part)", "MF");
            countryCodes.Add("Saint Pierre and Miquelon", "PM");
            countryCodes.Add("Saint Vincent and the Grenadines", "VC");
            countryCodes.Add("Samoa", "WS");
            countryCodes.Add("San Marino", "SM");
            countryCodes.Add("Sao Tome and Principe", "ST");
            countryCodes.Add("Saudi Arabia", "SA");
            countryCodes.Add("Senegal", "SN");
            countryCodes.Add("Serbia", "RS");
            countryCodes.Add("Seychelles", "SC");
            countryCodes.Add("Sierra Leone", "SL");
            countryCodes.Add("Singapore", "SG");
            countryCodes.Add("Sint Maarten (Dutch part)", "SX");
            countryCodes.Add("Slovakia", "SK");
            countryCodes.Add("Slovenia", "SI");
            countryCodes.Add("Solomon Islands", "SB");
            countryCodes.Add("Somalia", "SO");
            countryCodes.Add("South Africa", "ZA");
            countryCodes.Add("South Georgia and the South Sandwich Islands", "GS");
            countryCodes.Add("South Sudan", "SS");
            countryCodes.Add("Spain", "ES");
            countryCodes.Add("Sri Lanka", "LK");
            countryCodes.Add("Sudan", "SD");
            countryCodes.Add("Suriname", "SR");
            countryCodes.Add("Svalbard and Jan Mayen", "SJ");
            countryCodes.Add("Swaziland", "SZ");
            countryCodes.Add("Sweden", "SE");
            countryCodes.Add("Switzerland", "CH");
            countryCodes.Add("Syrian Arab Republic", "SY");
            countryCodes.Add("Taiwan, Province of China", "TW");
            countryCodes.Add("Tajikistan", "TJ");
            countryCodes.Add("United Republic of Tanzania", "TZ");
            countryCodes.Add("Thailand", "TH");
            countryCodes.Add("Timor-Leste", "TL");
            countryCodes.Add("Togo", "TG");
            countryCodes.Add("Tokelau", "TK");
            countryCodes.Add("Tonga", "TO");
            countryCodes.Add("Trinidad and Tobago", "TT");
            countryCodes.Add("Tunisia", "TN");
            countryCodes.Add("Turkey", "TR");
            countryCodes.Add("Turkmenistan", "TM");
            countryCodes.Add("Turks and Caicos Islands", "TC");
            countryCodes.Add("Tuvalu", "TV");
            countryCodes.Add("Uganda", "UG");
            countryCodes.Add("Ukraine", "UA");
            countryCodes.Add("United Arab Emirates", "AE");
            countryCodes.Add("United Kingdom", "GB");
            countryCodes.Add("United States", "US");
            countryCodes.Add("United States Minor Outlying Islands", "UM");
            countryCodes.Add("Uruguay", "UY");
            countryCodes.Add("Uzbekistan", "UZ");
            countryCodes.Add("Vanuatu", "VU");
            countryCodes.Add("Venezuela", "VE");
            countryCodes.Add("Viet Nam", "VN");
            countryCodes.Add("British Virgin Islands", "VG");
            countryCodes.Add("US Virgin Islands", "VI");
            countryCodes.Add("Wallis and Futuna", "WF");
            countryCodes.Add("Western Sahara", "EH");
            countryCodes.Add("Yemen", "YE");
            countryCodes.Add("Zambia", "ZM");
            countryCodes.Add("Zimbabwe", "ZW");
            countryCodes.Add("Aland Islands", "AX");
            return countryCodes[country];
        }
        //public static string GetCountryFromAPI(string ip, string[] languages)
        //{
        //    string response = string.Empty;
        //    string result = string.Empty;
        //    string path = AppDomain.CurrentDomain.GetData("DataDirectory") + "\\GeoLite2-Country.mmdb";

        //    using (var reader = new Reader(path))
        //    {
        //        dynamic jsonResponse = reader.Find(ip);

        //        try
        //        {
        //            string country_name = jsonResponse.country.names.en;

        //            if (country_name != "")
        //            {
        //                result = jsonResponse.country.names.en;
        //            }
        //            else
        //            {
        //                RegionInfo regionInfo = Utility.ResolveCountry(languages);
        //                result = regionInfo.EnglishName;
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            result = string.Empty;
        //        }
        //    }

        //    //string url = "http://freegeoip.net/json/" + ip;

        //    return result;
        //}

        //public static bool GetVisitorInfo(string ip, HttpBrowserCapabilities browser, string browserUserAgent, string referringURL, string language, string[] languages, string pageUrl)
        //{
        //    // System.Threading.Thread.Sleep(10000);

        //    // Get Visitor Statistics 
        //    try
        //    {
        //        digioz.PortalEntities db = new digioz.PortalEntities();
        //        VisitorInfo visitorInfo = new VisitorInfo();
        //        string country = string.Empty;

        //        // Get Country
        //        try
        //        {
        //            country = GetCountryFromAPI(ip, languages);
        //        }
        //        catch
        //        {
        //            country = string.Empty;
        //        }

        //        visitorInfo.PageURL = pageUrl;
        //        visitorInfo.OperatingSystem = browser.Platform;
        //        visitorInfo.IPAddress = ip;
        //        visitorInfo.BrowserName = browser.Browser;
        //        visitorInfo.BrowserType = browser.Type;
        //        visitorInfo.BrowserUserAgent = browserUserAgent;
        //        visitorInfo.BrowserVersion = browser.Version;
        //        visitorInfo.IsCrawler = browser.Crawler;
        //        visitorInfo.JSVersion = browser.EcmaScriptVersion.ToString();
        //        visitorInfo.ReferringURL = referringURL;
        //        visitorInfo.Language = language;
        //        visitorInfo.Country = country;
        //        visitorInfo.SearchEngine = Utility.GetSearchEngineName(visitorInfo.ReferringURL);
        //        visitorInfo.Timestamp = DateTime.Now;

        //        db.VisitorInfos.Add(visitorInfo);
        //        db.SaveChanges();
        //    }
        //    catch (Exception ex)
        //    {
        //        Utility.AddLogEntry(ex.Message);
        //    }

        //    return true;
        //}

        // Hashes an email with MD5.  Suitable for use with Gravatar profile
        public static string HashEmailForGravatar(string email)
        {
            // Create a new instance of the MD5CryptoServiceProvider object.  
            MD5 md5Hasher = MD5.Create();

            // Convert the input string to a byte array and compute the hash.  
            byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(email));

            // Create a new Stringbuilder to collect the bytes  
            // and create a string.  
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data  
            // and format each one as a hexadecimal string.  
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            return sBuilder.ToString();  // Return the hexadecimal string. 
        }

        public static bool SaveFileFromUrl(string fileName, string url)
        {
            byte[] content;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            WebResponse response = request.GetResponse();

            Stream stream = response.GetResponseStream();

            using (BinaryReader br = new BinaryReader(stream))
            {
                content = br.ReadBytes(500000);
                br.Close();
            }
            response.Close();

            FileStream fs = new FileStream(fileName, FileMode.Create);
            BinaryWriter bw = new BinaryWriter(fs);
            try
            {
                bw.Write(content);
            }
            finally
            {
                fs.Close();
                bw.Close();
            }

            return true;
        }

        public static string GetRemoteImageExtension(string url)
        {
            string ext = string.Empty;

            WebRequest request = WebRequest.Create(url);
            using (WebResponse response = request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            {
                string contentType = response.ContentType;

                if (contentType == "image/png")
                {
                    ext = "png";
                }
                else if (contentType == "image/gif")
                {
                    ext = "gif";
                }
                else if (contentType == "image/jpeg")
                {
                    ext = "jpg";
                }
                else if (contentType == "image/bmp")
                {
                    ext = "bmp";
                }
                else if (contentType == "image/tiff")
                {
                    ext = "tiff";
                }
                else if (contentType == "image/gif")
                {
                    ext = "gif";
                }
                else
                {
                    ext = "jpg";
                }
            }

            return ext;
        }

        //public static void WriteVisitorSession(string sessionId, string pageUrl, string userName, string ipAddress)
        //{
        //    try
        //    {
        //        var logic = new VisitorSessionLogic();
        //        var prevSession = logic.GetAll().SingleOrDefault(x => x.SessionId == sessionId);

        //        if (prevSession != null)
        //        {
        //            prevSession.PageUrl = pageUrl;

        //            if (!string.IsNullOrEmpty(userName))
        //            {
        //                prevSession.UserName = userName;
        //            }

        //            prevSession.DateModified = DateTime.Now;
        //            logic.Edit(prevSession);
        //        }
        //        else
        //        {
        //            VisitorSession session = new VisitorSession();

        //            if (ipAddress != null)
        //            {
        //                session.IpAddress = ipAddress;
        //                session.PageUrl = pageUrl;
        //                session.SessionId = sessionId;

        //                if (!string.IsNullOrEmpty(userName))
        //                {
        //                    session.UserName = userName;
        //                }

        //                session.DateCreated = DateTime.Now;
        //                session.DateModified = session.DateCreated;
        //            }

        //            logic.Add(session);
        //        }
        //    }
        //    catch
        //    {
        //        // Ignore for now
        //    }
        //}

        public static string ReadTextFile(string path)
        {
            string content = File.ReadAllText(path);

            return content;
        }

        public static string GetUniqueKey(int maxSize)
        {
            char[] chars = new char[62];
            chars =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
            byte[] data = new byte[1];
            RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider();
            crypto.GetNonZeroBytes(data);
            data = new byte[maxSize];
            crypto.GetNonZeroBytes(data);
            StringBuilder result = new StringBuilder(maxSize);

            foreach (byte b in data)
            {
                result.Append(chars[b % (chars.Length)]);
            }

            return result.ToString();
        }

        public static List<string> GetFilesFrom(String searchFolder, String[] filters, bool isRecursive)
        {
            List<String> filesFound = new List<String>();
            var searchOption = isRecursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            foreach (var filter in filters)
            {
                var files = Directory.GetFiles(searchFolder, $"*.{filter}", searchOption);

                foreach (var file in files)
                {
                    string fileName = Path.GetFileName(file.ToString());
                    filesFound.Add(fileName);
                }
            }

            return filesFound;
        }

        public static string CreditCardType(string cardNumber)
        {

            /*CARD TYPES            *PREFIX           *WIDTH
            American Express       34, 37            15
            Diners Club            300 to 305, 36    14
            Carte Blanche          38                14
            Discover               6011              16
            EnRoute                2014, 2149        15
            JCB                    3                 16
            JCB                    2131, 1800        15
            Master Card            51 to 55          16
            Visa                   4                 13, 16
            */
            string cardType = "";
            cardNumber = cardNumber.Replace(" ", "");
            cardNumber = cardNumber.Replace("-", "");
            Regex reNum = new Regex(@"^\d+$");
            bool isNumeric = reNum.Match(cardNumber).Success;
            if (cardNumber.Length < 14 || isNumeric)
            {
                switch (int.Parse(cardNumber.Substring(0, 2)))
                {
                    case 34 :
                    case 37: 
                        cardType = "amex";
                        break;
                    case 36:
                        cardType = "Diners Club";
                        break;
                    case 51: case 52: case 53: case 54: case 55:
                        cardType = "mastercard";
                        break;
                    default:
                        switch (int.Parse(cardNumber.Substring(0, 4)))
                        {
                            case 2014: case 2149:
                                cardType = "EnRoute";
                                break;
                            case 2131: case 1800:
                                cardType = "JCB";
                                break;
                            case 6011:
                                cardType = "discover";
                                break;                                
                            default:
                                if (cardNumber.Substring(0, 1) == "4") cardType = "visa";
                                break;
                        }
                        break;
                }
            }
            return cardType;
        }

        /// <summary>
        /// Method to convert a DataTable to
        /// a Generic List of Objects using Reflection
        /// 
        // Example 1:
        // Get private fields + non properties
        // var fields = typeof(T).GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
        //
        // Example 2: Your case
        // List<Branch> branche = BindList<Branch>(dt1);
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dt">The dt.</param>
        /// <returns></returns>
        public static List<T> BindList<T>(DataTable dt)
        {
            //var fields = typeof(T).GetFields();
            var fields = typeof(T).GetProperties();

            List<T> lst = new List<T>();

            foreach (DataRow dr in dt.Rows)
            {
                // Create the object of T
                var ob = Activator.CreateInstance<T>();

                foreach (var fieldInfo in fields)
                {
                    foreach (DataColumn dc in dt.Columns)
                    {
                        // Matching the columns with fields
                        if (fieldInfo.Name == dc.ColumnName)
                        {
                            // Get the value from the datatable cell
                            object value = dr[dc.ColumnName];

                            // Set the value into the object
                            if (value != DBNull.Value)
                            {
                                fieldInfo.SetValue(ob, value);
                            }

                            break;
                        }
                    }
                }

                lst.Add(ob);
            }

            return lst;
        }

        /// <summary>
        /// Generic function to Bind a POCO object
        /// with the values stored in a DataTable.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dt">The dt.</param>
        /// <returns></returns>
        public static T BindObject<T>(DataTable dt)
        {
            //var fields = typeof(T).GetFields();
            var fields = typeof(T).GetProperties();

            List<T> lst = new List<T>();
            DataRow dr = dt.Rows[0];

            // Create the object of T
            var ob = Activator.CreateInstance<T>();

            foreach (var fieldInfo in fields)
            {
                foreach (DataColumn dc in dt.Columns)
                {
                    // Matching the columns with fields
                    if (fieldInfo.Name == dc.ColumnName)
                    {
                        // Get the value from the datatable cell
                        object value = dr[dc.ColumnName];

                        // Set the value into the object
                        if (value != DBNull.Value)
                        {
                            fieldInfo.SetValue(ob, value);
                        }

                        break;
                    }
                }
            }

            return ob;
        }

        /// <summary>
        /// Function to get either a list of subfolders in 
        /// a directory that match a pattern or all of them
        /// </summary>
        /// <param name="path"></param>
        /// <param name="pattern"></param>
        /// <param name="returnFullPath"></param>
        /// <returns></returns>
        /// Usage 1: GetFolderInDirectory(@"C:\Temp\", "foo");
        /// Usage 2: GetFolderInDIrectory(@"C:\Temp\");
        public static List<string> GetFoldersInDirectory(string path, string pattern = "", bool returnFullPath = false)
        {
            var subFolders = new List<string>();
            List<string> folderList;
            
            if (pattern == string.Empty)
            {
                folderList = Directory.GetDirectories(path).ToList();

            }
            else
            {
                var folderListTemp = from f in Directory.EnumerateDirectories(path)
                                     where f.Contains(pattern)
                                     select f;
                folderList = folderListTemp.ToList();
            }

            if (returnFullPath)
            {
                return folderList;
            }
            else
            {
                foreach (var item in folderList)
                {
                    var sections = item.Split('\\');
                    var folderName = sections[sections.Length - 1];
                    subFolders.Add(folderName);
                }
            }

            return subFolders;
        }

        /// <summary>
        /// Helper Class which reads RSS Feeds URL from the Database
        /// and fetches the contents of them returning a List of Content Objects
        /// </summary>
        /// <returns></returns>
        public static List<RSSViewModel> GetRSSFeeds(List<Rss> rssList)
        {
            //var rssLogic = new RssLogic();
            //var rssList = rssLogic.GetAll();
            var rssContentList = new List<RSSViewModel>();

            foreach (var rss in rssList)
            {
                int feedCount = 0;
                XmlReader reader = XmlReader.Create(rss.Url);
                SyndicationFeed feed = SyndicationFeed.Load(reader);
                reader.Close();

                foreach (SyndicationItem item in feed.Items)
                {
                    feedCount++;

                    var newsItem = new RSSViewModel()
                    {
                        Id = item.Id,
                        Title = item.Title.Text,
                        Body = item.Summary.Text,
                        Category = item.Categories.ToString()
                    };

                    if (feedCount <= rss.MaxCount)
                    {
                        rssContentList.Add(newsItem);
                    }
                }
            }

            return rssContentList;
        }
    }
}