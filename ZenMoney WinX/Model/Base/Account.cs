using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;

namespace zMoneyWinX.Model
{
    [JsonObject, Table("Account")]
    public partial class Account : DBObject
    {
        [JsonProperty("id"), PrimaryKey]
        public Guid Id { get; set; }

        [JsonProperty("user"), Indexed]
        public int User { get; set; }

        [JsonProperty("instrument")]
        public int Instrument { get; set; }

        [JsonProperty("type"), Indexed]
        public string Type { get; set; }

        [JsonProperty("role"), Indexed]
        public int? Role { get; set; }

        [JsonProperty("private"), Indexed]
        public bool Private { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("inBalance"), Indexed]
        public bool InBalance { get; set; }

        [JsonProperty("creditLimit")]
        public double CreditLimit { get; set; }

        [JsonProperty("startBalance")]
        public double? StartBalance { get; set; }

        [JsonProperty("balance")]
        public double Balance { get; set; }

        [JsonProperty("company")]
        public int? Company { get; set; }

        [JsonProperty("archive"), Indexed]
        public bool Archive { get; set; }

        [JsonProperty("enableCorrection")]
        public bool EnableCorrection { get; set; }

        [JsonProperty("startDate")]
        public string StartDate { get; set; }

        [JsonProperty("capitalization")]
        public bool? Capitalization { get; set; }

        [JsonProperty("percent")]
        public double? Percent { get; set; }

        [JsonProperty("changed")]
        public long Changed { get; set; }

        [JsonProperty("syncID"), Ignore]
        public List<string> SyncId
        {
            get
            {
                if (!string.IsNullOrEmpty(_SyncId))
                    return JsonConvert.DeserializeObject<List<string>>(_SyncId);
                else
                    return null;
            }
            set
            {
                _SyncId = JsonConvert.SerializeObject(value);
            }
        }

        [JsonProperty("enableSMS")]
        public bool EnableSms { get; set; }

        [JsonProperty("payoffStep")]
        public int? PayoffStep { get; set; }

        [JsonProperty("payoffInterval")]
        public string PayoffInterval { get; set; }

        [JsonProperty("endDateOffsetInterval")]
        public string EndDateOffsetInterval { get; set; }

        [JsonProperty("endDateOffset")]
        public int? EndDateOffset { get; set; }
    }
}
