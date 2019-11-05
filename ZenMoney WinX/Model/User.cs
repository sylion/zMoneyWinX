using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using zMoneyWinX.Client;

namespace zMoneyWinX.Model
{
    public partial class User : DBObject
    {
        public static List<User> Users()
        {
            using (SQLiteConnection db = new SQLiteConnection(DBInitializer.DB_PATH))
                return db.Table<User>().ToList();
        }

        public static User GetCurrentUser()
        {
            using (SQLiteConnection db = new SQLiteConnection(DBInitializer.DB_PATH))
                return db.Get<User>(i => i.Login == SettingsManager.login);
        }

        public static string GetTitle(int? ID)
        {
            if (ID == null)
                return "";
            using (SQLiteConnection db = new SQLiteConnection(DBInitializer.DB_PATH))
                return db.Get<User>(i => i.Id == ID).Login;
        }

        public static int GetDefaultCurrency()
        {
            using (SQLiteConnection db = new SQLiteConnection(DBInitializer.DB_PATH))
                try
                {
                    return db.Get<User>(i => i.Login == SettingsManager.login).Currency;
                }
                catch
                {
                    return 2;
                }
        }

        public static void UpdateCurrency(int CurrencyID)
        {
            using (SQLiteConnection db = new SQLiteConnection(DBInitializer.DB_PATH))
            {
                User User = db.Get<User>(i => i.Login == SettingsManager.login);
                User.Currency = CurrencyID;
                User.isChanged = true;
                User.Changed = SettingsManager.toUnixTime(DateTime.UtcNow);
                db.InsertOrReplace(User);
            }
        }

        public static int CurrentUserID
        {
            get
            {
                try
                {
                    using (SQLiteConnection db = new SQLiteConnection(DBInitializer.DB_PATH))
                        try
                        {
                            return db.Table<User>().FirstOrDefault(i => i.Login == SettingsManager.login).Id;
                        }
                        catch
                        {
                            return db.Table<User>().FirstOrDefault().Id;
                        }
                }
                catch
                {
                    return 0;
                }
            }
        }
    }
}