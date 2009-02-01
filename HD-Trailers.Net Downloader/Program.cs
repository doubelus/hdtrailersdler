using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sloppycode.net;
using System.Net;
using System.IO;
using System.Collections.Specialized;
using System.Configuration;

namespace HDTrailersNETDownloader
{
    class Program
    {
        #region setup variables
        static RssItems feedItems;
        static string[] QualityPreference;
        static string CurrentQualityPreference;
        static string DownloadFolder;
        static string AllorToday;
        static bool GrabPoster;
        static bool CreateFolder;
        static bool VerboseLogging;
        static bool PhysicalLog;
        static bool PauseWhenDone;
        static FileStream logFS;
        static StreamWriter sw;
        #endregion

        static void Main(string[] args)
        {
            //Load config values
            Config_Load();
            if (VerboseLogging)
            {
                WriteLog("Config loaded");
                string tString = "Quality Preference: " + QualityPreference[0];
                for (int i = 1; i < QualityPreference.Length; i++)
                    tString = tString + "," + QualityPreference[i];
                WriteLog(tString);
            }


            feedItems = GetFeedItems(@"http://www.hd-trailers.net/blog/feed/");

            if (VerboseLogging)
                WriteLog("RSS feed items (" + feedItems.Count.ToString() + ") grabbed successfully");

            for (int i = 0; i < feedItems.Count; i++)
            {
                //Add code to select day
                //if (Convert.ToDateTime(feedItems[i].Pubdate) > DateTime.Now.AddDays(-15))
                //{
                WriteLog("");
                WriteLog("Next trailer is : " + feedItems[i].Title);

                    //Will come back null if preferred quality is not available
                    string tempTrailerURL = GetDownloadURL(feedItems[i].Link, QualityPreference);

                    if (tempTrailerURL != null)
                    {
                        //if(GrabPoster)
                        //{
                        //    string tempPosterURL = GetPosterURL(feedItems[i].Link);
                        //}
                        bool tempBool;

                        if (VerboseLogging)
                        {
                            WriteLog("Extracted download url (" + CurrentQualityPreference + ") for " + feedItems[i].Title + ":");
                            WriteLog(tempTrailerURL);
                        }

                        
                        if (CreateFolder)
                        {
                            bool tempDirectoryCreated = false;

                            if (!Directory.Exists(DownloadFolder + @"\" + feedItems[i].Title.Replace(":", "")))
                            {
                                tempDirectoryCreated = true;
                                Directory.CreateDirectory(DownloadFolder + @"\" + feedItems[i].Title.Replace(":", ""));
                            }

                            tempBool = GetTrailer(tempTrailerURL, feedItems[i].Title, DownloadFolder + @"\" + feedItems[i].Title.Replace(":", "") + @"\");

                            //Delete the directory if it didn't download
                            if (tempBool == false && tempDirectoryCreated == true)
                                Directory.Delete(DownloadFolder + @"\" + feedItems[i].Title.Replace(":", ""));

                        }
                        else
                            tempBool = GetTrailer(tempTrailerURL, feedItems[i].Title.Replace(":", ""), DownloadFolder + @"\");

                        if (tempBool)
                            WriteLog(feedItems[i].Title + " (" + CurrentQualityPreference + ") : Downloaded");
                    }
                    else
                        WriteLog("Preferred quality not available. Skipping...");
                //}
            }
            
            
            WriteLog("Done");

            if(PauseWhenDone)
                Console.ReadLine();

        }

        static RssItems GetFeedItems(string url)
        {

            RssReader reader = new RssReader();
            RssFeed feed = reader.Retrieve(url);

            return feed.Items;

        }

        //Pass in the blog post link and desired quality (480p, 720p, 1080p)
        static string GetDownloadURL(string link, string[] quality)
        {
            #region DumpHTMLSourceToString
            HttpWebRequest site = (HttpWebRequest)WebRequest.Create(link);
            HttpWebResponse response = (HttpWebResponse)site.GetResponse();
            Stream dataStream = response.GetResponseStream();
            StreamReader read = new StreamReader(dataStream);
            String data = read.ReadToEnd();
            #endregion


            #region ExtractDownloadURLsFromSource
            try
            {
                string tempString = data.Substring(data.IndexOf(@"<p><strong>Download</strong>:") + 30, data.IndexOf(@"</p>", data.IndexOf(@"<p><strong>Download</strong>:")) - data.IndexOf(@"<p><strong>Download</strong>:") - 30);

                // Sample link: [0] = "<a href=\"http://movies.apple.com/movies/magnolia_pictures/twolovers/twolovers-clip_h480p.mov\">480p</a>"
                string[] tempStringArray = tempString.Split(new Char[] { ',' });

                NameValueCollection nvc = new NameValueCollection(tempStringArray.Length);

                for (int i = 0; i < tempStringArray.Length; i++)
                {
                    nvc.Add(tempStringArray[i].Substring(tempStringArray[i].IndexOf(">") +1,(tempStringArray[i].IndexOf(@"</a>") - tempStringArray[i].IndexOf(">") - 1)),
                        tempStringArray[i].Substring(tempStringArray[i].IndexOf("http"), tempStringArray[i].IndexOf("\">") - tempStringArray[i].IndexOf("http")));

                }


            #endregion

            string tempString2 = null;
            //Need a loop here to pick highest priority quality. 
            for (int i = 0; i < quality.Length; i++)
            {
                //Does a trailer of the preferred quality exist? If so, set it.. if not, try the next one
               tempString2 = nvc.Get(quality[i]).Replace(@"amp;","");
               CurrentQualityPreference = quality[i];
                
                //If you find one with the proper key, jump out of the for-loop
               if (tempString2 != null)
                   i = quality.Length;

            }
            return tempString2;

            }
            catch
            {
                WriteLog("Something is weird with this one... check source");
                return null;
            }


        }

        static string GetPosterURL(string link)
        {
            #region DumpHTMLSourceToString
            HttpWebRequest site = (HttpWebRequest)WebRequest.Create(link);
            HttpWebResponse response = (HttpWebResponse)site.GetResponse();
            Stream dataStream = response.GetResponseStream();
            StreamReader read = new StreamReader(dataStream);
            String data = read.ReadToEnd();
            #endregion


            #region ExtractPosterURLFromSource
            //figure out how to parse this
            string tempString = data.Substring(data.IndexOf("margin-left: 10px; margin-right: 10px;\" src=\""), 50);


            #endregion

            return tempString;


        }

        static void Config_Load()
        {

            //Load our config
            // Get the AppSettings section.

            QualityPreference = ConfigurationManager.AppSettings["QualityPreference"].Split(new Char[] { ',' });
            DownloadFolder = ConfigurationManager.AppSettings["DownloadFolder"];
            AllorToday = ConfigurationManager.AppSettings["AllorToday"];
            GrabPoster = Convert.ToBoolean(ConfigurationManager.AppSettings["GrabPoster"]);
            CreateFolder = Convert.ToBoolean(ConfigurationManager.AppSettings["CreateFolder"]);
            VerboseLogging = Convert.ToBoolean(ConfigurationManager.AppSettings["VerboseLogging"]);
            PhysicalLog = Convert.ToBoolean(ConfigurationManager.AppSettings["PhysicalLog"]);
            PauseWhenDone = Convert.ToBoolean(ConfigurationManager.AppSettings["PauseWhenDone"]);

            if (PhysicalLog)
            {
                
                
                if (!File.Exists("HD-Trailers.NET Downloader.log"))
                    logFS = new FileStream("HD-Trailers.NET Downloader.log",FileMode.Create);
                else
                    logFS = new FileStream("HD-Trailers.NET Downloader.log",FileMode.Open);

                sw = new StreamWriter(logFS);

                
            }



        }

        static bool GetTrailer(string downloadURL, string trailerTitle, string downloadPath)
        {
            if (VerboseLogging)
                WriteLog("DownloadPath = " + downloadPath);

            bool tempBool = false;
            trailerTitle = trailerTitle.Replace(":", "");

            //Make this work for .WMV and .MOV. Add more later as needed
            string fileName;
                
            if(downloadURL.Contains(".wmv"))
                fileName = trailerTitle + "_" + CurrentQualityPreference + ".wmv";
            else if(downloadURL.Contains(".zip"))
                fileName = trailerTitle + "_" + CurrentQualityPreference + ".zip";
            else
                fileName = trailerTitle + "_" + CurrentQualityPreference + ".mov";

            
            if (!File.Exists(downloadPath + @"\" + fileName))
            {
    
                    using (WebClient Client = new WebClient())
                    {
                        if (VerboseLogging)
                            WriteLog("Grabbing trailer: " + trailerTitle);

                        if(downloadURL.Contains("yahoo"))
                        {
                            Client.Headers.Add("Referer","http://movies.yahoo.com/");                            
                        }

                        Client.DownloadFile(downloadURL, downloadPath + @"\" + fileName);

                        if (VerboseLogging)
                            WriteLog("Grab successful");
                    }

                    
                    tempBool = true;
                
                
            }
            else
            {
                tempBool = false;
                WriteLog("File already exists! Skipping...");
            }
            return tempBool;
            
        }

        static void GetPoster(string downloadURL, string trailerTitle, string downloadPath)
        {
            using (WebClient Client = new WebClient())
            {
                if (VerboseLogging)
                    WriteLog("Grabbing poster: " + trailerTitle);

                Client.DownloadFile(downloadURL, downloadPath + @"\folder.jpg");

                if (VerboseLogging)
                    WriteLog("Grab successful");
            }

        }

        static void WriteLog(string text)
        {
            if (text != "")
            {
                Console.WriteLine(DateTime.Now.ToShortTimeString() + " - " + text);

                if(PhysicalLog)                    
                    sw.WriteLine(DateTime.Now.ToShortTimeString() + " - " + text);
            }
            else
            {
                Console.WriteLine();

                if(PhysicalLog)
                    sw.WriteLine();
            }


        }
    }
}
