using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Web;
using System.IO;
using IMDb_Scraper;

namespace HDTrailersNETDownloader
{

    public class YouTube3D : GenericFetcher
    {
        public YouTube3D()
        {
            validurls = new List<string>();
            validurls.Add("http://www.youtube.com/");
        }
        ~YouTube3D()
        {
            validurls.Clear();
        }

        public override void GetFeedItems(string url)
        {
            try
            {
                string data = Program.ReadDataFromLink(url);
                string[] tempStringArray = StringFunctions.splitBetween(data, "<h3 class=\"video-title-container\">", "</h3>");
                for (int i = 0; i < tempStringArray.Length; i++)
                {
                    string name1 = StringFunctions.subStrBetween(tempStringArray[i], "<span class=\"title video-title\" dir=\"ltr\">", "</span>");
                    string name;
                    name = HttpUtility.HtmlDecode(name1);
                    string tmpurl = "http://www.youtube.com" + StringFunctions.subStrBetween(tempStringArray[i], "<a href=\"", "\" title=\"");

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
                mi.nvc = new NameValueCollection();
                mi.nvc.Add("720p", mi.url);

                string posterUrl = "";
                mi.nvc.Add("poster", posterUrl);
                mi.imdbId = "";
                for (int n = 0; n < mi.nvc.Count; n++)

                    Console.WriteLine(mi.nvc[n]);

                foreach (string s in mi.nvc.Keys)

                    Console.WriteLine(mi.nvc[s]);
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
