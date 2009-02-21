using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sloppycode.net;
using System.Net;
using System.IO;
using System.Collections.Specialized;
using System.Configuration;
using System.Collections;

namespace HDTrailersNETDownloader
{
    class Program
    {
        #region setup variables
        static RssItems feedItems;
        static string[] QualityPreference;
        static string CurrentQualityPreference;
        static string CurrentSource;
        static string DownloadFolder;
        static string AllorToday;
        static bool GrabPoster;
        static bool CreateFolder;
        static bool VerboseLogging;
        static bool PhysicalLog;
        static bool PauseWhenDone;
        static bool UseExclusions;
        static ArrayList Exclusions = new ArrayList();
        static int KeepFor;
        static FileStream logFS;
        static StreamWriter sw;
        static string pathsep = Path.DirectorySeparatorChar.ToString();

        #endregion

        static void Main(string[] args)
        {
            WriteLog("HD-Trailers.Net Downloader v.81 BETA");
            WriteLog("CodePlex: http://www.codeplex.com/hdtrailersdler");
            WriteLog("By Brian Charbonneau - blog: http://www.brianssparetime.com");
            WriteLog("Please visit http://www.hd-trailers.net for archives");
            WriteLog("");

         
            
            //Load config values
            Config_Load();





            //Delete folders/files if needed
            DeleteEm();

            feedItems = GetFeedItems(@"http://www.hd-trailers.net/blog/feed/");

            if (VerboseLogging)
                WriteLog("RSS feed items (" + feedItems.Count.ToString() + ") grabbed successfully");

            try
            {
                for (int i = 0; i < feedItems.Count; i++)
                {
                    //Add code to select day
                    //if (Convert.ToDateTime(feedItems[i].Pubdate) > DateTime.Now.AddDays(-15))
                    //{
                    WriteLog("");
                    WriteLog("Next trailer (" + Convert.ToString(i + 1) + ") is : " + feedItems[i].Title);

                    //Will come back null if preferred quality is not available
                    string tempTrailerURL = GetDownloadURL(feedItems[i].Link, QualityPreference);

                    if (tempTrailerURL != null && !Exclusions.Contains(feedItems[i].Title))
                    {

                        bool tempBool;

                        if (VerboseLogging)
                        {
                            WriteLog("Extracted download url (" + CurrentQualityPreference + ") for " + feedItems[i].Title + ":");
                            WriteLog(tempTrailerURL);
                        }


                        if (CreateFolder)
                        {
                            bool tempDirectoryCreated = false;

                            if (!Directory.Exists(DownloadFolder + pathsep + feedItems[i].Title.Replace(":", "")))
                            {
                                tempDirectoryCreated = true;
                                Directory.CreateDirectory(DownloadFolder + pathsep + feedItems[i].Title.Replace(":", ""));
                            }

                            tempBool = GetTrailer(tempTrailerURL, feedItems[i].Title, DownloadFolder + pathsep + feedItems[i].Title.Replace(":", "") + pathsep);



                            //Assuming we downloaded the trailer OK and the config has been set to grab posters...
                            if (tempBool && GrabPoster)
                                GetPoster(CurrentSource, DownloadFolder + pathsep + feedItems[i].Title.Replace(":", "") + pathsep);

                            //Delete the directory if it didn't download
                            if (tempBool == false && tempDirectoryCreated == true)
                                Directory.Delete(DownloadFolder + pathsep + feedItems[i].Title.Replace(":", ""));

                        }
                        else
                        {
                            //Uncomment and remove when done debugging
                            tempBool = GetTrailer(tempTrailerURL, feedItems[i].Title.Replace(":", ""), DownloadFolder + pathsep);
                            //tempBool = true;
                            //Assuming we downloaded the trailer OK and the config has been set to grab posters...
                            if (tempBool && GrabPoster)
                                GetPoster(CurrentSource, DownloadFolder + pathsep);

                        }

                        //If download went ok, and we're using exclusions, add to list
                        if (tempBool && UseExclusions)
                        {
                            Exclusions.Add(feedItems[i].Title);
                            if (VerboseLogging)
                                WriteLog("Exclusion added");
                        }

                        if (tempBool)
                            WriteLog(feedItems[i].Title + " (" + CurrentQualityPreference + ") : Downloaded");

                    }
                    else
                    {
                        if (tempTrailerURL == null)
                            WriteLog("Preferred quality not available. Skipping...");
                        else if (Exclusions.Contains(feedItems[i].Title))
                            WriteLog("Title found in exclusions list. Skipping...");

                    }
                    //}
                }
            }
            catch (Exception e)
            {
                WriteLog("ERROR: " + e.Message);
                sw.Dispose();

            }

            if (UseExclusions)
            {
                WriteLog("");
                //We're using exclusions... write to file for next run
                if(VerboseLogging)
                    WriteLog("Serializing exclusion list...");
                WriteExclusions();

                if(VerboseLogging)
                    WriteLog("Serialization Complete.");
            }
            
            WriteLog("Done");

            if(PauseWhenDone)
                Console.ReadLine();

            sw.Dispose();

        }

        static void DeleteEm()
        {
            //Delete old trailers. If KeepFor = 0 ignore
            if (KeepFor > 0)
            {
                WriteLog("Delete option selected. Deleting files/folders older than: " + KeepFor.ToString() + " days");
                string[] dirList = (string[])Directory.GetDirectories(DownloadFolder);

                for (int i = 0; i < dirList.Length; i++)
                {
                    if ((Directory.GetCreationTime(dirList[i]).AddDays(KeepFor)) < DateTime.Now)
                    {
                        Directory.Delete(dirList[i], true);
                        if (VerboseLogging)
                            WriteLog("Deleted directory: " + dirList[i]);
                    }

                }

                string[] fileList = (string[])Directory.GetFiles(DownloadFolder);
                for (int i = 0; i < fileList.Length; i++)
                {
                    if ((File.GetCreationTime(fileList[i]).AddDays(KeepFor)) < DateTime.Now && !fileList[i].Contains("folder"))
                    {
                        File.Delete(fileList[i]);
                        if (VerboseLogging)
                            WriteLog("Deleted file: " + fileList[i]);
                    }
                }
            }
        }

        static RssItems GetFeedItems(string url)
        {
            try
            {

                RssReader reader = new RssReader();
                RssFeed feed = reader.Retrieve(url);

                return feed.Items;
               
            }
            catch (Exception e)
            {
                WriteLog("ERROR: Could not get feed. Exception to follow.");
                WriteLog(e.Message);

                return null;

            }
            

        }

        
        static string GetDownloadURL(string link, string[] quality)
        {
            #region DumpHTMLSourceToString
            HttpWebRequest site = (HttpWebRequest)WebRequest.Create(link);
            HttpWebResponse response = (HttpWebResponse)site.GetResponse();
            Stream dataStream = response.GetResponseStream();
            StreamReader read = new StreamReader(dataStream);
            String data = read.ReadToEnd();

            //Set data to CurrentSource.. will use to pull poster
            CurrentSource = data;
            #endregion


            #region ExtractDownloadURLsFromSource
            try
            {
                string tempString = data.Substring(data.IndexOf(@"Download</strong>:") + 18, data.IndexOf(@"</p>", data.IndexOf(@"Download</strong>:")) - data.IndexOf(@"Download</strong>:") - 18);

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
            KeepFor = Convert.ToInt32(ConfigurationManager.AppSettings["KeepFor"]);
            UseExclusions = Convert.ToBoolean(ConfigurationManager.AppSettings["UseExclusions"]);
            

            if (PhysicalLog)
            {   
                if (!File.Exists("HD-Trailers.NET Downloader.log"))
                    logFS = new FileStream("HD-Trailers.NET Downloader.log",FileMode.Create);
                else
                    logFS = new FileStream("HD-Trailers.NET Downloader.log",FileMode.Append);

                sw = new StreamWriter(logFS);                
            }


            if (VerboseLogging)
            {
                WriteLog("Config loaded");
                string tString = "Quality Preference: " + QualityPreference[0];
                for (int i = 1; i < QualityPreference.Length; i++)
                    tString = tString + "," + QualityPreference[i];
                WriteLog(tString);


            }

            if (UseExclusions)
            {
                if(VerboseLogging)
                WriteLog("Using exclusions...");
                
                GetExclusions();
                
                if(VerboseLogging)
                WriteLog(Exclusions.Count.ToString() + " exclusions loaded.");
            }
            else
            {
                if(VerboseLogging)
                WriteLog("Not using exclusions...");
            }
            WriteLog("");



        }

        static void WriteExclusions()
        {

            System.Xml.Serialization.XmlSerializer serializer =
              new System.Xml.Serialization.XmlSerializer(typeof(ArrayList));
            System.IO.TextWriter writer =
              new System.IO.StreamWriter("HD-Trailers.Net Downloader Exclusions.xml", false);
            serializer.Serialize(writer, Exclusions);
            writer.Close();
        }

        static void GetExclusions()
        {
            //We are using exclusions. Load into arraylist or create empty arraylist
            if(File.Exists("HD-Trailers.Net Downloader Exclusions.xml"))
            {
            System.Xml.Serialization.XmlSerializer serializer =
  new System.Xml.Serialization.XmlSerializer(typeof(ArrayList));
            System.IO.TextReader reader =
              new System.IO.StreamReader("HD-Trailers.Net Downloader Exclusions.xml");
            Exclusions = (ArrayList)serializer.Deserialize(reader);
            reader.Close();
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

            
            if (!File.Exists(downloadPath + pathsep + fileName))
            {

                try
                {
                    using (WebClient Client = new WebClient())
                    {
                        if (VerboseLogging)
                            WriteLog("Grabbing trailer: " + trailerTitle);

                        if (downloadURL.Contains("yahoo"))
                        {
                            Client.Headers.Add("Referer", "http://movies.yahoo.com/");
                        }

                        Client.DownloadFile(downloadURL, downloadPath + pathsep + fileName);

                        if (VerboseLogging)
                            WriteLog("Grab successful");
                    }


                    tempBool = true;
                }
                catch(Exception e)
                {
                    WriteLog("ERROR: " + e.Message);
                    tempBool = false;
                    return tempBool;
                }
                
                
            }
            else
            {
                tempBool = false;
                WriteLog("File already exists! Skipping...");
            }
            return tempBool;
            
        }

        static void GetPoster(string source, string downloadPath)
        {

            try
            {
 
                //First get Poster URL
                // Match this: margin-left: 10px; margin-right: 10px;" src="
                // + 45
                // Then poster URL ends on next "
                string tempStart = "margin-left: 10px; margin-right: 10px;\" src=\"";
                string tempEnd = "\"";

                
                int urlStart = source.IndexOf(tempStart) + 45;
                int urlEnd = source.IndexOf(tempEnd, (source.IndexOf(tempStart) + 45));
                int difference = urlEnd - urlStart;

                

                string tempString = source.Substring(source.IndexOf(tempStart) + 45, difference);


                //Now get the actual poster
                using (WebClient Client = new WebClient())
                {

                    if (VerboseLogging)
                        WriteLog("Grabbing poster... ");

                    Client.DownloadFile(tempString, downloadPath + @"folder.jpg");

                    if (VerboseLogging)
                        WriteLog("Poster grab successful");
                }
            }
            catch (Exception e)
            {
                WriteLog("ERROR: Could not grab poster.. exception to follow:");
                WriteLog(e.Message);
            }

        }

        static void WriteLog(string text)
        {
            try
            {
                if (text != "")
                {
                    Console.WriteLine(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + " - " + text);

                    if (PhysicalLog)
                        sw.WriteLine(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + " - " + text);
                }
                else
                {
                    Console.WriteLine();

                    if (PhysicalLog)
                        sw.WriteLine();
                }
            }
            catch (Exception e)
            { }


        }
    }
}
