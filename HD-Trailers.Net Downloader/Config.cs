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
        public string AppleUserAgent { get; private set; }


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

        public void Init()
        {
            //Load our config
            // Get the AppSettings section.
            NameValueCollection appSetting = ConfigurationManager.AppSettings;

            this.QualityPreference = (GetStringFromAppsettings(appSetting, "QualityPreference", "720p,480p")).Split(new Char[] { ',' });
            this.TrailerDownloadFolder = GetStringFromAppsettings(appSetting, "TrailerDownloadFolder", "c:/Trailers");
            this.MetadataDownloadFolder = GetStringFromAppsettings(appSetting, "MetadataDownloadFolder", "c:/Trailers");
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

            this.AppleUserAgent = GetStringFromAppsettings(appSetting, "AppleUserAgent", "QuickTime/7.6.2");
        }

        public string Info()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine();
            sb.Append("QualityPreference: ");
            for (int i = 0; i < QualityPreference.Length; i++)
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
            sb.AppendFormat("{0}: {1}\n", "EmailAddress", EmailAddress.ToString());
            sb.AppendFormat("{0}: {1}\n", "EmailSummary", EmailSummary.ToString());
            sb.AppendFormat("{0}: {1}\n", "SMTPServer", SMTPServer.ToString());

            return sb.ToString();
        }
    }
}
