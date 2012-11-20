
namespace YoutubeQuery
{
    class Track
    {
        public string artist {get;set;}
        public string name {get; set;}
        public string album { get; set; }
        public string source {get;set;}
        public string sourceid {get;set;}
        public string event_id {get;set;}
        public string created_at { get; set; }
        public string updated_at { get; set; }
        public string id { get; set; }
        
        public Track(string _artist, string _name, string _album, string _source, string _sourceid, string _event_id, string _created_at, string _updated_at, string _id)
        {
            artist = _artist;
            name = _name;
            album = _album;
            source = _source;
            sourceid = _sourceid;
            event_id = _event_id;
            created_at = _created_at;
            updated_at = _updated_at;
            id = _id;
        }
    }
}
