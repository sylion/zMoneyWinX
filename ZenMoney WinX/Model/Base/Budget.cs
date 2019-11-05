using Newtonsoft.Json;
using SQLite;
using System;

namespace zMoneyWinX.Model
{
    [JsonObject, Table("Budget")]
    public partial class Budget : DBObject
    {
        [JsonProperty("user")]
        public int User { get; set; }

        [JsonProperty("changed")]
        public long Changed { get; set; }

        [JsonIgnore]
        public DateTime Date { get; set; }

        [JsonProperty("tag")]
        public Guid? Tag { get; set; }

        [JsonProperty("income")]
        public double Income { get; set; }

        [JsonProperty("outcome")]
        public double Outcome { get; set; }

        [JsonProperty("incomeLock")]
        public bool IncomeLock { get; set; }

        [JsonProperty("outcomeLock")]
        public bool OutcomeLock { get; set; }
    }
}
