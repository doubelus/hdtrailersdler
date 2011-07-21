using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using Sloppycode.net;

namespace HDTrailersNETDownloader
{
    public class MovieItem
    {
        public String url;
        public String name;
        public String imdbId;
        public String pubDate;
        public NameValueCollection nvc;
        public bool needLoad = true;

        public MovieItem(string name, string url, string pubDate)
        {
            this.name = name;
            this.url = url;
            this.pubDate = pubDate;
            nvc = new NameValueCollection();
        }
    }

    abstract public class GenericFetcher : CollectionBase
    {
        public List<string> validurls;
        abstract public void LoadItem(MovieItem mi);
        abstract public void GetFeedItems(string url);

        public MovieItem this[int index]
        {
            get
            {
                //we must cast our return object as PhoneNumber 
                MovieItem mi = (MovieItem)this.List[index];
                if (mi.needLoad)
                {
                    LoadItem(mi);
                    mi.needLoad = false;
                }

                return mi;
            }
            set
            {
                //warning, this is not for adding, but for reassigning 
                //this will throw an exception if the index does not already 
                //exist. Use Add(phoneNumber) to add to collection 
                this.List[index] = value;
            }
        }

        public void Add(MovieItem movieItem)
        {
            this.List.Add(movieItem);
        }
        public string IsUrlValid(String urlstring)
        {
            foreach (string urltemp in validurls) // Loop through List with foreach
            {
                if (String.Compare(urltemp.ToUpperInvariant(), urlstring.ToUpperInvariant(), StringComparison.Ordinal) == 0)
                {
                    return urltemp;
                }
            }
            return System.String.Empty; 
        }

        public bool Remove(int index)
        {
            try
            {
                this.List.Remove(index);
                return true;
            }
            catch (Exception e)
            {
                Program.log.WriteLine("Exception in Remove");
                Program.log.WriteLine(e.ToString());
                return false;
            }
        }

    }
    abstract public class GenericFetcherRSS : GenericFetcher
    {
        public override void GetFeedItems(string url)
        {
            try
            {
                RssReader reader = new RssReader();
                RssFeed feed = reader.Retrieve(url);
                RssItems feedItems = feed.Items;
                for (int i = 0; i < feedItems.Count; i++)
                {
                    Add(new MovieItem(feedItems[i].Title, feedItems[i].Link, feedItems[i].Pubdate));
                }
            }
            catch (Exception e)
            {
                Program.log.WriteLine("ERROR: Could not get feed: " + url + " Exception to follow.");
                Program.log.WriteLine(e.Message);
                return;
            }
        }
    }
}
