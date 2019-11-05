using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using SQLite;

namespace zMoneyWinX.Model
{
    [JsonObject, Table("Reminder")]
    public partial class Reminder : DBObject
    {
        [JsonProperty("id"), PrimaryKey]
        public Guid Id { get; set; }

        [JsonProperty("user")]
        public int User { get; set; }

        [JsonProperty("changed")]
        public long Changed { get; set; }

        [JsonProperty("income")]
        public double Income { get; set; }

        [JsonProperty("incomeAccount")]
        public Guid IncomeAccount { get; set; }

        [JsonProperty("incomeInstrument")]
        public int IncomeInstrument { get; set; }

        [JsonProperty("outcome")]
        public double Outcome { get; set; }

        [JsonProperty("outcomeAccount")]
        public Guid OutcomeAccount { get; set; }

        [JsonProperty("outcomeInstrument")]
        public int OutcomeInstrument { get; set; }

        [JsonProperty("tag"), Ignore]
        public List<Guid> Tags
        {
            get
            {
                if (!string.IsNullOrEmpty(_Tags))
                    return JsonConvert.DeserializeObject<List<Guid>>(_Tags);
                else
                    return null;
            }
            set
            {
                _Tags = JsonConvert.SerializeObject(value);
            }
        }

        [JsonProperty("step")]
        public int Step { get; set; }

        [JsonProperty("notify")]
        public bool Notify { get; set; }

        [JsonProperty("startDate")]
        public string StartDate { get; set; }

        [JsonProperty("endDate")]
        public string EndDate { get; set; }

        
        [JsonProperty("points"), Ignore]
        public List<int> Points
        {
            get
            {
                if (!string.IsNullOrEmpty(_Points))
                    return JsonConvert.DeserializeObject<List<int>>(_Points);
                else
                    return null;
            }
            set
            {
                _Points = JsonConvert.SerializeObject(value);
            }
        }

        [JsonProperty("interval")]
        public string Interval { get; set; }

        [JsonProperty("payee")]
        public string Payee { get; set; }

        [JsonProperty("comment")]
        public string Comment { get; set; }

        [JsonProperty("merchant")]
        public Guid? Merchant { get; set; }
    }
}