using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Newtonsoft.Json;

namespace YoutubeQuery
{
    class Program
    {
        static void Main(string[] args)
        {
            ExcecuteProgram();
            Console.WriteLine("Complete");
            Console.ReadLine();
        }

        private static void ExcecuteProgram()
        {
            List<MusicEvent> musicEvents = GetEventsList();
            foreach (var musicEvent in musicEvents)
            {
                try
                {
                    int eventID = musicEvent.id;
                    List<Track> currentTracks = GetTracksForEvent(eventID);

                    foreach (var currentTrack in currentTracks)
                    {
                        if (true)//string.IsNullOrEmpty(currentTrack.sourceid) || string.IsNullOrEmpty(currentTrack.name))
                        {
                            var udpatedTrack = GetUpdatedInfo(currentTrack);
                            UpdateTrack(eventID, udpatedTrack);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        /// <summary>
        /// Updates track information for a given event
        /// </summary>
        /// <param name="eventID">Event where the track belongs</param>
        /// <param name="currentTrack">The track information to be updated</param>
        private static void UpdateTrack(int eventID, Track currentTrack)
        {
            string jsonTrack = JsonConvert.SerializeObject(currentTrack);

            WebRequest trackPostRequest = WebRequest.Create(
                                string.Format(@"http://tonightsplaylist.herokuapp.com/events/{0}/tracks/{1}.json", eventID, currentTrack.id));
            trackPostRequest.Credentials = CredentialCache.DefaultCredentials;
            trackPostRequest.Method = "PUT";
            trackPostRequest.ContentType = "application/json";
            byte[] byteArray = Encoding.UTF8.GetBytes(jsonTrack);
            trackPostRequest.ContentLength = byteArray.Length;

            Stream dataStream = trackPostRequest.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();

            var response = trackPostRequest.GetResponse();
            using (var responseReader = new StreamReader(response.GetResponseStream()))
            {
                var responseContent = responseReader.ReadToEnd();
                Console.WriteLine(responseContent);
            }
        }

        /// <summary>
        /// Returns the list of tracks for a given event
        /// </summary>
        /// <param name="eventID">Event to get the tracks for</param>
        /// <returns>List of all tracks</returns>
        private static List<Track> GetTracksForEvent(int eventID)
        {
            string url = "http://tonightsplaylist.herokuapp.com/events/" + eventID.ToString() + "/tracks.json";
            WebRequest getEventTracksRequest = WebRequest.Create(url);
            getEventTracksRequest.Method = "GET";
            WebResponse getEventTracksResponse = getEventTracksRequest.GetResponse();
            Stream tracksDataStream = getEventTracksResponse.GetResponseStream();
            StreamReader tracksSR = new StreamReader(tracksDataStream);

            List<Track> currentTracks = JsonConvert.DeserializeObject<List<Track>>(tracksSR.ReadToEnd());
            return currentTracks;
        }

        /// <summary>
        /// Gets all events from tonights playlsit
        /// </summary>
        /// <returns>Returns a list of MusicEvents</returns>
        private static List<MusicEvent> GetEventsList()
        {
            WebRequest getEvents = WebRequest.Create("http://tonightsplaylist.herokuapp.com/events.json");
            WebResponse getEventsResponse = getEvents.GetResponse();
            Stream eventsStream = getEventsResponse.GetResponseStream();
            StreamReader eventsSR = new StreamReader(eventsStream);
            string eventsRaw = eventsSR.ReadToEnd();
            List<MusicEvent> musicEvents = JsonConvert.DeserializeObject<List<MusicEvent>>(eventsRaw);
            return musicEvents;
        }

        public static Track GetUpdatedInfo(Track track)
        {
            string videoId = string.Empty;
            string numResults = "10";

            XElement document = GetResultsFromYoutubeSearch(track.artist, numResults);

            var entries = (from node in document.Elements()
                           where node.Name.LocalName == "entry" && !node.Elements().Any(e => e.Name.LocalName == "control")
                           select node).FirstOrDefault();
            if (null != entries)
            {
                var url = RetrieveUrlFromYoutubeResults(entries);
                string videoUrl = string.IsNullOrWhiteSpace(url) ? string.Empty : url;
                string tmp = videoUrl.Substring(26);
                int index = tmp.IndexOf('?');
                videoId = tmp.Substring(0, index);
                var title = entries.Elements().Where(n => n.Name.LocalName == "title").FirstOrDefault().Value.ToString();
                track.source = "Youtube";
                track.sourceid = videoId;
                track.name = title;
            }

            return track;
        }


        /// <summary>
        /// Retrieves the first video Url from a XMl payload returned by Youtube search
        /// </summary>
        /// <param name="entries">List of entries from youtube in XML</param>
        /// <returns>First url found</returns>
        private static string RetrieveUrlFromYoutubeResults(XElement entries)
        {
            var contentNode = (
                        from node in entries.Elements()
                        where node.Name.LocalName == "group"
                            && node.Elements().Any(n => n.Name.LocalName == "content"
                            && n.Attributes().Any(a => a.Name.LocalName == "url"))
                        select node
                        ).FirstOrDefault().Elements().First<XElement>(n => n.Name.LocalName == "content");

            var url = (from attr in contentNode.Attributes()
                       where attr.Name.LocalName == "url"
                       select attr).FirstOrDefault().Value;
            return url;
        }

        /// <summary>
        /// Returns a set of results from Youtube based on an artist search
        /// </summary>
        /// <param name="artistName">Name of the artist to search for</param>
        /// <param name="numResults">Number of results to return</param>
        /// <returns></returns>
        private static XElement GetResultsFromYoutubeSearch(string artistName, string numResults)
        {
            string baseUrl = "https://gdata.youtube.com/feeds/api/videos?";

            string queryUrl = baseUrl + "q=%22" + artistName + "%22&max-results=" + numResults + "&orderby=relevance&category=music";

            //https://gdata.youtube.com/feeds/api/videos?q="muse+supremacy"&max-results=1&alt=json&orderby=relevance

            WebResponse response = null;
            StreamReader sr = null;
            XElement document;
            try
            {
                WebRequest request = WebRequest.Create(queryUrl);
                response = request.GetResponse();
                Stream dataStream = response.GetResponseStream();
                sr = new StreamReader(dataStream);
                string responseFromServer = sr.ReadToEnd();
                XmlReader reader = XmlReader.Create(new StringReader(responseFromServer));
                document = XElement.Load(reader);
            }
            finally
            {
                sr.Close();
                response.Close();
            }
            return document;
        }
    }
}
