using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;

namespace HDTrailersNETDownloader
{
    public class hdTrailersNetWeb : GenericFetcher
    {
        public hdTrailersNetWeb()
        {
            validurls = new List<string>();
            validurls.Add("http://www.hd-trailers.net");
            validurls.Add("http://www.hd-trailers.net/Page/1/");
            validurls.Add("http://www.hd-trailers.net/TopMovies/");
            validurls.Add("http://www.hd-trailers.net/OpeningThisWeek/");
            validurls.Add("http://www.hd-trailers.net/ComingSoon/");
            validurls.Add("http://www.hd-trailers.net/BluRay/");
            validurls.Add("http://www.hd-trailers.net/AcademyAward83/");
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
                    string name = StringFunctions.subStrBetween(tempStringArray[i], "title=\"", "\"");
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
                string data = Program.ReadDataFromLink(mi.url);
                string[] tempStringArray = StringFunctions.splitBetween(data,"<tr style=\"\">","</tr>");
                for (int i = 0; i < tempStringArray.Length; i++)
                {
                    if (tempStringArray[i].Contains("standardTrailerName"))
                    {
                        string name = StringFunctions.subStrBetween(tempStringArray[i],"<span class=\"standardTrailerName\">","</span>");
                        mi.name +=" (" + name + ")";
                        string[] links = StringFunctions.splitBetween(tempStringArray[i],"<a","</a>");
                        foreach (string link in links)
                        {
                            if (link.Contains("title"))
                            {
                                string url = StringFunctions.subStrBetween(link,"href=\"","\"");
                                string quality = StringFunctions.subStrBetween(link,">","</a>");
                                if (!url.Contains("how-to-download-hd-trailers-from-apple"))
                                    mi.nvc.Add(url,quality);
                            }
                        }
                        string poster = StringFunctions.subStrBetween(data,"<span class=\"topTableImage\">","</span>");
                        poster = StringFunctions.subStrBetween(poster,"src=\"","\"");
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
