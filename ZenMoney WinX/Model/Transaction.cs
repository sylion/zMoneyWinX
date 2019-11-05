using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Media;
using zMoneyWinX.Client;

namespace zMoneyWinX.Model
{
    public partial class Transaction : DBObject
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
        public bool isPlanned { get; private set; } = false;

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
        #endregion

        public static DateTime convertGroupName(string val)
        {
            if (val == ResourceLoader.GetForViewIndependentUse().GetString("PlannedForToday"))
                return DateTime.Now.Date.AddDays(10);
            if (val == ResourceLoader.GetForViewIndependentUse().GetString("DateTomorrow"))
                return DateTime.Now.Date.AddDays(1);
            if (val == ResourceLoader.GetForViewIndependentUse().GetString("DateToday"))
                return DateTime.Now.Date;
            if (val == ResourceLoader.GetForViewIndependentUse().GetString("DateYesterday"))
                return DateTime.Now.Date.AddDays(-1);
            return DateTime.Parse(val);
        }

        public static async Task<List<Transaction>> createMetaData(List<Transaction> items, List<Account> accounts = null)
        {
            //For not full sync process
            await Task.Run(() =>
            {
                if (accounts == null)
                    accounts = Account.Accounts().ToList();
            });

            foreach (Transaction item in items)
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

        public async Task Update(TransactionEdit item, List<Guid> _tags)
        {
            await Task.Run(() =>
            {
                Tags = _tags;

                Income = item.editIncome;
                IncomeAccount = item.editIncomeAccount;

                Outcome = item.editOutcome;
                OutcomeAccount = item.editOutcomeAccount;

                Date = item.editDate;
                Comment = item.editComment;

                if (!string.IsNullOrWhiteSpace(item.editPayee))
                {
                    Merchant tmpMerchant = Model.Merchant.GetByTitle(item.editPayee);
                    if (tmpMerchant == null)
                    {
                        tmpMerchant = Model.Merchant.newMerchant(item.editPayee);
                        App.Provider.VMMerchant.Insert(tmpMerchant);
                        Merchant = tmpMerchant.Id;
                    }
                    else
                        Merchant = tmpMerchant.Id;
                }
                else
                    Merchant = null;

                TransType = item.editTransType;
                ReminderMarker = item.ReminderMarker;

                User = Model.User.CurrentUserID;

                if (TransType != TransactionType.Transfer)
                    Payee = Model.Merchant.GetTitle(Merchant);
                else
                    Payee = "";

                if (Id == Guid.Empty)
                {
                    Id = Guid.NewGuid();
                    isCreated = true;
                    Created = SettingsManager.toUnixTime(DateTime.UtcNow);
                }

                isChanged = true;
                Changed = SettingsManager.toUnixTime(DateTime.UtcNow);

                //Instruments
                IncomeInstrument = Account.GetByID(IncomeAccount).Instrument;
                OutcomeInstrument = Account.GetByID(OutcomeAccount).Instrument;

                if (TransType == TransactionType.Income)
                {
                    Outcome = 0;
                    OutcomeAccount = IncomeAccount;
                }
                if (TransType == TransactionType.Outcome)
                {
                    Income = 0;
                    IncomeAccount = OutcomeAccount;
                }
                if (TransType == TransactionType.mDept)
                    IncomeAccount = Account.DebtID;
                if (TransType == TransactionType.pDept)
                    OutcomeAccount = Account.DebtID;

                //TransDateStamp For filter
                TransDateStamp = SettingsManager.toUnixTime(DateTime.Parse(Date));
            });
            NotifyPropertyChanged();
        }

        private static IEnumerable<Transaction> LoadDisplayData(ref IEnumerable<Transaction> res, SQLiteConnection db)
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

        public static List<Transaction> TransactionsHistory(TransactionFilter filter)
        {
            if (filter != null)
                return TransactionsFiltered(filter);

            long dateFrom = 0;
            switch (SettingsManager.historymode)
            {
                case SettingsManager.HistoryMode.Week:
                    dateFrom = SettingsManager.toUnixTime(DateTime.Now.Date.AddDays(-7));
                    break;
                case SettingsManager.HistoryMode.Month:
                    dateFrom = SettingsManager.toUnixTime(DateTime.Now.Date.AddMonths(-1));
                    break;
                case SettingsManager.HistoryMode.HalfYear:
                    dateFrom = SettingsManager.toUnixTime(DateTime.Now.Date.AddMonths(-6));
                    break;
                case SettingsManager.HistoryMode.Year:
                    dateFrom = SettingsManager.toUnixTime(DateTime.Now.Date.AddYears(-1));
                    break;
                default:
                    break;
            }
            using (SQLiteConnection db = new SQLiteConnection(DBInitializer.DB_PATH, SQLiteOpenFlags.ReadOnly))
            {
                List<Guid> Alien = Account.AlienAccounts();
                IEnumerable<Transaction> res = db.Table<Transaction>().Where(i => i.TransDateStamp >= dateFrom &&
                                (!Alien.Contains(i.IncomeAccount) || !Alien.Contains(i.OutcomeAccount)
                                && i.TransType != TransactionType.mDept && i.TransType != TransactionType.pDept));
                res = LoadDisplayData(ref res, db);
                return res.ToList();
            }
        }

        protected static List<Transaction> TransactionsFiltered(TransactionFilter filter)
        {
            using (SQLiteConnection db = new SQLiteConnection(DBInitializer.DB_PATH, SQLiteOpenFlags.ReadOnly))
            {
                List<Guid> Alien = Account.AlienAccounts();
                IEnumerable<Transaction> res = db.Table<Transaction>().Where(i => !Alien.Contains(i.IncomeAccount) || !Alien.Contains(i.OutcomeAccount)
                                                                             && i.TransType != TransactionType.mDept && i.TransType != TransactionType.pDept);

                if (filter != null)
                {
                    if (filter.DatePeriod != TransactionPeriod.All)
                    {
                        res = res.Where(i => i.TransDateStamp >= filter.DateFromStamp);
                        res = res.Where(i => i.TransDateStamp <= filter.DateToStamp);
                    }

                    if (filter.TransType != TransactionTypeFilter.All)
                    {
                        switch (filter.TransType)
                        {
                            case TransactionTypeFilter.Income:
                                res = res.Where(i => i.TransType == TransactionType.Income);
                                break;
                            case TransactionTypeFilter.Outcome:
                                res = res.Where(i => i.TransType == TransactionType.Outcome);
                                break;
                            case TransactionTypeFilter.Transfer:
                                res = res.Where(i => i.TransType == TransactionType.Transfer);
                                break;
                            case TransactionTypeFilter.Dept:
                                res = res.Where(i => i.TransType == TransactionType.pDept || i.TransType == TransactionType.mDept);
                                break;
                            default:
                                break;
                        }
                    }

                    if (filter.Account != null && filter.Account.Count > 0)
                        res = res.Where(i => filter.Account.Contains(i.IncomeAccount) || filter.Account.Contains(i.OutcomeAccount));

                    if (filter.Merchant != null && filter.Merchant != Guid.Empty)
                        res = res.Where(i => i.Merchant == filter.Merchant);

                    if (!string.IsNullOrWhiteSpace(filter.Comment))
                        res = res.Where(i => i.Comment != null && i.Comment.ToLower().Contains(filter.Comment.ToLower()));

                    if (filter.Tags != null && filter.Tags.Count > 0)
                    {
                        if (!filter.Exclude)
                            res = res.Where(i => i.Tags != null && filter.Tags.Intersect(i.Tags).Any()).ToList();
                        else
                            res = res.Where(i => (i.Tags != null && !filter.Tags.Intersect(i.Tags).Any()) || i.Tags == null).ToList();
                    }
                }

                //===Display Metadata===
                res = LoadDisplayData(ref res, db);

                return res.ToList();
            }
        }

        public static void DeleteForAccount(Guid accountID)
        {
            using (SQLiteConnection db = new SQLiteConnection(DBInitializer.DB_PATH))
            {
                db.RunInTransaction(() =>
                {
                    foreach (var i in db.Table<Transaction>().Where(i => (i.IncomeAccount == accountID || i.OutcomeAccount == accountID) && !i.isDeleted))
                    {
                        i.isDeleted = true;
                        i.Changed = SettingsManager.toUnixTime(DateTime.UtcNow);
                        db.InsertOrReplace(i);
                    }
                });
            }
        }

        public static Transaction getByID(Guid ID)
        {
            using (SQLiteConnection db = new SQLiteConnection(DBInitializer.DB_PATH))
            {
                Transaction i = db.Get<Transaction>(ID);
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
    }
}