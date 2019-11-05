using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using zMoneyWinX.Client;

namespace zMoneyWinX.Model
{
    public partial class Account : DBObject
    {      
        #region Extension
        [JsonIgnore]
        public bool? Savings { get; set; }

        [JsonIgnore, Ignore]
        public string displayBalance
        {
            get
            {
                return Balance.ToString("#,0.##") + " " + displayInstrument;
            }
        }

        [JsonIgnore, Ignore]
        public string displayCreditBalance
        {
            get
            {
                if (CreditLimit > 0)
                {
                    if (Balance > 0)
                        return (Balance + CreditLimit).ToString("#,0.##") + " " + displayInstrument;
                    else
                        return (CreditLimit - (Balance * -1)).ToString("#,0.##") + " " + displayInstrument;
                }
                else
                    return "";
            }
        }

        [JsonIgnore, Ignore]
        public Visibility settingsVisibility
        {
            get
            {
                if (Id != Guid.Empty && (Type == "cash" || Type == "ccard" || Type == "checking" || Type == "loan" || Type == "deposit" || Type == "debt") && string.IsNullOrEmpty(OwnerName))
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }
        }

        [JsonIgnore, Ignore]
        public Visibility commentVisibility
        {
            get
            {
                if (CreditLimit > 0)
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }
        }

        [JsonIgnore]
        public string displayInstrument { get; set; }
        [JsonIgnore]
        public string displayCompany { get; set; }

        [JsonIgnore]
        public string _SyncId { get; set; }

        [JsonIgnore, Ignore]
        public Uri displayAccountIcon => Extensions.AccountIcon(Type);

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

        [JsonIgnore, Ignore]
        public string OwnerName { get; set; }
        #endregion

        public static List<Account> createMetaData(List<Account> items)
        {
            foreach (Account item in items)
            {
                item.displayCompany = Model.Company.getTitle(item.Company);
                item.displayInstrument = Model.Instrument.GetSymbol(item.Instrument);

                item.isCreated = false;
                item.isChanged = false;
                item.isDeleted = false;
            }
            return items;
        }

        public void createMetaData()
        {
            displayCompany = Model.Company.getTitle(Company);
            displayInstrument = Model.Instrument.GetSymbol(Instrument);
            isDeleted = false;

            if (Type == "cash" || Type == "debt")
            {
                Company = null;
                SyncId = null;
                EnableSms = false;
                EnableCorrection = false;
                CreditLimit = 0;
            }
        }

        public static List<Account> AccountsTrans()
        {
            using (SQLiteConnection db = new SQLiteConnection(DBInitializer.DB_PATH))
                return db.Table<Account>().Where(i => !i.Archive && i.Type != "debt").OrderBy(i => i.Type).OrderBy(i => i.Title).ToList();
        }

        public static List<Account> AccountsTrans(Guid inc, Guid outc)
        {
            using (SQLiteConnection db = new SQLiteConnection(DBInitializer.DB_PATH))
                return db.Table<Account>().Where(i => (!i.Archive || i.Id == inc || i.Id == outc) && i.Type != "debt").OrderBy(i => i.Type).OrderBy(i => i.Title).ToList();
        }

        public static List<Guid> AlienAccounts()
        {
            using (SQLiteConnection db = new SQLiteConnection(DBInitializer.DB_PATH))
                return db.Table<Account>().Where(i => !i.Archive && i.Type != "debt" && i.Role != null && i.Role != Model.User.CurrentUserID).OrderBy(i => i.Type).OrderBy(i => i.Title).Select(i => i.Id).ToList();
        }

        public static ObservableCollection<Account> Accounts()
        {
            using (SQLiteConnection db = new SQLiteConnection(DBInitializer.DB_PATH))
                return new ObservableCollection<Account>(db.Table<Account>().OrderBy(i => i.Type));
        }

        public static Guid DebtID
        {
            get
            {
                using (SQLiteConnection db = new SQLiteConnection(DBInitializer.DB_PATH))
                    return db.Get<Account>(i => i.Type == "debt").Id;
            }
        }
        public static Account GetByID(Guid ID)
        {
            using (SQLiteConnection db = new SQLiteConnection(DBInitializer.DB_PATH))
                return db.Get<Account>(ID);
        }

        public static ObservableCollection<GroupInfoList> GroupedAccounts()
        {
            ObservableCollection<GroupInfoList> groups = new ObservableCollection<GroupInfoList>();
            IEnumerable<Account> accList = new List<Account>();
            GroupInfoList info = new GroupInfoList();

            int CurrentUserIDStr = Model.User.CurrentUserID;

            using (SQLiteConnection db = new SQLiteConnection(DBInitializer.DB_PATH))
            {
                //Active accounts
                if (!SettingsManager.accountsdisplaymode)
                    accList = db.Table<Account>().Where(i => !i.Archive && !i.isDeleted && i.InBalance && (i.Role == null || i.Role == CurrentUserIDStr)).OrderBy(i => i.Type).OrderBy(i => i.Title);
                else
                    accList = db.Table<Account>().Where(i => !i.Archive && !i.isDeleted && (i.Role == null || i.Role == CurrentUserIDStr)).OrderBy(i => i.Type).OrderBy(i => i.Title);
                try
                {
                    //Rename debt account for current locale
                    accList.FirstOrDefault(i => i.Type == "debt").Title = ResourceLoader.GetForViewIndependentUse().GetString("accDebt");
                }
                catch { }

                info = new GroupInfoList(ResourceLoader.GetForViewIndependentUse().GetString("AccountsActive"));
                foreach (Account item in accList)
                    info.Add(item);

                if (info.Count() > 0)
                    groups.Add(info);

                if (accList.Where(i => i.Type == "debt").Any())
                {
                    //mDebt
                    Guid debtID = DebtID;
                    var _transList = db.Table<Transaction>().Where(i => (i.TransType == TransactionType.mDept || i.TransType == TransactionType.pDept) && !i.isDeleted)
                        .GroupBy(i => new { Payee = i.Payee, OutInstr = i.displayOutcomeInstrument, InInstr = i.displayIncomeInstrument })
                        .Where(i => i.Where(a => a.TransType == TransactionType.mDept).Sum(a => a.Income) > i.Where(a => a.TransType == TransactionType.pDept).Sum(a => a.Outcome));

                    info = new GroupInfoList(ResourceLoader.GetForViewIndependentUse().GetString("AccountsOweme"));
                    foreach (var item in _transList)
                        info.Add(new Account
                        {
                            Id = Guid.Empty,
                            Title = item.Key.Payee,
                            Balance = item.Where(a => a.OutcomeAccount != debtID).Sum(a => a.Outcome) - item.Where(a => a.IncomeAccount != debtID).Sum(a => a.Income),
                            displayInstrument = item.Key.OutInstr
                        });
                    if (info.Count() > 0)
                        groups.Add(info);

                    //pDebt
                    _transList = db.Table<Transaction>().Where(i => (i.TransType == TransactionType.mDept || i.TransType == TransactionType.pDept) && !i.isDeleted)
                        .GroupBy(i => new { Payee = i.Payee, OutInstr = i.displayOutcomeInstrument, InInstr = i.displayIncomeInstrument })
                        .Where(i => i.Where(a => a.TransType == TransactionType.mDept).Sum(a => a.Income) < i.Where(a => a.TransType == TransactionType.pDept).Sum(a => a.Outcome));

                    info = new GroupInfoList(ResourceLoader.GetForViewIndependentUse().GetString("AccountsIOwe"));
                    foreach (var item in _transList)
                        info.Add(new Account
                        {
                            Id = Guid.Empty,
                            Title = item.Key.Payee,
                            Balance = item.Where(a => a.IncomeAccount != debtID).Sum(a => a.Income) - item.Where(a => a.OutcomeAccount != debtID).Sum(a => a.Outcome),
                            displayInstrument = item.Key.InInstr
                        });
                    if (info.Count() > 0)
                        groups.Add(info);
                }

                //Archived and aliens accounts
                if (SettingsManager.accountsdisplaymode)
                {
                    accList = db.Table<Account>().Where(i => !i.isDeleted && (i.Role != null && i.Role != CurrentUserIDStr)).OrderBy(i => i.Type).OrderBy(i => i.Title);
                    info = new GroupInfoList(ResourceLoader.GetForViewIndependentUse().GetString("AccountsAlien"));
                    foreach (Account item in accList)
                    {
                        item.OwnerName = Model.User.GetTitle(item.Role);
                        info.Add(item);
                    }
                    if (info.Count() > 0)
                        groups.Add(info);

                    accList = db.Table<Account>().Where(i => i.Archive && !i.isDeleted && (i.Role == null || i.Role == CurrentUserIDStr)).OrderBy(i => i.Type).OrderBy(i => i.Title);
                    info = new GroupInfoList(ResourceLoader.GetForViewIndependentUse().GetString("AccountsArchive"));
                    foreach (Account item in accList)
                        info.Add(item);

                    if (info.Count() > 0)
                        groups.Add(info);
                }
            }
            return groups;
        }

        public static string GetBalance(BalanceType type = BalanceType.All)
        {
            try
            {
                int CurrentUserIDStr = Model.User.CurrentUserID;
                double res = 0;
                using (SQLiteConnection db = new SQLiteConnection(DBInitializer.DB_PATH))
                {
                    int Currency = Model.User.GetDefaultCurrency();

                    List<Account> acc = new List<Account>();
                    if (type == BalanceType.Negative)
                    {
                        res = db.Table<Account>().Where(i => i.Type != "debt" && i.InBalance && !i.Archive && i.Instrument == Currency && !i.isDeleted && (i.Role == null || i.Role == CurrentUserIDStr) && i.Balance < 0).Sum(i => i.Balance);
                        acc = db.Table<Account>().Where(i => i.Type != "debt" && i.InBalance && !i.Archive && i.Instrument != Currency && !i.isDeleted && (i.Role == null || i.Role == CurrentUserIDStr) && i.Balance < 0).ToList();
                    }
                    else if (type == BalanceType.All)
                    {
                        res = db.Table<Account>().Where(i => i.InBalance && !i.Archive && i.Instrument == Currency && !i.isDeleted && (i.Role == null || i.Role == CurrentUserIDStr)).Sum(i => i.Balance);
                        acc = db.Table<Account>().Where(i => i.InBalance && !i.Archive && i.Instrument != Currency && !i.isDeleted && (i.Role == null || i.Role == CurrentUserIDStr)).ToList();
                    }
                    else if (type == BalanceType.Available)
                    {
                        res = db.Table<Account>().Where(i => i.Type != "debt" && i.InBalance && !i.Archive && i.Instrument == Currency && !i.isDeleted && (i.Role == null || i.Role == CurrentUserIDStr)).Sum(i => i.CreditBalance);
                        acc = db.Table<Account>().Where(i => i.Type != "debt" && i.InBalance && !i.Archive && i.Instrument != Currency && !i.isDeleted && (i.Role == null || i.Role == CurrentUserIDStr)).ToList();
                    }

                    if (acc.Count > 0)
                    {
                        foreach (Account item in acc)
                        {
                            if (type == BalanceType.Available)
                            {
                                if (item.Instrument == 2)
                                    res += item.CreditBalance * Model.Instrument.GetRate(item.Instrument);
                                else
                                    res += item.CreditBalance * Model.Instrument.GetRate(item.Instrument) / Model.Instrument.GetRate(Currency);
                            }
                            else
                            {
                                if (item.Instrument == 2)
                                    res += item.Balance * Model.Instrument.GetRate(item.Instrument);
                                else
                                    res += item.Balance * Model.Instrument.GetRate(item.Instrument) / Model.Instrument.GetRate(Currency);
                            }
                        }
                    }


                }
                return res.ToString("n0");
            }
            catch
            {
                return "0";
            }
        }
        public static string GetBalanceAll(BalanceType type)
        {
            try
            {
                int CurrentUserIDStr = Model.User.CurrentUserID;
                double res = 0;
                using (SQLiteConnection db = new SQLiteConnection(DBInitializer.DB_PATH))
                {
                    int Currency = Model.User.GetDefaultCurrency();

                    List<Account> acc = new List<Account>();
                    if (type == BalanceType.Negative)
                    {
                        res = db.Table<Account>().Where(i => i.Type != "debt" && !i.Archive && i.Instrument == Currency && !i.isDeleted && (i.Role == null || i.Role == CurrentUserIDStr) && i.Balance < 0).Sum(i => i.Balance);
                        acc = db.Table<Account>().Where(i => i.Type != "debt" && !i.Archive && i.Instrument != Currency && !i.isDeleted && (i.Role == null || i.Role == CurrentUserIDStr) && i.Balance < 0).ToList();
                    }
                    else if (type == BalanceType.All)
                    {
                        res = db.Table<Account>().Where(i => !i.Archive && i.Instrument == Currency && !i.isDeleted && (i.Role == null || i.Role == CurrentUserIDStr)).Sum(i => i.Balance);
                        acc = db.Table<Account>().Where(i => !i.Archive && i.Instrument != Currency && !i.isDeleted && (i.Role == null || i.Role == CurrentUserIDStr)).ToList();
                    }
                    else if (type == BalanceType.Available)
                    {
                        res = db.Table<Account>().Where(i => i.Type != "debt" && !i.Archive && i.Instrument == Currency && !i.isDeleted && (i.Role == null || i.Role == CurrentUserIDStr)).Sum(i => i.CreditBalance);
                        acc = db.Table<Account>().Where(i => i.Type != "debt" && !i.Archive && i.Instrument != Currency && !i.isDeleted && (i.Role == null || i.Role == CurrentUserIDStr)).ToList();
                    }

                    if (acc.Count > 0)
                    {
                        foreach (Account item in acc)
                        {
                            if (type == BalanceType.Available)
                            {
                                if (item.Instrument == 2)
                                    res += item.CreditBalance * Model.Instrument.GetRate(item.Instrument);
                                else
                                    res += item.CreditBalance * Model.Instrument.GetRate(item.Instrument) / Model.Instrument.GetRate(Currency);
                            }
                            else
                            {
                                if (item.Instrument == 2)
                                    res += item.Balance * Model.Instrument.GetRate(item.Instrument);
                                else
                                    res += item.Balance * Model.Instrument.GetRate(item.Instrument) / Model.Instrument.GetRate(Currency);
                            }
                        }
                    }


                }
                return res.ToString("n0");
            }
            catch
            {
                return "0";
            }
        }
        public double CreditBalance
        {
            get
            {
                if (CreditLimit > 0)
                {
                    if (Balance > 0)
                        return (Balance + CreditLimit);
                    else
                        return (CreditLimit - (Balance * -1));
                }
                else
                    return Balance;
            }
        }

        public enum BalanceType
        {
            All = 0,
            Negative = 1,
            Available = 2
        }
    }
}