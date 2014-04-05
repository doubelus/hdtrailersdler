using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Web;
using System.IO;

namespace HDTrailersNETDownloader
{
    public class hdTrailersNetWeb : GenericFetcher
    {
        public hdTrailersNetWeb()
        {
            validurls = new List<string>();
            validurls.Add("http://www.hd-trailers.net/");
            validurls.Add("http://www.hd-trailers.net/Page/1/");
            validurls.Add("http://www.hd-trailers.net/TopMovies/");
            validurls.Add("http://www.hd-trailers.net/OpeningThisWeek/");
            validurls.Add("http://www.hd-trailers.net/ComingSoon/");
            validurls.Add("http://www.hd-trailers.net/BluRay/");
            validurls.Add("http://www.hd-trailers.net/AcademyAward83/");
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
