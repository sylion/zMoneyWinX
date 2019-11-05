using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using zMoneyWinX.Client;

namespace zMoneyWinX.Model
{
    public partial class Reminder : DBObject
    {
        [JsonIgnore]
        public string _Tags { get; set; }

        [JsonIgnore]
        public string _Points { get; set; }

        [JsonIgnore, Indexed]
        public TransactionType TransType { get; set; }
        [JsonIgnore, Indexed]
        public long TransDateStamp { get; set; }

        public static void CreateSingle(Transaction item)
        {
            Reminder reminder = new Reminder();
            reminder.StartDate = item.Date;
            reminder.EndDate = null;
            reminder.Interval = null;
            reminder.Step = 0;
            reminder.Points = new List<int>() { 0 };

            reminder.Id = Guid.NewGuid();
            reminder.User = item.User;
            reminder.Income = item.Income;
            reminder.Outcome = item.Outcome;
            reminder.Changed = SettingsManager.toUnixTime(DateTime.UtcNow);
            reminder.IncomeInstrument = item.IncomeInstrument;
            reminder.OutcomeInstrument = item.OutcomeInstrument;
            reminder.IncomeAccount = item.IncomeAccount;
            reminder.OutcomeAccount = item.OutcomeAccount;
            reminder.Comment = item.Comment;
            reminder.Payee = item.Payee;
            reminder.Merchant = item.Merchant;
            reminder._Tags = item._Tags;
            reminder.isCreated = true;
            reminder.TransType = item.TransType;
            reminder.TransDateStamp = item.TransDateStamp;

            ReminderMarker marker = new ReminderMarker();
            marker.Id = Guid.NewGuid();
            marker.Reminder = reminder.Id;
            marker.Date = item.Date;
            marker.User = item.User;
            marker.Income = item.Income;
            marker.Outcome = item.Outcome;
            marker.Changed = SettingsManager.toUnixTime(DateTime.UtcNow);
            marker.IncomeInstrument = item.IncomeInstrument;
            marker.OutcomeInstrument = item.OutcomeInstrument;
            marker.IncomeAccount = item.IncomeAccount;
            marker.OutcomeAccount = item.OutcomeAccount;
            marker.Comment = item.Comment;
            marker.Payee = item.Payee;
            marker.Merchant = item.Merchant;
            marker._Tags = item._Tags;
            marker.isCreated = true;
            marker.TransType = item.TransType;
            marker.TransDateStamp = item.TransDateStamp;
            marker.State = ReminderMarker.GetStateStr(ReminderMarker.States.Planned);

            Insert(reminder);
            Insert(marker);
        }

        public void Delete()
        {
            using (SQLiteConnection db = new SQLiteConnection(DBInitializer.DB_PATH, SQLiteOpenFlags.ReadWrite))
            {
                foreach (var item in db.Table<ReminderMarker>().Where(i => i.Reminder == Id).ToList())
                {
                    item.State = ReminderMarker.GetStateStr(ReminderMarker.States.Deleted);
                    item.Changed = SettingsManager.toUnixTime(DateTime.UtcNow);
                    Insert(item);
                }
                Changed = SettingsManager.toUnixTime(DateTime.UtcNow);
                isDeleted = true;
                Insert(this);
            }
        }

        public static List<Guid> Markers(Guid id)
        {
            using (SQLiteConnection db = new SQLiteConnection(DBInitializer.DB_PATH, SQLiteOpenFlags.ReadWrite))
                return db.Table<ReminderMarker>().Where(i => i.Reminder == id).ToList().Select(i => i.Id).ToList();
        }

        public static void Delete(Guid id)
        {
            using (SQLiteConnection db = new SQLiteConnection(DBInitializer.DB_PATH, SQLiteOpenFlags.ReadWrite))
            {
                Reminder res = db.Get<Reminder>(id);
                foreach (var item in db.Table<ReminderMarker>().Where(i => i.Reminder == res.Id).ToList())
                {
                    item.State = ReminderMarker.GetStateStr(ReminderMarker.States.Deleted);
                    item.Changed = SettingsManager.toUnixTime(DateTime.UtcNow);
                    Insert(item);
                }
                res.Changed = SettingsManager.toUnixTime(DateTime.UtcNow);
                res.isDeleted = true;
                Insert(res);
            }
        }

        public static void DeletePermament(Guid id)
        {
            using (SQLiteConnection db = new SQLiteConnection(DBInitializer.DB_PATH, SQLiteOpenFlags.ReadWrite))
            {
                foreach (var item in db.Table<ReminderMarker>().Where(i => i.Reminder == id))
                {
                    foreach (var c in db.Table<Transaction>().Where(i => i.ReminderMarker == item.Id).ToList().Select(i => { i.ReminderMarker = null; return i; }))
                    {
                        db.InsertOrReplace(c);
                    }
                }
                db.Table<ReminderMarker>().Delete(i => i.Reminder == id);
                db.Table<Reminder>().Delete(i => i.Id == id);
            }
        }
    }
}