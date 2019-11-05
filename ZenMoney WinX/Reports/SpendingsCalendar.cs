using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using zMoneyWinX.Client;
using zMoneyWinX.Model;

namespace zMoneyWinX.Reports
{
    public class SpendingsCalendarReport : INotifyProperty
    {
        public SpendingsCalendarReport()
        {
            DateFrom = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).Date;
            DateTo = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).Date.AddMonths(1).AddSeconds(-1);
        }
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
        public List<SpendingsCalendarModel> Report { get; private set; }
        public void NextPeriod()
        {
            if (DateFrom.Year == DateTime.Now.Year && DateFrom.Month >= DateTime.Now.Month)
                return;

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
            List<SpendingsCalendarModel> res = new List<SpendingsCalendarModel>();
            await Task.Run(() =>
            {
                //Add empty cells from previous month
                for (int i = DayToInt(DayOfWeek.Monday) - DayToInt(DateFrom.DayOfWeek); i < 0; i++)
                    res.Add(new SpendingsCalendarModel() { CellDate = DateFrom.AddDays(i), inCurrentMonth = false });

                var DefCur = User.GetDefaultCurrency();
                var _from = SettingsManager.toUnixTime(DateFrom.Date);
                var _to = SettingsManager.toUnixTime(DateTo.Date);
                List<Transaction> trans;
                List<ReminderMarker> plannedtrans;
                using (var db = new SQLiteConnection(DBInitializer.DB_PATH, SQLiteOpenFlags.ReadOnly))
                {
                    trans = db.Table<Transaction>()
                                .Where(i => !i.isDeleted && i.TransDateStamp >= _from && i.TransDateStamp <= _to &&
                                (i.TransType == TransactionType.Outcome || i.TransType == TransactionType.Transfer)).ToList()
                                .Where(i => i.TransType == TransactionType.Outcome || i.TransIs == TransactionType.Outcome).ToList();

                    plannedtrans = db.Table<ReminderMarker>()
                                .Where(i => !i.isDeleted && i.TransDateStamp >= _from && i.TransDateStamp <= _to &&
                                (i.TransType == TransactionType.Outcome || i.TransType == TransactionType.Transfer)).ToList()
                                .Where(i => i.TransType == TransactionType.Outcome || i.TransIs == TransactionType.Outcome).ToList();
                }
                List<Instrument> currencies = Instrument.Instruments();

                for (int iter = 0; iter < DateTime.DaysInMonth(DateFrom.Year, DateFrom.Month); iter++)
                {
                    double sum = 0;
                    long currentStamp = SettingsManager.toUnixTime(DateFrom.AddDays(iter).Date);
                    foreach (var item in trans.Where(x => x.TransDateStamp == currentStamp))
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
                    if (sum != 0)
                        res.Add(new SpendingsCalendarModel() { CellDate = DateFrom.AddDays(iter), inCurrentMonth = true, Sum = sum });
                    else
                    {
                        foreach (var item in plannedtrans.Where(x => x.TransDateStamp == currentStamp && x.State == ReminderMarker.GetStateStr(ReminderMarker.States.Planned)))
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
                        if (sum != 0)
                            res.Add(new SpendingsCalendarModel() { CellDate = DateFrom.AddDays(iter), inCurrentMonth = true, Sum = sum, isPlanned = true });
                        else
                            res.Add(new SpendingsCalendarModel() { CellDate = DateFrom.AddDays(iter), inCurrentMonth = true, Sum = sum });
                    }
                    sum = 0;
                }

                //Add empty cells from next month
                for (int i = 0; i < DayToInt(DayOfWeek.Sunday) - DayToInt(DateTo.DayOfWeek); i++)
                    res.Add(new SpendingsCalendarModel() { CellDate = DateFrom.AddDays(i), inCurrentMonth = false });
            });
            var Amount = res.Max(i => i.Sum);
            while (Amount > res.Where(i => !i.isPlanned).Sum(i => i.Sum) / res.Where(i => !i.isPlanned && !i.isAbnormal && i.Sum > 0).Count() * 3)
            {
                res.FirstOrDefault(i => i.Sum == Amount).isAbnormal = true;
                Amount = res.Where(i => !i.isAbnormal).Max(i => i.Sum);
            }
            foreach (var item in res)
            {
                if (!item.inCurrentMonth)
                {
                    item.BackroundColor = new SolidColorBrush(Colors.Transparent);
                    continue;
                }

                Color color;
                if (item.isAbnormal && !item.isPlanned)
                {
                    item.BackroundColor = new SolidColorBrush(Colors.Brown);
                    continue;
                }
                else if(item.isAbnormal && item.isPlanned)
                {
                    item.BackroundColor = new SolidColorBrush(Colors.DeepSkyBlue);
                    continue;
                }
                else if (!item.isPlanned)
                    color = Colors.Red;
                else
                    color = Colors.SkyBlue;

                if (item.Sum <= 0)
                    item.BackroundColor = new SolidColorBrush(Colors.Transparent);
                else
                    item.BackroundColor = new SolidColorBrush(Color.FromArgb((byte)(item.Sum / Amount * 255), color.R, color.G, color.B));
            }
            Report = res.ToList();
        }

        private int DayToInt(DayOfWeek day)
        {
            switch (day)
            {
                case DayOfWeek.Monday:
                    return 0;
                case DayOfWeek.Tuesday:
                    return 1;
                case DayOfWeek.Wednesday:
                    return 2;
                case DayOfWeek.Thursday:
                    return 3;
                case DayOfWeek.Friday:
                    return 4;
                case DayOfWeek.Saturday:
                    return 5;
                case DayOfWeek.Sunday:
                    return 6;
                default:
                    return 0;
            }
        }
    }

    public class SpendingsCalendarModel
    {
        private SolidColorBrush _BackroundColor { get; set; }
        private DateTime _CellDate { get; set; }
        private bool _inCurrentMonth { get; set; } = true;
        private bool _isPlanned { get; set; } = false;
        private double _Sum { get; set; } = 0;
        private bool _isAbnormal { get; set; } = false;

        public SolidColorBrush BackroundColor { get { return _BackroundColor; } set { _BackroundColor = value; } }
        public DateTime CellDate { get { return _CellDate; } set { _CellDate = value; } }
        public bool inCurrentMonth { get { return _inCurrentMonth; } set { _inCurrentMonth = value; } }
        public Visibility IsVis { get { return _inCurrentMonth ? Visibility.Visible : Visibility.Collapsed; } }
        public Thickness Border { get { return CellDate.Date == DateTime.Now.Date ? new Thickness(1) : new Thickness(0); } }

        public string Day { get { return CellDate.Day.ToString(); } }
        public double Sum { get { return _Sum; } set { _Sum = value; } }
        public bool isPlanned { get { return _isPlanned; } set { _isPlanned = value; } }
        public bool isAbnormal { get { return _isAbnormal; } set { _isAbnormal = value; } }
    }
}
