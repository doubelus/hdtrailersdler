using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sloppycode.net;
using System.Net;
using System.IO;
using System.Collections.Specialized;
using System.Collections;
using System.Net.Mail;
using System.Globalization;


namespace HDTrailersNETDownloader
{
    class Program
    {

        static Config config = new Config();
        static Logging log = new Logging();
        static ArrayList Exclusions = new ArrayList();

        static string pathsep = Path.DirectorySeparatorChar.ToString();
        static string MailBody;
        static string Version = "HD-Trailers.Net Downloader v.89 BETA";



        /// <summary>
        /// read data from appconfig. configure the logging object according to the appconfig information
        /// </summary>
        static void Init()
        {
            config.Init();
            log.Init(config.VerboseLogging, config.PhysicalLog);

            log.VerboseWrite("Config loaded");
            log.VerboseWrite(config.Info());

            if (config.UseExclusions)
            {
                log.VerboseWrite("Using exclusions...");

                Exclusions = ReadExclusions();

                log.VerboseWrite(Exclusions.Count.ToString() + " exclusions loaded.");
            }
            else
            {
                log.VerboseWrite("Not using exclusions...");
            }
            log.WriteLine("");
        }

        static ArrayList ReadExclusions()
        {
            ArrayList exclusions;

            //We are using exclusions. Load into arraylist or create empty arraylist
            if (File.Exists("HD-Trailers.Net Downloader Exclusions.xml"))
            {
                System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(ArrayList));
                System.IO.TextReader reader =
                  new System.IO.StreamReader("HD-Trailers.Net Downloader Exclusions.xml");
                exclusions = (ArrayList)serializer.Deserialize(reader);
                reader.Close();
            }
            else
                exclusions = new ArrayList();

            return exclusions;
        }

        static void WriteExclusions(ArrayList exclusions)
        {
            System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(ArrayList));
            System.IO.TextWriter writer = new System.IO.StreamWriter("HD-Trailers.Net Downloader Exclusions.xml", false);
            serializer.Serialize(writer, exclusions);
            writer.Close();
        }

        static void DeleteEm()
        {
            //Delete old trailers. If KeepFor = 0 ignore
            if (config.KeepFor > 0)
            {
                try
                {
                    log.WriteLine("Delete option selected. Deleting files/folders older than: " + config.KeepFor.ToString() + " days");
                    string[] dirList = (string[])Directory.GetDirectories(config.TrailerDownloadFolder);

                    for (int i = 0; i < dirList.Length; i++)
                    {
                        if ((Directory.GetCreationTime(dirList[i]).AddDays(config.KeepFor)) < DateTime.Now)
                        {
                            Directory.Delete(dirList[i], true);
                            if (config.VerboseLogging)
                                log.WriteLine("Deleted directory: " + dirList[i]);
                        }
                    }

                    string[] fileList = (string[])Directory.GetFiles(config.TrailerDownloadFolder);
                    for (int i = 0; i < fileList.Length; i++)
                    {
                        if ((File.GetCreationTime(fileList[i]).AddDays(config.KeepFor)) < DateTime.Now && !fileList[i].Contains("folder"))
                        {
                            File.Delete(fileList[i]);
                            if (config.VerboseLogging)
                                log.WriteLine("Deleted file: " + fileList[i]);
                        }
                    }
                }
                catch (Exception e)
                {
                    log.WriteLine("Error deleting subdirectories");
                    log.WriteLine("Exception: " + e.ToString());
                }
            }
            return;
        }

        static void ProcessFeedItem(string title, string link)
        {
            string qualPreference = "";

            log.WriteLine("");
            log.WriteLine("Next trailer: " + title);

            NameValueCollection nvc;
            nvc = GetDownloadUrls(link);
            if ((nvc == null) || (nvc.Count == 0))
            {
                log.WriteLine("Error: No Download URLs found. Skipping...");
                return;
            }

            if (config.VerboseLogging)
            {
                for (int j = 0; j < nvc.Count; j++)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendFormat("    {0,-10} {1}", nvc.GetKey(j), nvc.Get(j));
                    log.VerboseWrite(sb.ToString());
                }
            }

            string tempTrailerURL = GetPreferredURL(nvc, config.QualityPreference, ref qualPreference);

            if (tempTrailerURL == null)
            {
                log.WriteLine("Preferred quality not available. Skipping...");
                AddToEmailSummary(title + " (" + qualPreference + ") : Not available. Skipping...");
                return;
            }
            if (Exclusions.Contains(title))
            {
                log.WriteLine("Title found in exclusions list. Skipping...");
                AddToEmailSummary(title + " (" + qualPreference + ") : Title found in exclusions list. Skipping...");
                return;
            }
            if ((config.TrailerOnly) && (!title.Contains("Trailer")))
            {
                log.WriteLine("Title not a trailer. Skipping...");
                AddToEmailSummary(title + " (" + qualPreference + ") : Title not a trailer. Skipping...");
                return;
            }

            bool tempBool;
            string directory;
            string fname;
            string posterUrl = nvc["poster"];

            bool tempDirectoryCreated = false;

            if (config.CreateFolder)
                directory = config.TrailerDownloadFolder + pathsep + title.Replace(":", "").Replace("?", "");
            else
                directory = config.TrailerDownloadFolder;
            fname = title.Replace(":", "").Replace("?", "");

            log.VerboseWrite("Extracted download url (" + qualPreference + ") for " + title + ": " + tempTrailerURL);

            if ((config.CreateFolder) && (!Directory.Exists(directory)))
            {
                tempDirectoryCreated = true;
                Directory.CreateDirectory(directory);
            }
            tempBool = GetOrResumeTrailer(tempTrailerURL, fname, directory + pathsep, qualPreference, posterUrl);

            //Delete the directory if it didn't download
            if (tempBool == false && tempDirectoryCreated == true)
                Directory.Delete(directory);

            //If download went ok, and we're using exclusions, add to list
            if (tempBool && config.UseExclusions)
            {
                Exclusions.Add(title);
                log.VerboseWrite("Exclusion added");
            }

            if (tempBool)
            {
                log.WriteLine(title + " (" + qualPreference + ") : Downloaded");
                AddToEmailSummary(title + " (" + qualPreference + ") : Downloaded");
            }
        }

        static void Main(string[] args)
        {
            RssItems feedItems;

            log.WriteLine(Version);
            log.WriteLine("CodePlex: http://www.codeplex.com/hdtrailersdler");
            log.WriteLine("By Brian Charbonneau - blog: http://www.brianssparetime.com");
            log.WriteLine("Please visit http://www.hd-trailers.net for archives");
            log.WriteLine("");

            Init();

            //Delete folders/files if needed
            DeleteEm();

            feedItems = GetFeedItems(@"http://www.hd-trailers.net/blog/feed/");
            log.VerboseWrite("RSS feed items (" + feedItems.Count.ToString() + ") grabbed successfully");

            try
            {
                for (int i = 0; i < feedItems.Count; i++)
                {
                    ProcessFeedItem(feedItems[i].Title, feedItems[i].Link);
                }
            }
            catch (Exception e)
            {
                log.WriteLine("ERROR: " + e.Message);
            }

            //Do housekeeping like serializing exclusions and sending email summary
            log.VerboseWrite("");
            log.VerboseWrite("Housekeeping:");

            if (config.UseExclusions)
            {
                //We're using exclusions... write to file for next run
                log.VerboseWrite("");
                log.VerboseWrite("Serializing exclusion list...");
                
                WriteExclusions(Exclusions);

                log.VerboseWrite("Serialization complete.");
            }

            if (config.EmailSummary)
            {
                log.VerboseWrite("");
                log.VerboseWrite("Sending email summary...");
                               
                SendEmailSummary();

                log.VerboseWrite("Email summary sent.");
            }
            
            log.WriteLine("Done");

            if(config.PauseWhenDone)
                Console.ReadLine();
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
                log.WriteLine("ERROR: Could not get feed. Exception to follow.");
                log.WriteLine(e.Message);

                return null;
            }
        }

        static string ReadDataFromLink(string link)
        {
            try
            {
                HttpWebRequest site = (HttpWebRequest)WebRequest.Create(link);
                HttpWebResponse response = (HttpWebResponse)site.GetResponse();
                Stream dataStream = response.GetResponseStream();
                StreamReader read = new StreamReader(dataStream);
                String data = read.ReadToEnd();

                read.Close();
                dataStream.Close();
                response.Close();
                site.Abort();

                return data;
            }
            catch (Exception e)
            {
                log.WriteLine("Exception in ReadDataFromLink (" + link + ")");
                log.WriteLine(e.ToString());
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="link"></param>
        /// <param name="quality"></param>
        /// <returns></returns>
        static NameValueCollection GetDownloadUrls(string link)
        {
            try
            {
                NameValueCollection nvc = new NameValueCollection();

                //Set data to CurrentSource.. will use to pull poster
                string data = ReadDataFromLink(link);
                // CurrentSource = data;

                int pos = data.IndexOf(@"Download</strong>:");
                if (pos == -1)
                    return nvc;

                // find the urls for the movies, extract the string following "Download:
                string tempString = data.Substring(pos + 18);
                // find the end of the screen line (a </p> or a <br />)
                string[] tempStringArray = tempString.Split(new string[] { @"</p>", @"<br" }, StringSplitOptions.None);
                tempString = tempStringArray[0];

                // extract all the individual links from the tempString
                // Sample link: [0] = "<a href=\"http://movies.apple.com/movies/magnolia_pictures/twolovers/twolovers-clip_h480p.mov\">480p</a>"
                tempStringArray = tempString.Split(new Char[] { ',' });

                for (int i = 0; i < tempStringArray.Length; i++)
                {
                    string s1 = tempStringArray[i].Substring(tempStringArray[i].IndexOf(">") + 1, (tempStringArray[i].IndexOf(@"</a>") - tempStringArray[i].IndexOf(">") - 1));
                    string s2 = tempStringArray[i].Substring(tempStringArray[i].IndexOf("http"), tempStringArray[i].IndexOf("\">") - tempStringArray[i].IndexOf("http"));

                    nvc.Add(s1, s2);
                }


                // now find the poster url
                // look for first 'Link to Catalog' then pick the src attribute from the first img tag

                tempString = data.Substring(data.IndexOf("<strong>Link to Catalog</strong>"));
                tempString = tempString.Substring(tempString.IndexOf("<img "));
                tempString = tempString.Substring(tempString.IndexOf("src=\"") + 5);
                tempString = tempString.Substring(0, tempString.IndexOf("\""));

                nvc.Add("poster", tempString);
                return nvc;

            }
            catch (Exception e)
            {
                log.WriteLine("Exception in GetDownloadUrls (" + link + ")");
                log.WriteLine(e.ToString());
                return null;
            }
        }

        static string GetPreferredURL(NameValueCollection nvc, string[] quality, ref string qualPref)
        {
            try 
            {
                string tempString2 = null;
                //Need a loop here to pick highest priority quality. 
                for (int i = 0; i < quality.Length; i++)
                {
                    //Does a trailer of the preferred quality exist? If so, set it.. if not, try the next one
                   tempString2 = nvc.Get(quality[i]);
                   if (tempString2 != null)
                   {
                       tempString2 = nvc.Get(quality[i]).Replace(@"amp;", "");
                       qualPref = quality[i];

                       //If you find one with the proper key, jump out of the for-loop
                       if (tempString2 != null)
                           i = quality.Length;
                   }
                }

                return tempString2;

            }
            catch (Exception e)
            {
                log.WriteLine("ERROR: Something is weird with this one... check source");
                log.WriteLine(e.ToString());
                AddToEmailSummary("ERROR: Something is weird with this one... check source");
                return null;
            }
        }


        static bool GetOrResumeTrailer(string downloadURL, string trailerTitle, string downloadPath, string qualPref, string posterUrl)
        {
            HttpWebRequest myWebRequest;
            HttpWebResponse myWebResponse;
            bool tempBool = false;

            int StartPointInt;

            log.VerboseWrite("Trailer DownloadPath = " + downloadPath);

            trailerTitle = trailerTitle.Replace(":", "").Replace("?", "");

            //Make this work for .WMV and .MOV. Add more later as needed
            string fileName;
            string upperDownloadUrl = downloadURL.ToUpper();

            if (upperDownloadUrl.Contains(".WMV"))
                fileName = trailerTitle + "_" + qualPref + ".wmv";
            else if (upperDownloadUrl.Contains(".ZIP"))
                fileName = trailerTitle + "_" + qualPref + ".zip";
            else
                fileName = trailerTitle + "_" + qualPref + ".mov";

            if (File.Exists(downloadPath + pathsep + fileName))
            {
                FileInfo fi = new FileInfo(downloadPath + pathsep + fileName);
                StartPointInt = Convert.ToInt32(fi.Length);
            }
            else
                StartPointInt = 0;

            myWebRequest = (HttpWebRequest)WebRequest.Create(downloadURL);
            if (upperDownloadUrl.Contains("APPLE.COM"))
            {
                myWebRequest.UserAgent = config.AppleUserAgent;
            }

            myWebResponse = (HttpWebResponse)myWebRequest.GetResponse();

            // Ask the server for the file size and store it
            int fileSize = Convert.ToInt32(myWebResponse.ContentLength);

            myWebResponse.Close();
            myWebRequest.Abort();

            if ((config.MinTrailerSize > 0) && (fileSize < config.MinTrailerSize))
            {
                log.WriteLine("Trailer size smaller then MinTrailerSize. Skipping ...");
                return false;
            }


            if (StartPointInt < fileSize)
            {
                Stream strResponse;
                FileStream strLocal;

                // Create a request to the file we are downloading
                myWebRequest = (HttpWebRequest)WebRequest.Create(downloadURL);
                myWebRequest.Credentials = CredentialCache.DefaultCredentials;
                if (upperDownloadUrl.Contains("APPLE.COM"))
                {
                    myWebRequest.UserAgent = config.AppleUserAgent;
                }

                if (StartPointInt > 0)
                    myWebRequest.AddRange(StartPointInt, fileSize);
                
                // Retrieve the response from the server
                myWebResponse = (HttpWebResponse)myWebRequest.GetResponse();

                // Open the URL for download
                strResponse = myWebResponse.GetResponseStream();

                // Create a new file stream where we will be saving the data (local drive)
                if (StartPointInt == 0)
                {
                    strLocal = new FileStream(downloadPath + pathsep + fileName, FileMode.Create, FileAccess.Write, FileShare.None);
                }
                else
                {
                    if (myWebResponse.StatusCode == HttpStatusCode.PartialContent)
                        log.WriteLine(StartPointInt.ToString() + " bytes of " + Convert.ToInt32(fileSize).ToString() + " located on disk. Resuming...");
                    else
                        log.WriteLine(StartPointInt.ToString() + " bytes of " + Convert.ToInt32(fileSize).ToString() + " located on disk. Server will not resume!!");

                    strLocal = new FileStream(downloadPath + pathsep + fileName, FileMode.Append, FileAccess.Write, FileShare.None);
                }

                // It will store the current number of bytes we retrieved from the server
                int bytesSize = 0;

                // A buffer for storing and writing the data retrieved from the server
                byte[] downBuffer = new byte[65536];


                // Loop through the buffer until the buffer is empty
                while ((bytesSize = strResponse.Read(downBuffer, 0, downBuffer.Length)) > 0)
                {
                    // Write the data from the buffer to the local hard drive
                    strLocal.Write(downBuffer, 0, bytesSize);
                    StartPointInt += bytesSize;
                    
                    double t = ((double)StartPointInt) / fileSize;
                    log.ConsoleWrite(t.ToString("###.0%\r", CultureInfo.InvariantCulture));
                }                // When the above code has ended, close the streams

                strResponse.Close();
                strLocal.Close();
                myWebResponse.Close();

                tempBool = true;
            }
            else if (StartPointInt == Convert.ToInt32(fileSize))
            {
                tempBool = true;
                log.WriteLine("File exists and is same size. Skipping...");
            }
            else
            {
                tempBool = false;
                log.WriteLine("Something else is wrong.. size on disk is greater than size on web.");
            }

            //Assuming we downloaded the trailer OK and the config has been set to grab posters...
            if ( (tempBool) && (config.GrabPoster))
                GetPoster(posterUrl, downloadPath);

            return tempBool;
        }

        static void GetPoster(string source, string downloadPath)
        {
            try
            {
                string fname = downloadPath + pathsep +@"folder.jpg";

                if ((source == null) || (source.Length == 0))
                {
                    log.VerboseWrite("No poster url found. Skipping....");
                    return;
                }
                if (File.Exists(fname))
                {
                    log.VerboseWrite("Poster already downloaded. Skipping ...");
                    return;
                }

                using (WebClient Client = new WebClient())
                {
                    //Now get the actual poster
                    log.VerboseWrite("Grabbing poster... ");

                    Client.DownloadFile(source, fname);

                    log.VerboseWrite("Poster grab successful");
                }
                return;
            }

            catch (Exception e)
            {
                log.WriteLine("ERROR: Could not grab poster.. exception to follow:");
                log.WriteLine(e.Message);
            }
        }
    
        


        static void AddToEmailSummary(string text)
        {
            MailBody = MailBody + "\r\n" + text;
        }

        static void SendEmailSummary()
        {
            try
            {
                // To
                MailMessage mailMsg = new MailMessage();
                mailMsg.To.Add(config.EmailAddress);

                // From
                MailAddress mailAddress = new MailAddress("fake_addr@trailers.com","HD-Trailer.NET Downloader");
                mailMsg.From = mailAddress;

                // Subject and Body
                mailMsg.Subject = Version + " Download Summary for " + DateTime.Now.ToShortDateString();
                mailMsg.Body = MailBody;

                // Init SmtpClient and send
                SmtpClient smtpClient = new SmtpClient(config.SMTPServer, 25);
                //System.Net.NetworkCredential credentials = new System.Net.NetworkCredential(args[5], args[5]);
                //smtpClient.Credentials = credentials;

                smtpClient.Send(mailMsg);
            }
            catch (Exception ex)
            {
                log.WriteLine("ERROR: " + ex.Message);
            }



        }
    }
}
