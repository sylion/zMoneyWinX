using System;
using System.Collections.Generic;
using System.Linq;
using Windows.ApplicationModel.Resources;

namespace zMoneyWinX.Model
{
    public class TransactionEdit : Transaction
    {
        public TransactionEdit(Transaction item)
        {
            if (item.IncomeAccount != Guid.Empty || item.OutcomeAccount != Guid.Empty)
                AccountList = Account.AccountsTrans(item.IncomeAccount, item.OutcomeAccount);
            else
                AccountList = Account.AccountsTrans();

            CurrentUserIDStr = Model.User.CurrentUserID;

            Id = item.Id;
            Income = item.Income;
            IncomeAccount = item.IncomeAccount;
            IncomeInstrument = item.IncomeInstrument;

            Outcome = item.Outcome;
            OutcomeAccount = item.OutcomeAccount;
            OutcomeInstrument = item.OutcomeInstrument;

            Tags = item.Tags;

            if (string.IsNullOrEmpty(item.Date))
                Date = DateTime.UtcNow.ToString("yyyy-MM-dd");
            else
                Date = item.Date;
            Payee = item.Payee;
            Merchant = item.Merchant;
            Comment = item.Comment;
            TransType = item.TransType;
            ReminderMarker = item.ReminderMarker;

            ResetAccountList();
        }

        #region Account selector
        public delegate void VoidSignalHandler();
        public event VoidSignalHandler RebindIncomeAccountsEvent;
        public event VoidSignalHandler RebindOutcomeAccountsEvent;
        private void RebindIncomeAccountsEventInvoke() => RebindIncomeAccountsEvent?.Invoke();
        private void RebindOutcomeAccountsEventInvoke() => RebindOutcomeAccountsEvent?.Invoke();

        private int CurrentUserIDStr;

        private List<Account> AccountList;
        public List<Account> IncomeAccountList;
        public List<Account> OutcomeAccountList;

        private void UpdateIncomeAccountList()
        {
            if (TransType == TransactionType.Transfer)
            {
                var curAcc = AccountList.FirstOrDefault(i => i.Id == OutcomeAccount);
                if (curAcc.Role != null && curAcc.Role != CurrentUserIDStr)
                    IncomeAccountList = AccountList.Where(i => i.Id != OutcomeAccount && (i.Role == null || i.Role == CurrentUserIDStr)).ToList();
                else
                    IncomeAccountList = AccountList.Where(i => i.Id != OutcomeAccount).ToList();

                if (OutcomeAccount == IncomeAccount)
                    IncomeAccount = IncomeAccountList.FirstOrDefault().Id;
            }
            RebindIncomeAccountsEventInvoke();
            NotifyPropertyChanged();
        }

        private void UpdateOutcomeAccountList()
        {
            if (TransType == TransactionType.Transfer)
            {
                var curAcc = AccountList.FirstOrDefault(i => i.Id == IncomeAccount);
                if (curAcc.Role != null && curAcc.Role != CurrentUserIDStr)
                    OutcomeAccountList = AccountList.Where(i => i.Id != IncomeAccount && (i.Role == null || i.Role == CurrentUserIDStr)).ToList();
                else
                    OutcomeAccountList = AccountList.Where(i => i.Id != IncomeAccount).ToList();

                if (OutcomeAccount == IncomeAccount)
                    OutcomeAccount = OutcomeAccountList.FirstOrDefault().Id;
            }
            RebindOutcomeAccountsEventInvoke();
            NotifyPropertyChanged();
        }

        private void ResetAccountList()
        {
            if (TransType == TransactionType.Transfer)
            {
                UpdateIncomeAccountList();
                UpdateOutcomeAccountList();
            }
            else
            {
                IncomeAccountList = AccountList.Where(i => i.Role == null || i.Role == CurrentUserIDStr).ToList();
                OutcomeAccountList = IncomeAccountList;

                if (IncomeAccount == Guid.Empty || OutcomeAccount == Guid.Empty)
                    OutcomeAccount = OutcomeAccountList.FirstOrDefault().Id;

                IncomeAccount = OutcomeAccount;

                RebindOutcomeAccountsEventInvoke();
                RebindIncomeAccountsEventInvoke();
            }
            NotifyPropertyChanged();
        }
        #endregion

        public string validate()
        {
            string res = "";
            if (editTransType == TransactionType.Transfer)
            {
                if (editIncomeAccount == null || editIncomeAccount == Guid.Empty || editOutcomeAccount == null || editOutcomeAccount == Guid.Empty || editIncomeAccount == editOutcomeAccount)
                    res += "\n" + ResourceLoader.GetForViewIndependentUse().GetString("ErrorAccountsEquals");
                if (editIncome == 0 || editOutcome == 0)
                    res += "\n" + ResourceLoader.GetForViewIndependentUse().GetString("ErrorTransferTotals");
            }

            if (editTransType == TransactionType.Income || editTransType == TransactionType.pDept)
            {
                if (editIncomeAccount == null || editIncomeAccount == Guid.Empty)
                    res += "\n" + ResourceLoader.GetForViewIndependentUse().GetString("ErrorIncomeAccountNull");
                if (editIncome == 0)
                    res += "\n" + ResourceLoader.GetForViewIndependentUse().GetString("ErrorIncomeAccountTotal");
            }

            if (editTransType == TransactionType.Outcome || editTransType == TransactionType.mDept)
            {
                if (editOutcomeAccount == null || editOutcomeAccount == Guid.Empty)
                    res += "\n" + ResourceLoader.GetForViewIndependentUse().GetString("ErrorOutcomeAccountNull");
                if (editOutcome == 0)
                    res += "\n" + ResourceLoader.GetForViewIndependentUse().GetString("ErrorOutcomeAccountTotal");
            }
            if (editTransType == TransactionType.pDept || editTransType == TransactionType.mDept)
                if (string.IsNullOrWhiteSpace(editPayee))
                    res += "\n" + ResourceLoader.GetForViewIndependentUse().GetString("ErrorPayeeNull");

            return res;
        }

        #region Edit
        public TransactionType editTransType
        {
            get { return TransType; }
            set
            {
                TransType = value;
                ResetAccountList();
                NotifyPropertyChanged();
            }
        }
        public Guid editIncomeAccount
        {
            get
            {
                return IncomeAccount;
            }
            set
            {
                IncomeAccount = value;
                UpdateOutcomeAccountList();
                NotifyPropertyChanged();
            }
        }
        public Guid editOutcomeAccount
        {
            get
            {
                return OutcomeAccount;
            }
            set
            {
                OutcomeAccount = value;
                UpdateIncomeAccountList();
                NotifyPropertyChanged();
            }
        }
        public string editDate
        {
            get
            {
                return Date;
            }
            set
            {
                if (value != null)
                    Date = value;
                else
                    Date = DateTime.Now.ToString("yyyy-MM-dd");
                NotifyPropertyChanged();
            }
        }
        public double editIncome
        {
            get
            {
                return Income;
            }
            set
            {
                Income = value;
                if (TransType != TransactionType.Transfer)
                    Outcome = value;

                NotifyPropertyChanged();
            }
        }
        public double editOutcome
        {
            get
            {
                return Outcome;
            }
            set
            {
                Outcome = value;
                Income = value;

                NotifyPropertyChanged();
            }
        }
        public string editComment
        {
            get
            {
                return Comment;
            }
            set
            {
                Comment = value;
                NotifyPropertyChanged();
            }
        }
        public Guid? editMerchant
        {
            get
            {
                return Merchant;
            }
            set
            {
                Merchant = value;
                NotifyPropertyChanged();
            }
        }
        public string editPayee
        {
            get
            {
                return Payee;
            }
            set
            {
                Payee = value;
                NotifyPropertyChanged();
            }
        }

        public string OutcomeCurrencySymbol
        {
            get
            {
                return AccountList.FirstOrDefault(i => i.Id == OutcomeAccount)?.displayInstrument;
            }
        }
        public string IncomeCurrencySymbol
        {
            get
            {
                return AccountList.FirstOrDefault(i => i.Id == IncomeAccount)?.displayInstrument;
            }
        }
        #endregion
    }
}
