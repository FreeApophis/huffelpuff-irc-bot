/*
 *  The Radio Plugin controls a Radio Stream with the liquidsoap API
 * 
 *  Copyright (c) 2008-2009 Thomas Bruderer <apophis@apophis.ch>
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>. 
 */
 
using System;
using System.IO;
using System.Net;
using System.Data;
using System.Timers;
using System.Reflection;
using System.Net.Sockets;
using System.Collections.Generic;

using Huffelpuff;
using Huffelpuff.SimplePlugins;

using Meebey.SmartIrc4net;

using MySql.Data;
using MySql.Data.MySqlClient;

namespace Plugin
{
    class RadioPlugin : IPlugin
    {
        private static string[] GenreArray = {
            "Blues", "Classic Rock", "Country", "Dance", "Disco", "Funk", "Grunge", "Hip-Hop", "Jazz", "Metal",
            "New Age", "Oldies", "Other", "Pop", "R&B", "Rap", "Reggae", "Rock", "Techno", "Industrial",
            "Alternative", "Ska", "Death Metal", "Pranks", "Soundtrack", "Euro-Techno", "Ambient", "Trip-Hop", "Vocal", "Jazz+Funk",
            "Fusion", "Trance", "Classical", "Instrumental", "Acid", "House", "Game", "Sound Clip", "Gospel", "Noise",
            "AlternRock", "Bass", "Soul", "Punk", "Space", "Meditative", "Instrumental Pop", "Instrumental Rock", "Ethnic", "Gothic",
            "Darkwave", "Techno-Industrial", "Electronic", "Pop-Folk", "Eurodance", "Dream", "Southern Rock",  "Comedy", "Cult", "Gangsta",
            "Top 40", "Christian Rap", "Pop/Funk", "Jungle", "Native American", "Cabaret", "New Wave", "Psychadelic", "Rave", "Showtunes",         
            "Trailer", "Lo-Fi", "Tribal", "Acid Punk", "Acid Jazz", "Polka", "Retro", "Musical", "Rock & Roll", "Hard Rock",
            "Folk", "Folk-Rock", "National Folk", "Swing", "Fast Fusion", "Bebob", "Latin", "Revival", "Celtic", "Bluegrass",
            "Avantgarde", "Gothic Rock", "Progressive Rock", "Psychedelic Rock", "Symphonic Rock", "Slow Rock", "Big Band", "Chorus", "Easy Listening", "Acoustic",
            "Humour", "Speech", "Chanson", "Opera", "Chamber Music", "Sonata", "Symphony", "Booty Bass", "Primus", "Porn Groove",
            "Satire", "Slow Jam", "Club", "Tango", "Samba", "Folklore", "Ballad", "Power Ballad", "Rhythmic Soul", "Freestyle",
            "Duet", "Punk Rock", "Drum Solo", "A capella", "Euro-House", "Dance Hall", "Reserved", "Reserved"
        };
        
        private class Artist {
            public Artist(int artistID, string name, string url, string image, string mbgid, string country, string state, string city, double latitude, double longitude)
            {
                this.artistID = artistID;
                this.name = name;
                this.url = url;
                this.image = image;
                this.mbgid = mbgid;
                this.country = country;
                this.state = state;
                this.city = city;
                this.longitude = longitude;
                this.latitude = latitude;
                albums = new List<Album>();
            }        
            public int artistID  { get; private set; }
            public string name { get; private set; }
            public string url { get; private set; }
            public string image { get; private set; }
            public string mbgid { get; private set; }
            public string country { get; private set; }
            public string state { get; private set; }
            public string city { get; private set; }
            public double latitude { get; private set; }
            public double longitude { get; private set; }
            public List<Album> albums { get; private set; }

        }
        
        private class Album {
            public Album(int albumID, Artist artist, string name, int genre, string mbgid, string license_artwork, DateTime releasedate, string url, string filename, double rating) {
                this.albumID = albumID;
                
                this.artist= artist;
                this.artistID= artist.artistID;
                this.artist.albums.Add(this);
                
                this.name = name;
                this.genre= genre;
                this.mbgid = mbgid;
                this.license_artwork = license_artwork;
                this.releasedate = releasedate;
                this.url = url;
                this.filename = filename;
                this.rating = rating;
                if(rating > 0.0) {
                    if (rating > maxRating)
                        maxRating = rating;
                    if (rating < minRating)
                        minRating = rating;
                }
                
                this.existsOnNeptun = false;
                
                this.tracks = new List<Track>();
            }
            public int albumID  { get; private set; }
            public int artistID  { get; private set; }
            public Artist artist  { get; private set; }
            public string name { get; private set; }
            public int genre { get; private set; }
            public string mbgid { get; private set; }
            public string license_artwork { get; private set; }
            public DateTime releasedate { get; private set; }
            public string url { get; private set; }
            public string filename { get; private set; }
            public double rating { get; private set; }
            public bool existsOnNeptun { get; set; }
            public List<Track> tracks { get; private set; }
            
            public static double maxRating = 0.0;
            public static double minRating = 1.0;
        }
        
        private class Track {
            public Track(int trackID, Album album, string name, string filename, double duration, int trackno, int genre, string license, string mbgid) {
                this.trackID = trackID;
                if(trackID > maxID) {
                    maxID = trackID;
                }
                
                this.album = album;
                this.albumID = album.albumID;
                this.album.tracks.Add(this);
                
                this.name = name;
                this.filename = filename;
                this.duration = duration;
                this.trackno = trackno;
                this.genre = genre;
                this.license = license;
                this.mbgid = mbgid;
                
                this.rating = album.rating;
                
                this.tags = new List<Tag>();
            }

        
            public int trackID { get; private set; }
            public int albumID { get; private set; }
            public Album album { get; private set; }
            public string name { get; private set; }
            public string filename { get; private set; }
            public double duration { get; private set; }
            public int trackno { get; private set; }
            public int genre { get; private set; }
            public string Genre { 
                get { 
                    return GenreArray[genre];
                }
            }
            public string license { get; private set; }
            public string mbgid { get; private set; }
            
            public double rating { get; private set; }
            public List<Tag> tags { get; private set; }
            
            public static int maxID = 0;

        }
        
        private class Tag {
            public Tag(Track track, string tagIdstr, double weight)
            {
                this.track = track;
                this.trackID = track.trackID;
                this.track.tags.Add(this);
                
                this.tagIdstr = tagIdstr;
                this.weight = weight;
            }
            
            public int trackID { get; private set; }
            public Track track { get; private set; }
            public string tagIdstr { get; private set; }
            public double weight { get; private set; }
        }
        
        private Dictionary<int, Artist> Artists;
        private Dictionary<int, Album> Albums;
        private Dictionary<int, Track> Tracks;

        
        public string Name {
            get {
                return Assembly.GetExecutingAssembly().FullName;
            }
        }
        
        private bool ready = false;
        public bool Ready {
            get {
                return ready;
            }
        }
        
        private bool active = false;
        
        public bool Active {
            get {
                return active;
            }
        }
        
        private MySqlConnection connection = new MySqlConnection(PersistentMemory.GetValue("MySQLConnection"));
        private IrcBot bot;
        public void Init(IrcBot botInstance)
        {
            bot = botInstance;
            
            // *** Artists Table ***
            Artists = new Dictionary<int, Artist>();
            MySqlCommand ArtistsCommand = new MySqlCommand("SELECT * FROM artists_new", connection);
            MySqlDataAdapter ArtistsAdapter = new MySqlDataAdapter(ArtistsCommand);
            DataTable ArtistsDatatable = new DataTable();  
            ArtistsAdapter.Fill(ArtistsDatatable);
            foreach(DataRow dr in ArtistsDatatable.Rows)
            {
                string mbgid = "";
                if(!(dr["mbgid"] is DBNull)) {
                    mbgid = (string)dr["mbgid"];
                }                
                Artists.Add(    (int)dr["artistID"], new Artist(
                    (int)dr["artistID"],
                    (string)dr["name"],
                    (string)dr["url"], 
                    (string)dr["image"],
                    mbgid, 
                    (string)dr["country"], 
                    (string)dr["state"], 
                    (string)dr["city"], 
                    (double)dr["latitude"], 
                    (double)dr["longitude"])
                   );
            }

            // *** Albums Table ***
            Albums = new Dictionary<int, Album>();
            MySqlCommand AlbumsCommand = new MySqlCommand("SELECT * FROM albums_new", connection);
            MySqlDataAdapter AlbumsAdapter = new MySqlDataAdapter(AlbumsCommand);
            DataTable AlbumsDatatable = new DataTable();  
            AlbumsAdapter.Fill(AlbumsDatatable);
            foreach(DataRow dr in AlbumsDatatable.Rows)
            {
                string mbgid = "";
                if(!(dr["mbgid"] is DBNull)) {
                    mbgid = (string)dr["mbgid"];
                } 
                double rating = 0.0;
                if(!(dr["rating"] is DBNull)) {
                    rating = (double)dr["rating"];
                } 
                Albums.Add((int)dr["albumID"], new Album(
                    (int)dr["albumID"],
                    Artists[(int)dr["artistID"]],
                    (string)dr["name"],
                    (int)dr["genre"],
                    mbgid,
                    (string)dr["license_artwork"],
                    (DateTime)dr["releasedate"],
                    (string)dr["url"],
                    (string)dr["filename"],
                    rating)
                );
            }

            // *** Tracks Table ***
            Tracks = new Dictionary<int, Track>();
            MySqlCommand TracksCommand = new MySqlCommand("SELECT * FROM tracks_new", connection);
            MySqlDataAdapter TracksAdapter = new MySqlDataAdapter(TracksCommand);
            DataTable TracksDatatable = new DataTable();  
            TracksAdapter.Fill(TracksDatatable);
            foreach(DataRow dr in TracksDatatable.Rows)
            {
                string mbgid = "";
                if(!(dr["mbgid"] is DBNull)) {
                    mbgid = (string)dr["mbgid"];
                } 
                double duration = 0.0;
                if(!(dr["duration"] is DBNull)) {
                    duration = (double)dr["duration"];
                } 
                Tracks.Add((int)dr["trackID"], new Track(
                    (int)dr["trackID"],
                    Albums[(int)dr["albumID"]],
                    (string)dr["name"],
                    (string)dr["filename"],
                    duration,
                    (int)dr["trackno"],
                    (int)dr["genre"],
                    (string)dr["license"],
                    mbgid)
                );
            }

            // *** Add Tags ***
            MySqlCommand TagsCommand = new MySqlCommand("SELECT * FROM tags_new", connection);
            MySqlDataAdapter TagsAdapter = new MySqlDataAdapter(TagsCommand);
            DataTable TagsDatatable = new DataTable();  
            TagsAdapter.Fill(TagsDatatable);
            foreach(DataRow dr in TagsDatatable.Rows)
            {
                new Tag(Tracks[(int)dr["trackID"]], (string)dr["tagIdstr"], (double)dr["weight"]);
            }
            
            Console.WriteLine("Artists : " + Artists.Count);
            Console.WriteLine("Albums  : " + Albums.Count);
            Console.WriteLine("Tracks  : " + Tracks.Count);

            scheduler.Elapsed += new ElapsedEventHandler(mainScheduler);

            ready = true;
        }
        
        private string GetPath(int albumID)
        {
            int tenthousand = albumID - (albumID % 10000);
            int hundred = albumID - (albumID % 100);
            string path = "/media/neptun/RaidOnly/jamendo/" + tenthousand + "-" + (tenthousand + 9999) + "/" + hundred + "-" + (hundred + 99) + "/" + albumID + "/";
            return path;
        }
        
        private Random rand = new Random();
        private Track GetRandomSong()
        {
            // Probability Linear to Rating with minRating and maxRating as limits
            Track t = null;
            do {
                int key = rand.Next(Track.maxID);
                if (Tracks.ContainsKey(key)) {
                    t = Tracks[key];
                }
            } while((t==null) || (t.rating <= (rand.NextDouble() * (Album.maxRating-Album.minRating)) + Album.minRating));        
            return t;
        }
        
        private Timer scheduler = new Timer(22200);
        public void Activate()
        {
            scheduler.Enabled = true;
            active = true;
            bot.AddPublicCommand(new Commandlet("!request", "With !request <Song|Album|Artist> [Greetings] you can request a Song to be played on the Radio", HandleRequest, this));
        }

        
        public void Deactivate()
        {
            scheduler.Enabled = false;
            active = false;
        }
        
        public string AboutHelp()
        {
            return "The Plugin which brings Jamendo to your radio!";
        }

        private Dictionary<int, Track> scheduledSongs = new Dictionary<int, Track>();
        private IPEndPoint liquidsoapServer = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1234);
        private int lastLiqID = -1;
        
        private void mainScheduler(object sender, ElapsedEventArgs e)
        {
            TcpClient telnet = new TcpClient();
            telnet.Connect(liquidsoapServer);
            
            TextReader reader = new StreamReader(telnet.GetStream());
            TextWriter writer = new StreamWriter(telnet.GetStream());
            ((StreamWriter)writer).AutoFlush = true;
            
            writer.WriteLine("request.queue");
            string response = reader.ReadLine();
            while (reader.ReadLine() != "END");
            if (response.Split(new char[] {' '}).Length < 5)
            {
                // if there are less than 5 songs in the queue (current + 4)
                // TODO: Alternatives to just random Songs
                Track newtrack = GetRandomSong();
                
                // TODO: Check if file exists, or get next Song
                writer.WriteLine("request.push " + GetPath(newtrack.albumID) + String.Format("{0:00}", newtrack.trackno) + " - " + newtrack.name + ".ogg");
                response = reader.ReadLine();

                scheduledSongs.Add(int.Parse(response), newtrack);
                while (reader.ReadLine() != "END");
            }            
            
            writer.WriteLine("on_air");
            response = reader.ReadLine();
            int currentSong  = int.Parse(response.Split(new char[] {' '})[0]);
            while (reader.ReadLine() != "END");
            
            if (currentSong != lastLiqID) {
                if (scheduledSongs.ContainsKey(lastLiqID)) {
                    scheduledSongs.Remove(lastLiqID);
                }                
                if (scheduledSongs.ContainsKey(currentSong)) {
                    Track song = scheduledSongs[currentSong];
                    
                    //bot.SendMessage
                    bot.SendMessage(SendType.Message, "Apophis", "SONG PLAYING: " + song.name + "(" + song.album.name + " by " + song.album.artist.name + ") Rating: " + song.rating + " Genre:" + song.Genre);
                    string s = "";
                    foreach(Tag tag in song.tags) {
                        s =  tag.tagIdstr + " [" + tag.weight + "] | ";
                    }
                    bot.SendMessage(SendType.Message, "Apophis", "Released:" + song.album.releasedate + "Tags: " + s);
                    bot.SendMessage(SendType.Message, "Apophis", "License: " + song.license);
                    bot.SendMessage(SendType.Message, "Apophis", "Cover: http://imgjam.com/albums/" + song.albumID + "/covers/1.300.jpg licensed as: "  + song.album.license_artwork);
                    bot.SendMessage(SendType.Message, "Apophis", "Artist: " + song.album.artist.image + " From: " + song.album.artist.city + "/" + song.album.artist.state + "/" + song.album.artist.country);
                    
                }
                lastLiqID = currentSong;
            }
            
            telnet.Close();
        }
        
        private void HandleRequest(object sender, IrcEventArgs e)
        {
            //TODO: Request Handler
        }
        
    }
}