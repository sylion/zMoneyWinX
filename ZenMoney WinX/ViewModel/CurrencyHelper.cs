using System.Collections.Generic;
using System.Linq;
using zMoneyWinX.Model;

namespace zMoneyWinX.ViewModel
{
    public class CurrencyHelper
    {
        public sealed class CurrencyConverter : INotifyProperty
        {
            public List<Instrument> instruments { get; set; }

            private int _FromCode { get; set; }
            public int FromCode
            {
                get
                {
                    return _FromCode;
                }
                set
                {
                    _FromCode = value;
                    FromSymb = instruments.FirstOrDefault(i => i.Id == value).Symbol;
                    NotifyPropertyChanged();
                }
            }
            private int _ToCode { get; set; }
            public int ToCode
            {
                get
                {
                    return _ToCode;
                }
                set
                {
                    _ToCode = value;
                    ToSymb = instruments.FirstOrDefault(i => i.Id == value).Symbol;
                    NotifyPropertyChanged();
                }
            }

            private string _FromSymb { get; set; }
            public string FromSymb
            {
                get
                {
                    return _FromSymb;
                }
                set
                {
                    _FromSymb = value;
                    NotifyPropertyChanged();
                }
            }
            private string _ToSymb { get; set; }
            public string ToSymb
            {
                get
                {
                    return _ToSymb;
                }
                set
                {
                    _ToSymb = value;
                    NotifyPropertyChanged();
                }
            }


            public double _Amount { get; set; }
            public double Amount
            {
                get
                {
                    return _Amount;
                }
                set
                {
                    _Amount = value;
                    NotifyPropertyChanged();
                }
            }

            public double _Result { get; set; }
            public double Result
            {
                get
                {
                    return _Result;
                }
                set
                {
                    _Result = value;
                    NotifyPropertyChanged();
                }
            }

            public CurrencyConverter()
            {
                instruments = Instrument.Instruments();
            }

            public void init()
            {
                FromCode = instruments.FirstOrDefault().Id;
                ToCode = User.GetDefaultCurrency();
            }

            public void Convert()
            {
                var UserCur = User.GetDefaultCurrency();
                var UserCurRate = Instrument.GetRate(UserCur);

                var FromRate = Instrument.GetRate(FromCode) / UserCurRate;
                var ToRate = Instrument.GetRate(ToCode) / UserCurRate;

                try
                {
                    Result = Amount * (FromRate / ToRate);
                }
                catch { Result = 0; }
            }
        }
    }
}
