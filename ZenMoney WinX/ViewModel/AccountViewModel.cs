using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using zMoneyWinX.Client;
using zMoneyWinX.Model;

namespace zMoneyWinX.ViewModel
{
    public class AccountViewModel : INotifyProperty
    {
        public async Task Init()
        {
            await Task.Run(() =>
            {
                if (_groupedAccounts == null)
                    _groupedAccounts = Account.GroupedAccounts();
            });
        }

        private ObservableCollection<GroupInfoList> _groupedAccounts = null;
        public static List<Account> AccountsTrans => Account.AccountsTrans();
        public static Guid DebtID => Account.DebtID;
        public static ObservableCollection<Account> Accounts => Account.Accounts();
        private string _Balance = null;

        public ObservableCollection<GroupInfoList> GroupedAccounts
        {
            get
            {
                if (_groupedAccounts == null)
                    _groupedAccounts = Account.GroupedAccounts();
                return _groupedAccounts;
            }
            set
            {
                _groupedAccounts = value;
            }
        }

        public void Insert(List<Account> Items)
        {
            Account.Insert(Items);
            Refresh();
        }
        public void Delete(Account Item)
        {
            Item.isDeleted = true;
            Item.Changed = SettingsManager.toUnixTime(DateTime.UtcNow);
            DBObject.Insert(Item);
            Signals.DeleteTransForAccountInvoke(Item.Id);
        }
        public async void Update(Account Item)
        {
            DBObject.Insert(Item);
            Refresh();
            await App.Provider.VMTransaction.Refresh();
        }
        public void Insert(Account Item)
        {
            DBObject.Insert(Item);
            Refresh();
        }
        public void Delete(Guid ID)
        {
            DBObject.Delete(ID, typeof(Account));
            Refresh();
        }
        public void Refresh()
        {
            GroupedAccounts.Clear();
            foreach (GroupInfoList x in Account.GroupedAccounts())
                GroupedAccounts.Add(x);

            Balance = Account.GetBalance();
        }

        public void UpdateBalance()
        {
            Balance = Account.GetBalance();
        }

        public string Balance
        {
            get
            {
                if (_Balance == null)
                    _Balance = Account.GetBalance();
                return _Balance;
            }
            set
            {
                _Balance = value;
                NotifyPropertyChanged();
            }
        }

        //TODO: Recalc multi currency depts
        public void recalcAccount(Transaction _item, bool Reverse)
        {
            Account acc;

            Transaction item;
            if (!Reverse)
                item = _item;
            else
                item = Transaction.getByID(_item.Id);

            switch (item.TransType)
            {
                case TransactionType.Outcome:
                    acc = Account.GetByID(item.OutcomeAccount);
                    if (acc != null)
                    {
                        if (Reverse)
                            acc.Balance += item.Outcome;
                        else
                            acc.Balance -= item.Outcome;
                        Account.Insert(acc);
                    }
                    break;
                case TransactionType.Income:
                    acc = Account.GetByID(item.IncomeAccount);
                    if (acc != null)
                    {
                        if (Reverse)
                            acc.Balance -= item.Income;
                        else
                            acc.Balance += item.Income;
                        Account.Insert(acc);
                    }
                    break;
                case TransactionType.Transfer:
                case TransactionType.mDept:
                case TransactionType.pDept:
                    acc = Account.GetByID(item.OutcomeAccount);
                    if (acc != null)
                    {
                        if (Reverse)
                            acc.Balance += item.Outcome;
                        else
                            acc.Balance -= item.Outcome;
                        Account.Insert(acc);
                    }
                    acc = Account.GetByID(item.IncomeAccount);
                    if (acc != null)
                    {
                        if (Reverse)
                            acc.Balance -= item.Income;
                        else
                            acc.Balance += item.Income;
                        Account.Insert(acc);
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
