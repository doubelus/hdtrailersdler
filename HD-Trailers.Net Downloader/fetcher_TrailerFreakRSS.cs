﻿using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using IMDb_Scraper;

namespace HDTrailersNETDownloader
{

    public class trailerFreakRSS : GenericFetcherRSS
    {
        public trailerFreakRSS()
        {
            validurls = new List<string>();
            validurls.Add("http://feeds2.feedburner.com/Feed-For-Trailer-Freaks");
        }
        ~trailerFreakRSS()
        {
            validurls.Clear();
        }

        public override void LoadItem(MovieItem mi)
        {
            try
            {
                /*  DateTime TrailerDateTime;
                   TrailerDateTime = new DateTime();
                   //<pubDate>2011-06-26 16:49:33</pubDate>
                   TrailerDateTime = DateTime.ParseExact(pubDate, "yyyy-MM-dd HH:mm:ss", null);
                   //CHANGE 5 TO 30 !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                   if ((DateTime.Now - TrailerDateTime).Days > 30) return;
                   */

                string data = Program.ReadDataFromLink(mi.url);

                string main = StringFunctions.subStrBetween(data, "<div class=\"maincontent\">", "</table>");
                string[] trailers = StringFunctions.splitBetween(main, "<tr>", "</tr>");

                int best = 0;
                foreach (String trailer in trailers)
                {
                    if (trailer.Contains("trailerinfo") && trailer.Contains("href"))
                    {
                        string type = StringFunctions.subStrBetween(trailer, "<b>", "</b>");
                        int match = StringFunctions.countMach(type, mi.name);
                        if (match > best)
                        {
                            best = match;
                            mi.nvc = new NameValueCollection();
                            string[] links = StringFunctions.splitBetween(trailer, "<a", "</a>");
                            foreach (String linkStr in links)
                            {
                                string link = StringFunctions.subStrBetween(linkStr, "href=\"", "\"");
                                string size = StringFunctions.subStrBetween(linkStr, ">");
                                size = size.ToLowerInvariant();
                                mi.nvc.Add(size, link);
                            }
                        }
                    }
                }
                string posterUrl = "http://www.trailerfreaks.com/" + StringFunctions.subStrBetween(main, "<img src = \"", "\"");
                mi.nvc.Add("poster", posterUrl);
                mi.imdbId = StringFunctions.subStrBetween(main, "www.imdb.com/title/", "/");
            }
            catch (Exception e)
            {
                Program.log.WriteLine("Exception in LoadItem (" + mi.name + " " + mi.url + ")");
                Program.log.WriteLine(e.ToString());
                return;
            }
        }
    }
}
