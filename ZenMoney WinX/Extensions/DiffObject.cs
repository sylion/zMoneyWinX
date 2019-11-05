using Newtonsoft.Json;
using System.Collections.Generic;

namespace zMoneyWinX.Model
{
    [JsonObject]
    public sealed class DiffObject
    {
        [JsonProperty("lastServerTimestamp")]    
        public long LastSyncronizationTimeStamp { get; set; }

        [JsonProperty("currentClientTimestamp")]    
        public long ClientTimeStamp { get; set; }

        [JsonProperty("merchant")]
        public List<Merchant> Merchants { get; set; }

        [JsonProperty("tag")]
        public List<Tag> Tags { get; set; }

        [JsonProperty("account")]
        public List<Account> Accounts { get; set; }

        [JsonProperty("transaction")]
        public List<Transaction> Transactions { get; set; }

        [JsonProperty("budget")]
        public List<Budget> Budget { get; set; }

        [JsonProperty("user")]
        public List<User> Users { get; set; }

        [JsonProperty("reminder")]
        public List<Reminder> Reminder { get; set; }

        [JsonProperty("remindermarker")]
        public List<ReminderMarker> ReminderMarker { get; set; }

        [JsonProperty("deletion")]
        public List<Deletion> Deletions { get; set; }
    }
}