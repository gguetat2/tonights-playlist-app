
namespace YoutubeQuery
{
    public class MusicEvent
    {
        public string venue { get; set; }
        public string city { get; set; }
        public string zip { get; set; }
        public string fulladdress { get; set; }
        public string date { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
        public int id { get; set; }

        public MusicEvent(string _venue, string _city, string _zip, string _fulladdress, string _date, string _created_at, string _updated_at, int _id)
        {
            venue = _venue;
            city = _city;
            zip = _zip;
            fulladdress = _fulladdress;
            date = _date;
            created_at = _created_at;
            updated_at = _updated_at;
            id = _id;
        }
    }
}
