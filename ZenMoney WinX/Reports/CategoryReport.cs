using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using zMoneyWinX.Client;
using zMoneyWinX.Model;

namespace zMoneyWinX.Reports
{
    public class CategoryReport : INotifyProperty
    {
        private DateTime DateFrom { get; set; }
        private DateTime DateTo { get; set; }

        public DateTime GetDateFrom { get { return DateFrom; } }
        public DateTime GetDateTo { get { return DateTo; } }

        public string currentDate
        {
            get
            {
                if (DateFrom.Year == DateTime.Now.Year)
                    return DateFrom.ToString("MMMM");
                else
                    return DateFrom.ToString("MMMM yyyy");
            }
        }

        public bool Expanded = false;

        public TransactionType transType = TransactionType.Outcome;

        public List<CategoryReportModel> Report { get; private set; }

        public CategoryReport()
        {
            DateFrom = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).Date;
            DateTo = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).Date.AddMonths(1).AddSeconds(-1);
        }

        public void NextPeriod()
        {
            DateFrom = DateFrom.AddMonths(1).Date;
            DateTo = DateFrom.AddMonths(1).AddSeconds(-1);
            NotifyPropertyChanged();
        }

        public void PrevPeriod()
        {
            DateFrom = DateFrom.AddMonths(-1).Date;
            DateTo = DateFrom.AddMonths(1).AddSeconds(-1);
            NotifyPropertyChanged();
        }

        public async Task Refresh()
        {
            List<CategoryReportModel> res = new List<CategoryReportModel>();
            await Task.Run(() =>
            {
                var DefCur = User.GetDefaultCurrency();
                var DefCurSymb = Instrument.GetSymbol(DefCur);
                const int HistorySize = 6;
                const TransactionType transf = TransactionType.Transfer;
                double amount = 0;
                double averamount = 0;

                var budget = Budget.GetBudget(DateFrom).ToList();

                using (var db = new SQLiteConnection(DBInitializer.DB_PATH))
                {
                    //Tag list
                    var AllTags = Tag.Tags;
                    List<Tag> tags;
                    if (transType == TransactionType.Outcome)
                        tags = AllTags.Where(i => i.ShowOutcome).ToList();
                    else
                        tags = AllTags.Where(i => i.ShowIncome).ToList();

                    if (!Expanded)
                        tags = tags.Where(i => i.Parent == null).ToList();

                    //Periods
                    var _from = SettingsManager.toUnixTime(DateFrom);
                    var _to = SettingsManager.toUnixTime(DateTo);
                    var _fromHist = SettingsManager.toUnixTime(DateFrom.AddMonths(-HistorySize));
                    var _toHist = SettingsManager.toUnixTime(DateFrom);

                    //Curr period transactions
                    List<Transaction> trans = db.Table<Transaction>()
                        .Where(i => !i.isDeleted && i.TransDateStamp >= _from && i.TransDateStamp <= _to &&
                        (i.TransType == transType || i.TransType == transf)).ToList().Where(i => i.TransType == transType || i.TransIs == transType).ToList();

                    //Transactions history for average values
                    List<Transaction> transHistory = db.Table<Transaction>()
                        .Where(i => !i.isDeleted && i.TransDateStamp >= _fromHist && i.TransDateStamp <= _toHist &&
                        (i.TransType == transType || i.TransType == transf)).ToList().Where(i => i.TransType == transType || i.TransIs == transType).ToList();

                    //Currencies for exchange
                    List<Instrument> currencies = Instrument.Instruments();

                    #region Сategories
                    foreach (var tag in tags)
                    {
                        double sum = 0;
                        double sumHist = 0;
                        bool inBudget = false;

                        if (!Expanded)
                        {
                            foreach (var item in trans.Where(i => i.Tags != null && i.Tags.Contains(tag.Id)))
                            {
                                if (item.OperationInstrument == DefCur)
                                {
                                    sum += item.OperationAmount;
                                }
                                else
                                {
                                    if (item.OperationInstrument == 2)
                                        sum += item.OperationAmount * currencies.FirstOrDefault(i => i.Id == item.OperationInstrument).Rate;
                                    else
                                        sum += item.OperationAmount * currencies.FirstOrDefault(i => i.Id == item.OperationInstrument).Rate / currencies.FirstOrDefault(i => i.Id == DefCur).Rate;
                                }
                            }

                            List<Tag> ChildTags;
                            if (transType == TransactionType.Outcome)
                                ChildTags = AllTags.Where(i => i.Parent == tag.Id && i.ShowOutcome).ToList();
                            else
                                ChildTags = AllTags.Where(i => i.Parent == tag.Id && i.ShowIncome).ToList();

                            foreach (var child in ChildTags)
                            {
                                foreach (var item in trans.Where(i => i.Tags != null && i.Tags.Contains(child.Id)))
                                    if (item.OperationInstrument == DefCur)
                                        sum += item.OperationAmount;
                                    else
                                    {
                                        if (item.OperationInstrument == 2)
                                            sum += item.OperationAmount * currencies.FirstOrDefault(i => i.Id == item.OperationInstrument).Rate;
                                        else
                                            sum += item.OperationAmount * currencies.FirstOrDefault(i => i.Id == item.OperationInstrument).Rate / currencies.FirstOrDefault(i => i.Id == DefCur).Rate;
                                    }
                            }

                            List<Guid> tmpTagList;
                            if (transType == TransactionType.Outcome)
                                tmpTagList = AllTags.Where(i => i.Id == tag.Id || i.Parent == tag.Id && i.ShowOutcome).ToList().Select(i => i.Id).ToList();
                            else
                                tmpTagList = AllTags.Where(i => i.Id == tag.Id || i.Parent == tag.Id && i.ShowIncome).ToList().Select(i => i.Id).ToList();

                            if (checkBudget(budget.Where(i => i.Tag != null && tmpTagList.Contains((Guid)i.Tag)).ToList()))
                            {
                                inBudget = true;
                                if (transType == TransactionType.Outcome)
                                    sumHist = budget.Where(i => i.Tag != null && tmpTagList.Contains((Guid)i.Tag)).Sum(i => i.Outcome);
                                else
                                    sumHist = budget.Where(i => i.Tag != null && tmpTagList.Contains((Guid)i.Tag)).Sum(i => i.Income);
                            }
                            else
                            {
                                foreach (var x in transHistory
                                            .Where(i => i.Tags != null && i.Tags.Intersect(tmpTagList).Any())
                                            .GroupBy(i => i.OperationInstrument))
                                {
                                    if (x.Key == DefCur)
                                        sumHist += x.Sum(i => i.OperationAmount);
                                    else
                                    {
                                        if (x.Key == 2)
                                            sumHist += x.Sum(i => i.OperationAmount) * currencies.FirstOrDefault(i => i.Id == x.Key).Rate;
                                        else
                                            sumHist += x.Sum(i => i.OperationAmount) * currencies.FirstOrDefault(i => i.Id == x.Key).Rate / currencies.FirstOrDefault(i => i.Id == DefCur).Rate;
                                    }
                                }
                                sumHist /= HistorySize;
                            }
                            if (sum > 0)
                                res.Add(new CategoryReportModel(tag.Id, tag.Title, sum, sum.ToString("#,0.##") + " " + DefCurSymb, sumHist, inBudget));
                        }
                        else
                        {
                            foreach (var item in trans.Where(i => i.Tags != null && i.Tags.Contains(tag.Id)))
                            {
                                if (item.OperationInstrument == DefCur)
                                    sum += item.OperationAmount;
                                else
                                {
                                    if (item.OperationInstrument == 2)
                                        sum += item.OperationAmount * currencies.FirstOrDefault(i => i.Id == item.OperationInstrument).Rate;
                                    else
                                        sum += item.OperationAmount * currencies.FirstOrDefault(i => i.Id == item.OperationInstrument).Rate / currencies.FirstOrDefault(i => i.Id == DefCur).Rate;
                                }
                            }

                            if (checkBudget(budget.FirstOrDefault(i => i.Tag == tag.Id)))
                            {
                                inBudget = true;
                                if (transType == TransactionType.Outcome)
                                    sumHist = budget.FirstOrDefault(i => i.Tag != null && i.Tag == tag.Id).Outcome;
                                else
                                    sumHist = budget.FirstOrDefault(i => i.Tag != null && i.Tag == tag.Id).Income;
                            }
                            else
                            {
                                foreach (var x in transHistory
                                            .Where(i => i.Tags != null && i.Tags.Contains(tag.Id))
                                            .GroupBy(i => i.OperationInstrument))
                                {
                                    if (x.Key == DefCur)
                                        sumHist += x.Sum(i => i.OperationAmount);
                                    else
                                    {
                                        if (x.Key == 2)
                                            sumHist += x.Sum(i => i.OperationAmount) * currencies.FirstOrDefault(i => i.Id == x.Key).Rate;
                                        else
                                            sumHist += x.Sum(i => i.OperationAmount) * currencies.FirstOrDefault(i => i.Id == x.Key).Rate / currencies.FirstOrDefault(i => i.Id == DefCur).Rate;
                                    }
                                }
                                sumHist /= HistorySize;
                            }

                            if (sum > 0)
                                res.Add(new CategoryReportModel(tag.Id, tag.Title, sum, sum.ToString("#,0.##") + " " + DefCurSymb, sumHist, inBudget));
                        }
                    }
                    #endregion

                    #region Without category
                    double Sum = 0;
                    double SumHist = 0;
                    bool InBudget = false;

                    foreach (var item in trans.Where(i => (i.Tags == null || i.Tags.Count == 0) && i.TransType != transf))
                    {
                        if (item.OperationInstrument == DefCur)
                            Sum += item.OperationAmount;
                        else
                        {
                            if (item.OperationInstrument == 2)
                                Sum += item.OperationAmount * currencies.FirstOrDefault(i => i.Id == item.OperationInstrument).Rate;
                            else
                                Sum += item.OperationAmount * currencies.FirstOrDefault(i => i.Id == item.OperationInstrument).Rate / currencies.FirstOrDefault(i => i.Id == DefCur).Rate;
                        }
                    }

                    if (checkBudget(budget.FirstOrDefault(i => i.Tag == null)))
                    {
                        InBudget = true;
                        if (transType == TransactionType.Outcome)
                            SumHist = budget.FirstOrDefault(i => i.Tag == null).Outcome;
                        else
                            SumHist = budget.FirstOrDefault(i => i.Tag == null).Income;
                    }
                    else
                    {
                        foreach (var x in transHistory
                                .Where(i => (i.Tags == null || i.Tags.Count == 0) && i.TransType != transf)
                                .GroupBy(i => i.OperationInstrument))
                        {
                            if (x.Key == DefCur)
                                SumHist += x.Sum(i => i.OperationAmount);
                            else
                            {
                                if (x.Key == 2)
                                    SumHist += x.Sum(i => i.OperationAmount) * currencies.FirstOrDefault(i => i.Id == x.Key).Rate;
                                else
                                    SumHist += x.Sum(i => i.OperationAmount) * currencies.FirstOrDefault(i => i.Id == x.Key).Rate / currencies.FirstOrDefault(i => i.Id == DefCur).Rate;
                            }
                        }
                        SumHist /= HistorySize;
                    }
                    if (Sum > 0)
                        res.Add(new CategoryReportModel(Guid.Empty, ResourceLoader.GetForViewIndependentUse().GetString("TransTagEmpty"), Sum, Sum.ToString("#,0.##") + " " + DefCurSymb, SumHist, InBudget));
                    #endregion

                    #region Transfers
                    Sum = 0;
                    SumHist = 0;

                    foreach (var item in trans.Where(i => i.TransType == transf))
                    {
                        if (item.OperationInstrument == DefCur)
                            Sum += item.OperationAmount;
                        else
                        {
                            if (item.OperationInstrument == 2)
                                Sum += item.OperationAmount * currencies.FirstOrDefault(i => i.Id == item.OperationInstrument).Rate;
                            else
                                Sum += item.OperationAmount * currencies.FirstOrDefault(i => i.Id == item.OperationInstrument).Rate / currencies.FirstOrDefault(i => i.Id == DefCur).Rate;
                        }
                    }

                    foreach (var x in transHistory
                                .Where(i => i.TransType == transf)
                                .GroupBy(i => i.OperationInstrument))
                    {
                        if (x.Key == DefCur)
                            SumHist += x.Sum(i => i.OperationAmount);
                        else
                        {
                            if (x.Key == 2)
                                SumHist += x.Sum(i => i.OperationAmount) * currencies.FirstOrDefault(i => i.Id == x.Key).Rate;
                            else
                                SumHist += x.Sum(i => i.OperationAmount) * currencies.FirstOrDefault(i => i.Id == x.Key).Rate / currencies.FirstOrDefault(i => i.Id == DefCur).Rate;
                        }
                    }
                    SumHist /= HistorySize;

                    if (Sum > 0)
                        res.Add(new CategoryReportModel(Guid.Empty, ResourceLoader.GetForViewIndependentUse().GetString("TransTagTransfer"), Sum, Sum.ToString("#,0.##") + " " + DefCurSymb, SumHist));
                    #endregion

                    #region Header
                    foreach (var x in trans.GroupBy(i => i.OperationInstrument))
                    {
                        if (x.Key == DefCur)
                            amount += x.Sum(i => i.OperationAmount);
                        else
                        {
                            if (x.Key == 2)
                                amount += x.Sum(i => i.OperationAmount) * currencies.FirstOrDefault(i => i.Id == x.Key).Rate;
                            else
                                amount += x.Sum(i => i.OperationAmount) * currencies.FirstOrDefault(i => i.Id == x.Key).Rate / currencies.FirstOrDefault(i => i.Id == DefCur).Rate;
                        }
                    }

                    if (transType == TransactionType.Outcome)
                        averamount = budget.Sum(i => i.Outcome);
                    else
                        averamount = budget.Sum(i => i.Income);

                    InBudget = false;
                    if (averamount > 0)
                    {
                        InBudget = true;
                    }
                    else
                    {
                        foreach (var x in transHistory.GroupBy(i => i.OperationInstrument))
                        {
                            if (x.Key == DefCur)
                                averamount += x.Sum(i => i.OperationAmount);
                            else
                            {
                                if (x.Key == 2)
                                    averamount += x.Sum(i => i.OperationAmount) * currencies.FirstOrDefault(i => i.Id == x.Key).Rate;
                                else
                                    averamount += x.Sum(i => i.OperationAmount) * currencies.FirstOrDefault(i => i.Id == x.Key).Rate / currencies.FirstOrDefault(i => i.Id == DefCur).Rate;
                            }
                        }
                        averamount /= HistorySize;
                    }
                    //Add header category                    
                    CategoryReportModel tmp;
                    if (transType == TransactionType.Outcome)
                        tmp = new CategoryReportModel(Guid.Empty, ResourceLoader.GetForViewIndependentUse().GetString("TransTypeOutcome"), amount, amount.ToString("#,0.##") + " " + DefCurSymb, averamount, InBudget);
                    else
                        tmp = new CategoryReportModel(Guid.Empty, ResourceLoader.GetForViewIndependentUse().GetString("TransTypeIncome"), amount, amount.ToString("#,0.##") + " " + DefCurSymb, averamount, InBudget);
                    tmp.Percent = -1;
                    tmp.PercentStr = "";
                    res.Insert(0, tmp);
                    #endregion

                    #region summary
                    foreach (var item in res)
                    {
                        if (item.Percent >= 0)
                        {
                            item.Percent = (item.Amount / amount * 100);
                            item.PercentStr = item.Percent.ToString("#0.#") + "%";
                        }

                        if (item.AverageAmount != 0 && item.AverageAmount == item.Amount)
                        {
                            item.AveragePercent = 100;
                            item.AveragePercentMore = 0;
                            item.AveragePercentStr = "";
                        }
                        else if (item.AverageAmount != 0 && item.AverageAmount >= item.Amount)
                        {
                            item.AveragePercent = item.Amount / item.AverageAmount * 100;
                            if (item.inBudget)
                                item.AveragePercentStr = ResourceLoader.GetForViewIndependentUse().GetString("CategoryReportBelowBudget")
                                    .Replace("%1", (item.AverageAmount - item.Amount).ToString("#,#"));
                            else
                                item.AveragePercentStr = ResourceLoader.GetForViewIndependentUse().GetString("CategoryReportBelow")
                                    .Replace("%1", (item.AverageAmount - item.Amount).ToString("#,#"));
                            item.AveragePercentMore = 0;
                        }
                        else if (item.AverageAmount != 0 && item.AverageAmount <= item.Amount)
                        {
                            item.AveragePercent = 100;
                            item.AveragePercentMore = 100 - (item.AverageAmount / item.Amount * 100);
                            if (item.inBudget)
                                item.AveragePercentStr = ResourceLoader.GetForViewIndependentUse().GetString("CategoryReportAboveBudget")
                                    .Replace("%1", (item.Amount - item.AverageAmount).ToString("#,#"))
                                    .Replace("%2", item.AveragePercentMore.ToString("#,#"));
                            else
                                item.AveragePercentStr = ResourceLoader.GetForViewIndependentUse().GetString("CategoryReportAbove")
                                    .Replace("%1", (item.Amount - item.AverageAmount).ToString("#,#"))
                                    .Replace("%2", item.AveragePercentMore.ToString("#,#"));
                        }
                        else
                        {
                            item.AveragePercent = 0;
                            item.AveragePercentMore = 0;
                            item.AveragePercentStr = "";
                        }
                    }
                    #endregion
                }
            });
            Report = res.OrderByDescending(i => i.Amount).ToList();
        }

        private bool checkBudget(Budget item)
        {
            if (item == null)
                return false;

            if (transType == TransactionType.Outcome)
                if (item.Outcome > 0)
                    return true;

            if (transType == TransactionType.Income)
                if (item.Income > 0)
                    return true;

            return false;
        }
        private bool checkBudget(List<Budget> items)
        {
            foreach(var item in items)
            {
                if (checkBudget(item))
                    return true;
            }
            return false;
        }
    }

    public class CategoryReportModel
    {
        public CategoryReportModel(Guid _TagId, string _Name, double _Amount, string _AmountStr, double _AverageAmount, bool _inBudget = false)
        {
            TagId = _TagId;
            Name = _Name;
            Amount = _Amount;
            AmountStr = _AmountStr;
            AverageAmount = _AverageAmount;
            inBudget = _inBudget;
        }

        public Guid TagId { get; set; }
        public string Name { get; set; }
        public double Amount { get; set; }
        public string AmountStr { get; set; }
        public double Percent { get; set; }
        public string PercentStr { get; set; }

        public double AverageAmount { get; set; }
        public double AveragePercent { get; set; }
        public double AveragePercentMore { get; set; }
        public string AveragePercentStr { get; set; }

        public bool inBudget { get; set; }
    }

    public class PercentToVis : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if ((double)value > 0)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class PercentToWeight : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if ((double)value > 0)
                return FontWeights.Normal;
            else
                return FontWeights.SemiBold;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
