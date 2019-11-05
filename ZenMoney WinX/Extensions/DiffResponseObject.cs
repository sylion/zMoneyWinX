using System.Collections.Generic;
using Newtonsoft.Json;
using zMoneyWinX.Model;

namespace zMoneyWinX.Model
{
    [JsonObject]
    public sealed class DiffResponseObject
    {
        [JsonProperty("serverTimestamp")]
        public long ServerTimeStamp { get; set; }

        [JsonProperty("instrument")]
        public List<Instrument> Instruments { get; set; }

        [JsonProperty("country")]
        public List<Country> Countries { get; set; }

        [JsonProperty("company")]
        public List<Company> Companies { get; set; }

        [JsonProperty("user")]
        public List<User> Users { get; set; }

        [JsonProperty("account")]
        public List<Account> Accounts { get; set; }

        [JsonProperty("tag")]
        public List<Tag> Tags { get; set; }

        [JsonProperty("budget")]
        public List<Budget> Budgets { get; set; }

        [JsonProperty("merchant")]
        public List<Merchant> Merchants { get; set; }

        [JsonProperty("reminder")]
        public List<Reminder> Reminders { get; set; }

        [JsonProperty("reminderMarker")]
        public List<ReminderMarker> ReminderMarkers { get; set; }

        [JsonProperty("transaction")]
        public List<Transaction> Transactions { get; set; }

        [JsonProperty("deletion")]
        public List<Deletion> Deletions { get; set; }
    }
}