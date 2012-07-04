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
using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic.FileIO;
using NDesk.Options;
using IMDb_Scraper;
using Nfo.Movie;
using Nfo.File;

namespace HDTrailersNETDownloader
{
    class Program
    {
        static Config config = new Config();
        static public Logging log = new Logging();
        static ArrayList Exclusions = new ArrayList();
        static IMDb imdb = new IMDb();
        static NfoMovie NFOTrailer = new NfoMovie();
        static NfoFile NFOTrailerFile = new NfoFile();
        static trailerFreakRSS itemsTF = new trailerFreakRSS();

//        private static string _assembly = null;
//        private static string _input = null;
//        private static string _output = null;

        private static int MAX_PATH = 260;

        static string pathsep = Path.DirectorySeparatorChar.ToString();
        static string MailBody;
//        static List<string> extra; 
        static string Version = "HD-Trailers.Net Downloader v2.0.11";
        static int NewTrailerCount = 0;
        [PreEmptive.Attributes.Setup(CustomEndpoint = "so-s.info/PreEmptive.Web.Services.Messaging/MessagingServiceV2.asmx")]
        [PreEmptive.Attributes.Teardown()]
        static void Main(string[] args)
        {
            bool help = false;
            var options = new OptionSet() 
                .Add("e|edit", e => EditConfigFile())
                .Add("?|h|help", h => DisplayHelp());
                
            options.Parse(args);
            
            try
            {
                if(help) {
                    DisplayHelp();
                }
                //               RssItems feedItems;
                string AppDatadirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "HD-Trailers.Net Downloader");
                if (!Init())
                    return;
                log.WriteLine(Version);
                log.WriteLine("CodePlex: http://www.codeplex.com/hdtrailersdler");
                log.WriteLine("HD Trailer Blog: http://www.hd-trailers.net");
                log.WriteLine("Program Icon: http://jamespeng.deviantart.com");
                log.WriteLine("C# IMDB Scraper: http://web3o.blogspot.com/2010/11/aspnetc-imdb-scraping-api.html");
                log.WriteLine("Program Icon: http://jamespeng.deviantart.com");
 //               log.WriteLine(Assembly.GetExecutingAssembly().GetName().Version);
                log.WriteLine("");

                log.WriteLine("CommonApplicateData: " + System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData));
                log.WriteLine("LocalApplicationData: " + Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
                log.WriteLine("LocalAppData: " + Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "HD-Trailers.Net Downloader"));

                if (!CheckConfigParameter())
                    return;

                //Delete folders/files if needed
                DeleteEm();

                hdTrailersNetRSS itemsHD = new hdTrailersNetRSS();
                if (itemsHD.IsUrlValid(config.FeedAddress) != System.String.Empty)
                {
                    log.WriteLine("Connecting using: hdTrailersNetRSS class");
                    itemsHD.GetFeedItems(config.FeedAddress);
                    log.WriteLine("Number of Items in Feed:" + itemsHD.Count);

                    for (int i = 0; i < itemsHD.Count; i++)
                    {
                        ProcessFeedItem(itemsHD[i].name, itemsHD[i]);
                    }
                }
                hdTrailersNetRSS2 itemsHD2 = new hdTrailersNetRSS2();
                if (itemsHD2.IsUrlValid(config.FeedAddress) != System.String.Empty)
                {
                    log.WriteLine("Connecting using: hdTrailersNetRSS2 class");
                    itemsHD2.GetFeedItems(config.FeedAddress);
                    log.WriteLine("Number of Items in Feed:" + itemsHD2.Count);
                    for (int i = 0; i < itemsHD2.Count; i++)
                    {
                        ProcessFeedItem(itemsHD2[i].name, itemsHD2[i]);
                    }
                }
                hdTrailersNetWeb itemsHDW = new hdTrailersNetWeb();
                if (itemsHDW.IsUrlValid(config.FeedAddress) != System.String.Empty)
                {
                    log.WriteLine("Connecting using: hdTrailersNetWeb class");
                    itemsHDW.GetFeedItems(config.FeedAddress);
                    log.WriteLine("Number of Items in Feed:" + itemsHDW.Count);
                    for (int i = 0; i < itemsHDW.Count; i++)
                    {
                        ProcessFeedItem(itemsHDW[i].name, itemsHDW[i]);
                    }
                }
                trailerFreakRSS itemsTF = new trailerFreakRSS();
                if (itemsTF.IsUrlValid(config.FeedAddress) != System.String.Empty)
                {
                    log.WriteLine("Connecting using: trailerFreakRSS class");
                    itemsTF.GetFeedItems(config.FeedAddress);
                    log.WriteLine("Number of Items in Feed:" + itemsTF.Count);
                    for (int i = 0; i < itemsTF.Count; i++)
                    {
                        ProcessFeedItem(itemsTF[i].name, itemsTF[i]);
                    }
                }
                movieMazeDeRSS itemsMM = new movieMazeDeRSS();
                if (itemsMM.IsUrlValid(config.FeedAddress) != System.String.Empty)
                {
                    log.WriteLine("Connecting using: movieMazeDeRSS class");
                    itemsMM.GetFeedItems(config.FeedAddress);
                    log.WriteLine("Number of Items in Feed:" + itemsMM.Count);
                    for (int i = 0; i < itemsMM.Count; i++)
                    {
                        ProcessFeedItem(itemsMM[i].name, itemsMM[i]);
                    }
                }
                //Do housekeeping like serializing exclusions and sending email summary
                log.VerboseWrite("");
                log.VerboseWrite("Housekeeping:");

                // write exclusion list if necessary
                WriteExclusions(Exclusions);

                // send email if desired
                SendEmailSummary();

                // run .exe if desired
                if (config.RunEXE)
                    RunEXE();

                log.WriteLine("Done");

                if (config.PauseWhenDone)
                    Console.ReadLine();
            }
            catch (Exception e)
            {
                log.WriteLine("1. Unhandled Exception....");
                log.WriteLine("Exception: " + e.Message);
            }

        }

        /// <summary>
        /// read data from appconfig. configure the logging object according to the appconfig information
        /// </summary>
        public static bool Init()
        {
            try
            {
                config.Init();
                log.Init(config.VerboseLogging, config.PhysicalLog);
                log.VerboseWrite("Config loaded");
                log.VerboseWrite(config.Info());

                if (config.UseExclusions)
                    Exclusions = ReadExclusions();
                else
                    log.VerboseWrite("Not using exclusions...");

                log.WriteLine("");
                return true;
            }
            catch (Exception e)
            {
                log.WriteLine("Unhandled exception during application Init routine. Application will close ....");
                log.WriteLine("Exception: " + e.ToString());
                return false;
            }
        }


        /// <summary>
        /// call this to check the configuration parameter for correctness
        /// </summary>
        /// <returns>true if configuration parameters are recognized as valid, false otherwise</returns>
        static bool CheckConfigParameter()
        {
            try
            {

                if (config.TrailerDownloadFolder.Length == 0)
                {
                    log.WriteLine("Illegal TrailerDownloadFolder. Quitting ....");
                    return false;
                }
                if (!Directory.Exists(config.TrailerDownloadFolder))
                {
                    log.VerboseWrite("Creating TrailerDownloadFolder: " + config.TrailerDownloadFolder);
                    Directory.CreateDirectory(config.TrailerDownloadFolder);
                }
                if (config.UserAgentId.Length != config.UserAgentString.Length)
                {
                    log.WriteLine("Count of UserAgentId (" + config.UserAgentId.Length.ToString() + ") doesn't match count of UserAgentString (" + config.UserAgentString.Length.ToString() + "). Quitting ....");
                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                log.VerboseWrite("Unhandled Exception checking configuration Parameter. Application will close....");
                log.VerboseWrite("Exception: " + e.ToString());
                return false;
            }
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
                            if (config.DeleteToRecycleBin)
                                FileSystem.DeleteDirectory(dirList[i], Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs, Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);
                            else
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
                            if (config.DeleteToRecycleBin)
                                FileSystem.DeleteFile(fileList[i], Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs, Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);
                            else
                                File.Delete(fileList[i]);
                            if (config.VerboseLogging)
                                log.WriteLine("Deleted file: " + fileList[i]);
                        }
                    }
                }
                catch (Exception e)
                {
                    log.WriteLine("Error deleting subdirectories.");
                    log.WriteLine("Exception: " + e.ToString());
                }
            }
            return;
        }

        /// <summary>
        /// based on the trailername the target directory for storage is computed. this includes the search for
        /// already existing directories, and returning the correct complete path to the target directory
        /// </summary>
        /// <param name="fname">trailername</param>
        /// <returns>the complete qualfied path for the target directory</returns>
        static string ManageDirectory(string fname)
        {
            string dirName = config.TrailerDownloadFolder;
            if (!config.CreateFolder)
            {
                return dirName;
            }

            string[] dirList;

            dirList = Directory.GetDirectories(config.TrailerDownloadFolder, fname);

            if ((dirList == null) || (dirList.Length != 1))
            {
                dirList = Directory.GetDirectories(config.TrailerDownloadFolder, "????-??-?? " + fname);
            }

            if ((dirList != null) && (dirList.Length == 1))
            {
                // a subdirectory with this name exists, we are done
                dirName = dirList[0];
                return dirName;
            }

            // we didn't find a match with the direct name or with a preceeding date info
            // we are going to need a new directoryname
            if (config.AddDates)
                dirName = dirName + pathsep + DateTime.Now.ToString("yyyy-MM-dd") + " " + fname;
            else
                dirName = dirName + pathsep + fname;
            return dirName;
        }

        //      static void ProcessFeedItem(string title, string link)
        static void ProcessFeedItem(string title, MovieItem link)
        {
            string qualPreference = "";
            string newtitle;
            log.WriteLine("");
            log.WriteLine("Next trailer: " + title);

            if ((config.TrailersOnly) && (!title.Contains("Trailer")))
            {
                log.WriteLine("Title not a trailer. Skipping...");
                AddToEmailSummary(title + " (" + qualPreference + ") : Title not a trailer. Skipping...");
                return;
            }

            NameValueCollection nvc;
            //            nvc = GetDownloadUrls(link, title);
            nvc = link.nvc;
            if ((nvc == null) || (nvc.Count == 0))
            {
                log.WriteLine("Error: No Download URLs found. Skipping...");
                return;
            }

            if (config.VerboseLogging)
            {
                StringBuilder sb = new StringBuilder("\n");

                for (int j = 0; j < nvc.Count; j++)
                    sb.AppendFormat("    {0,-10} {1}\n", nvc.GetKey(j), nvc.Get(j));

                log.VerboseWrite(sb.ToString());
            }
            string tempTrailerURL = GetPreferredURL(nvc, config.QualityPreference, ref qualPreference);
            string fname = LegalFileName(title);
            string dirName = ManageDirectory(fname);

            // Compare download url to sitestoskip item in config. If match detected, skip and log.
            for (int t = 0; t < config.SitesToSkip.Count(); t++)
                if (tempTrailerURL.Contains(config.SitesToSkip[t]))
                {
                    log.WriteLine("Trailer source (" + config.SitesToSkip[t] + ") is identified as Site To Skip in config. Skipping...");
                    AddToEmailSummary("Trailer source (" + config.SitesToSkip[t] + ") is identified as Site To Skip in config. Skipping...");
                    return;

                }

            if (tempTrailerURL == null)
            {
                log.WriteLine("Preferred quality not available. Skipping...");
                AddToEmailSummary(title + " (" + qualPreference + ") : Not available. Skipping...");
                return;
            }
            if ((config.SkipTheatricalTrailers) && (title.Contains("Theatrical")))
            {
                log.WriteLine("Skip Theatrical Trailers set. Skipping...");
                AddToEmailSummary(title + ": Skip Theatrical Trailers set. Skipping...");
                return;
            }
            if ((config.SkipTeaserTrailers) && (title.Contains("Teaser")))
            {
                log.WriteLine("Skip Teaser Trailers set. Skipping...");
                AddToEmailSummary(title + ": Skip Teaser Trailers set. Skipping...");
                return;
            }
            if ((config.SkipRedBandTrailers) && (fname.Contains("Red Band")))
            {
                log.WriteLine("Skip Red Band Trailers set. Skipping...");
                AddToEmailSummary(title + ": Skip Red Band Trailers set. Skipping...");
                return;
            }
            //  This is test code to 
            //            string myString;
            //            myString = title;
            //            string input = "Super 8 (Theatrical Trailer No 1)";
            //            string regex = "(\\[.*\\])|(\".*\")|('.*')|(\\(.*\\))";
            //            output = Regex.Replace(input, regex, "(Trailer)");
            //            string str2 = myString.Remove(myString.IndexOf("Trailer") + "Trailer".Length) + ")";
            //            str2 = myString.Remove(myString.IndexOf("("), myString.IndexOf("Trailer")- myString.IndexOf("(") );
            //            myString = title;
            //            output = myString.Remove(myString.IndexOf("(") + 1, myString.IndexOf("Trailer") - myString.IndexOf("(") - 1);

            if (config.TrailersIdenticaltoTheatricalTrailers)
            {
                log.WriteLine("Config Specifies TrailersIdenticaltoTheatricalTrailers");
                newtitle = title.Replace("Theatrical ", "");
            }
            else
            {
                newtitle = title;
            }
            if (config.ConsiderTheatricalandNumberedTrailersasIdentical)
            {
                if (!newtitle.Contains("Teaser"))
                {
                    log.WriteLine("Config Specifies ConsiderTheatricalandNumberedTrailersasIdentical");
                    string regex = "(\\[.*\\])|(\".*\")|('.*')|(\\(.*\\))";
                    newtitle = Regex.Replace(newtitle, regex, "(Trailer)");
                }
            }
            else
            {
                newtitle = title;
            }

            if (Exclusions.Contains(newtitle))
            {
                log.WriteLine("Title found in exclusions list. Skipping...");
                AddToEmailSummary(title + ": Title found in exclusions list. Skipping...");
                return;
            }
            if ((config.IncludeGenres.Length == 0) ||
                (config.ExcludeGenres.Length == 0) ||
                (config.IncludeLanguages.Length == 0) ||
                (config.IncludeGenres.IndexOf("all", StringComparison.OrdinalIgnoreCase) == -1) || 
                (config.ExcludeGenres.IndexOf("none", StringComparison.OrdinalIgnoreCase) == -1) || 
                (config.IncludeLanguages.IndexOf("all", StringComparison.OrdinalIgnoreCase) == -1) || 
                config.CreateXBMCNfoFile)
            {
                Regex reg = new Regex("\\(([^)]*)\\)");
                string MovieName = reg.Replace(fname, "");
                //                String MovieName = "The Pruitt-Igoe Myth";
                MovieName.Trim();
                log.WriteLine("Looking up MovieName on IMDB");
                if ((link.imdbId != null) && (link.imdbId.Length > 0))
                {
                    imdb.ImdbLookup(link.imdbId);
                }
                else
                {
                    imdb.ImdbLookup(MovieName);
                }
            }
            if ((config.IncludeGenres.Length != 0) &&
                (config.IncludeGenres.IndexOf("all", StringComparison.OrdinalIgnoreCase) == -1))
            {
                if (!imdb.isGenre(config.IncludeGenres))
                {
                    log.WriteLine("Trailer Genre not in include list. Skipping...");
                    AddToEmailSummary(title + ": Trailer Genre not in include list. Skipping...");
                    return;
                }
            }
            if ((config.ExcludeGenres.Length != 0) &&
                (config.ExcludeGenres.IndexOf("none", StringComparison.OrdinalIgnoreCase) == -1))
            {
                if (imdb.isGenre(config.ExcludeGenres))
                {
                    log.WriteLine("Trailer Genre is in the exclude list. Skipping...");
                    AddToEmailSummary(title + ": Trailer Genre is in the exclude list. Skipping...");
                    return;
                }

            }
            if ((config.IncludeLanguages.Length != 0) &&
                (config.IncludeLanguages.IndexOf("all", StringComparison.OrdinalIgnoreCase) == -1))
            {
                if (!imdb.isLanguage(config.IncludeLanguages))
                {
                    log.WriteLine("Trailer language is not in include list. Skipping...");
                    AddToEmailSummary(title + ": Trailer language is not in include list. Skipping...");
                    return;
                }

            }

            bool tempBool;
            string posterUrl = nvc["poster"];
            bool tempDirectoryCreated = false;

            log.VerboseWrite("Extracted download url (" + qualPreference + "): " + tempTrailerURL);
            log.VerboseWrite("Local directory: " + dirName);

            if ((config.CreateFolder) && (!Directory.Exists(dirName)))
            {
                Directory.CreateDirectory(dirName);     
                tempDirectoryCreated = true;
            }

            tempBool = GetOrResumeTrailer(tempTrailerURL, fname, dirName, qualPreference, posterUrl);
            //Delete the directory if it didn't download
            if (tempBool == false && tempDirectoryCreated == true)
                Directory.Delete(dirName);

            //If download went ok, and we're using exclusions, add to list
            if (tempBool && config.UseExclusions)
            {
                Exclusions.Add(newtitle);
                log.VerboseWrite("Exclusion added");
            }

            if (tempBool)
            {
                log.WriteLine(title + " (" + qualPreference + ") : Downloaded");
                AddToEmailSummary(title + " (" + qualPreference + ") : Downloaded");
            }
            if (tempBool && config.CreateXBMCNfoFile)
            {
                NfoFile NFOTrailerFile = new NfoFile();
                if (imdb.Id != null)
                {
                    NfoMovie NFOTrailer = new NfoMovie();

                    NFOTrailer.Title = imdb.Title;
                    NFOTrailer.Quality = qualPreference;
                    NFOTrailer.Rating = imdb.Rating;
                    NFOTrailer.Year = imdb.Year;
                    NFOTrailer.Releasedate = imdb.ReleaseDate;
                    NFOTrailer.Top250 = imdb.Top250;
                    NFOTrailer.Votes = imdb.Votes;
                    NFOTrailer.Plot = imdb.Plot;
                    NFOTrailer.Tagline = imdb.Tagline;
                    NFOTrailer.Runtime = imdb.Runtime;
                    if (fname.Contains("Red Band"))
                    {
                        NFOTrailer.Mpaa = "R";
                    }
                    else
                    {

                        if (imdb.MpaaRating.Length != 0)
                        {
                            if (config.IfIMDBMissingMPAARatingUse.Length != 0)
                            {
                                NFOTrailer.Mpaa = config.IfIMDBMissingMPAARatingUse;
                            }
                            else
                            {
                                NFOTrailer.Mpaa = "";
                            }
                        }
                        else
                        {
                            NFOTrailer.Mpaa = imdb.MpaaRating;
                        }
                    }
                    NFOTrailer.Id = imdb.Id;
                    NFOTrailer.Runtime = imdb.Runtime;
                    string[] strStrings = imdb.Genres.ToArray(typeof(string)) as string[];
                    string JoinedString = String.Join(" / ", strStrings);
                    NFOTrailer.Genre = JoinedString;
                    String NfoName = MakeFileName(".nfo", fname, dirName, qualPreference);
                    //                  String NfoName = BuildFileName(fname, dirName, ".nfo");
                    NFOTrailerFile.saveNfoMovie(NFOTrailer, dirName + pathsep + NfoName);
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
                log.WriteLine("ERROR: Could not get feed. Exception to follow.");
                log.WriteLine(e.Message);

                return null;
            }
        }

        static public string ReadDataFromLink(string link)
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
        static NameValueCollection GetDownloadUrls(string link, string title)
        {
            try
            {
                NameValueCollection nvc = new NameValueCollection();

                //Set data to CurrentSource.. will use to pull poster
                string data = ReadDataFromLink(link);
                // CurrentSource = data;

                //  Original line iaj 04/06/2010
                int pos = data.IndexOf(@"Download</strong>:");
                //                String MovieName = "The Pruitt-Igoe Myth";
                //                Regex reg = new Regex("\\(([^)]*)\\)");
                //                string MovieName = reg.Replace(title, "");
                //                int pos = data.IndexOf("title=\"" + MovieName + "\" alt=\"" + MovieName + "\">");
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

        static string BuildFileName(string fName, string dirName, string ext)
        {
            if ((!config.CreateFolder) && (config.AddDates))
            {
                fName = DateTime.Now.ToString("yyyy-MM-dd") + " " + fName;
            }
            if (config.XBMCFilenames)
            {
                fName = fName.Insert(fName.Length - 4, "-trailer");
            }
            string nfofilename = dirName + pathsep + fName;
            return Path.ChangeExtension(nfofilename, ext);
        }
        static string MakeFileName(string upperDownloadUrl, string fName, string dirName, string qualPref)
        {
            if (config.AppendTrailerQuality)
            {
                qualPref = "_" + qualPref;
            }
            else
            {
                qualPref = "";
            }
            if (upperDownloadUrl.Contains(".WMV"))
                fName = fName + qualPref + ".wmv";
            else if (upperDownloadUrl.Contains(".ZIP"))
                fName = fName + qualPref + ".zip";
            else if (upperDownloadUrl.Contains(".nfo"))
                fName = fName + qualPref + ".nfo";
            else if (upperDownloadUrl.Contains(".MP4"))
                fName = fName + qualPref + ".mp4";
            else
                fName = fName + qualPref + ".mov";

            DirectoryInfo di = new DirectoryInfo(dirName);
            FileInfo[] fi;

            fi = di.GetFiles(fName);
            if ((fi == null) || (fi.Length != 1))
            {
                fi = di.GetFiles("????-??-?? " + fName);
            }
            if ((fi != null) && (fi.Length == 1))
            {
                return fi[0].Name;
            }

            if ((!config.CreateFolder) && (config.AddDates))
            {
                fName = DateTime.Now.ToString("yyyy-MM-dd") + " " + fName;
            }
            //            if (fName.Contains(".nfo")) return fName;
            if (config.XBMCFilenames)
            {
                fName = fName.Insert(fName.Length - 4, "-trailer");
            }
            return fName;
        }


        static bool GetOrResumeTrailer(string downloadURL, string fName, string dirName, string qualPref, string posterUrl)
        {
            string Filename;
            string FullFileName;
            HttpWebRequest myWebRequest;
            HttpWebResponse myWebResponse;
            bool tempBool = false;

            int StartPointInt;
            //Make this work for .WMV and .MOV. Add more later as needed
            string upperDownloadUrl = downloadURL.ToUpper();
            string userAgentString = ManageUserAgent(upperDownloadUrl);


            fName = MakeFileName(upperDownloadUrl, fName, dirName, qualPref);
            log.VerboseWrite("Filename : " + fName);
//            if (upperDownloadUrl.Contains("YOUTUBE"))
//            {
//                log.WriteLine("YouTube Trailers not supported at the moment. Skipping ...");
//                return false;
//            }
            FullFileName = dirName + pathsep + fName;
// Check length of filename. MAX_PATH set to 260. Need to subtract 4 since .tmp will be added to filename
            if (FullFileName.Length > MAX_PATH - 4)
            {
                log.WriteLine("Trailer Filename too Long. Skipping...");
                AddToEmailSummary(FullFileName + ": Trailer Filename too Long. Skipping...");
                return false;
            }
            if (File.Exists(FullFileName))
            {
                Filename = dirName + pathsep + fName;
                FileInfo fi = new FileInfo(Filename);
                StartPointInt = Convert.ToInt32(fi.Length);
            }
            else if (File.Exists(FullFileName + ".tmp"))
            {
                Filename = FullFileName + ".tmp";
                FileInfo fi = new FileInfo(Filename);
                StartPointInt = Convert.ToInt32(fi.Length);
            }
            else
            {
                Filename = FullFileName + ".tmp";
                StartPointInt = 0;
            }
            myWebRequest = (HttpWebRequest)WebRequest.Create(downloadURL);
            if (userAgentString != null)
            {
                myWebRequest.UserAgent = userAgentString;
            }
            try
            {
                using (myWebResponse = (HttpWebResponse)myWebRequest.GetResponse())
                {
                    log.WriteLine("Stream successfully opened");
                    myWebResponse = (HttpWebResponse)myWebRequest.GetResponse();

                    // Ask the server for the file size and store it
                    int fileSize = Convert.ToInt32(myWebResponse.ContentLength);
                    if(fileSize < 0) {
                        StartPointInt = 0;
                        log.WriteLine("Invalid trailer size (" + fileSize + " bytes) from Server. Skipping ...");
                        return false;
                    }

                    myWebResponse.Close();
                    myWebRequest.Abort();

                    if ((config.MinTrailerSize > 0) && (fileSize < config.MinTrailerSize) && (fileSize != -1))
                    {
                        log.WriteLine("Trailer size smaller then MinTrailerSize. Skipping ...");
                        return false;
                    }


                    if ((StartPointInt < fileSize) || (fileSize == -1))
                    {
                        Stream strResponse;
                        FileStream strLocal;

                        // Create a request to the file we are downloading
                        myWebRequest = (HttpWebRequest)WebRequest.Create(downloadURL);
                        myWebRequest.Credentials = CredentialCache.DefaultCredentials;
                        if (userAgentString != null)
                        {
                            myWebRequest.UserAgent = userAgentString;
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
                            strLocal = new FileStream(Filename, FileMode.Create, FileAccess.Write, FileShare.None);
                        }
                        else
                        {
                            if (myWebResponse.StatusCode == HttpStatusCode.PartialContent)
                                log.WriteLine(StartPointInt.ToString() + " bytes of " + Convert.ToInt32(fileSize).ToString() + " located on disk. Resuming...");
                            else
                                log.WriteLine(StartPointInt.ToString() + " bytes of " + Convert.ToInt32(fileSize).ToString() + " located on disk. Server will not resume!!");

                            strLocal = new FileStream(Filename, FileMode.Append, FileAccess.Write, FileShare.None);
                        }

                        // It will store the current number of bytes we retrieved from the server
                        int bytesSize = 0;

                        // A buffer for storing and writing the data retrieved from the server
                        byte[] downBuffer = new byte[65536];

                        if (fileSize != -1)
                        {
                            // Loop through the buffer until the buffer is empty
                            while ((bytesSize = strResponse.Read(downBuffer, 0, downBuffer.Length)) > 0)
                            {
                                // Write the data from the buffer to the local hard drive
                                strLocal.Write(downBuffer, 0, bytesSize);
                                StartPointInt += bytesSize;
                                double t = ((double)StartPointInt) / fileSize;
                                log.ConsoleWrite(t.ToString("###.0%\r", CultureInfo.InvariantCulture));
                            }
                        }
                        else
                        {
                            var cli = new WebClient();
                            string data = cli.DownloadString(downloadURL);
                        }

                        // When the above code has ended, close the streams
                        strResponse.Close();
                        strLocal.Close();
                        myWebResponse.Close();
                        FileInfo fi = new FileInfo(Filename);
                        StartPointInt = Convert.ToInt32(fi.Length);
                        if ((StartPointInt == fileSize) || (fileSize == -1))
                        {
                            File.Move(Filename, FullFileName);
                        }
                        // Increment NewTrailerCount(er)
                        NewTrailerCount = NewTrailerCount + 1;
                        tempBool = true;
                    }
                    else if (StartPointInt == Convert.ToInt32(fileSize))
                    {
                        tempBool = false;
                        log.WriteLine("File exists and is same size. Skipping...");
                    }
                    else
                    {
                        tempBool = false;
                        log.WriteLine("Something else is wrong.. size on disk is greater than size on web.");
                    }

                    //Assuming we downloaded the trailer OK and the config has been set to grab posters...
                    if ((tempBool) && (config.GrabPoster))
                        GetPoster(posterUrl, dirName, fName);

                    return tempBool;
                }
            }

            catch (WebException e)
            {
                Program.log.WriteLine("Exception in GetOrResumeTrailer");
                Program.log.WriteLine(e.ToString());
                return false;
            }

        }
        /// <summary>
        /// based on the url look in the configuration if a useragent string is required
        /// </summary>
        /// <param name="url">the capitalized download url</param>
        /// <returns>a valid user agent string or null</returns>
        static string ManageUserAgent(string url)
        {
            for (int i = 0; i < config.UserAgentId.Length; i++)
                if (url.Contains(config.UserAgentId[i].ToUpper()))
                    return config.UserAgentString[i];

            return null;
        }

        static void GetPoster(string source, string downloadPath, string filename)
        {
            try
            {
                String fname;
                if (config.CreateFolder)
                {
                    fname = downloadPath + pathsep + @"folder.jpg";
                }
                else
                {
                    //fname = BuildFileName(filename, downloadPath, "jpg");
                    fname = Path.ChangeExtension(filename, "jpg");
                    fname = downloadPath + pathsep + fname;
                    //                string fname = downloadPath + pathsep + filename;
                }
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


        /// <summary>
        /// Using the standard serialzer to read the exclusion list
        /// </summary>
        /// <returns>exclusion list</returns>
        static ArrayList ReadExclusions()
        {
            if (config.UseExclusions)
            {
                try
                {
                    ArrayList exclusions;

                    log.VerboseWrite("Using exclusions...");

                    //We are using exclusions. Load into arraylist or create empty arraylist

                    string pathstring = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "HD-Trailers.Net Downloader");
                    if (!Directory.Exists(pathstring))
                        Directory.CreateDirectory(pathstring);

                    if (File.Exists(Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "HD-Trailers.Net Downloader"), "HD-Trailers.Net Downloader Exclusions.xml")))
                    {
                        System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(ArrayList));
                        System.IO.TextReader reader = new System.IO.StreamReader(Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "HD-Trailers.Net Downloader"), "HD-Trailers.Net Downloader Exclusions.xml"));
                        exclusions = (ArrayList)serializer.Deserialize(reader);
                        reader.Close();
                    }
                    else
                        exclusions = new ArrayList();

                    log.VerboseWrite(exclusions.Count.ToString() + " exclusions loaded.");

                    return exclusions;
                }
                catch (Exception e)
                {
                    log.VerboseWrite("Exception reading exclusion file. Substituting empty exclusion list.");
                    log.VerboseWrite("Exception: " + e.ToString());
                    return new ArrayList();
                }
            }
            else
                return new ArrayList();
        }

        /// <summary>
        /// write exclusion list if necessary
        /// </summary>
        /// <param name="exclusions"></param>
        static void WriteExclusions(ArrayList exclusions)
        {
            try
            {
                if (config.UseExclusions)
                {
                    //We're using exclusions... write to file for next run
                    log.VerboseWrite("");
                    log.VerboseWrite("Serializing exclusion list...");

                    System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(ArrayList));
                    //                    string pathstring = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "HD-Trailers.Net Downloader");
                    System.IO.TextWriter writer = new System.IO.StreamWriter(Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "HD-Trailers.Net Downloader"), "HD-Trailers.Net Downloader Exclusions.xml"));
                    //                    System.IO.TextWriter writer = new System.IO.StreamWriter(Path.Combine(pathstring, pathsep, "HD-Trailers.Net Downloader Exclusions.xml"));
                    serializer.Serialize(writer, exclusions);
                    writer.Close();

                    log.VerboseWrite("Serialization complete.");
                }
            }
            catch (Exception e)
            {
                log.VerboseWrite("Writing exclusion list failed with exception.");
                log.VerboseWrite("Exception: " + e.ToString());
            }
        }

        static void SendEmailSummary()
        {
            try
            {
                if (config.EmailSummary)
                {
                    log.VerboseWrite("");
                    log.VerboseWrite("Sending email summary...");

                    // To
                    MailMessage mailMsg = new MailMessage();
                    mailMsg.To.Add(config.EmailAddress);

                    // From
                    MailAddress mailAddress = new MailAddress(config.EmailReturnAddress, config.EmailReturnDisplayName);
                    mailMsg.From = mailAddress;

                    // Subject and Body
                    mailMsg.Subject = Version + " Download Summary for " + DateTime.Now.ToShortDateString();
                    mailMsg.Body = MailBody;

                    // Init SmtpClient and send
                    SmtpClient smtpClient = new SmtpClient(config.SMTPServer, config.SMTPPort);
                    if (!config.UseDefaultCredentials)
                    {
                        smtpClient.UseDefaultCredentials = false;
                        if (config.SMTPUsername.Length != 0)
                        {
                            System.Net.NetworkCredential credentials = new System.Net.NetworkCredential(config.SMTPUsername, config.SMTPPassword);
                            smtpClient.Credentials = credentials;
                        }
                    }
                    else
                    {
                        smtpClient.Credentials = CredentialCache.DefaultNetworkCredentials;
                    }
                    smtpClient.Send(mailMsg);
                    log.VerboseWrite("Email summary sent.");
                }
            }
            catch (Exception e)
            {
                log.WriteLine("Exception Sending Email.");
                log.WriteLine("Exception: " + e.Message);
            }
        }

        static void RunEXE()
        {
            try
            {

                if ((config.RunOnlyWhenNewTrailers && NewTrailerCount == 0))
                    log.VerboseWrite("Not running exe. No new trailers downloaded");
                else
                {
                    log.VerboseWrite("");
                    log.VerboseWrite("Running EXE...");

                    Console.WriteLine("Running");

                    Process pr = new Process();

                    pr.StartInfo.FileName = config.Executable;


                    // %N = # of new videos downloaded this run
                    string tempString = @config.EXEArguements.Replace("%N", NewTrailerCount.ToString()); ;

                    pr.StartInfo.Arguments = tempString;



                    pr.Start();

                    while (pr.HasExited == false)
                        if ((DateTime.Now.Second % 5) == 0)
                        { // Show a tick every five seconds.

                            Console.Write(".");

                            System.Threading.Thread.Sleep(1000);

                        }



                    log.VerboseWrite("EXE run complete.");
                }

            }
            catch (Exception e)
            {
                log.WriteLine("Exception Running EXE.");
                log.WriteLine("Exception: " + e.Message);
            }
        }

        /// <summary>
        /// removes all illegal characters so a string can represent a legal filename. the input string
        /// is not allowed to include a file extension, 'sample' is a legal input, 'sample.txt' is not
        /// a check for special filenames (CON, LPT1,...) is not being performed
        /// </summary>
        /// <param name="input">string to converted into a legal filename (not including file extension)</param>
        /// <returns>a string contaning a legal filename</returns>
        static string LegalFileName(string input)
        {
            // list of things not useful in filename. 'periods' are permitted, but for simplicity are being replaced
            int i;
            string[] illegalChars = { "<", ">", ":", "\"", "/", "\\", "|", "?", "*", ".", "'","\"" };

            if ((input == null) || (input.Length == 0))
                return null;

            StringBuilder sb = new StringBuilder(input);

            for (i = 1; i <= 31; i++)
                sb.Replace((char)i, '*');

            for (i = 0; i < illegalChars.Length; i++)
                sb.Replace(illegalChars[i], "");

            string ret = sb.ToString().Trim();
            if ((ret == null) || (ret.Length == 0))
                return null;

            return ret;
        }

        static void DisplayHelp()
        {
            Console.WriteLine ("Usage: greet [OPTIONS]+ message");
            Console.WriteLine ("Greet a list of individuals with an optional message.");
            Console.WriteLine ("If no message is specified, a generic greeting is used.");
            Console.WriteLine ();
            Console.WriteLine ("Options:");
            Process pr = new Process(); 
            pr.StartInfo.FileName = "Notepad.exe"; 
            pr.StartInfo.Arguments = "C:\\Users\\Ian\\AppData\\Local\\HD-Trailers.Net Downloader\\HD-Trailers.Net Downloader.config"; 
            pr.Start();
            while (pr.HasExited == false)
            {
                if ((DateTime.Now.Second % 5) == 0)
                { 
                }
                System.Threading.Thread.Sleep(1000);
            }            
            Environment.Exit(0); 
        }
        static void EditConfigFile()
        {
            Console.WriteLine("Usage: greet [OPTIONS]+ message");
            Console.WriteLine("Greet a list of individuals with an optional message.");
            Console.WriteLine("If no message is specified, a generic greeting is used.");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Process pr = new Process();
            pr.StartInfo.FileName = "Notepad.exe";
            pr.StartInfo.Arguments = "C:\\Users\\Ian\\AppData\\Local\\HD-Trailers.Net Downloader\\HD-Trailers.Net Downloader.config";
            pr.Start();
            while (pr.HasExited == false)
            {
                if ((DateTime.Now.Second % 5) == 0)
                {
                }
                System.Threading.Thread.Sleep(1000);
            }
            Environment.Exit(0);
        }
    }
}