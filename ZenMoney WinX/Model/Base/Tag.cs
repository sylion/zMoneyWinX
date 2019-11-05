using Newtonsoft.Json;
using SQLite;
using System;

namespace zMoneyWinX.Model
{
    [JsonObject, Table("Tag")]
    public partial class Tag : DBObject
    {
        [JsonProperty("id"), PrimaryKey]
        public Guid Id { get; set; }

        [JsonProperty("user")]
        public int User { get; set; }

        [JsonProperty("changed")]
        public long Changed { get; set; }

        [JsonProperty("icon")]
        public string Icon { get; set; }

        [JsonProperty("budgetIncome")]
        public bool BudgetIncome { get; set; }

        [JsonProperty("budgetOutcome")]
        public bool BudgetOutcome { get; set; }

        [JsonProperty("required")]
        public bool? Required { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("showIncome")]
        public bool ShowIncome { get; set; }

        [JsonProperty("showOutcome")]
        public bool ShowOutcome { get; set; }

        [JsonProperty("parent")]
        public Guid? Parent { get; set; }
    }
}
