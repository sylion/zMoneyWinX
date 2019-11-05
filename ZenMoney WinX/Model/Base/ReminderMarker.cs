using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;

namespace zMoneyWinX.Model
{
    [JsonObject, Table("ReminderMarker")]
    public partial class ReminderMarker : DBObject
    {
        [JsonProperty("id"), PrimaryKey]
        public Guid Id { get; set; }

        [JsonProperty("user")]
        public int User { get; set; }

        [JsonProperty("date")]
        public string Date { get; set; }

        [JsonProperty("income")]
        public double Income { get; set; }

        [JsonProperty("outcome")]
        public double Outcome { get; set; }

        [JsonProperty("changed")]
        public long Changed { get; set; }

        [JsonProperty("incomeInstrument")]
        public int IncomeInstrument { get; set; }

        [JsonProperty("outcomeInstrument")]
        public int OutcomeInstrument { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("reminder")]
        public Guid Reminder { get; set; }

        [JsonProperty("incomeAccount")]
        public Guid IncomeAccount { get; set; }

        [JsonProperty("outcomeAccount")]
        public Guid OutcomeAccount { get; set; }

        [JsonProperty("comment")]
        public string Comment { get; set; }

        [JsonProperty("payee")]
        public string Payee { get; set; }

        [JsonProperty("merchant")]
        public Guid? Merchant { get; set; }

        [JsonProperty("notify")]
        public bool Notify { get; set; }
        
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
    }
}