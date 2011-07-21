using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Collections;
using System.Text;


namespace HDTrailersNETDownloader
{

   public class hdTrailersNet : GenericFetcherRSS
    {
        public hdTrailersNet()
        {
            validurls = new List<string>();
            validurls.Add("http://www.hd-trailers.net/blog/feed/");
        }
        ~hdTrailersNet()
        {
            validurls.Clear();
        }
        public override void LoadItem(MovieItem mi)
        {
            try
            {
                string data = Program.ReadDataFromLink(mi.url);
                int pos = data.IndexOf(@"Download</strong>:");
                if (pos == -1)
                    return;

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

                    mi.nvc.Add(s1, s2);
                }


                // now find the poster url
                // look for first 'Link to Catalog' then pick the src attribute from the first img tag

                tempString = data.Substring(data.IndexOf("<strong>Link to Catalog</strong>"));
                tempString = tempString.Substring(tempString.IndexOf("<img "));
                tempString = StringFunctions.subStrBetween(tempString, "src=\"", "\"");
                mi.nvc.Add("poster", tempString);

            }
            catch (Exception e)
            {
                Program.log.WriteLine("Exception in LoadItem (" + mi.name + " " + mi.url + ")");
                Program.log.WriteLine(e.ToString());
            }
        }


    }
}
