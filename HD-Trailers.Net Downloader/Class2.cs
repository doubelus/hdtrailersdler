using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Nfo.Movie
{
    [XmlRoot(
    ElementName = "movie",
    DataType = "Name"
)]
    public class NfoMovie
    {
        private String title = "";
        private String rating = "";
        private String year = "";
        private String releasedate = "";
        private String top250 = "";
        private String votes = "";
        private String outline = "";
        private String plot = "";
        private String tagline = "";
        private String runtime = "";
        private String thumb = "";
        private String mpaa = "";
        private String playcount = "";
        private String id = "";
        private String credits = "";
        private String director = "";
        private String premiered = "";
        private String studio = "";
        private String trailer = "";
        private String quality = "";
        private List<Actor> actors = new List<Actor>();
//        private List<String> genres = new List<String>();
        private String genre;

        [XmlElement(ElementName = "title")]
        public String Title
        {
            get { return title; }
            set { title = value; }
        }

        [XmlElement(ElementName = "rating")]
        public String Rating
        {
            get { return rating; }
            set { rating = value; }
        }

        [XmlElement(ElementName = "year")]
        public String Year
        {
            get { return year; }
            set { year = value; }
        }

        [XmlElement(ElementName = "releasedate")]
        public String Releasedate
        {
            get { return releasedate; }
            set { releasedate = value; }
        }

        [XmlElement(ElementName = "top250")]
        public String Top250
        {
            get { return top250; }
            set { top250 = value; }
        }

        [XmlElement(ElementName = "votes")]
        public String Votes
        {
            get { return votes; }
            set { votes = value; }
        }

        [XmlElement(ElementName = "outline")]
        public String Outline
        {
            get { return outline; }
            set { outline = value; }
        }

        [XmlElement(ElementName = "plot")]
        public String Plot
        {
            get { return plot; }
            set { plot = value; }
        }

        [XmlElement(ElementName = "tagline")]
        public String Tagline
        {
            get { return tagline; }
            set { tagline = value; }
        }

        [XmlElement(ElementName = "runtime")]
        public String Runtime
        {
            get { return runtime; }
            set { runtime = value; }
        }

        [XmlElement(ElementName = "thumb")]
        public String Thumb
        {
            get { return thumb; }
            set { thumb = value; }
        }

        [XmlElement(ElementName = "mpaa")]
        public String Mpaa
        {
            get { return mpaa; }
            set { mpaa = value; }
        }

        [XmlElement(ElementName = "playcount")]
        public String Playcount
        {
            get { return playcount; }
            set { playcount = value; }
        }

        [XmlElement(ElementName = "id")]
        public String Id
        {
            get { return id; }
            set { id = value; }
        }

        [XmlElement(ElementName = "credits")]
        public String Credits
        {
            get { return credits; }
            set { credits = value; }
        }

        [XmlElement(ElementName = "director")]
        public String Director
        {
            get { return director; }
            set { director = value; }
        }

        [XmlElement(ElementName = "premiered")]
        public String Premiered
        {
            get { return premiered; }
            set { premiered = value; }
        }

        [XmlElement(ElementName = "studio")]
        public String Studio
        {
            get { return studio; }
            set { studio = value; }
        }

        [XmlElement(ElementName = "quality")]
        public String Quality
        {
            get { return quality; }
            set { quality = value; }
        }

        [XmlElement(ElementName = "trailer")]
        public String Trailer
        {
            get { return trailer; }
            set { trailer = value; }
        }

        [XmlElement(ElementName = "actor")]
        public Actor[] Actor
        {
            get
            {
                Actor[] act = new Actor[actors.Count];
                actors.CopyTo(act);
                return act;
            }
            set
            {
                if (value == null) return;
                Actor[] newActors = (Actor[])value;
                actors.Clear();
                foreach (Actor newActor in newActors) actors.Add(newActor);
            }
        }

//        [XmlElement(ElementName = "genre")]
//        public ArrayList Genre
//        {
//            get
//            {
//                String[] gen = new String[genres.Count];
//                genres.CopyTo(gen);
//                return gen;
//            }
//            set
//            {
//                if (value == null) return;
//                String[] newGenres = value;
//                genres.Clear();
//                foreach (String newGenre in newGenres) genres.Add(newGenre);
//           }
//        }

        [XmlElement(ElementName = "genre")]
        public String Genre
        {
            get { return genre; }
            set { genre = value; }
        }
    }
}

public class Actor
{
    private String name;
    private String role;
    private String thumb;

    [XmlElement(ElementName = "name")]
    public String Name
    {
        get { return name; }
        set { name = value; }
    }

    [XmlElement(ElementName = "role")]
    public String Role
    {
        get { return role; }
        set { role = value; }
    }

    [XmlElement(ElementName = "thumb")]
    public String Thumb
    {
        get { return thumb; }
        set { thumb = value; }
    }
}