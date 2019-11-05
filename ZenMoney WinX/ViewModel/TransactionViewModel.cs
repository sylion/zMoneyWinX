using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using zMoneyWinX.Client;
using zMoneyWinX.Model;

namespace zMoneyWinX.ViewModel
{
    public class TransactionViewModel : INotifyProperty
    {
        public TransactionViewModel()
        {
            Signals.DeleteTransForAccountEvent -= Signals_DeleteTransForAccountEvent;
            Signals.DeleteTransForAccountEvent += Signals_DeleteTransForAccountEvent;
        }

        private async void Signals_DeleteTransForAccountEvent(Guid item)
        {
            Transaction.DeleteForAccount(item);
            await Refresh();
            App.Provider.VMAccount.Refresh();
        }

        public async Task Init()
        {
            await Task.Run(() =>
            {
                if (_groupedTransactions == null)
                    _groupedTransactions = getGroupedTransactionsHstory();
            });
        }

        private ObservableCollection<GroupInfoList> _groupedTransactions = null;

        public ObservableCollection<GroupInfoList> getGroupedScheduledLatePayments()
        {
            ObservableCollection<GroupInfoList> groups = new ObservableCollection<GroupInfoList>();
            var query = from item in ReminderMarker.LatePayments()
                        orderby item.Date
                        group item by item.displayGroupName into g
                        select new { GroupName = g.Key, Items = g };
            foreach (var g in query)
            {
                GroupInfoList info = new GroupInfoList(g.GroupName);
                foreach (ReminderMarker item in g.Items)
                    info.Add(item);
                groups.Add(info);
            }
            return groups;
        }
        public ObservableCollection<GroupInfoList> getGroupedScheduledPayments(ReminderMarker.States _state)
        {
            ObservableCollection<GroupInfoList> groups = new ObservableCollection<GroupInfoList>();
            var query = from item in ReminderMarker.History(_state)
                        orderby item.Date
                        group item by item.displayGroupName into g
                        select new { GroupName = g.Key, Items = g };
            foreach (var g in query)
            {
                GroupInfoList info = new GroupInfoList(g.GroupName);
                foreach (ReminderMarker item in g.Items)
                    info.Add(item);
                groups.Add(info);
            }
            return groups;
        }
        public ObservableCollection<GroupInfoList> getGroupedScheduledPayments(Guid Id, ReminderMarker.States _state)
        {
            ObservableCollection<GroupInfoList> groups = new ObservableCollection<GroupInfoList>();
            var query = from item in ReminderMarker.History(Id, _state)
                        orderby item.Date
                        group item by item.displayGroupName into g
                        select new { GroupName = g.Key, Items = g };
            foreach (var g in query)
            {
                GroupInfoList info = new GroupInfoList(g.GroupName);
                foreach (ReminderMarker item in g.Items)
                    info.Add(item);
                groups.Add(info);
            }
            return groups;
        }

        private ObservableCollection<GroupInfoList> getGroupedTransactionsHstory()
        {
            ObservableCollection<GroupInfoList> groups = new ObservableCollection<GroupInfoList>();

            double Outcome = 0, Income = 0;
            bool Totals = (Filter != null);
            List<Instrument> rates = Instrument.Instruments();

            int DefaultCurrency = User.GetDefaultCurrency();

            var query = from item in Transaction.TransactionsHistory(Filter)
                        where !item.isDeleted
                        orderby item.Date descending, item.Created descending
                        group item by item.displayGroupName into g
                        select new { GroupName = g.Key, Items = g };

            foreach (var g in query)
            {
                GroupInfoList info = new GroupInfoList(g.GroupName);
                foreach (Transaction item in g.Items)
                    info.Add(item);

                if (Totals)
                {
                    Income += g.Items.Where(i => i.TransType != TransactionType.mDept && i.TransType != TransactionType.Transfer && i.IncomeInstrument == DefaultCurrency).Sum(i => i.Income);
                    Outcome += g.Items.Where(i => i.TransType != TransactionType.pDept && i.TransType != TransactionType.Transfer && i.OutcomeInstrument == DefaultCurrency).Sum(i => i.Outcome);

                    foreach (Transaction item in g.Items.Where(i => i.TransType != TransactionType.mDept && i.TransType != TransactionType.Transfer && i.IncomeInstrument != DefaultCurrency))
                    {
                        if (item.IncomeInstrument == 2)
                            Income += item.Income * rates.FirstOrDefault(i => i.Id == item.IncomeInstrument).Rate;
                        else
                            Income += item.Income * rates.FirstOrDefault(i => i.Id == item.IncomeInstrument).Rate / rates.FirstOrDefault(i => i.Id == DefaultCurrency).Rate;
                    }

                    foreach (Transaction item in g.Items.Where(i => i.TransType != TransactionType.pDept && i.TransType != TransactionType.Transfer && i.OutcomeInstrument != DefaultCurrency))
                    {
                        if (item.OutcomeInstrument == 2)
                            Outcome += item.Outcome * rates.FirstOrDefault(i => i.Id == item.OutcomeInstrument).Rate;
                        else
                            Outcome += item.Outcome * rates.FirstOrDefault(i => i.Id == item.OutcomeInstrument).Rate / rates.FirstOrDefault(i => i.Id == DefaultCurrency).Rate;
                    }
                }
                groups.Add(info);
            }

            GroupInfoList scheduled = new GroupInfoList(ResourceLoader.GetForViewIndependentUse().GetString("PlannedForToday"));
            foreach (ReminderMarker item in ReminderMarker.ScheduledForToday())
                scheduled.Add(item);

            if (scheduled.Count > 0 && Filter == null)
                groups.Insert(0, scheduled);

            if (Totals)
            {
                OutcomeSum = Outcome.ToString("#,0.00") + " " + Instrument.GetSymbol(DefaultCurrency);
                IncomeSum = Income.ToString("#,0.00") + " " + Instrument.GetSymbol(DefaultCurrency);
            }
            return groups;
        }
        public ObservableCollection<GroupInfoList> GroupedTransactions
        {
            get
            {
                if (_groupedTransactions == null)
                    _groupedTransactions = getGroupedTransactionsHstory();
                return _groupedTransactions;
            }
            set
            {
                _groupedTransactions = value;
            }
        }

        public void Delete(Transaction Item)
        {
            foreach (GroupInfoList x in GroupedTransactions)
                if (x.Key.ToString() == Item.displayGroupName)
                {
                    object res = x.SingleOrDefault(i => ((Transaction)i).Id == Item.Id);
                    x.Remove(res);
                    if (x.Count <= 0)
                        GroupedTransactions.Remove(x);
                    break;
                }

            Item.isDeleted = true;
            Item.Changed = SettingsManager.toUnixTime(DateTime.UtcNow);
            DBObject.Insert(Item);

            App.Provider.VMAccount.recalcAccount(Item, true);
            App.Provider.VMAccount.Refresh();
        }
        public void Delete(Guid ID)
        {
            CollectionDelete(ID);
            DBObject.Delete(ID, typeof(Transaction));
        }
        public void Insert(Transaction Item)
        {
            DBObject.Insert(Item);
            CollectionInsert(Transaction.getByID(Item.Id));

            App.Provider.VMAccount.recalcAccount(Item, false);
            App.Provider.VMAccount.Refresh();
        }
        public void Update(List<Transaction> Items)
        {
            foreach (Transaction Item in Items)
            {
                DBObject.Insert(Item);
                CollectionDelete(Item.Id);
                CollectionInsert(Transaction.getByID(Item.Id));
            }
        }
        public void Update(Transaction Item)
        {
            App.Provider.VMAccount.recalcAccount(Item, true);
            App.Provider.VMAccount.recalcAccount(Item, false);

            DBObject.Insert(Item);
            CollectionDelete(Item.Id);
            CollectionInsert(Transaction.getByID(Item.Id));

            App.Provider.VMAccount.Refresh();
        }

        public void DeleteMarker(Guid ID)
        {
            foreach (GroupInfoList x in GroupedTransactions)
            {
                object res = x.SingleOrDefault(i => ((DBObject)i).ObjId == ID);
                if (res != null)
                {
                    x.Remove(res);
                    if (x.Count <= 0)
                        GroupedTransactions.Remove(x);
                    break;
                }
            }
        }
        public void DeleteMarkerChain(Guid ID)
        {
            foreach (var item in Reminder.Markers(ID))
                foreach (GroupInfoList x in GroupedTransactions)
                {
                    object res = x.SingleOrDefault(i => ((DBObject)i).ObjId == item);
                    if (res != null)
                    {
                        x.Remove(res);
                        if (x.Count <= 0)
                            GroupedTransactions.Remove(x);
                        break;
                    }
                }
        }

        private void CollectionDelete(Guid ID)
        {
            foreach (GroupInfoList x in GroupedTransactions)
            {
                object res = x.SingleOrDefault(i => ((DBObject)i).ObjId == ID);
                if (res != null)
                {
                    x.Remove(res);
                    if (x.Count <= 0)
                        GroupedTransactions.Remove(x);
                    break;
                }
            }
        }
        private void CollectionInsert(Transaction Item)
        {
            foreach (GroupInfoList x in _groupedTransactions)
            {
                if (x.Key.ToString() == Item.displayGroupName)
                {
                    foreach (Transaction c in x)
                    {
                        if (c.Created < Item.Created)
                        {
                            x.Insert(x.IndexOf(c), Item);
                            return;
                        }
                    }
                    x.Add(Item);
                    return;
                }
            }

            GroupInfoList g = new GroupInfoList(Item.displayGroupName);
            g.Add(Item);
            foreach (GroupInfoList x in _groupedTransactions)
            {
                if (Transaction.convertGroupName(x.Key.ToString()) < Transaction.convertGroupName(g.Key.ToString()))
                {
                    int i = _groupedTransactions.IndexOf(x);
                    _groupedTransactions.Insert(i, g);
                    return;
                }
            }
            _groupedTransactions.Add(g);
        }
        public async Task Refresh()
        {
            ObservableCollection<GroupInfoList> res = new ObservableCollection<GroupInfoList>();

            await Task.Run(() =>
             {
                 res = getGroupedTransactionsHstory();
             });

            GroupedTransactions.Clear();
            foreach (GroupInfoList x in res)
                GroupedTransactions.Add(x);
        }

        public string IncomeSum { get; set; }
        public string OutcomeSum { get; set; }
        private TransactionFilter _Filter = null;
        public TransactionFilter Filter { get { return _Filter; } set { _Filter = value; SetFilter(); } }
        private async void SetFilter()
        {
            Signals.invokeSyncStarted();
            await Refresh();
            NotifyPropertyChanged();
            Signals.invokeSyncEnded();
        }
    }
}