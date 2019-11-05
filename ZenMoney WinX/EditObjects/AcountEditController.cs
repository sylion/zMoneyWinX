using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using zMoneyWinX.Client;

namespace zMoneyWinX.Model
{
    public class AcountEditController : Account
    {
        public AcountEditController()
        {
            Create = true;
            CanEditType = true;
            CanEditName = true;
            CanEditCurrency = true;
            editInstrument = Model.User.GetDefaultCurrency();
            editType = "cash";
            accountTypes = AccountTypeStr.getAccTypes();
            AccountList = AccountsTrans();
            AccountList.Insert(0, new Account() { Id = Guid.Empty, Title = ResourceLoader.GetForViewIndependentUse().GetString("AccountEditDontCreate") });
        }
        public AcountEditController(Account item)
        {
            Id = item.Id;
            User = item.User;
            editInstrument = item.Instrument;
            editType = item.Type;
            Role = item.Role;
            Private = item.Private;
            editTitle = item.Title;
            editInBalance = item.InBalance;
            editCreditLimit = item.CreditLimit;
            StartBalance = item.StartBalance;
            Balance = item.Balance;
            editCompanyId = item.Company;
            editArchive = item.Archive;
            editEnableCorrection = item.EnableCorrection;
            StartDate = item.StartDate;
            Capitalization = item.Capitalization;
            Percent = item.Percent;
            Changed = item.Changed;
            SyncId = item.SyncId;
            editEnableSms = item.EnableSms;
            PayoffStep = item.PayoffStep;
            PayoffInterval = item.PayoffInterval;
            EndDateOffsetInterval = item.EndDateOffsetInterval;
            EndDateOffset = item.EndDateOffset;


            editCompany = item.displayCompany;
            editSavings = item.Savings;

            if (item.Type == "deposit")
            {
                if (string.IsNullOrEmpty(PayoffInterval))
                    editPercentMode = 2;
                else if (PayoffStep == 3)
                    editPercentMode = 1;
                else
                    editPercentMode = 0;
            }

            if (item.Type == "loan" || item.Type == "deposit" || item.Type == "debt")
            {
                CanEditType = false;
                accountTypes = AccountTypeStr.getAccTypes(true);
            }
            else
            {
                CanEditType = true;
                accountTypes = AccountTypeStr.getAccTypes();
            }

            if (item.Type == "debt")
            {
                CanEditName = false;
                CanEditCurrency = false;
            }
            else
            {
                CanEditName = true;
                CanEditCurrency = true;
            }

            AccountList = new List<Account>();
            AccountList.Add(new Account() { Id = Guid.Empty, Title = "Do not create" });
        }
        public bool Create { get; private set; }
        public List<AccountTypeStr> accountTypes { get; private set; }
        public ObservableCollection<Mode> PercentMode { get; private set; } = Mode.getPercentMode();
        public ObservableCollection<Mode> PaymentMode { get; private set; } = Mode.getPaymentMode();
        public List<Period> PeriodMode { get; private set; } = Period.getPeriodMode();
        public List<Account> AccountList { get; private set; }

        private string _TermHeader { get; set; }
        public string TermHeader { get { return _TermHeader; } set { _TermHeader = value; NotifyPropertyChanged(); } }
        private string _InOutAccount { get; set; }
        public string InOutAccount { get { return _InOutAccount; } set { _InOutAccount = value; NotifyPropertyChanged(); } }


        public bool CanEditType { get; private set; }
        public bool CanEditName { get; private set; }
        public bool CanEditCurrency { get; private set; }
        public bool CanEditAmount { get { return Create; } }
        public Visibility isStartDateVisible { get { return Type == "deposit" || Type == "loan" ? Visibility.Visible : Visibility.Collapsed; } }
        public Visibility isAmountVisible { get { return Type == "deposit" || Type == "loan" ? Visibility.Visible : Visibility.Collapsed; } }
        public Visibility isPeriodVisible { get { return Type == "deposit" || Type == "loan" ? Visibility.Visible : Visibility.Collapsed; } }
        public Visibility isPercentVisible { get { return Type == "deposit" || Type == "loan" ? Visibility.Visible : Visibility.Collapsed; } }
        public Visibility isPercentModeVisible { get { return Type == "deposit" ? Visibility.Visible : Visibility.Collapsed; } }
        public Visibility isPaymentModeVisible { get { return Type == "loan" ? Visibility.Visible : Visibility.Collapsed; } }
        public Visibility isCapitalizationVisible { get { return Type == "deposit" ? Visibility.Visible : Visibility.Collapsed; } }
        public Visibility isAccountListVisible { get { return Create && (Type == "deposit" || Type == "loan") ? Visibility.Visible : Visibility.Collapsed; } }

        public Visibility isCreditLimitVisible { get { return Type != "debt" && Type != "cash" && Type != "deposit" && Type != "loan" ? Visibility.Visible : Visibility.Collapsed; } }
        public Visibility isStartBalanceVisible { get { return Create && Type != "debt" && Type != "deposit" && Type != "loan" ? Visibility.Visible : Visibility.Collapsed; } }
        public Visibility isCompanyVisible { get { return Type != "debt" && Type != "cash" ? Visibility.Visible : Visibility.Collapsed; } }
        public Visibility isSavingsVisible { get { return Type != "debt" && Type != "loan" && Type != "deposit" ? Visibility.Visible : Visibility.Collapsed; } }
        public Visibility isAliasVisible { get { return Type != "debt" && Type != "cash" ? Visibility.Visible : Visibility.Collapsed; } }
        public Visibility isSMSVisible { get { return Type != "debt" && Type != "cash" ? Visibility.Visible : Visibility.Collapsed; } }
        public Visibility isCorrectionVisible { get { return Type != "debt" && Type != "cash" ? Visibility.Visible : Visibility.Collapsed; } }
        public Visibility isArchiveVisible { get { return Type == "debt" ? Visibility.Collapsed : Visibility.Visible; } }
        public Visibility isPrivateVisible { get { return Type == "debt" ? Visibility.Collapsed : Visibility.Visible; } }

        //Edit fields
        public string editTitle { get { return Title; } set { Title = value; NotifyPropertyChanged(); } }
        public string editType
        {
            get
            { return Type; }
            set
            {
                Type = value;
                if (Type == "loan")
                {
                    _TermHeader = ResourceLoader.GetForViewIndependentUse().GetString("AccountEditLoanTerm");
                    _InOutAccount = ResourceLoader.GetForViewIndependentUse().GetString("AccountEditLoanInAccount");
                }
                if (Type == "deposit")
                {
                    _TermHeader = ResourceLoader.GetForViewIndependentUse().GetString("AccountEditDepositTerm");
                    _InOutAccount = ResourceLoader.GetForViewIndependentUse().GetString("AccountEditDepositOutAccount");
                }
                NotifyPropertyChanged();
            }
        }
        public int editInstrument { get { return Instrument; } set { Instrument = value; NotifyPropertyChanged(); } }

        public DateTimeOffset editStartDate
        {
            get { if (StartDate != null) return DateTime.Parse(StartDate); else StartDate = DateTime.Now.ToString("yyyy-MM-dd"); return DateTime.Now; }
            set { StartDate = value.DateTime.ToString("yyyy-MM-dd"); NotifyPropertyChanged(); }
        }
        public double editPercent
        {
            get { if (Percent != null) return (double)Percent; else Percent = 0; return 0; }
            set { Percent = value; NotifyPropertyChanged(); }
        }
        private int _editPercentMode { get; set; }
        public int editPercentMode { get { return _editPercentMode; } set { _editPercentMode = value; NotifyPropertyChanged(); } }

        public int editPaymentMode
        {
            get
            {
                if (Capitalization == null)
                    Capitalization = true;
                return (bool)Capitalization ? 0 : 1;
            }
            set
            {
                Capitalization = value == 0;
                NotifyPropertyChanged();
            }
        }

        public bool editCapitalization
        {
            get { if (Capitalization != null) return (bool)Capitalization; else Capitalization = false; return false; }
            set { Capitalization = value; NotifyPropertyChanged(); }
        }
        public int editPeriod
        {
            get
            {
                if (EndDateOffset != null)
                    return (int)EndDateOffset;
                else
                    EndDateOffset = 0;
                return 0;
            }
            set
            {
                EndDateOffset = value;
                if (value > 3)
                {
                    PercentMode.Clear();
                    foreach (var item in Mode.getPercentMode(true))
                        PercentMode.Add(item);
                }
                else
                {
                    PercentMode.Clear();
                    foreach (var item in Mode.getPercentMode())
                        PercentMode.Add(item);
                    if (_editPercentMode == 1)
                        _editPercentMode = 0;
                }
                NotifyPropertyChanged();
            }
        }
        public string editPeriodMode
        {
            get
            {
                if (!string.IsNullOrEmpty(EndDateOffsetInterval))
                    return EndDateOffsetInterval;
                else
                    EndDateOffsetInterval = "month";
                return "month";
            }
            set
            {
                EndDateOffsetInterval = value;
                NotifyPropertyChanged();
            }
        }
        private Guid? _editAccount { get; set; } = Guid.Empty;
        public Guid? editAccount { get { return _editAccount; } set { _editAccount = value; NotifyPropertyChanged(); } }

        public double editCreditLimit { get { return CreditLimit; } set { CreditLimit = value; NotifyPropertyChanged(); } }
        public double editStartBalance
        {
            get
            {
                if (StartBalance != null)
                    return (double)StartBalance;
                else
                {
                    StartBalance = 0;
                    return (double)StartBalance;
                }
            }
            set { StartBalance = value; NotifyPropertyChanged(); }
        }

        private string _editCompany { get; set; }
        public string editCompany { get { return _editCompany; } set { _editCompany = value; NotifyPropertyChanged(); } }
        public int? editCompanyId { get { return Company; } set { Company = value; NotifyPropertyChanged(); } }

        private ObservableCollection<string> _editSyncId { get; set; }
        public ObservableCollection<string> editSyncId
        {
            get
            {
                if (_editSyncId == null)
                {
                    if (SyncId == null)
                        SyncId = new List<string>();
                    _editSyncId = new ObservableCollection<string>(SyncId);
                }
                return _editSyncId;
            }
            set
            {
                _editSyncId = value;
                NotifyPropertyChanged();
            }
        }

        public bool? editSavings { get { return Savings; } set { Savings = value; NotifyPropertyChanged(); } }

        public bool editInBalance { get { return InBalance; } set { InBalance = value; NotifyPropertyChanged(); } }
        public bool editEnableSms { get { return EnableSms; } set { EnableSms = value; NotifyPropertyChanged(); } }
        public bool editEnableCorrection { get { return EnableCorrection; } set { EnableCorrection = value; NotifyPropertyChanged(); } }
        public bool editArchive { get { return Archive; } set { Archive = value; NotifyPropertyChanged(); } }
        public bool editRole
        {
            get { return Role != null; }
            set { if (value) Role = Model.User.CurrentUserID; else Role = null; NotifyPropertyChanged(); }
        }

        public string Validate()
        {
            string res = "";
            if (string.IsNullOrWhiteSpace(editTitle) || string.IsNullOrEmpty(editTitle))
                res += "\n" + ResourceLoader.GetForViewIndependentUse().GetString("ErrorEmptyTitle");

            if (Type == "deposit")
            {
                if (StartBalance == null || StartBalance <= 0)
                    res += "\n" + ResourceLoader.GetForViewIndependentUse().GetString("AccountValidationDepositAmount");
                if (EndDateOffset == null || EndDateOffset <= 0)
                    res += "\n" + ResourceLoader.GetForViewIndependentUse().GetString("AccountValidationDepositTerm");
            }
            if (Type == "loan")
            {
                if (StartBalance == null || StartBalance <= 0)
                    res += "\n" + ResourceLoader.GetForViewIndependentUse().GetString("AccountValidationLoanAmount");
                if (EndDateOffset == null || EndDateOffset <= 0)
                    res += "\n" + ResourceLoader.GetForViewIndependentUse().GetString("AccountValidationLoanTerm");
            }
            return res;
        }

        public void Save(Account res)
        {
            if (Create || Id == Guid.Empty)
                res.Id = Guid.NewGuid();
            else
                res.Id = Id;

            res.User = Model.User.CurrentUserID;
            res.Instrument = Instrument;
            res.Type = Type;
            res.Role = Role;
            res.Private = Private;
            res.Title = Title;
            res.InBalance = InBalance;
            res.CreditLimit = CreditLimit;

            if (Create && Type != "deposit" && Type != "loan")
                res.Balance = (double)StartBalance;
            else if (Type != "deposit" && Type != "loan")
                res.Balance = Balance;

            res.StartBalance = (double)StartBalance;
            res.Company = Model.Company.getID(editCompany);
            res.Archive = Archive;
            res.EnableCorrection = EnableCorrection;
            res.StartDate = StartDate;
            res.Changed = SettingsManager.toUnixTime(DateTime.UtcNow);
            res.SyncId = editSyncId.ToList();
            res.EnableSms = EnableSms;

            res.Savings = Savings;
            res.isChanged = true;
            res.isCreated = Create;

            if (Type == "loan")
            {
                res.Percent = Percent;
                res.PayoffStep = 1;
                res.PayoffInterval = "month";
                res.EndDateOffset = EndDateOffset;
                res.EndDateOffsetInterval = EndDateOffsetInterval;
                res.Capitalization = Capitalization;
            }
            else if (Type == "deposit")
            {
                res.Percent = Percent;
                switch (editPercentMode)
                {
                    case 0:
                        res.PayoffStep = 1;
                        break;
                    case 1:
                        res.PayoffStep = 3;
                        break;
                    default:
                        res.PayoffStep = 0;
                        break;
                }
                if (PayoffStep == 0)
                    res.PayoffInterval = "";
                else if (PayoffStep == 1 && EndDateOffsetInterval == "year")
                    res.PayoffInterval = "year";
                else
                    res.PayoffInterval = "month";
                res.EndDateOffset = EndDateOffset;
                res.EndDateOffsetInterval = EndDateOffsetInterval;
                res.Capitalization = Capitalization;
            }

            if (Type != "deposit" && Type != "loan")
            {
                res.Percent = null;
                res.PayoffStep = null;
                res.PayoffInterval = null;
                res.EndDateOffset = null;
                res.EndDateOffsetInterval = null;
            }

            if (Type == "cash")
            {
                res.CreditLimit = 0;
                res.SyncId = null;
                res.Company = null;
                res.EnableCorrection = false;
                res.EnableSms = false;
            }

            res.createMetaData();

            if (Create)
                App.Provider.VMAccount.Insert(res);
            else
                App.Provider.VMAccount.Update(res);

            Transaction trans;
            if (Create && Type == "deposit" && editAccount != null && editAccount != Guid.Empty)
            {
                var OutcomeAcc = GetByID((Guid)editAccount);
                trans = new Transaction();
                trans.Id = Guid.NewGuid();
                trans.OutcomeAccount = (Guid)editAccount;
                trans.IncomeAccount = res.Id;
                trans.Income = (double)StartBalance;

                if (OutcomeAcc.Instrument == res.Instrument)
                    trans.Outcome = (double)StartBalance;
                else
                    trans.Outcome = (double)StartBalance * Model.Instrument.GetRate(res.Instrument) / Model.Instrument.GetRate(OutcomeAcc.Instrument);

                trans.TransType = TransactionType.Transfer;
                trans.Date = DateTime.Now.ToString("yyyy-MM-dd");
                trans.Changed = SettingsManager.toUnixTime(DateTime.UtcNow);
                trans.Created = SettingsManager.toUnixTime(DateTime.UtcNow);
                trans.TransDateStamp = SettingsManager.toUnixTime(DateTime.Now.Date);
                trans.IncomeInstrument = res.Instrument;
                trans.OutcomeInstrument = OutcomeAcc.Instrument;
                trans.User = Model.User.CurrentUserID;
                trans.isCreated = true;
                trans.isChanged = true;
                App.Provider.VMTransaction.Insert(trans);
            }
            else if (Create && Type == "deposit")
            {
                trans = new Transaction();
                trans.Id = Guid.NewGuid();
                trans.IncomeAccount = res.Id;
                trans.OutcomeAccount = res.Id;
                trans.Income = (double)StartBalance;
                trans.Outcome = 0;
                trans.TransType = TransactionType.Income;
                trans.Date = DateTime.Now.ToString("yyyy-MM-dd");
                trans.Changed = SettingsManager.toUnixTime(DateTime.UtcNow);
                trans.Created = SettingsManager.toUnixTime(DateTime.UtcNow);
                trans.TransDateStamp = SettingsManager.toUnixTime(DateTime.Now.Date);
                trans.IncomeInstrument = res.Instrument;
                trans.OutcomeInstrument = res.Instrument;
                trans.User = Model.User.CurrentUserID;
                trans.isCreated = true;
                trans.isChanged = true;
                App.Provider.VMTransaction.Insert(trans);
            }

            if (Create && Type == "loan" && editAccount != null && editAccount != Guid.Empty)
            {
                var IncomeAcc = GetByID((Guid)editAccount);
                trans = new Transaction();
                trans.Id = Guid.NewGuid();
                trans.IncomeAccount = (Guid)editAccount;
                trans.OutcomeAccount = res.Id;
                trans.Outcome = (double)StartBalance;

                if (IncomeAcc.Instrument == res.Instrument)
                    trans.Income = (double)StartBalance;
                else
                    trans.Income = (double)StartBalance * Model.Instrument.GetRate(res.Instrument) / Model.Instrument.GetRate(IncomeAcc.Instrument);

                trans.TransType = TransactionType.Transfer;
                trans.Date = DateTime.Now.ToString("yyyy-MM-dd");
                trans.Changed = SettingsManager.toUnixTime(DateTime.UtcNow);
                trans.Created = SettingsManager.toUnixTime(DateTime.UtcNow);
                trans.TransDateStamp = SettingsManager.toUnixTime(DateTime.Now.Date);
                trans.IncomeInstrument = IncomeAcc.Instrument;
                trans.OutcomeInstrument = res.Instrument;
                trans.User = Model.User.CurrentUserID;
                trans.isCreated = true;
                trans.isChanged = true;
                App.Provider.VMTransaction.Insert(trans);
            }
            else if (Create && Type == "loan")
            {
                trans = new Transaction();
                trans.Id = Guid.NewGuid();
                trans.IncomeAccount = res.Id;
                trans.OutcomeAccount = res.Id;
                trans.Income = 0;
                trans.Outcome = (double)StartBalance;
                trans.TransType = TransactionType.Outcome;
                trans.Date = DateTime.Now.ToString("yyyy-MM-dd");
                trans.Changed = SettingsManager.toUnixTime(DateTime.UtcNow);
                trans.Created = SettingsManager.toUnixTime(DateTime.UtcNow);
                trans.TransDateStamp = SettingsManager.toUnixTime(DateTime.Now.Date);
                trans.IncomeInstrument = res.Instrument;
                trans.OutcomeInstrument = res.Instrument;
                trans.User = Model.User.CurrentUserID;
                trans.isCreated = true;
                trans.isChanged = true;
                App.Provider.VMTransaction.Insert(trans);
            }
        }
    }

    public class Period
    {
        public string Id { get; set; }
        public string Title { get; set; }

        public static List<Period> getPeriodMode()
        {
            List<Period> res = new List<Period>();
            res.Add(new Period() { Id = "month", Title = ResourceLoader.GetForViewIndependentUse().GetString("AccountEditTermTitleMonth") });
            res.Add(new Period() { Id = "year", Title = ResourceLoader.GetForViewIndependentUse().GetString("AccountEditTermTitleYear") });
            return res;
        }
    }

    public sealed class Mode
    {
        public int Id { get; set; }
        public string Title { get; set; }

        public static ObservableCollection<Mode> getPercentMode(bool All = false)
        {
            ObservableCollection<Mode> res = new ObservableCollection<Mode>();
            res.Add(new Mode() { Id = 0, Title = ResourceLoader.GetForViewIndependentUse().GetString("AccountEditPaymentsMonth") });
            if (All)
                res.Add(new Mode() { Id = 1, Title = ResourceLoader.GetForViewIndependentUse().GetString("AccountEditPaymentsQuarter") });
            res.Add(new Mode() { Id = 2, Title = ResourceLoader.GetForViewIndependentUse().GetString("AccountEditPaymentsEnd") });
            return res;
        }

        public static ObservableCollection<Mode> getPaymentMode()
        {
            ObservableCollection<Mode> res = new ObservableCollection<Mode>();
            res.Add(new Mode() { Id = 0, Title = ResourceLoader.GetForViewIndependentUse().GetString("AccountEditPaymentModeConst") });
            res.Add(new Mode() { Id = 1, Title = ResourceLoader.GetForViewIndependentUse().GetString("AccountEditPaymentModeFloat") });
            return res;
        }
    }

    public sealed class AccountTypeStr
    {
        public string Title { get; set; }
        public string Type { get; set; }

        public static List<AccountTypeStr> getAccTypes(bool All = false)
        {
            List<AccountTypeStr> res = new List<AccountTypeStr>();
            AccountTypeStr item = new AccountTypeStr() { Title = ResourceLoader.GetForViewIndependentUse().GetString("AccountTypeCash"), Type = "cash" };
            res.Add(item);
            item = new AccountTypeStr() { Title = ResourceLoader.GetForViewIndependentUse().GetString("AccountTypeCcard"), Type = "ccard" };
            res.Add(item);
            item = new AccountTypeStr() { Title = ResourceLoader.GetForViewIndependentUse().GetString("AccountTypeChecking"), Type = "checking" };
            res.Add(item);
            item = new AccountTypeStr() { Title = ResourceLoader.GetForViewIndependentUse().GetString("AccountTypeDeposit"), Type = "deposit" };
            res.Add(item);
            item = new AccountTypeStr() { Title = ResourceLoader.GetForViewIndependentUse().GetString("AccountTypeLoan"), Type = "loan" };
            res.Add(item);
            if (All)
            {
                item = new AccountTypeStr() { Title = ResourceLoader.GetForViewIndependentUse().GetString("AccountTypeDebt"), Type = "debt" };
                res.Add(item);
            }
            return res;
        }
    }

    public enum AccountType
    {
        cash = 1,
        ccard = 2,
        debt = 3,
        uit = 4,
        checking = 5,
        deposit = 6,
        loan = 7
    }
}
