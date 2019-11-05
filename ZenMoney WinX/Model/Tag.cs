using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Media;
using zMoneyWinX.Client;

namespace zMoneyWinX.Model
{
    public partial class Tag : DBObject
    {
        #region Extension
        [JsonIgnore]
        public string ColorStr { get; set; }
        [JsonIgnore, Ignore]
        public SolidColorBrush TagColor { get { return new SolidColorBrush(Extensions.GetColorFromHex(ColorStr)); } set { ColorStr = value.Color.ToString(); NotifyPropertyChanged(); } }

        [JsonIgnore, Ignore]
        private bool _isChecked { get; set; }
        [JsonIgnore, Ignore]
        public bool isChecked
        {
            get
            {
                return _isChecked;
            }
            set
            {
                _isChecked = value;
                NotifyPropertyChanged();
            }
        }
        #endregion

        public static Tag Empty => new Tag() { Id = Guid.Empty };
        public void update(TagEdit item, bool Created = false)
        {
            Title = item.Title;
            ShowIncome = item.ShowIncome;
            ShowOutcome = item.ShowOutcome;
            Parent = item.Parent;
            Icon = item.Icon;
            ColorStr = item.ColorStr;
            BudgetIncome = item.BudgetIncome;
            BudgetOutcome = item.BudgetOutcome;
            Required = item.Required;
            isChanged = true;
            User = Model.User.CurrentUserID;
            if (Created)
            {
                Id = Guid.NewGuid();
                isCreated = Created;
            }
            Changed = SettingsManager.toUnixTime(DateTime.UtcNow);
            NotifyPropertyChanged();
        }

        public static Tag GetByID(Guid? ID)
        {
            if (ID == null || ID == Guid.Empty)
                return null;

            using (SQLiteConnection db = new SQLiteConnection(DBInitializer.DB_PATH))
                return db.Get<Tag>(ID);
        }

        public static List<Tag> Tags
        {
            get
            {
                using (SQLiteConnection db = new SQLiteConnection(DBInitializer.DB_PATH))
                    return db.Table<Tag>().OrderBy(i => i.Title).ToList();//.Select(c => { c.isChecked = false; return c; }).ToList();
            }
        }
    }
}