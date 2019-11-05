using Newtonsoft.Json;
using SQLite;

namespace zMoneyWinX.Model
{
    [JsonObject, Table("User")]
    public partial class User : DBObject
    {
        [JsonProperty("id"), PrimaryKey]
        public int Id { get; set; }

        [JsonProperty("country")]
        public int Country { get; set; }

        [JsonProperty("login")]
        public string Login { get; set; }

        [JsonProperty("changed")]
        public long Changed { get; set; }

        [JsonProperty("currency")]
        public int Currency { get; set; }

        [JsonProperty("paidTill")]
        public string PaidTill { get; set; }

        [JsonProperty("parent")]
        public string Parent { get; set; }

        [JsonProperty("countryCode")]
        public string countryCode { get; set; }
    }
}
