using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace zMoneyWinX.Model
{
    public class DBObject : INotifyProperty
    {
        [JsonIgnore, Ignore]
        public Guid ObjId
        {
            get
            {
                if (this is Transaction)
                    return ((Transaction)this).Id;
                else if (this is ReminderMarker)
                    return ((ReminderMarker)this).Id;
                else
                    return Guid.Empty;
            }
        }

        [JsonIgnore]
        public bool isChanged { get; set; }
        [JsonIgnore]
        public bool isCreated { get; set; }
        [JsonIgnore]
        public bool isDeleted { get; set; }

        public static void Delete(object ID, Type type)
        {
            using (SQLiteConnection db = new SQLiteConnection(DBInitializer.DB_PATH))
            {
                if (type == typeof(Account))
                    db.Delete<Account>(ID);
                if (type == typeof(Transaction))
                    db.Delete<Transaction>(ID);
                if (type == typeof(Tag))
                    db.Delete<Tag>(ID);
                if (type == typeof(Merchant))
                    db.Delete<Merchant>(ID);
                if (type == typeof(User))
                    db.Delete<User>(ID);
            }
        }

        public static void Insert(object Item, bool fullSync = false)
        {
            List<Account> _accs = null;
            List<Tag> _tags = null;

            using (SQLiteConnection db = new SQLiteConnection(DBInitializer.DB_PATH))
                if (Item is IEnumerable)
                    db.RunInTransaction(() =>
                    {
                        foreach (var x in (IEnumerable)Item)
                        {
                        //TODO: Delete this if zm add isdebet field
                        if (!fullSync && x is Account)
                            {
                                if (_accs == null)
                                    _accs = db.Table<Account>().ToList();

                                var curAc = _accs.FirstOrDefault(i => i.Id == ((Account)x).Id);
                                if (curAc != null)
                                    ((Account)x).Savings = curAc.Savings;
                            }
                            if (!fullSync && x is Tag)
                            {
                                if (_tags == null)
                                    _tags = db.Table<Tag>().ToList();
                                var curTag = _tags.FirstOrDefault(i => i.Id == ((Tag)x).Id);
                                if (curTag != null)
                                    ((Tag)x).ColorStr = curTag.ColorStr;
                            }

                        //Dont touch this. All markers must be deleted if Reminder was changed
                        if (!fullSync && x is Reminder)
                            {
                                if (db.Table<Reminder>().Any(i => i.Id == ((Reminder)x).Id))
                                {
                                    foreach (var item in db.Table<ReminderMarker>().Where(i => i.Reminder == ((Reminder)x).Id))
                                    {
                                        foreach (var c in db.Table<Transaction>().Where(i => i.ReminderMarker == item.Id).ToList().Select(i => { i.ReminderMarker = null; return i; }))
                                        {
                                            db.InsertOrReplace(c);
                                        }
                                    }
                                    db.Table<ReminderMarker>().Delete(i => i.Reminder == ((Reminder)x).Id);
                                }
                            }
                            db.InsertOrReplace(x);
                        }
                    });
                else
                    db.InsertOrReplace(Item);
        }
    }
}