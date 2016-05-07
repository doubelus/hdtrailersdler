using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;

namespace HDTrailersNETDownloader
{

    public class hdTrailersNetRSS2 : GenericFetcherRSS
    {
        public hdTrailersNetRSS2()
        {
            validurls = new List<string>();

            validurls.Add("http://feeds.hd-trailers.net/hd-trailers");
        }
        ~hdTrailersNetRSS2()
        {
            validurls.Clear();
        }
        public override void LoadItem(MovieItem mi)
        {
            try
            {
                string url;
                string trailerString; 

                string data = Program.ReadDataFromLink(mi.url);
                string trailertype = StringFunctions.subStrBetween(mi.name, "(", ")" );
                string[] tempStringArray = StringFunctions.splitBetween(data, "<tr itemprop=", "</tr>");
                for (int i = 0; i < tempStringArray.Length; i++)
                {
                    if (tempStringArray[i].Contains("standardTrailerName"))
                    {
                        trailerString = "standardTrailerName";
                    } 
                    else if (tempStringArray[i].Contains("restrictedTrailerName"))
                    {
                        trailerString = "restrictedTrailerName";
                    }
                    else
                    {
                        trailerString = "";
                    }

                    if(trailerString.Length > 0) 
                    {
                        string name = StringFunctions.subStrBetween(tempStringArray[i], "<span class=\"" + trailerString + "\" itemprop=\"name\">", "</span>");
                        if (trailertype == name)
                        {
                            mi.name = mi.name.Substring(0, mi.name.IndexOf("("));

                            mi.name += " (" + name + ")";
                            mi.name = Regex.Replace(mi.name, @"\s+", " "); 
                            string[] links = StringFunctions.splitBetween(tempStringArray[i], "<a", "</a>");
                            foreach (string link in links)
                            {
                                if (link.Contains("title") && !link.Contains("Redirection"))
                                {
                                    string quality;
                                    string ContentID;
                                    string YahooString1;
                                    quality = StringFunctions.subStrBetween(link, ">", "</a>");
                                    if (link.Contains("yahoo"))
                                    {
                                        ContentID = StringFunctions.subStrBetween(link, "?id=", "&amp");
                                        YahooString1 = "http://video.query.yahoo.com/v1/public/yql?q=SELECT%20*%20FROM%20yahoo.media.video.streams%20WHERE%20id%3D%22[CONTENT_ID]%22%20AND%20format%3D%22mp4%22%20AND%20protocol%3D%22http%22%20AND%20plrs%3D%22sdwpWXbKKUIgNzVhXSce__%22%20AND%20region%3D%22US%22%3B&env=prod&format=json";
                                        YahooString1 = YahooString1.Replace("[CONTENT_ID]", ContentID);
                                        string data1 = Program.ReadDataFromLink(YahooString1);
                                        string[] tempStringArray1 = StringFunctions.splitBetween(data1, "{", "}");
                                        for (int ii = 0; ii < tempStringArray1.Length; ii++)
                                        {
                                            string Resolution = StringFunctions.subStrBetween(tempStringArray1[ii], "\"height\":", ".0");
                                            string Quality1 = quality.Replace("p", "");
                                            if(!String.IsNullOrEmpty(Resolution)) 
                                            {
                                                if(Resolution == "540") Resolution = "480";
                                                if (Resolution.Contains(Quality1))
                                                {
                                                    string host = StringFunctions.subStrBetween(tempStringArray1[ii], "\"host\":\"", "\""); ;
                                                    string path = StringFunctions.subStrBetween(tempStringArray1[ii], "\"path\":\"", "\""); ;
                                                    url = host + path;
                                                    mi.nvc.Add(quality, url);
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        url = StringFunctions.subStrBetween(link, "href=\"", "\"");
                                        if (!url.Contains("how-to-download-hd-trailers-from-apple"))
                                            mi.nvc.Add(quality, url);
                                    }
                                }
                            }
                            string poster = StringFunctions.subStrBetween(data, "<span class=\"topTableImage\">", "</span>");
                            poster = StringFunctions.subStrBetween(poster, "src=\"", "\"");
                            if (!poster.StartsWith("http:", StringComparison.OrdinalIgnoreCase)) poster = "http:" + poster;
                            mi.nvc.Add("poster", poster);
                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Program.log.WriteLine("Exception in GetDownloadUrls (" + mi.url + ")");
                Program.log.WriteLine(e.ToString());
            }
        }

    }
}
