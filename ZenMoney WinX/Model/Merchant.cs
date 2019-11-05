using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using zMoneyWinX.Client;

namespace zMoneyWinX.Model
{
    public partial class Merchant : DBObject
    {
        public static Merchant newMerchant(string Name)
        {
            Merchant merchant = new Merchant()
            { Id = Guid.NewGuid(), Title = Name, isCreated = true, isChanged = true, Changed = SettingsManager.toUnixTime(DateTime.UtcNow), User = Model.User.CurrentUserID };
            return merchant;
        }

        public static List<Merchant> CreateMetadata(List<Merchant> items)
        {
            int ID = Model.User.CurrentUserID;
            foreach (Merchant x in items.Where(i => i.User == null))
            {
                x.User = ID;
                x.Changed = SettingsManager.toUnixTime(DateTime.UtcNow);
                x.isChanged = true;
            }
            return items;
        }

        public static string GetTitle(Guid? ID)
        {
            if (ID == null || ID == Guid.Empty)
                return "";
            using (SQLiteConnection db = new SQLiteConnection(DBInitializer.DB_PATH))
                return db.Get<Merchant>(i => i.Id == ID).Title;
        }

        public static Merchant GetByTitle(string Title)
        {
            if (string.IsNullOrWhiteSpace(Title))
                return null;

            using (SQLiteConnection db = new SQLiteConnection(DBInitializer.DB_PATH))
            {
                try
                {
                    return db.Get<Merchant>(i => i.Title == Title);
                }
                catch { return null; }
            }
        }

        public static ObservableCollection<Merchant> Merchants()
        {
            using (SQLiteConnection db = new SQLiteConnection(DBInitializer.DB_PATH))
                return new ObservableCollection<Merchant>(db.Table<Merchant>().Where(i => !i.isDeleted).OrderBy(i => i.Title));
        }
    }
}