using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections;
using System.Net;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

/****************************************************************
 * Free ASP.net IMDb Scraper API for the new IMDb Template.
 * Author: Abhinay Rathore
 * Website: http://www.AbhinayRathore.com
 * Blog: http://web3o.blogspot.com
 * Last Updated: March 6, 2011
 * ****************************************************************/

namespace HDTrailersNETDownloader
{
    public class IMDb
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Year { get; set; }
        public string Rating { get; set; }
        public ArrayList Genres { get; set; }
        public ArrayList Directors { get; set; }
        public ArrayList Writers { get; set; }
        public ArrayList Stars { get; set; }
        public ArrayList Cast { get; set; }
        public string MpaaRating { get; set; }
        public string ReleaseDate { get; set; }
        public string Plot { get; set; }
        public string Poster { get; set; }
        public string PosterLarge { get; set; }
        public string PosterSmall { get; set; }
        public string Runtime { get; set; }
        public string Top250 { get; set; }
        public string Oscars { get; set; }
        public string Storyline { get; set; }
        public string Tagline { get; set; }
        public string Votes { get; set; }
        public ArrayList ReleaseDates { get; set; }
        public ArrayList MediaImages { get; set; }
        public string ImdbURL { get; set; }

        //constructor
        public IMDb()
        {
        }
//        public IMDb(string MovieName, bool GetExtraInfo = true)
//       {
//            ImdbLookup(MovieName, GetExtraInfo);
//        }
        public IMDb(string MovieName, bool GetExtraInfo = true)
        {
            ImdbLookup(MovieName, GetExtraInfo);
        }

        //Get IMDb URL from Bing search
        private string getIMDbUrlFromBing(string MovieName)
        {
            DateTime thisyear = DateTime.Now;
            string thisyearstr = thisyear.Year.ToString();
            DateTime lastyear = DateTime.Now.AddYears(-1);
            string lastyearstr = thisyear.Year.ToString();

//            string url = "http://www.bing.com/search?q=imdb%20" + System.Uri.EscapeUriString(MovieName) + System.Uri.EscapeUriString("("+thisyearstr + " OR " + lastyearstr + ")");
            string url = "http://www.google.com/search?q=site:imdb.com%20" + System.Uri.EscapeUriString(MovieName) + System.Uri.EscapeUriString("(" + thisyearstr + " OR " + lastyearstr + ")");
            string html = getUrlData(url);
            ArrayList imdbUrls = matchAll(@"<a href=""(http://www.imdb.com/title/tt\d{7}/)"".*?>.*?</a>", html);
            if (imdbUrls.Count > 0)
                return (string)imdbUrls[0]; //return first IMDb result
            else
                return string.Empty;
        }
        
        public bool ImdbLookup(string MovieName, bool GetExtraInfo = true)
        {
            string imdburl;
            if((MovieName.Length == 9) && (MovieName.Substring(0, 2) == "tt")) {
                imdburl = "http://www.imdb.com/title/" + MovieName + "/";
            } else {
                imdburl = getIMDbUrlFromBing(MovieName);
            }
            if (!string.IsNullOrEmpty(imdburl))
            {
                string html = getUrlData(imdburl);
                parseIMDbPage(html, GetExtraInfo);
            }
            return true;
        }

        //parse IMDb page data
        private void parseIMDbPage(string html, bool GetExtraInfo)
        {
            Id = match(@"<link rel=""canonical"" href=""http://www.imdb.com/title/(tt\d{7})/"" />", html);
            if (Id == "") return;
            Title = match(@"<title>(.*?) \(.*?</title>", html);
            Year = match(@"<title>.*?\(.*?(\d{4}).*?\).*?</title>", html);
            Rating = match(@">(\d.\d)<span>/10", html);
            Genres = matchAll(@"<a.*?>(.*?)</a>", match(@"Genres:</h4>(.*?)</div>", html));
            Directors = matchAll(@"<a.*?>(.*?)</a>", match(@"Directors?:[\n\r\s]*</h4>(.*?)(</div>|>.?and )", html));
            Writers = matchAll(@"<a.*?>(.*?)</a>", match(@"Writers?:[\n\r\s]*</h4>(.*?)(</div>|>.?and )", html));
            Stars = matchAll(@"<a.*?>(.*?)</a>", match(@"Stars?:(.*?)</div>", html));
            Cast = matchAll(@"class=""name"">[\n\r\s]*<a.*?>(.*?)</a>", html);
            Plot = match(@"<p><p>(.*?)(<a|</p>)", html);
            ReleaseDate = match(@"Release Date:</h4>.*?(\d{1,2} (January|February|March|April|May|June|July|August|September|October|November|December) (19|20)\d{2}).*(\(|<span)", html);
            Runtime = match(@"Runtime:</h4>[\s]*.*?(\d{1,4}) min[\s]*.*?\<\/div\>", html);
            if (String.IsNullOrEmpty(Runtime)) Runtime = match(@"infobar.*?([0-9]+) min.*?</div>", html);
            Top250 = match(@"Top 250 #(\d{1,3})<", html);
            Oscars = match(@"Won (\d{1,2}) Oscars\.", html);
            Storyline = match(@"Storyline</h2>[\s]*<p>(.*?)[\s]*(<em|</p>)", html);
            Tagline = match(@"Taglines?:</h4>(.*?)(<span|</div)", html);
            MpaaRating = match(@"infobar"">.*?<img.*?alt=""(.*?)"" src="".*?certificates.*?"".*?>", html);
            MpaaRating = MpaaRating.Replace("_", "-");
            Votes = match(@"href=""ratings"".*?>(\d+,?\d*) votes</a>", html);
            Poster = match(@"img_primary"">[\n\r\s]*?<a.*?><img src=""(.*?)"".*?</td>", html);
            if (!string.IsNullOrEmpty(Poster) && Poster.IndexOf("nopicture") < 0)
            {
                PosterSmall = Poster.Substring(0, Poster.IndexOf("_V1.")) + "_V1._SY150.jpg";
                PosterLarge = Poster.Substring(0, Poster.IndexOf("_V1.")) + "_V1._SY500.jpg";
            }
            else
            {
                Poster = string.Empty;
                PosterSmall = string.Empty;
                PosterLarge = string.Empty;
            }
            if (!string.IsNullOrEmpty(Id))
            {
                ImdbURL = "http://www.imdb.com/title/" + Id + "/";
                if (GetExtraInfo)
                {
                    ReleaseDates = getReleaseDates();
                    MediaImages = getMediaImages();
                }
            }

        }

        //Get all release dates
        private ArrayList getReleaseDates()
        {
            ArrayList list = new ArrayList();
            string releasehtml = getUrlData("http://www.imdb.com/title/" + Id + "/releaseinfo");
            foreach (string r in matchAll(@"<tr>(.*?)</tr>", match(@"Date</th></tr>\n*?(.*?)</table>", releasehtml)))
            {
                Match rd = new Regex(@"<td>(.*?)</td>\n*?.*?<td align=""right"">(.*?)</td>", RegexOptions.Multiline).Match(r);
                list.Add(StripHTML(rd.Groups[1].Value.Trim()) + " = " + StripHTML(rd.Groups[2].Value.Trim()));
            }
            return list;
        }

        //Get all media images
        private ArrayList getMediaImages()
        {
            ArrayList list = new ArrayList();
            string mediaurl = "http://www.imdb.com/title/" + Id + "/mediaindex";
            string mediahtml = getUrlData(mediaurl);
            int pagecount = matchAll(@"<a href=""\?page=(.*?)"">", match(@"<span style=""padding: 0 1em;"">(.*?)</span>", mediahtml)).Count;
            for (int p = 1; p <= pagecount + 1; p++)
            {
                mediahtml = getUrlData(mediaurl + "?page=" + p);
                foreach (Match m in new Regex(@"src=""(.*?)""", RegexOptions.Multiline).Matches(match(@"<div class=""thumb_list"" style=""font-size: 0px;"">(.*?)</div>", mediahtml)))
                    list.Add(m.Groups[1].Value);
            }
            return list;
        }

        //Match single instance
        private string match(string regex, string html, int i = 1)
        {
            return new Regex(regex, RegexOptions.Multiline).Match(html).Groups[i].Value;
        }

        //Match all instances and return as ArrayList
        private ArrayList matchAll(string regex, string html, int i = 1)
        {
            ArrayList list = new ArrayList();
            foreach (Match m in new Regex(regex, RegexOptions.Multiline).Matches(html))
                list.Add(m.Groups[i].Value);
            return list;
        }

        //Strip HTML Tags
        static string StripHTML(string inputString)
        {
            return Regex.Replace(inputString, @"<.*?>", string.Empty);
        }

        //Get URL Data
        private string getUrlData(string url)
        {
            WebClient client = new WebClient();
            Stream datastream = client.OpenRead(url);
            StreamReader reader = new StreamReader(datastream);
            StringBuilder sb = new StringBuilder();
            while (!reader.EndOfStream)
                sb.Append(reader.ReadLine());
            return sb.ToString();
        }
        // isGenre
        public bool isGenre(string gen)
        {
            string [] genre1 = gen.Split('/');
            foreach (string genre2 in genre1)
            {
                genre2.Trim();
                foreach (string genre3 in Genres)
                    if(genre2.IndexOf(genre3, StringComparison.OrdinalIgnoreCase) >= 0)
                        return true;
            }
            return false;            
        }
     }
}