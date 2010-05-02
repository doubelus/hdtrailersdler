using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Text;
using System.Configuration;

namespace HDTrailersNETDownloader
{
    class Config
    {
        public string[] QualityPreference { get; private set; }
        public string TrailerDownloadFolder { get; private set; }
        public string MetadataDownloadFolder { get; private set; }
        public bool CreateFolder { get; private set; }
        public bool VerboseLogging { get; private set; }
        public bool PauseWhenDone { get; private set; }
        public bool PhysicalLog { get; private set; }
        public int KeepFor { get; private set; }
        public bool GrabPoster { get; private set; }
        public bool UseExclusions { get; private set; }
        public bool TrailerOnly { get; private set; }
        public int MinTrailerSize { get; private set; }
        public bool EmailSummary { get; private set; }
        public string EmailAddress { get; private set; }
        public string SMTPServer { get; private set; }
        public string EmailReturnAddress { get; private set; }
        public string EmailReturnDisplayName { get; private set; }
        public bool AddDates { get; private set; }
        public string[] UserAgentId { get; private set; }
        public string[] UserAgentString { get; private set; }
        public string FeedAddress { get; private set; }
        public bool RunEXE { get; private set; }
        public bool RunOnlyWhenNewTrailers { get; private set; }
        public string Executable { get; private set; }
        public string EXEArguements { get; private set; }


        public Config()
        {
            this.PhysicalLog = false;
        }

        // return a string from a NameValue
        private string GetStringFromAppsettings(NameValueCollection coll, string name, string def)
        {
            string ret = coll[name];
            if (ret == null)
                return def;
            return ret;
        }

        // return a bool from a NameValue
        private Boolean GetBooleanFromAppsettings(NameValueCollection coll, string name, string def)
        {
            string ret = GetStringFromAppsettings(coll, name, def);
            return Convert.ToBoolean(ret, CultureInfo.InvariantCulture);
        }

        // return a bool from a NameValue
        private Int32 GetInt32FromAppsettings(NameValueCollection coll, string name, string def)
        {
            string ret = GetStringFromAppsettings(coll, name, def);
            return Convert.ToInt32(ret, CultureInfo.InvariantCulture);
        }

        private string[] GetStringArrayFromAppsettings(NameValueCollection coll, string name, string def)
        {
            string[] ret;
            string res = GetStringFromAppsettings(coll, name, def);
            if (( res == null) || (res.Length == 0))
                return new string[0];

            ret = res.Split(new Char[] { ',' });
            if (ret == null)
                return new string[0];

            return ret;
        }

        public void Init()
        {
            //Load our config
            // Get the AppSettings section.
            NameValueCollection appSetting = ConfigurationManager.AppSettings;

            this.QualityPreference = GetStringArrayFromAppsettings(appSetting, "QualityPreference", "720p,480p");
            this.TrailerDownloadFolder = GetStringFromAppsettings(appSetting, "TrailerDownloadFolder", "c:\\Trailers").TrimEnd('\\');
            this.MetadataDownloadFolder = GetStringFromAppsettings(appSetting, "MetadataDownloadFolder", "c:\\Trailers").TrimEnd('\\');
            this.GrabPoster = GetBooleanFromAppsettings(appSetting, "GrabPoster", "true");
            this.CreateFolder = GetBooleanFromAppsettings(appSetting, "CreateFolder", "true");
            this.VerboseLogging = GetBooleanFromAppsettings(appSetting, "VerboseLogging", "true");
            this.PhysicalLog = GetBooleanFromAppsettings(appSetting, "PhysicalLog", "true");
            this.PauseWhenDone = GetBooleanFromAppsettings(appSetting, "PauseWhenDone", "true");
            this.KeepFor = GetInt32FromAppsettings(appSetting, "KeepFor", "0");
            this.MinTrailerSize = GetInt32FromAppsettings(appSetting, "MinTrailerSize", "100000");
            this.UseExclusions = GetBooleanFromAppsettings(appSetting, "UseExclusions", "true");
            this.TrailerOnly = GetBooleanFromAppsettings(appSetting, "TrailersOnly", "true");
            this.EmailSummary = GetBooleanFromAppsettings(appSetting, "EmailSummary", "false");
            this.EmailAddress = GetStringFromAppsettings(appSetting, "EmailAddress", "");
            this.SMTPServer = GetStringFromAppsettings(appSetting, "SMTPServer", "");
            this.EmailReturnAddress = GetStringFromAppsettings(appSetting, "EmailReturnAddress", "");
            this.EmailReturnDisplayName = GetStringFromAppsettings(appSetting, "EmailReturnDisplayName", "");
            this.AddDates = GetBooleanFromAppsettings(appSetting, "AddDates", "true");
            this.UserAgentId = GetStringArrayFromAppsettings(appSetting, "UserAgentIds", "");
            this.UserAgentString = GetStringArrayFromAppsettings(appSetting, "UserAgentStrings", "");
            this.FeedAddress = GetStringFromAppsettings(appSetting, "FeedAddress", @"http://www.hd-trailers.net/blog/feed/");
            this.RunEXE = GetBooleanFromAppsettings(appSetting, "RunEXE", "false");
            this.RunOnlyWhenNewTrailers = GetBooleanFromAppsettings(appSetting, "RunOnlyWhenNewTrailers", "false");
            this.Executable = GetStringFromAppsettings(appSetting, "Executable", "");
            this.EXEArguements = GetStringFromAppsettings(appSetting, "EXEArguements", "");
        }

        public string Info()
        {
            int i;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine();
            sb.Append("QualityPreference: ");
            for (i = 0; i < QualityPreference.Length; i++)
                sb.AppendFormat("{0}   ", QualityPreference[i]);
            sb.AppendLine();
            sb.AppendFormat("{0}: {1}\n", "TrailerDownloadFolder", TrailerDownloadFolder.ToString());
            sb.AppendFormat("{0}: {1}\n", "MetadataDownloadFolder", MetadataDownloadFolder.ToString());
            sb.AppendFormat("{0}: {1}\n", "GrabPoster", GrabPoster.ToString());
            sb.AppendFormat("{0}: {1}\n", "CreateFolder", CreateFolder.ToString());
            sb.AppendFormat("{0}: {1}\n", "VerboseLogging", VerboseLogging.ToString());
            sb.AppendFormat("{0}: {1}\n", "PhysicalLog", PhysicalLog.ToString());
            sb.AppendFormat("{0}: {1}\n", "PauseWhenDone", PauseWhenDone.ToString());
            sb.AppendFormat("{0}: {1}\n", "KeepFor", KeepFor.ToString());
            sb.AppendFormat("{0}: {1}\n", "UseExclusions", UseExclusions.ToString());
            sb.AppendFormat("{0}: {1}\n", "TrailersOnly", TrailerOnly.ToString());
            sb.AppendFormat("{0}: {1}\n", "MinTrailerSize", MinTrailerSize.ToString());
            sb.AppendFormat("{0}: {1}\n", "AddDates", AddDates.ToString());
            if ((UserAgentId == null) || (UserAgentId.Length == 0))
            {
                sb.AppendLine("No UserAgentId defined");
            }
            else
            {
                for (i=0; i<UserAgentId.Length; i++)
                    sb.AppendFormat("UserAgendId({0}): {1}\n", i+1, UserAgentId[i]);
            }
            if ((UserAgentString == null) || (UserAgentString.Length == 0))
            {
                sb.AppendLine("No UserAgentString defined");
            }
            else
            {
                for (i = 0; i < UserAgentString.Length; i++)
                    sb.AppendFormat("UserAgentString({0}): {1}\n", i+1, UserAgentString[i]);
            }
            sb.AppendFormat("{0}: {1}\n", "EmailAddress", EmailAddress.ToString());
            sb.AppendFormat("{0}: {1}\n", "EmailSummary", EmailSummary.ToString());
            sb.AppendFormat("{0}: {1}\n", "SMTPServer", SMTPServer.ToString());
            sb.AppendFormat("{0}: {1}\n", "EmailReturnAddress", EmailReturnAddress.ToString());
            sb.AppendFormat("{0}: {1}\n", "EmailReturnDisplayName", EmailReturnDisplayName.ToString());

            return sb.ToString();
        }
    }
}
