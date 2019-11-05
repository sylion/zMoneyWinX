using System;
using System.Collections.Generic;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using zMoneyWinX.Client;
using zMoneyWinX.Model;

namespace zMoneyWinX.ViewModel
{
    public class TransactionScheduleHelper : INotifyProperty
    {
        private string _ScheduleSymb { get; set; }
        public string ScheduleSymb { get { return _ScheduleSymb; } set { _ScheduleSymb = value; NotifyPropertyChanged(); } }
        private string _ScheduleRepeat { get; set; }
        public string ScheduleRepeat { get { return _ScheduleRepeat; } set { _ScheduleRepeat = value; NotifyPropertyChanged(); } }


        private bool _HelperEnabled { get; set; }
        public bool HelperEnabled { get { return _HelperEnabled; } set { _HelperEnabled = value; NotifyPropertyChanged(); } }

        private int _Mode { get; set; }
        public int Mode
        {
            get { return _Mode; }
            set
            {
                _Mode = value;
                if (_Mode == 1)
                {
                    _ScheduleSymb = ResourceLoader.GetForViewIndependentUse().GetString("ScheduleSymbWeek");
                    _ScheduleRepeat = ResourceLoader.GetForViewIndependentUse().GetString("ScheduleRepeatWeek");
                }
                else if(Mode == 0)
                {
                    _ScheduleSymb = ResourceLoader.GetForViewIndependentUse().GetString("ScheduleSymbDay");
                    _ScheduleRepeat = ResourceLoader.GetForViewIndependentUse().GetString("ScheduleRepeatDay");
                }
                else if (Mode == 2)
                {
                    _ScheduleSymb = ResourceLoader.GetForViewIndependentUse().GetString("ScheduleSymbMonth");
                    _ScheduleRepeat = ResourceLoader.GetForViewIndependentUse().GetString("ScheduleRepeatDay");
                }
                else if (Mode == 3)
                {
                    _ScheduleSymb = ResourceLoader.GetForViewIndependentUse().GetString("ScheduleSymbYear");
                    _ScheduleRepeat = ResourceLoader.GetForViewIndependentUse().GetString("ScheduleRepeatDay");
                }
                NotifyPropertyChanged();
            }
        }

        private bool _HasEndDate { get; set; }
        public bool HasEndDate { get { return _HasEndDate; } set { _HasEndDate = value; NotifyPropertyChanged(); } }
        public Visibility EndDateVisible { get { return HasEndDate ? Visibility.Visible : Visibility.Collapsed; } }

        private string _EndDate { get; set; }
        public string EndDate { get { return _EndDate; } set { _EndDate = value; NotifyPropertyChanged(); } }

        public int _Step { get; set; }
        public int Step { get { return _Step; } set { _Step = value; NotifyPropertyChanged(); } }

        public Visibility WeekVisible { get { return Mode == 1 ? Visibility.Visible : Visibility.Collapsed; } }
        private bool _Week1 { get; set; }
        public bool Week1 { get { return _Week1; } set { _Week1 = value; NotifyPropertyChanged(); } }
        private bool _Week2 { get; set; }
        public bool Week2 { get { return _Week2; } set { _Week2 = value; NotifyPropertyChanged(); } }
        private bool _Week3 { get; set; }
        public bool Week3 { get { return _Week3; } set { _Week3 = value; NotifyPropertyChanged(); } }
        private bool _Week4 { get; set; }
        public bool Week4 { get { return _Week4; } set { _Week4 = value; NotifyPropertyChanged(); } }
        private bool _Week5 { get; set; }
        public bool Week5 { get { return _Week5; } set { _Week5 = value; NotifyPropertyChanged(); } }
        private bool _Week6 { get; set; }
        public bool Week6 { get { return _Week6; } set { _Week6 = value; NotifyPropertyChanged(); } }
        private bool _Week7 { get; set; }
        public bool Week7 { get { return _Week7; } set { _Week7 = value; NotifyPropertyChanged(); } }

        public TransactionScheduleHelper()
        {
            Week1 = true;
            Step = 1;
            Mode = 0;
            EndDate = DateTime.Now.Date.ToString("yyyy-MM-dd");
        }

        List<ReminderMarker> Markers = new List<ReminderMarker>();
        public void makeReminder(Transaction item)
        {
            if (Step <= 0)
                Step = 1;
            if (HasEndDate && string.IsNullOrEmpty(EndDate))
                HasEndDate = false;

            Reminder reminder = new Reminder();
            reminder.Id = Guid.NewGuid();
            reminder.StartDate = item.Date;
            reminder.Interval = Interval;
            if (HasEndDate)
                reminder.EndDate = EndDate;
            else
                reminder.EndDate = null;

            if (Mode == 1)
                reminder.Step = Step * 7;
            else
                reminder.Step = Step;

            reminder.Points = makePoints(item, reminder.Id);
            reminder.Points.Sort();

            reminder.User = item.User;
            reminder.Income = item.Income;
            reminder.Outcome = item.Outcome;
            reminder.Changed = SettingsManager.toUnixTime(DateTime.UtcNow);
            reminder.IncomeInstrument = item.IncomeInstrument;
            reminder.OutcomeInstrument = item.OutcomeInstrument;
            reminder.IncomeAccount = item.IncomeAccount;
            reminder.OutcomeAccount = item.OutcomeAccount;
            reminder.Comment = item.Comment;
            reminder.Payee = item.Payee;
            reminder.Merchant = item.Merchant;
            reminder._Tags = item._Tags;
            reminder.isCreated = true;
            reminder.TransType = item.TransType;
            reminder.TransDateStamp = item.TransDateStamp;
            DBObject.Insert(reminder);

            foreach (var marker in Markers)
                DBObject.Insert(marker);
        }

        private List<int> makePoints(Transaction item, Guid ReminderId)
        {
            List<int> res = new List<int>();
            DateTime startDate = DateTime.Parse(item.Date);
            DateTime endpoint;
            if (HasEndDate)
                endpoint = DateTime.Parse(EndDate);
            else
                endpoint = startDate.AddYears(1);


            switch (Mode)
            {
                //Day
                case 0:
                    res.Add(0);
                    while (startDate <= endpoint)
                    {
                        makeMarker(item, ReminderId, startDate.Date.ToString("yyyy-MM-dd"));
                        startDate = startDate.AddDays(Step);
                    }
                    break;
                //Month
                case 2:
                    while (startDate <= endpoint)
                    {
                        makeMarker(item, ReminderId, startDate.Date.ToString("yyyy-MM-dd"));
                        startDate = startDate.AddMonths(Step);
                    }
                    break;
                //Year
                case 3:
                    while (startDate <= endpoint)
                    {
                        makeMarker(item, ReminderId, startDate.Date.ToString("yyyy-MM-dd"));
                        startDate = startDate.AddYears(Step);
                    }
                    break;
                //Week
                case 1:
                    if (!Week1 && !Week2 && !Week3 && !Week4 && !Week5 && !Week6 && !Week7)
                    {
                        res.Add(0);
                        while (startDate <= endpoint)
                        {
                            makeMarker(item, ReminderId, startDate.Date.ToString("yyyy-MM-dd"));
                            startDate = startDate.AddDays(Step * 7);
                        }
                        break;
                    }
                    else
                    {
                        var tmpDate = startDate;
                        for (int i = 0; i < 7; i++)
                        {
                            if (Week1 && startDate.AddDays(i).DayOfWeek == DayOfWeek.Monday)
                                res.Add(i);
                            if (Week2 && startDate.AddDays(i).DayOfWeek == DayOfWeek.Tuesday)
                                res.Add(i);
                            if (Week3 && startDate.AddDays(i).DayOfWeek == DayOfWeek.Wednesday)
                                res.Add(i);
                            if (Week4 && startDate.AddDays(i).DayOfWeek == DayOfWeek.Thursday)
                                res.Add(i);
                            if (Week5 && startDate.AddDays(i).DayOfWeek == DayOfWeek.Friday)
                                res.Add(i);
                            if (Week6 && startDate.AddDays(i).DayOfWeek == DayOfWeek.Saturday)
                                res.Add(i);
                            if (Week7 && startDate.AddDays(i).DayOfWeek == DayOfWeek.Sunday)
                                res.Add(i);
                        }

                        int tmpPoint = 0;
                        while (startDate <= endpoint)
                        {
                            if (Week1 && DayToInt(startDate.DayOfWeek) <= DayToInt(DayOfWeek.Monday))
                                makeMarker(item, ReminderId, startDate.AddDays(DayToInt(DayOfWeek.Monday) - DayToInt(startDate.DayOfWeek)).Date.ToString("yyyy-MM-dd"));

                            if (Week2 && DayToInt(startDate.DayOfWeek) <= DayToInt(DayOfWeek.Tuesday))
                                makeMarker(item, ReminderId, startDate.AddDays(DayToInt(DayOfWeek.Tuesday) - DayToInt(startDate.DayOfWeek)).Date.ToString("yyyy-MM-dd"));

                            if (Week3 && DayToInt(startDate.DayOfWeek) <= DayToInt(DayOfWeek.Wednesday))
                                makeMarker(item, ReminderId, startDate.AddDays(DayToInt(DayOfWeek.Wednesday) - DayToInt(startDate.DayOfWeek)).Date.ToString("yyyy-MM-dd"));

                            if (Week4 && DayToInt(startDate.DayOfWeek) <= DayToInt(DayOfWeek.Thursday))
                                makeMarker(item, ReminderId, startDate.AddDays(DayToInt(DayOfWeek.Thursday) - DayToInt(startDate.DayOfWeek)).Date.ToString("yyyy-MM-dd"));

                            if (Week5 && DayToInt(startDate.DayOfWeek) <= DayToInt(DayOfWeek.Friday))
                                makeMarker(item, ReminderId, startDate.AddDays(DayToInt(DayOfWeek.Friday) - DayToInt(startDate.DayOfWeek)).Date.ToString("yyyy-MM-dd"));

                            if (Week6 && DayToInt(startDate.DayOfWeek) <= DayToInt(DayOfWeek.Saturday))
                                makeMarker(item, ReminderId, startDate.AddDays(DayToInt(DayOfWeek.Saturday) - DayToInt(startDate.DayOfWeek)).Date.ToString("yyyy-MM-dd"));

                            if (Week7 && DayToInt(startDate.DayOfWeek) == DayToInt(DayOfWeek.Sunday))
                                makeMarker(item, ReminderId, startDate.AddDays(DayToInt(DayOfWeek.Sunday) - DayToInt(startDate.DayOfWeek)).Date.ToString("yyyy-MM-dd"));

                            startDate = startDate.AddDays(Step);
                            startDate = startDate.AddDays(1 - DayToInt(startDate.DayOfWeek));
                            tmpPoint += Step + (1 - DayToInt(startDate.DayOfWeek));
                        }
                    }
                    break;
                default:
                    break;
            }
            return res;
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

        private void makeMarker(Transaction item, Guid ReminderId, string Date)
        {
            ReminderMarker marker = new ReminderMarker();
            marker.Id = Guid.NewGuid();
            marker.Reminder = ReminderId;
            marker.Date = Date;
            marker.User = item.User;
            marker.Income = item.Income;
            marker.Outcome = item.Outcome;
            marker.Changed = SettingsManager.toUnixTime(DateTime.UtcNow);
            marker.IncomeInstrument = item.IncomeInstrument;
            marker.OutcomeInstrument = item.OutcomeInstrument;
            marker.IncomeAccount = item.IncomeAccount;
            marker.OutcomeAccount = item.OutcomeAccount;
            marker.Comment = item.Comment;
            marker.Payee = item.Payee;
            marker.Merchant = item.Merchant;
            marker._Tags = item._Tags;
            marker.isCreated = true;
            marker.isChanged = true;
            marker.TransType = item.TransType;
            marker.TransDateStamp = SettingsManager.toUnixTime(DateTime.Parse(Date).Date);
            marker.State = ReminderMarker.GetStateStr(ReminderMarker.States.Planned);

            Markers.Add(marker);
        }

        private string Interval
        {
            get
            {
                switch (Mode)
                {
                    case 2:
                        return "month";
                    case 3:
                        return "year";
                    default:
                        return "day";
                }
            }
        }
    }
}
