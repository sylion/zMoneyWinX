using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using zMoneyWinX.Client;

namespace zMoneyWinX.Model
{
    class DBInitializer
    {
        public const string DBName = "ZMAv7DBv10.db";
        public static string DB_PATH = Path.Combine(ApplicationData.Current.LocalFolder.Path, DBName);

        public static List<string> OldDBs => new List<string>
        {
            "ZMAv7DBv1.db", "ZMAv7DBv2.db", "ZMAv7DBv3.db", "ZMAv7DBv4.db", "ZMAv7DBv5.db", "ZMAv7DBv6.db", "ZMAv7DBv7.db", "ZMAv7DBv8.db", "ZMAv7DBv9.db"
        };

        public static async Task initDB()
        {
            if (!await CheckFileExists(DBName))
            {
                using (SQLiteConnection db = new SQLiteConnection(DB_PATH))
                {
                    db.CreateTable<Account>();
                    db.CreateTable<Budget>();
                    db.CreateTable<Company>();
                    db.CreateTable<Country>();
                    db.CreateTable<Instrument>();
                    db.CreateTable<Merchant>();
                    db.CreateTable<Reminder>();
                    db.CreateTable<ReminderMarker>();
                    db.CreateTable<Tag>();
                    db.CreateTable<Transaction>();
                    db.CreateTable<User>();
                }
            }
        }
        public static async Task<bool> CheckFileExists(string fileName)
        {
            try
            {
                StorageFile store = await ApplicationData.Current.LocalFolder.GetFileAsync(fileName);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static async void DropDB()
        {
            try
            {
                StorageFile store = await ApplicationData.Current.LocalFolder.GetFileAsync(DBName);
                await store.DeleteAsync();
            }
            catch { }
        }

        public static async Task<DiffObject> getDiffObject()
        {
            DiffObject diffObject = new DiffObject();
            await Task.Run(() =>
            {
                diffObject.LastSyncronizationTimeStamp = SettingsManager.lastsynctime;
                diffObject.ClientTimeStamp = SettingsManager.toUnixTime(DateTime.UtcNow);
                using (SQLiteConnection db = new SQLiteConnection(DB_PATH))
                {
                    diffObject.Transactions = db.Table<Transaction>().Where(i => i.Changed >= diffObject.LastSyncronizationTimeStamp && (i.isCreated || i.isChanged)).ToList();
                    diffObject.Accounts = db.Table<Account>().Where(i => i.Changed >= diffObject.LastSyncronizationTimeStamp && (i.isCreated || i.isChanged)).ToList();
                    diffObject.Tags = db.Table<Tag>().Where(i => i.Changed >= diffObject.LastSyncronizationTimeStamp && (i.isCreated || i.isChanged)).ToList();
                    diffObject.Merchants = db.Table<Merchant>().Where(i => i.Changed >= diffObject.LastSyncronizationTimeStamp && (i.isCreated || i.isChanged)).ToList();
                    diffObject.Users = db.Table<User>().Where(i => i.Changed >= diffObject.LastSyncronizationTimeStamp && (i.isCreated || i.isChanged)).ToList();
                    diffObject.Budget = db.Table<Budget>().Where(i => i.Changed >= diffObject.LastSyncronizationTimeStamp && (i.isCreated || i.isChanged)).ToList();
                    diffObject.Reminder = db.Table<Reminder>().Where(i => i.Changed >= diffObject.LastSyncronizationTimeStamp && (i.isCreated || i.isChanged)).ToList();
                    diffObject.ReminderMarker = db.Table<ReminderMarker>().Where(i => i.Changed >= diffObject.LastSyncronizationTimeStamp && (i.isCreated || i.isChanged)).ToList();

                    diffObject.Deletions = new List<Deletion>();
                    foreach (Transaction item in db.Table<Transaction>().Where(i => i.Changed >= diffObject.LastSyncronizationTimeStamp && i.isDeleted).ToList())
                        diffObject.Deletions.Add(new Deletion() { Id = item.Id, User = item.User, Stamp = item.Changed, Object = "transaction" });
                    foreach (Account item in db.Table<Account>().Where(i => i.Changed >= diffObject.LastSyncronizationTimeStamp && i.isDeleted).ToList())
                        diffObject.Deletions.Add(new Deletion() { Id = item.Id, User = item.User, Stamp = item.Changed, Object = "account" });
                    foreach (Tag item in db.Table<Tag>().Where(i => i.Changed >= diffObject.LastSyncronizationTimeStamp && i.isDeleted).ToList())
                        diffObject.Deletions.Add(new Deletion() { Id = item.Id, User = item.User, Stamp = item.Changed, Object = "tag" });
                    foreach (Merchant item in db.Table<Merchant>().Where(i => i.Changed >= diffObject.LastSyncronizationTimeStamp && i.isDeleted).ToList())
                        diffObject.Deletions.Add(new Deletion() { Id = item.Id, User = User.CurrentUserID, Stamp = item.Changed, Object = "merchant" });
                    foreach (Reminder item in db.Table<Reminder>().Where(i => i.Changed >= diffObject.LastSyncronizationTimeStamp && i.isDeleted).ToList())
                        diffObject.Deletions.Add(new Deletion() { Id = item.Id, User = User.CurrentUserID, Stamp = item.Changed, Object = "reminder" });
                }
            });
            return diffObject;
        }
    }
}
