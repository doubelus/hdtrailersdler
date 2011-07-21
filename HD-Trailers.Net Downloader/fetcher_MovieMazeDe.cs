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

namespace HDTrailersNETDownloader
{
    public class movieMazeDeRSS : GenericFetcherRSS
    {
        public movieMazeDeRSS()
        {
            validurls = new List<string>();
            validurls.Add("http://www.moviemaze.de/rss/trailer.phtml");
        }
        ~movieMazeDeRSS()
        {
            validurls.Clear();
        }

        public override void LoadItem(MovieItem mi)
        {
            try
            {
                mi.name = StringFunctions.subStrBetween(mi.name, "\"", "\"");

                string data = Program.ReadDataFromLink(mi.url);
                string url = StringFunctions.subStrBetween(data, "clipxml:'", "'");
                string newdata = Program.ReadDataFromLink(url);

                string type = StringFunctions.subStrBetween(newdata, "<title>", "</title>");
                type = StringFunctions.subStrBetween(type, "<![CDATA[", "]]>");
                mi.name = mi.name + " (" + type + ")";

                string urllow = StringFunctions.subStrBetween(newdata, "<urllow>", "</urllow>");
                urllow = StringFunctions.subStrBetween(urllow, "<![CDATA[", "]]>");
                if (urllow != null)
                    mi.nvc.Add("480P", urllow);
                string dwnurl = StringFunctions.subStrBetween(newdata, "<downloadurl>", "</downloadurl>");
                dwnurl = StringFunctions.subStrBetween(dwnurl, "<![CDATA[", "]]>");
                if (dwnurl != null)
                    mi.nvc.Add("1080P", dwnurl);
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                //ADD A POSTER 
                //string posterUrl = "http://www.trailerfreaks.com/" + subStrBetween(main, "<img src = \"", "\"");
                //nvc.Add("poster", posterUrl);
                //ADD IMDB
                //imdbId = subStrBetween(main, "www.imdb.com/title/", "/");
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
