using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.ApplicationModel.Resources;
using zMoneyWinX.ViewModel;

namespace zMoneyWinX.Model
{
    public partial class Budget : DBObject
    {
        #region Extension
        [JsonProperty("date"), Ignore]
        public string _Date { set { Date = DateTime.Parse(value); } get { return Date.ToString("yyyy-MM-dd"); } }

        [PrimaryKey, JsonIgnore]
        public string Key { get; set; }
        [JsonIgnore]
        public string displayTag { get; set; }
        #endregion

        public static List<Budget> createMetaData(List<Budget> items)
        {
            List<Tag> tags = TagViewModel.Tags;

            foreach (Budget item in items)
            {
                if (item.Tag == null)
                    item.Key = item.Date.ToString("dd.MM.yyyy");
                else
                    item.Key = item.Date.ToString("dd.MM.yyyy") + item.Tag.ToString();

                if (item.Tag == null)
                    item.displayTag = ResourceLoader.GetForViewIndependentUse().GetString("TransTagEmpty");
                else if (item.Tag == Guid.Empty)
                    item.displayTag = "";
                else
                    item.displayTag = tags.Find(x => x.Id == item.Tag).Title;
            }
            var tmp = JsonConvert.SerializeObject(items.Select(i=>i.Key));
            return items;
        }

        public static ObservableCollection<Budget> GetBudget(DateTime date)
        {
            using (SQLiteConnection db = new SQLiteConnection(DBInitializer.DB_PATH))
                return new ObservableCollection<Budget>(db.Table<Budget>().ToList().Where(i => i.Date.Year == date.Date.Year && i.Date.Month == date.Date.Month));
        }
    }
}
