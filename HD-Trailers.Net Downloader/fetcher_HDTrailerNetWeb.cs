﻿using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Web;
using System.IO;
using System.Text.RegularExpressions;

namespace HDTrailersNETDownloader
{
    public class hdTrailersNetWeb : GenericFetcher
    {
        public hdTrailersNetWeb()
        {
            validurls = new List<string>();
            validurls.Add("http://www.hd-trailers.net/");
            validurls.Add("http://www.hd-trailers.net/Page/1/");
            validurls.Add("http://www.hd-trailers.net/Page/2/");
            validurls.Add("http://www.hd-trailers.net/Page/3/");
            validurls.Add("http://www.hd-trailers.net/Page/4/");
            validurls.Add("http://www.hd-trailers.net/Page/5/");
            validurls.Add("http://www.hd-trailers.net/Page/6/");
            validurls.Add("http://www.hd-trailers.net/Page/7/");
            validurls.Add("http://www.hd-trailers.net/Page/8/");
            validurls.Add("http://www.hd-trailers.net/Page/9/");

            validurls.Add("http://www.hd-trailers.net/Top-Movies/");
            validurls.Add("http://www.hd-trailers.net/Most-Watched/");
            validurls.Add("http://www.hd-trailers.net/Opening-This-Week/");
            validurls.Add("http://www.hd-trailers.net/Coming-Soon/");
            validurls.Add("http://www.hd-trailers.net//library/0/");
            validurls.Add("http://www.hd-trailers.net//library/a/");
            validurls.Add("http://www.hd-trailers.net//library/b/");
            validurls.Add("http://www.hd-trailers.net//library/c/");
            validurls.Add("http://www.hd-trailers.net//library/d/");
            validurls.Add("http://www.hd-trailers.net//library/e/");
            validurls.Add("http://www.hd-trailers.net//library/f/");
            validurls.Add("http://www.hd-trailers.net//library/g/");
            validurls.Add("http://www.hd-trailers.net//library/h/");
            validurls.Add("http://www.hd-trailers.net//library/i/");
            validurls.Add("http://www.hd-trailers.net//library/j/");
            validurls.Add("http://www.hd-trailers.net//library/k/");
            validurls.Add("http://www.hd-trailers.net//library/l/");
            validurls.Add("http://www.hd-trailers.net//library/m/");
            validurls.Add("http://www.hd-trailers.net//library/n/");
            validurls.Add("http://www.hd-trailers.net//library/o/");
            validurls.Add("http://www.hd-trailers.net//library/p/");
            validurls.Add("http://www.hd-trailers.net//library/q/");
            validurls.Add("http://www.hd-trailers.net//library/r/");
            validurls.Add("http://www.hd-trailers.net//library/s/");
            validurls.Add("http://www.hd-trailers.net//library/t/");
            validurls.Add("http://www.hd-trailers.net//library/u/");
            validurls.Add("http://www.hd-trailers.net//library/v/");
            validurls.Add("http://www.hd-trailers.net//library/w/");
            validurls.Add("http://www.hd-trailers.net//library/x/");
            validurls.Add("http://www.hd-trailers.net//library/y/");
            validurls.Add("http://www.hd-trailers.net//library/z/");
        }
        ~hdTrailersNetWeb()
        {
            validurls.Clear();
        }

        public override void GetFeedItems(string url)
        {
            try
            {
                string data = Program.ReadDataFromLink(url);
                string[] tempStringArray = StringFunctions.splitBetween(data,"<td class=\"indexTableTrailerImage\">","</td>");
                for (int i = 0; i < tempStringArray.Length; i++)
                {
                    string name1 = StringFunctions.subStrBetween(tempStringArray[i], "title=\"", "\"");
                    string name;
                    name = HttpUtility.HtmlDecode(name1);
                    string tmpurl = "http://www.hd-trailers.net" + StringFunctions.subStrBetween(tempStringArray[i], "href=\"", "\"");

                    Add(new MovieItem(name, tmpurl, ""));
                }
            }
            catch (Exception e)
            {
                Program.log.WriteLine("ERROR: Could not get web: " + url + " Exception to follow.");
                Program.log.WriteLine(e.Message);
                return;
            }
        }


        public override void LoadItem(MovieItem mi)
        {
            try
            {
                string url;
                string trailerString; 
                string trailerType;
                string data = Program.ReadDataFromLink(mi.url);
//                string[] tempStringArray = StringFunctions.splitBetween(data,"<tr style=\"\">","</tr>");
                string[] tempStringArray = StringFunctions.splitBetween(data, "<tr  itemprop=", "</tr>");
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
// Get trailer Type
                    trailerType = StringFunctions.subStrBetween(tempStringArray[i], "itemprop=\"name\">", "</span>");
                    mi.name = mi.name + " (" + trailerType + ")";
                    mi.name = Regex.Replace(mi.name, @"\s+", " "); 
                    if (trailerString.Length > 0)
                    {
                        string[] links = StringFunctions.splitBetween(tempStringArray[i],"<a","</a>");
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
                                        if (!String.IsNullOrEmpty(Resolution))
                                        {
                                            if (Resolution == "540") Resolution = "480";
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
                        poster = StringFunctions.subStrBetween(poster,"src=\"","\"");
                        if (!poster.StartsWith("http:", StringComparison.OrdinalIgnoreCase)) poster = "http:" + poster;
                        mi.nvc.Add("poster", poster);
                        break;
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
