using System;
using System.Collections.Generic;
using zMoneyWinX.Client;

namespace zMoneyWinX.Model
{
    public sealed class TransactionFilter : TransactionFilterEdit
    {

    }
    public class TransactionFilterEdit : INotifyProperty
    {
        public TransactionFilterEdit()
        {
            DatePeriod = TransactionPeriod.All;
            DateFrom = DateTime.Now.Date;
            DateTo = DateTime.Now.Date;
            TransType = TransactionTypeFilter.All;
            Tags = null;
            Merchant = null;
            Account = null;
        }
        //Date
        private TransactionPeriod _DatePeriod { get; set; }
        public TransactionPeriod DatePeriod { get { return _DatePeriod; } set { _DatePeriod = value; NotifyPropertyChanged(); } }
        private DateTime _DateFrom { get; set; }
        public DateTime DateFrom { get { return _DateFrom; } set { _DateFrom = value; NotifyPropertyChanged(); } }
        public long DateFromStamp { get { return SettingsManager.toUnixTime(_DateFrom); } }
        private DateTime _DateTo { get; set; }
        public DateTime DateTo { get { return _DateTo; } set { _DateTo = value; NotifyPropertyChanged(); } }
        public long DateToStamp { get { return SettingsManager.toUnixTime(_DateTo); } }

        //Account
        private List<Guid> _Account { get; set; }
        public List<Guid> Account { get { return _Account; } set { _Account = value; NotifyPropertyChanged(); } }

        //Tags
        private bool _Exclude { get; set; }
        public bool Exclude { get { return _Exclude; } set { _Exclude = value; NotifyPropertyChanged(); } }
        private List<Guid> _Tags { get; set; }
        public List<Guid> Tags { get { return _Tags; } set { _Tags = value; NotifyPropertyChanged(); } }

        //Comment
        private string _Comment { get; set; }
        public string Comment { get { return _Comment; } set { _Comment = value; NotifyPropertyChanged(); } }

        //Merchant
        private Guid? _Merchant { get; set; }
        public Guid? Merchant { get { return _Merchant; } set { _Merchant = value; NotifyPropertyChanged(); } }

        //TransType
        private TransactionTypeFilter _TransType { get; set; }
        public TransactionTypeFilter TransType { get { return _TransType; } set { _TransType = value; NotifyPropertyChanged(); } }
    }
    public enum TransactionTypeFilter
    {
        All = 0,
        Outcome = 1,
        Income = 2,
        Transfer = 3,
        Dept = 4
    }

    public enum TransactionPeriod
    {
        All = 0,
        Today = 1,
        Yesterday = 2,
        ThisWeek = 3,
        ThisMonth = 4,
        Period = 5
    }
}
