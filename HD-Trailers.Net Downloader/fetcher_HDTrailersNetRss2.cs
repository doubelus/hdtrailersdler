using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Collections;
using System.Text;


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
                string data = Program.ReadDataFromLink(mi.url);
                string trailertype = StringFunctions.subStrBetween(mi.name, "(", ")" );
                string[] tempStringArray = StringFunctions.splitBetween(data, "<tr style=\"\" ", "</tr>");
                for (int i = 0; i < tempStringArray.Length; i++)
                {
                    if (tempStringArray[i].Contains("standardTrailerName"))
                    {
                        string name = StringFunctions.subStrBetween(tempStringArray[i], "<span class=\"standardTrailerName\" itemprop=\"name\">", "</span>");
                        if (trailertype == name)
                        {
                            mi.name = mi.name.Substring(0, mi.name.IndexOf("("));
                            mi.name += " (" + name + ")";
                            string[] links = StringFunctions.splitBetween(tempStringArray[i], "<a", "</a>");
                            foreach (string link in links)
                            {
                                if (link.Contains("title"))
                                {
                                    string url = StringFunctions.subStrBetween(link, "href=\"", "\"");
                                    string quality = StringFunctions.subStrBetween(link, ">", "</a>");
                                    if (!url.Contains("how-to-download-hd-trailers-from-apple"))
                                        mi.nvc.Add(quality, url);
                                }
                            }
                            string poster = StringFunctions.subStrBetween(data, "<span class=\"topTableImage\">", "</span>");
                            poster = StringFunctions.subStrBetween(poster, "src=\"", "\"");
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
