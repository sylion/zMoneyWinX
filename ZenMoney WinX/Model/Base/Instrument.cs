using Newtonsoft.Json;
using SQLite;

namespace zMoneyWinX.Model
{
    [JsonObject, Table("Instrument")]
    public partial class Instrument : DBObject
    {
        [JsonProperty("id"), PrimaryKey]
        public int Id { get; set; }

        [JsonIgnore, Ignore]
        private string _Title { get; set; }

        [JsonProperty("shortTitle")]
        public string ShortTitle { get; set; }

        [JsonIgnore, Ignore]
        private string _Symbol { get; set; }

        [JsonProperty("changed")]
        public long Changed { get; set; }

        [JsonProperty("rate")]
        public double Rate { get; set; }
    }
}
