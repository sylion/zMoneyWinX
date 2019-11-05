using Newtonsoft.Json;
using SQLite;

namespace zMoneyWinX.Model
{
    [JsonObject, Table("Country")]
    public class Country : DBObject
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("currency")]
        public int Currency { get; set; }

        [JsonProperty("domain")]
        public string Domain { get; set; }
    }
}