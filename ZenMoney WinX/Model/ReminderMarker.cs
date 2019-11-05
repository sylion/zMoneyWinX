using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using zMoneyWinX.Client;

namespace zMoneyWinX.Model
{
    public partial class ReminderMarker : DBObject
    {
        [JsonIgnore]
        public string _Tags { get; set; }

        #region Extension
        [JsonIgnore, Indexed]
        public TransactionType TransType { get; set; }
        [JsonIgnore, Indexed]
        public long TransDateStamp { get; set; }
        #endregion

        #region Display only data
        [JsonIgnore, Ignore]
        public bool isPlanned { get; private set; } = true;

        [JsonIgnore, Ignore]
        public string displayTransactionIcon { get; set; }
        [JsonIgnore, Ignore]
        public string displayTransactionLetter { get; set; }


        [JsonIgnore, Ignore]
        public Uri displayOutcomeAccountIcon => Extensions.AccountIcon(displayOutcomeAccountType);
        [JsonIgnore, Ignore]
        public Uri displayIncomeAccountIcon => Extensions.AccountIcon(displayIncomeAccountType);
        [JsonIgnore, Ignore]
        public string displayIncomeInstrument { get; set; }
        [JsonIgnore, Ignore]
        public string displayOutcomeInstrument { get; set; }
        [JsonIgnore, Ignore]
        public string displayIncomeAccountType { get; set; }
        [JsonIgnore, Ignore]
        public string displayOutcomeAccountType { get; set; }
        [JsonIgnore, Ignore]
        public string displayGroupName
        {
            get
            {
                DateTime res = DateTime.Parse(Date);
                if (res.Year == DateTime.Now.Year)
                {
                    if (res.Day == DateTime.Now.Day + 1 && res.Month == DateTime.Now.Month)
                        return ResourceLoader.GetForViewIndependentUse().GetString("DateTomorrow");
                    else if (res.Day == DateTime.Now.Day && res.Month == DateTime.Now.Month)
                        return ResourceLoader.GetForViewIndependentUse().GetString("DateToday");
                    else if (res.Day == DateTime.Now.Day - 1 && res.Month == DateTime.Now.Month)
                        return ResourceLoader.GetForViewIndependentUse().GetString("DateYesterday");
                    else
                        return res.ToString("dd MMMM, ddd");
                }
                else
                    return res.ToString("dd MMMM yyyy");
            }
        }
        [JsonIgnore, Ignore]
        private string displayTagColorStr { get; set; }
        [JsonIgnore, Ignore]
        public Brush displayTagColor { get { return new SolidColorBrush(Extensions.GetColorFromHex(displayTagColorStr)); } }

        [JsonIgnore, Ignore]
        public string displayTransTags { get; set; }
        [JsonIgnore, Ignore]
        public string displayIncomeAccount { get; set; }
        [JsonIgnore, Ignore]
        public string displayOutcomeAccount { get; set; }
        [JsonIgnore, Ignore]
        public string displayPayee { get; set; }
        [JsonIgnore, Ignore]
        public string displayIncome
        {
            get
            {
                if (TransType == TransactionType.Transfer && IncomeInstrument == OutcomeInstrument && Income == Outcome)
                    return " ";
                if (TransType == TransactionType.Income || TransType == TransactionType.Transfer || TransType == TransactionType.pDept)// || (TransType == TransactionType.Credit && Income > 0))
                    return "+" + Income.ToString("#,0.##");
                else
                    return "";
            }
        }
        [JsonIgnore, Ignore]
        public string displayOutcome
        {
            get
            {
                if (TransType == TransactionType.Transfer && IncomeInstrument == OutcomeInstrument && Income == Outcome)
                    return Income.ToString("#,0.##");
                if (TransType == TransactionType.Outcome || TransType == TransactionType.Transfer || TransType == TransactionType.mDept)// || (TransType == TransactionType.Credit && Outcome > 0))
                    return "-" + Outcome.ToString("#,0.##");
                else
                    return "";
            }
        }

        [JsonIgnore, Ignore]
        public double OperationAmount
        {
            get
            {
                if (Outcome == 0 && Income != 0)
                    return Income;

                if (Outcome != 0 && Income == 0)
                    return Outcome;

                if (TransType == TransactionType.Transfer && OutcomeInstrument == IncomeInstrument && Outcome != Income)
                {
                    if (Outcome > Income)
                        return Outcome - Income;
                    else
                        return Income - Outcome;
                }

                return 0;
            }
        }

        public TransactionType TransIs
        {
            get
            {
                if (TransType == TransactionType.Transfer && OutcomeInstrument == IncomeInstrument && Outcome != Income)
                {
                    if (Outcome > Income)
                        return TransactionType.Outcome;
                    if (Outcome < Income)
                        return TransactionType.Income;
                }
                return TransactionType.Transfer;
            }
        }

        [JsonIgnore, Ignore]
        public int OperationInstrument
        {
            get
            {
                if (Outcome == 0 && Income != 0)
                    return IncomeInstrument;

                if (Outcome != 0 && Income == 0)
                    return OutcomeInstrument;

                if (TransType == TransactionType.Transfer && OutcomeInstrument == IncomeInstrument && Outcome != Income)
                {
                    if (Outcome > Income)
                        return OutcomeInstrument;
                    else
                        return IncomeInstrument;
                }

                return 0;
            }
        }

        [JsonIgnore, Ignore]
        public string displayState
        {
            get
            {
                if (State == GetStateStr(States.Planned))
                    return ResourceLoader.GetForViewIndependentUse().GetString("StatePlanned");
                else if (State == GetStateStr(States.Deleted))
                    return ResourceLoader.GetForViewIndependentUse().GetString("StateDeleted");
                else
                    return ResourceLoader.GetForViewIndependentUse().GetString("StateProcessed");
            }
        }

        public Visibility ShowPlannedMenu
        {
            get
            {
                if (State != GetStateStr(States.Deleted) && State != GetStateStr(States.Processed))
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }
        }
        #endregion

        public Transaction Edit()
        {
            Transaction trans = new Transaction();
            trans.Id = Guid.NewGuid();
            trans.Date = DateTime.UtcNow.ToString("yyyy-MM-dd");
            trans.User = User;
            trans.Income = Income;
            trans.Outcome = Outcome;
            trans.Changed = SettingsManager.toUnixTime(DateTime.UtcNow);
            trans.Created = SettingsManager.toUnixTime(DateTime.UtcNow);
            trans.IncomeInstrument = IncomeInstrument;
            trans.OutcomeInstrument = OutcomeInstrument;
            trans.IncomeAccount = IncomeAccount;
            trans.OutcomeAccount = OutcomeAccount;
            trans.Comment = Comment;
            trans.Payee = Payee;
            trans.Merchant = Merchant;
            trans._Tags = _Tags;
            trans.isCreated = true;
            trans.ReminderMarker = Id;
            trans.TransType = TransType;
            trans.TransDateStamp = TransDateStamp;

            return trans;
        }

        public Transaction Write()
        {
            Transaction trans = new Transaction();
            trans.Id = Guid.NewGuid();
            trans.Date = DateTime.UtcNow.ToString("yyyy-MM-dd");
            trans.User = User;
            trans.Income = Income;
            trans.Outcome = Outcome;
            trans.Changed = SettingsManager.toUnixTime(DateTime.UtcNow);
            trans.Created = SettingsManager.toUnixTime(DateTime.UtcNow);
            trans.IncomeInstrument = IncomeInstrument;
            trans.OutcomeInstrument = OutcomeInstrument;
            trans.IncomeAccount = IncomeAccount;
            trans.OutcomeAccount = OutcomeAccount;
            trans.Comment = Comment;
            trans.Payee = Payee;
            trans.Merchant = Merchant;
            trans._Tags = _Tags;
            trans.isCreated = true;
            trans.ReminderMarker = Id;
            trans.TransType = TransType;
            trans.TransDateStamp = TransDateStamp;

            State = GetStateStr(States.Processed);
            isChanged = true;
            Changed = SettingsManager.toUnixTime(DateTime.UtcNow);
            Insert(this);

            return trans;
        }

        public static void Processed(Guid id)
        {
            using (SQLiteConnection db = new SQLiteConnection(DBInitializer.DB_PATH, SQLiteOpenFlags.ReadWrite))
            {
                var res = db.Get<ReminderMarker>(id);
                res.State = GetStateStr(States.Processed);
                Insert(res);
            }
        }

        public void Delete()
        {
            State = GetStateStr(States.Deleted);
            isChanged = true;
            Changed = SettingsManager.toUnixTime(DateTime.UtcNow);
            Insert(this);
        }

        public static async Task<List<ReminderMarker>> createMetaData(List<ReminderMarker> items, List<Account> accounts = null)
        {
            //For not full sync process
            await Task.Run(() =>
            {
                if (accounts == null)
                    accounts = Account.Accounts().ToList();
            });

            foreach (ReminderMarker item in items)
            {
                //Transaction type
                if (item.IncomeAccount == item.OutcomeAccount)
                {
                    if (item.Income > 0)
                        item.TransType = TransactionType.Income;
                    else
                        item.TransType = TransactionType.Outcome;
                }
                else
                {
                    bool incomeType = accounts.Find(i => i.Id == item.IncomeAccount).Type == "debt";
                    bool outcomeType = accounts.Find(i => i.Id == item.OutcomeAccount).Type == "debt";
                    if (incomeType)
                        item.TransType = TransactionType.mDept;
                    else if (outcomeType)
                        item.TransType = TransactionType.pDept;
                    else
                        item.TransType = TransactionType.Transfer;
                }

                //TransDateStamp For filter
                item.TransDateStamp = SettingsManager.toUnixTime(DateTime.Parse(item.Date));

                item.isCreated = false;
                item.isChanged = false;
                item.isDeleted = false;
            }
            return items;
        }

        private static IEnumerable<ReminderMarker> LoadDisplayData(ref IEnumerable<ReminderMarker> res, SQLiteConnection db)
        {
            List<Account> Accounts = db.Table<Account>().ToList();
            List<Instrument> Instruments = db.Table<Instrument>().ToList();
            List<Merchant> Merchants = db.Table<Merchant>().ToList();
            List<Tag> Tags = db.Table<Tag>().ToList();

            return res.Select(i =>
            {
                if (i.Merchant != null)
                    i.displayPayee = Merchants.FirstOrDefault(x => x.Id == i.Merchant).Title;
                //Income/Outcome account 
                i.displayIncomeAccount = Accounts.FirstOrDefault(x => x.Id == i.IncomeAccount).Title;
                i.displayIncomeAccountType = Accounts.FirstOrDefault(x => x.Id == i.IncomeAccount).Type;
                i.displayOutcomeAccount = Accounts.FirstOrDefault(x => x.Id == i.OutcomeAccount).Title;
                i.displayOutcomeAccountType = Accounts.FirstOrDefault(x => x.Id == i.OutcomeAccount).Type;
                //Instruments
                i.displayIncomeInstrument = Instruments.FirstOrDefault(x => x.Id == i.IncomeInstrument).Symbol;
                i.displayOutcomeInstrument = Instruments.FirstOrDefault(x => x.Id == i.OutcomeInstrument).Symbol;
                //Transaction Tags
                if (i.Tags != null && i.Tags.Count > 0)
                {
                    string _tags = "";
                    foreach (Guid x in i.Tags)
                    {
                        Tag t = Tags.FirstOrDefault(c => c.Id == x);
                        if (t.Parent != null)
                        {
                            _tags += Tags.FirstOrDefault(c => c.Id == t.Parent).Title + " / " + t.Title + ", ";
                        }
                        else
                            _tags += t.Title + ", ";
                    }
                    _tags = _tags.TrimEnd().TrimEnd(',');
                    i.displayTransTags = _tags;
                    i.displayTransactionIcon = Tags.FirstOrDefault(c => c.Id == i.Tags.FirstOrDefault()).Icon;
                    i.displayTagColorStr = Tags.FirstOrDefault(c => c.Id == i.Tags.FirstOrDefault()).ColorStr;
                    i.displayTransactionLetter = Tags.FirstOrDefault(c => c.Id == i.Tags.FirstOrDefault()).Title;
                }
                else
                {
                    switch (i.TransType)
                    {
                        case TransactionType.Transfer:
                            i.displayTransTags = ResourceLoader.GetForViewIndependentUse().GetString("TransTagTransfer");
                            i.displayTransactionIcon = "transTransfer";
                            break;
                        case TransactionType.mDept:
                            i.displayTransTags = ResourceLoader.GetForViewIndependentUse().GetString("TransTagGive");
                            i.displayTransactionIcon = "transDeptM";
                            break;
                        case TransactionType.pDept:
                            i.displayTransTags = ResourceLoader.GetForViewIndependentUse().GetString("TransTagTake");
                            i.displayTransactionIcon = "transDeptP";
                            break;
                        default:
                            i.displayTransTags = ResourceLoader.GetForViewIndependentUse().GetString("TransTagEmpty");
                            i.displayTransactionIcon = "0000_none";
                            break;
                    }
                }
                return i;
            });
        }

        public static ReminderMarker getByID(Guid ID)
        {
            using (SQLiteConnection db = new SQLiteConnection(DBInitializer.DB_PATH))
            {
                ReminderMarker i = db.Get<ReminderMarker>(ID);
                Account IncomeAcc = db.Get<Account>(i.IncomeAccount);
                Account OutcomeAcc = db.Get<Account>(i.OutcomeAccount);
                if (i.Merchant != null)
                    i.displayPayee = db.Get<Merchant>(i.Merchant).Title;
                //Income/Outcome account
                i.displayIncomeAccount = IncomeAcc.Title;
                i.displayOutcomeAccount = OutcomeAcc.Title;
                i.displayIncomeAccountType = IncomeAcc.Type;
                i.displayOutcomeAccountType = OutcomeAcc.Type;
                //Instruments
                i.displayIncomeInstrument = db.Get<Instrument>(i.IncomeInstrument).Symbol;
                i.displayOutcomeInstrument = db.Get<Instrument>(i.OutcomeInstrument).Symbol;
                //Transaction Tags
                if (i.Tags != null && i.Tags.Count > 0)
                {
                    string _tags = "";
                    foreach (Guid x in i.Tags)
                    {
                        Tag t = db.Get<Tag>(x);
                        if (t.Parent != null)
                        {
                            _tags += db.Get<Tag>(t.Parent).Title + " / " + t.Title + ", ";
                        }
                        else
                            _tags += t.Title + ", ";
                    }
                    _tags = _tags.TrimEnd().TrimEnd(',');
                    i.displayTransTags = _tags;
                    i.displayTransactionIcon = db.Get<Tag>(i.Tags.FirstOrDefault()).Icon;
                    i.displayTagColorStr = db.Get<Tag>(i.Tags.FirstOrDefault()).ColorStr;
                    i.displayTransactionLetter = db.Get<Tag>(i.Tags.FirstOrDefault()).Title;
                }
                else
                {
                    switch (i.TransType)
                    {
                        case TransactionType.Transfer:
                            i.displayTransTags = ResourceLoader.GetForViewIndependentUse().GetString("TransTagTransfer");
                            i.displayTransactionIcon = "transTransfer";
                            break;
                        case TransactionType.mDept:
                            i.displayTransTags = ResourceLoader.GetForViewIndependentUse().GetString("TransTagGive");
                            i.displayTransactionIcon = "transDeptM";
                            break;
                        case TransactionType.pDept:
                            i.displayTransTags = ResourceLoader.GetForViewIndependentUse().GetString("TransTagTake");
                            i.displayTransactionIcon = "transDeptP";
                            break;
                        default:
                            i.displayTransTags = ResourceLoader.GetForViewIndependentUse().GetString("TransTagEmpty");
                            i.displayTransactionIcon = "0000_none";
                            break;
                    }
                }
                return i;
            }
        }

        public static List<ReminderMarker> History(States _state)
        {
            using (SQLiteConnection db = new SQLiteConnection(DBInitializer.DB_PATH, SQLiteOpenFlags.ReadOnly))
            {
                if (_state != States.All)
                {
                    string state = GetStateStr(_state);
                    List<Guid> Alien = Account.AlienAccounts();
                    IEnumerable<ReminderMarker> res = db.Table<ReminderMarker>().Where(i => i.State.ToLower() == state &&
                                    (!Alien.Contains(i.IncomeAccount) || !Alien.Contains(i.OutcomeAccount)
                                    && i.TransType != TransactionType.mDept && i.TransType != TransactionType.pDept));
                    res = LoadDisplayData(ref res, db);
                    return res.ToList();
                }
                else
                {
                    List<Guid> Alien = Account.AlienAccounts();
                    IEnumerable<ReminderMarker> res = db.Table<ReminderMarker>().Where(i =>
                                    (!Alien.Contains(i.IncomeAccount) || !Alien.Contains(i.OutcomeAccount)
                                    && i.TransType != TransactionType.mDept && i.TransType != TransactionType.pDept));
                    res = LoadDisplayData(ref res, db);
                    return res.ToList();
                }
            }
        }
        public static List<ReminderMarker> History(Guid Id, States _state)
        {
            if (_state != States.All)
            {
                using (SQLiteConnection db = new SQLiteConnection(DBInitializer.DB_PATH, SQLiteOpenFlags.ReadOnly))
                {
                    string state = GetStateStr(_state);
                    List<Guid> Alien = Account.AlienAccounts();
                    IEnumerable<ReminderMarker> res = db.Table<ReminderMarker>().Where(i => i.Reminder == Id && i.State.ToLower() == state &&
                                    (!Alien.Contains(i.IncomeAccount) || !Alien.Contains(i.OutcomeAccount)
                                    && i.TransType != TransactionType.mDept && i.TransType != TransactionType.pDept));
                    res = LoadDisplayData(ref res, db);
                    return res.ToList();
                }
            }
            else
            {
                using (SQLiteConnection db = new SQLiteConnection(DBInitializer.DB_PATH, SQLiteOpenFlags.ReadOnly))
                {
                    List<Guid> Alien = Account.AlienAccounts();
                    IEnumerable<ReminderMarker> res = db.Table<ReminderMarker>().Where(i => i.Reminder == Id &&
                                    (!Alien.Contains(i.IncomeAccount) || !Alien.Contains(i.OutcomeAccount)
                                    && i.TransType != TransactionType.mDept && i.TransType != TransactionType.pDept));
                    res = LoadDisplayData(ref res, db);
                    return res.ToList();
                }
            }
        }

        public static List<ReminderMarker> ScheduledForToday()
        {
            using (SQLiteConnection db = new SQLiteConnection(DBInitializer.DB_PATH, SQLiteOpenFlags.ReadOnly))
            {
                List<Guid> Alien = Account.AlienAccounts();

                string state = GetStateStr(States.Planned);
                var filter = SettingsManager.toUnixTime(DateTime.UtcNow.Date);

                IEnumerable<ReminderMarker> res = db.Table<ReminderMarker>().Where(i => i.TransDateStamp == filter && i.State.ToLower() == state &&
                                (!Alien.Contains(i.IncomeAccount) || !Alien.Contains(i.OutcomeAccount)
                                && i.TransType != TransactionType.mDept && i.TransType != TransactionType.pDept));
                res = LoadDisplayData(ref res, db);
                return res.ToList();
            }
        }

        public static List<ReminderMarker> LatePayments()
        {
            using (SQLiteConnection db = new SQLiteConnection(DBInitializer.DB_PATH, SQLiteOpenFlags.ReadOnly))
            {
                List<Guid> Alien = Account.AlienAccounts();

                string state = GetStateStr(States.Planned);
                var filter = SettingsManager.toUnixTime(DateTime.UtcNow.Date);

                IEnumerable<ReminderMarker> res = db.Table<ReminderMarker>().Where(i => i.TransDateStamp < filter && i.State.ToLower() == state &&
                                (!Alien.Contains(i.IncomeAccount) || !Alien.Contains(i.OutcomeAccount)
                                && i.TransType != TransactionType.mDept && i.TransType != TransactionType.pDept));
                res = LoadDisplayData(ref res, db);
                return res.ToList();
            }
        }

        public static bool HasLatePayments()
        {
            using (SQLiteConnection db = new SQLiteConnection(DBInitializer.DB_PATH, SQLiteOpenFlags.ReadOnly))
            {
                List<Guid> Alien = Account.AlienAccounts();

                string state = GetStateStr(States.Planned);
                var filter = SettingsManager.toUnixTime(DateTime.UtcNow.Date);

                return db.Table<ReminderMarker>().Where(i => i.TransDateStamp < filter && i.State.ToLower() == state &&
                                (!Alien.Contains(i.IncomeAccount) || !Alien.Contains(i.OutcomeAccount)
                                && i.TransType != TransactionType.mDept && i.TransType != TransactionType.pDept)).Any();
            }
        }

        public static string GetStateStr(States _state)
        {
            switch (_state)
            {
                case States.Deleted:
                    return "deleted";
                case States.Planned:
                    return "planned";
                default:
                    return "processed";
            }
        }

        public enum States
        {
            Planned = 0,
            Deleted = 1,
            Processed = 2,
            All = 3
        }
    }
}
