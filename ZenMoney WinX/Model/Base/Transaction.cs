using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;

namespace zMoneyWinX.Model
{
    [JsonObject, Table("Transaction")]
    public partial class Transaction : DBObject
    {
        [JsonProperty("id"), PrimaryKey]
        public Guid Id { get; set; }

        [JsonProperty("user"), Indexed]
        public int User { get; set; }

        [JsonProperty("date")]
        public string Date { get; set; }

        [JsonProperty("income")]
        public double Income { get; set; }

        [JsonProperty("outcome")]
        public double Outcome { get; set; }

        [JsonProperty("changed"), Indexed]
        public long Changed { get; set; }

        [JsonProperty("incomeInstrument")]
        public int IncomeInstrument { get; set; }

        [JsonProperty("outcomeInstrument")]
        public int OutcomeInstrument { get; set; }

        [JsonProperty("created")]
        public long Created { get; set; }

        [JsonProperty("originalPayee")]
        public string OriginalPayee { get; set; }

        [JsonProperty("incomeAccount"), Indexed]
        public Guid IncomeAccount { get; set; }

        [JsonProperty("outcomeAccount"), Indexed]
        public Guid OutcomeAccount { get; set; }
        
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

        [JsonProperty("comment"), Indexed]
        public string Comment { get; set; }

        [JsonProperty("payee")]
        public string Payee { get; set; }

        [JsonProperty("opIncome")]
        public double? OpIncome { get; set; }

        [JsonProperty("opOutcome")]
        public double? OpOutcome { get; set; }

        [JsonProperty("opIncomeInstrument")]
        public int? OpIncomeInstrument { get; set; }

        [JsonProperty("opOutcomeInstrument")]
        public int? OpOutcomeInstrument { get; set; }

        [JsonProperty("latitude")]
        public double? Lattitude { get; set; }

        [JsonProperty("longitude")]
        public double? Longtitude { get; set; }

        [JsonProperty("merchant"), Indexed]
        public Guid? Merchant { get; set; }

        [JsonProperty("incomeBankID")]
        public string IncomeBankId { get; set; }

        [JsonProperty("outcomeBankID")]
        public string OutcomeBankId { get; set; }

        [JsonProperty("reminderMarker")]
        public Guid? ReminderMarker { get; set; }
    }
}
