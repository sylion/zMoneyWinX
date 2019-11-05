using Newtonsoft.Json;
using SQLite;
using System;

namespace zMoneyWinX.Model
{
    [JsonObject, Table("Merchant")]
    public partial class Merchant : DBObject
    {
        [JsonProperty("id"), PrimaryKey]
        public Guid Id { get; set; }

        [JsonProperty("user")]
        public int? User { get; set; }

        private string _Title { get; set; }
        [JsonProperty("title")]
        public string Title { get { return _Title; } set { _Title = value; NotifyPropertyChanged(); } }

        [JsonProperty("changed")]
        public long Changed { get; set; }
    }
}
