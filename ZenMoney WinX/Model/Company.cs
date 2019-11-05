using SQLite;
using System.Collections.Generic;
using System.Linq;

namespace zMoneyWinX.Model
{
    public partial class Company : DBObject
    {
        public static string getTitle(int? ID)
        {
            if (ID == null)
                return "";

            using (SQLiteConnection db = new SQLiteConnection(DBInitializer.DB_PATH))
                return db.Get<Company>(ID).Title;
        }

        public static int? getID(string Title)
        {
            using (SQLiteConnection db = new SQLiteConnection(DBInitializer.DB_PATH))
                try
                {
                    return db.Get<Company>(i => i.Title == Title).Id;
                }
                catch
                {
                    return null;
                }
        }

        public static List<Company> Companies()
        {
            using (SQLiteConnection db = new SQLiteConnection(DBInitializer.DB_PATH))
                return db.Table<Company>().ToList();
        }
    }
}