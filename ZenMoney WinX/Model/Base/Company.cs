using Newtonsoft.Json;
using SQLite;

namespace zMoneyWinX.Model
{
    [JsonObject, Table("Company")]
    public partial class Company : DBObject
    {
        [JsonProperty("id"), PrimaryKey]
        public int Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("www")]
        public string Www { get; set; }

        [JsonProperty("country")]
        public int? Country { get; set; }

        [JsonProperty("fullTitle")]
        public string FullTitle { get; set; }

        [JsonProperty("changed")]
        public long Changed { get; set; }

        [JsonProperty("countryCode")]
        public string countryCode { get; set; }
    }
}
