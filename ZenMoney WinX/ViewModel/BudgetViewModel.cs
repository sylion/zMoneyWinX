using SQLite;
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
    public class BudgetViewModel : INotifyProperty
    {
        public BudgetViewModel()
        {
            _currentDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            BudgetCategories.NeedUpdateEvent -= BudgetCategories_NeedUpdateEvent;
            BudgetCategories.NeedUpdateEvent += BudgetCategories_NeedUpdateEvent;
        }

        private void BudgetCategories_NeedUpdateEvent()
        {
            NeedUpdate = true;
        }

        public bool NeedUpdate { get; set; } = false;

        private DateTime _currentDate = DateTime.Now;
        public ObservableCollection<Budget> budgetIncome = new ObservableCollection<Budget>();
        public ObservableCollection<Budget> budgetOutcome = new ObservableCollection<Budget>();
        public ObservableCollection<BudgetCategories> budgetTags = new ObservableCollection<BudgetCategories>();
        public string CurrentDate
        {
            get
            {
                if (_currentDate.Year == DateTime.Now.Year)
                    return _currentDate.ToString("MMMM");
                else
                    return _currentDate.ToString("MMMM yyyy");
            }
        }

        public bool NextMonth()
        {
            if (_currentDate > new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1))
            {
                if ((_currentDate.Subtract(new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1)).Days / (365.25 / 12)) >= 6)
                    return false;
            }
            _currentDate = _currentDate.AddMonths(1);
            NotifyPropertyChanged();
            return true;
        }

        public bool PrevMonth()
        {
            if (_currentDate < new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1))
            {
                if (((new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1)).Subtract(_currentDate).Days / (365.25 / 12)) >= 6)
                    return false;
            }
            _currentDate = _currentDate.AddMonths(-1);
            NotifyPropertyChanged();
            return true;
        }

        public async Task Refresh()
        {
            List<Budget> OutcomeBudget = new List<Budget>();
            List<Budget> IncomeBudget = new List<Budget>();
            await Task.Run(() =>
            {
                using (var db = new SQLiteConnection(DBInitializer.DB_PATH))
                {
                    long DateFrom = SettingsManager.toUnixTime(_currentDate);
                    long DateTo = SettingsManager.toUnixTime(_currentDate.Date.AddMonths(1).AddSeconds(-1));

                    //Outcome
                    var OutcomeTags = db.Table<Tag>().Where(i => i.BudgetOutcome).OrderBy(i => i.Title).ToList();
                    var OutcomeTagsId = OutcomeTags.Select(x => x.Id);
                    OutcomeBudget = db.Table<Budget>().Where(i => i.Date == _currentDate && i.Tag != null && i.Tag != Guid.Empty && OutcomeTagsId.Contains((Guid)i.Tag)).ToList();

                    foreach (var item in OutcomeTagsId.Where(i => !OutcomeBudget.Select(x => x.Tag).Contains(i)))
                    {
                        OutcomeBudget.Add(new Budget()
                        {
                            Date = _currentDate,
                            User = User.CurrentUserID,
                            Income = 0,
                            Outcome = 0,
                            Tag = item,
                            displayTag = OutcomeTags.Find(i => i.Id == item).Title,
                            Key = _currentDate.ToString("dd.MM.yyyy") + item.ToString(),
                            Changed = SettingsManager.toUnixTime(DateTime.UtcNow),
                            isChanged = true,
                            isCreated = true
                        });
                    }

                    OutcomeBudget = OutcomeBudget.OrderBy(i => i.displayTag).ToList();

                    //Add empty tag
                    var EmptyOutcome = db.Table<Budget>().FirstOrDefault(i => i.Date == _currentDate && i.Tag == null);
                    if (EmptyOutcome != null)
                        OutcomeBudget.Insert(0, EmptyOutcome);
                    else
                    {
                        OutcomeBudget.Insert(0, new Budget()
                        {
                            Date = _currentDate,
                            User = User.CurrentUserID,
                            Income = 0,
                            Outcome = 0,
                            Tag = null,
                            displayTag = ResourceLoader.GetForViewIndependentUse().GetString("TransTagEmpty"),
                            Key = _currentDate.ToString("dd.MM.yyyy"),
                            Changed = SettingsManager.toUnixTime(DateTime.UtcNow),
                            isChanged = true,
                            isCreated = true,
                        });
                    }

                    //Add header
                    var HeadOutcome = db.Table<Budget>().FirstOrDefault(i => i.Date == _currentDate && i.Tag != null && i.Tag == Guid.Empty);
                    if (HeadOutcome != null)
                    {
                        HeadOutcome.displayTag = ResourceLoader.GetForViewIndependentUse().GetString("TransTypeOutcome");
                        HeadOutcome.Outcome = HeadOutcome.Outcome + OutcomeBudget.Sum(i => i.Outcome);
                        OutcomeBudget.Insert(0, HeadOutcome);
                    }
                    else
                    {
                        OutcomeBudget.Insert(0, new Budget()
                        {
                            Date = _currentDate,
                            User = User.CurrentUserID,
                            Income = OutcomeBudget.Sum(i => i.Income),
                            Outcome = OutcomeBudget.Sum(i => i.Outcome),
                            Tag = Guid.Empty,
                            displayTag = ResourceLoader.GetForViewIndependentUse().GetString("TransTypeOutcome"),
                            Key = _currentDate.ToString("dd.MM.yyyy") + Guid.Empty.ToString(),
                            Changed = SettingsManager.toUnixTime(DateTime.UtcNow),
                            isChanged = true,
                            isCreated = true
                        });
                    }

                    //Income
                    var IncomeTags = db.Table<Tag>().Where(i => i.BudgetIncome).OrderBy(i => i.Title).ToList();
                    var IncomeTagsId = IncomeTags.Select(x => x.Id);
                    IncomeBudget = db.Table<Budget>().Where(i => i.Date == _currentDate && i.Tag != null && i.Tag != Guid.Empty && IncomeTagsId.Contains((Guid)i.Tag)).ToList();

                    foreach (var item in IncomeTagsId.Where(i => !IncomeBudget.Select(x => x.Tag).Contains(i)))
                    {
                        IncomeBudget.Add(new Budget()
                        {
                            Date = _currentDate,
                            User = User.CurrentUserID,
                            Income = 0,
                            Outcome = 0,
                            Tag = item,
                            displayTag = IncomeTags.Find(i => i.Id == item).Title,
                            Key = _currentDate.ToString("dd.MM.yyyy") + item.ToString(),
                            Changed = SettingsManager.toUnixTime(DateTime.UtcNow),
                            isChanged = true,
                            isCreated = true
                        });
                    }
                    IncomeBudget = IncomeBudget.OrderBy(i => i.displayTag).ToList();

                    var EmptyIncome = db.Table<Budget>().FirstOrDefault(i => i.Date == _currentDate && i.Tag == null);
                    if (EmptyIncome != null)
                        IncomeBudget.Insert(0, EmptyIncome);
                    else
                    {
                        IncomeBudget.Insert(0, new Budget()
                        {
                            Date = _currentDate,
                            User = User.CurrentUserID,
                            Income = 0,
                            Outcome = 0,
                            Tag = null,
                            displayTag = ResourceLoader.GetForViewIndependentUse().GetString("TransTagEmpty"),
                            Key = _currentDate.ToString("dd.MM.yyyy"),
                            Changed = SettingsManager.toUnixTime(DateTime.UtcNow),
                            isChanged = true,
                            isCreated = true
                        });
                    }

                    var HeadIncome = db.Table<Budget>().FirstOrDefault(i => i.Date == _currentDate && i.Tag != null && i.Tag == Guid.Empty);
                    if (HeadIncome != null)
                    {
                        HeadIncome.displayTag = ResourceLoader.GetForViewIndependentUse().GetString("TransTypeIncome");
                        HeadIncome.Income = HeadIncome.Income + IncomeBudget.Sum(i => i.Income);
                        IncomeBudget.Insert(0, HeadIncome);
                    }
                    else
                    {
                        IncomeBudget.Insert(0, new Budget()
                        {
                            Date = _currentDate,
                            User = User.CurrentUserID,
                            Income = IncomeBudget.Sum(i => i.Income),
                            Outcome = IncomeBudget.Sum(i => i.Outcome),
                            Tag = Guid.Empty,
                            displayTag = ResourceLoader.GetForViewIndependentUse().GetString("TransTypeIncome"),
                            Key = _currentDate.ToString("dd.MM.yyyy") + Guid.Empty.ToString(),
                            Changed = SettingsManager.toUnixTime(DateTime.UtcNow),
                            isChanged = true,
                            isCreated = true
                        });
                    }
                }

            });

            budgetOutcome.Clear();
            foreach (var item in OutcomeBudget)
                budgetOutcome.Add(item);

            budgetIncome.Clear();
            foreach (var item in IncomeBudget)
                budgetIncome.Add(item);

            budgetTags.Clear();
            foreach (var item in Tag.Tags)
                budgetTags.Add(new BudgetCategories(item.Id, item.Title, item.BudgetIncome, item.BudgetOutcome));
        }
    }

    public class BudgetCategories : INotifyProperty
    {
        public BudgetCategories()
        {

        }
        public BudgetCategories(Guid Id, string Title, bool ShowIncome, bool ShowOutcome)
        {
            _TagId = Id;
            _TagName = Title;
            _Income = ShowIncome;
            _Outcome = ShowOutcome;
        }

        private Guid _TagId { get; set; }
        private string _TagName { get; set; }
        private bool _Income { get; set; }
        private bool _Outcome { get; set; }

        public Guid TagId { get { return _TagId; } set { _TagId = value; } }
        public string TagName { get { return _TagName; } set { _TagName = value; } }
        public bool Income { get { return _Income; } set { _Income = value; ChangeIncome(); NotifyPropertyChanged(); } }
        public bool Outcome { get { return _Outcome; } set { _Outcome = value; ChangeOutcome(); NotifyPropertyChanged(); } }

        public delegate void VoidSignalHandler();
        public static event VoidSignalHandler NeedUpdateEvent;

        private void ChangeIncome()
        {
            var res = Tag.GetByID(_TagId);
            if (res == null)
                return;
            res.BudgetIncome = _Income;
            DBObject.Insert(res);
            NeedUpdateEvent?.Invoke();
        }

        private void ChangeOutcome()
        {
            var res = Tag.GetByID(_TagId);
            if (res == null)
                return;
            res.BudgetOutcome = _Outcome;
            DBObject.Insert(res);
            NeedUpdateEvent?.Invoke();
        }
    }
}
