using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.Net;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

/*******************************************************************************
 * Free ASP.net IMDb Scraper API for the new IMDb Template.
 * Author: Abhinay Rathore
 * Website: http://www.AbhinayRathore.com
 * Blog: http://web3o.blogspot.com
 * More Info: http://web3o.blogspot.com/2010/11/aspnetc-imdb-scraping-api.html
 * Last Updated: Feb 20, 2013
 *******************************************************************************/

namespace IMDb_Scraper
{
    public class IMDb
    {
        public bool status { get; set; }
        public string Id { get; set; }
        public string Title { get; set; }
        public string OriginalTitle { get; set; }
        public string Year { get; set; }
        public string Rating { get; set; }
        public ArrayList Genres { get; set; }
        public ArrayList Directors { get; set; }
        public ArrayList Writers { get; set; }
        public ArrayList Cast { get; set; }
        public ArrayList Producers { get; set; }
        public ArrayList Musicians { get; set; }
        public ArrayList Cinematographers { get; set; }
        public ArrayList Editors { get; set; }
        public string MpaaRating { get; set; }
        public string ReleaseDate { get; set; }
        public string Plot { get; set; }
        public ArrayList PlotKeywords { get; set; }
        public string Poster { get; set; }
        public string PosterLarge { get; set; }
        public string PosterFull { get; set; }
        public string Runtime { get; set; }
        public string Top250 { get; set; }
        public string Oscars { get; set; }
        public string Awards { get; set; }
        public string Nominations { get; set; }
        public string Storyline { get; set; }
        public string Tagline { get; set; }
        public string Votes { get; set; }
        public ArrayList Languages { get; set; }
        public ArrayList Countries { get; set; }
        public ArrayList ReleaseDates { get; set; }
        public ArrayList MediaImages { get; set; }
        public ArrayList RecommendedTitles { get; set; }
        public string ImdbURL { get; set; }

        //Search Engine URLs
        private string GoogleSearch = "http://www.google.com/search?q=site:imdb.com+";
        private string BingSearch = "http://www.bing.com/search?q=imdb+";
        private string AskSearch = "http://www.ask.com/web?q=imdb+";

        //Constructor
        public IMDb()
        {
        }

        public IMDb(string MovieName, bool GetExtraInfo = true)
        {
            string imdbUrl = getIMDbUrl(System.Uri.EscapeUriString(MovieName));
            status = false;
            if (!string.IsNullOrEmpty(imdbUrl))
            {
                parseIMDbPage(imdbUrl, GetExtraInfo);
            }
        }

        //Get IMDb URL from search results
        private string getIMDbUrl(string MovieName, string searchEngine = "google")
        {
            string url;
            DateTime thisyear = DateTime.Now;
            string thisyearstr = thisyear.Year.ToString();
            DateTime lastyear = DateTime.Now.AddYears(-1);
            string lastyearstr = lastyear.Year.ToString();
//            string url = GoogleSearch + MovieName; //default to Google search
//            if (searchEngine.ToLower().Equals("bing")) url = BingSearch + MovieName;
//            if (searchEngine.ToLower().Equals("ask")) url = AskSearch + MovieName;

//            string url = GoogleSearch + System.Uri.EscapeUriString(MovieName) + System.Uri.EscapeUriString(" and (" + thisyearstr + " or " + lastyearstr + ")");  //default to Google search
            url = GoogleSearch + " " +System.Web.HttpUtility.UrlEncode(MovieName) + " IMDb (" + thisyearstr + " or " + lastyearstr + ")";
            if (searchEngine.ToLower().Equals("bing")) url = BingSearch + MovieName + " (" + thisyearstr + " or " + lastyearstr + ")";
            if (searchEngine.ToLower().Equals("ask")) url = AskSearch + MovieName + "(" + thisyearstr + " or " + lastyearstr + ")";
            string html = getUrlData(url);
//            ArrayList imdbUrls = matchAll(@"<a href=""(http://www.imdb.com/title/tt\d{7}/)"".*?>.*?</a>", html);

            ArrayList imdbUrls = GetImdbLinks(html);

            if (imdbUrls.Count > 0)
            {
                string tmpstr = (string)imdbUrls[0];
                tmpstr = tmpstr.Substring(0, NthIndexOf(tmpstr,"/", 5)+1);
                return tmpstr; //return first IMDb result
            }
            else if (searchEngine.ToLower().Equals("google"))
            {
                //if Google search fails
                return getIMDbUrl(MovieName, "bing"); //search using Bing
            }
            else if (searchEngine.ToLower().Equals("bing"))
            {
                //if Bing search fails
                return getIMDbUrl(MovieName, "ask"); //search using Ask
            }
            else
            {
                //search fails
                return string.Empty;
            }
        }
        public ArrayList FindImdbMovieEntries(ArrayList inlist)
        {
            ArrayList outlist = new ArrayList();
            foreach (string s in inlist)
            {
                if (s.Contains("/title/tt"))
                {
                    outlist.Add(s);
                }
            }
            return (outlist);
        }
        public ArrayList GetImdbLinks(string message)
        {
            ArrayList list = new ArrayList();
            ArrayList list2 = new ArrayList();
            Regex urlRx = new Regex(@"((https?|ftp|file)\://|www.)[A-Za-z0-9\.\-]+(/[A-Za-z0-9\?\&\=;\+!'\(\)\*\-\._~%]*)*", RegexOptions.IgnoreCase);

            MatchCollection matches = urlRx.Matches(message);
            foreach (Match match in matches)
            {
                list.Add(match.Value);
            }
            list2 = FindImdbMovieEntries(list);
            return list2;
        }
        public static int NthIndexOf(string target, string value, int n)
        {
            Match m = Regex.Match(target, "((" + value + ").*?){" + n + "}");

            if (m.Success)
                return m.Groups[2].Captures[n - 1].Index;
            else
                return -1;
        }
        public bool ImdbLookup(string MovieName, bool GetExtraInfo = true)
        {
            string imdburl;
            if ((MovieName.Length == 9) && (MovieName.Substring(0, 2) == "tt"))
            {
                imdburl = "http://www.imdb.com/title/" + MovieName + "/";
            }
            else
            {
                imdburl = getIMDbUrl(MovieName);
            }
            if (!string.IsNullOrEmpty(imdburl))
            {
//                string html = getUrlData(imdburl);
//                parseIMDbPage(imdburl, GetExtraInfo);
                return parseIMDbPage(imdburl, GetExtraInfo);
            }
            return false;
        }

        //Parse IMDb page data
        private bool parseIMDbPage(string imdbUrl, bool GetExtraInfo)
        {
            int istart, iend;
            string substr1;
            string html = getUrlData(imdbUrl+"combined");
            if (!string.IsNullOrEmpty(html))
            {
                status = true;
                Id = match(@"<link rel=""canonical"" href=""http://www.imdb.com/title/(tt\d{7})/combined"" />", html);
                Title = match(@"<title>(IMDb \- )*(.*?) \(.*?</title>", html, 2);
                Title = System.Web.HttpUtility.HtmlDecode(Title);
                Title = System.Web.HttpUtility.HtmlDecode(Title);
                OriginalTitle = System.Web.HttpUtility.HtmlDecode(match(@"title-extra"">(.*?)<", html));
                OriginalTitle = System.Web.HttpUtility.HtmlDecode(OriginalTitle);
                istart = html.IndexOf("<title>");
                iend = html.IndexOf("</title>")+ 8;
                substr1 = html.Substring(istart, iend-istart);
                Year = match(@"<title>.*?\(.*?(\d{4}).*?\).*?</title>", substr1);
                Rating = match(@"<b>(\d.\d)/10</b>", html);
                Genres = matchAll(@"<a.*?>(.*?)</a>", match(@"Genre.?:(.*?)(</div>|See more)", html));
                Directors = new ArrayList();
                Directors = matchAll(@"<td valign=""top""><a.*?href=""/name/.*?/"">(.*?)</a>", match(@"Directed by</a></h5>(.*?)</table>", html));
                Writers = matchAll(@"<td valign=""top""><a.*?href=""/name/.*?/"">(.*?)</a>", match(@"Writing credits</a></h5>(.*?)</table>", html));
                Producers = matchAll(@"<td valign=""top""><a.*?href=""/name/.*?/"">(.*?)</a>", match(@"Produced by</a></h5>(.*?)</table>", html));
                Musicians = matchAll(@"<td valign=""top""><a.*?href=""/name/.*?/"">(.*?)</a>", match(@"Original Music by</a></h5>(.*?)</table>", html));
                Cinematographers = matchAll(@"<td valign=""top""><a.*?href=""/name/.*?/"">(.*?)</a>", match(@"Cinematography by</a></h5>(.*?)</table>", html));
                Editors = matchAll(@"<td valign=""top""><a.*?href=""/name/.*?/"">(.*?)</a>", match(@"Film Editing by</a></h5>(.*?)</table>", html));
                Cast = matchAll(@"<td class=""nm""><a.*?href=""/name/.*?/"".*?>(.*?)</a>", match(@"<h3>Cast</h3>(.*?)</table>", html));
                Plot = System.Web.HttpUtility.HtmlDecode(match(@"Plot:</h5>.*?<div class=""info-content"">(.*?)(<a|</div)", html));
                Plot = System.Web.HttpUtility.HtmlDecode(Plot);
                Plot = Plot.Replace("|", "");
                Plot = Plot.Trim();
                PlotKeywords = matchAll(@"<a.*?>(.*?)</a>", match(@"Plot Keywords:</h5>.*?<div class=""info-content"">(.*?)</div", html));
                ReleaseDate = match(@"Release Date:</h5>.*?<div class=""info-content"">.*?(\d{1,2} (January|February|March|April|May|June|July|August|September|October|November|December) (19|20)\d{2})", html);
                Runtime = match(@"Runtime:</h5><div class=""info-content"">(\d{1,4}) min[\s]*.*?</div>", html);
                Top250 = match(@"Top 250: #(\d{1,3})<", html);
                Oscars = match(@"Won (\d+) Oscars?\.", html);
                if (string.IsNullOrEmpty(Oscars) && "Won Oscar.".Equals(match(@"(Won Oscar\.)", html))) Oscars = "1";
                Awards = match(@"(\d{1,4}) wins", html);
                Nominations = match(@"(\d{1,4}) nominations", html);
                Tagline = System.Web.HttpUtility.HtmlDecode(match(@"Tagline:</h5>.*?<div class=""info-content"">(.*?)(<a|</div)", html));
                Tagline = System.Web.HttpUtility.HtmlDecode(Tagline);
                MpaaRating = match(@"MPAA</a>:</h5><div class=""info-content"">Rated (G|PG|PG-13|PG-14|R|NC-17|X) ", html);
                if (MpaaRating.Length == 0)
                {
                    // If MPAA rating is not set check the Certification
                    MpaaRating = match(@"Certification:</h5><div class=""info-content""><a href=""/search/title\?certificates=us:g"">USA:(G|PG|PG-13|PG-14|R|NC-17|X)", html);
                }
                Votes = match(@">(\d+,?\d*) votes<", html);
                Languages = matchAll(@"<a.*?>(.*?)</a>", match(@"Language.?:(.*?)(</div>|>.?and )", html));
                Countries = matchAll(@"<a.*?>(.*?)</a>", match(@"Country:(.*?)(</div>|>.?and )", html));
                Poster = match(@"<div class=""photo"">.*?<a name=""poster"".*?><img.*?src=""(.*?)"".*?</div>", html);
                if (!string.IsNullOrEmpty(Poster) && Poster.IndexOf("media-imdb.com") > 0)
                {
                    Poster = Regex.Replace(Poster, @"_V1.*?.jpg", "_V1._SY200.jpg");
                    PosterLarge = Regex.Replace(Poster, @"_V1.*?.jpg", "_V1._SY500.jpg");
                    PosterFull = Regex.Replace(Poster, @"_V1.*?.jpg", "_V1._SY0.jpg");
                }
                else
                {
                    Poster = string.Empty;
                    PosterLarge = string.Empty;
                    PosterFull = string.Empty;
                }
                ImdbURL = "http://www.imdb.com/title/" + Id + "/";
                if (GetExtraInfo)
                {
                    string plotHtml = getUrlData(imdbUrl + "plotsummary");
                    Storyline = match(@"<p class=""plotpar"">(.*?)(<i>|</p>)", plotHtml);
                    ReleaseDates = getReleaseDates();
                    MediaImages = getMediaImages();
                    RecommendedTitles = getRecommendedTitles();
                }
                return true;
            } else {
                Id = null;
                return false;
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
                {
                    String image = m.Groups[1].Value;
                    list.Add(Regex.Replace(image, @"_V1\..*?.jpg", "_V1._SY0.jpg"));
                }
            }
            return list;
        }

        //Get Recommended Titles
        private ArrayList getRecommendedTitles()
        {
            ArrayList list = new ArrayList();
            string recUrl = "http://www.imdb.com/widget/recommendations/_ajax/get_more_recs?specs=p13nsims%3A" + Id;
            string json = getUrlData(recUrl);
            list = matchAll(@"title=\\""(.*?)\\""", json);
            HashSet<String> set = new HashSet<string>();
            foreach(String rec in list) set.Add(rec);
            return new ArrayList(set.ToList());
        }

        /*******************************[ Helper Methods ]********************************/

        //Match single instance
        private string match(string regex, string html, int i = 1)
        {
            return new Regex(regex, RegexOptions.Multiline).Match(html).Groups[i].Value.Trim();
        }

        //Match all instances and return as ArrayList
        private ArrayList matchAll(string regex, string html, int i = 1)
        {
            ArrayList list = new ArrayList();
            foreach (Match m in new Regex(regex, RegexOptions.Multiline).Matches(html))
                list.Add(m.Groups[i].Value.Trim());
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
            try
            {
                WebClient client = new WebClient();
                Random r = new Random();
                //Random IP Address
                client.Headers["X-Forwarded-For"] = r.Next(0, 255) + "." + r.Next(0, 255) + "." + r.Next(0, 255) + "." + r.Next(0, 255);
                //Random User-Agent
                client.Headers["User-Agent"] = "Mozilla/" + r.Next(3, 5) + ".0 (Windows NT " + r.Next(3, 5) + "." + r.Next(0, 2) + "; rv:2.0.1) Gecko/20100101 Firefox/" + r.Next(3, 5) + "." + r.Next(0, 5) + "." + r.Next(0, 5);
                Stream datastream = client.OpenRead(url);
                StreamReader reader = new StreamReader(datastream);
                StringBuilder sb = new StringBuilder();
                while (!reader.EndOfStream)
                    sb.Append(reader.ReadLine());
                return sb.ToString();
            }
            catch (Exception e)
            {
//                log.WriteLine("1. Unhandled Exception....");
//                log.WriteLine("Exception: " + e.Message);
                return "";
            }

        }
        // isGenre
        public bool isGenre(string gen)
        {
            if (Genres != null)
            {
                string[] genre1 = gen.Split('/');
                foreach (string genre2 in genre1)
                {
                    genre2.Trim();
                    foreach (string genre3 in Genres)
                        if (genre2.IndexOf(genre3, StringComparison.OrdinalIgnoreCase) >= 0)
                            return true;
                }
            }
            return false;
        }
        // isLanguage
        public bool isLanguage(string lang)
        {
            if (Languages != null)
            {
                string[] lang1 = lang.Split('/');
                foreach (string lang2 in lang1)
                {
                    lang2.Trim();
                    foreach (string lang3 in Languages)
                        if (lang2.IndexOf(lang3, StringComparison.OrdinalIgnoreCase) >= 0)
                            return true;
                }
            }
            return false;
        }
    }
}
