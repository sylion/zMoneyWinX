using Newtonsoft.Json;
using SQLite;
using System.Collections.Generic;
using System.Linq;
using Windows.ApplicationModel.Resources;

namespace zMoneyWinX.Model
{
    public partial class Instrument : DBObject
    {
        [JsonProperty("symbol")]
        public string Symbol
        {
            get
            {
                switch (ShortTitle)
                {
                    case "EUR":
                        return "€";
                    case "UAH":
                        return "₴";
                    case "RUB":
                        return "₽";
                    case "USD":
                        return "$";
                    case "GBP":
                        return "£";
                    case "BYR":
                        return "Br";
                    case "KZT":
                        return "₸";
                    case "TRY":
                        return "₺";
                    case "AZN":
                        return "₼";
                    case "LVL":
                        return "Ls";
                    case "LTL":
                        return "Lt";
                    case "BRL":
                        return "R$";
                    case "HUF":
                        return "Ft";
                    case "KRW":
                        return "₩";
                    case "DKK":
                        return "kr";
                    case "INR":
                        return "₹";
                    case "CNY":
                        return "¥";
                    case "PLN":
                        return "zł";
                    case "RON":
                        return "lei";
                    case "CZK":
                        return "Kč";
                    case "JPY":
                        return "¥";
                    case "THB":
                        return "฿";
                    case "IDR":
                        return "Rp";
                    case "ILS":
                        return "₪";
                    case "NIO":
                        return "C$";
                    case "MYR":
                        return "RM";
                    case "VND":
                        return "₫";
                    case "IRR":
                        return "﷼";
                    case "HRK":
                        return "kn";
                    case "BAM":
                        return "KM";
                    case "GEL":
                        return "₾";
                    case "ISK":
                        return "kr";
                    case "PHP":
                        return "₱";
                    case "LKR":
                        return "₨";
                    case "GTQ":
                        return "Q";
                    case "LAK":
                        return "₭";
                    case "KHR":
                        return "៛";
                    case "BTC":
                        return "Ƀ";
                    default:
                        return _Symbol;
                }
            }
            set
            {
                _Symbol = value;
            }
        }

        [JsonProperty("title")]
        public string Title
        {
            get
            {
                try
                {
                    var res = ResourceLoader.GetForViewIndependentUse().GetString("cur" + ShortTitle);
                    if (!string.IsNullOrEmpty(res))
                        return res;
                    else
                        return _Title;
                }
                catch
                {
                    return _Title;
                }
            }
            set
            {
                _Title = value;
            }
        }

        #region DisplayOnly
        [Ignore, JsonIgnore]
        public string DisplayRate { get; set; }
        #endregion

        public static List<Instrument> Instruments()
        {
            using (SQLiteConnection db = new SQLiteConnection(DBInitializer.DB_PATH))
                return db.Table<Instrument>().ToList();
        }

        public static List<Instrument> RatesList()
        {
            var UserCur = User.GetDefaultCurrency();
            using (SQLiteConnection db = new SQLiteConnection(DBInitializer.DB_PATH))
            {
                if (UserCur == 2)
                {
                    var res = db.Table<Instrument>().Where(i => i.Id != UserCur).ToList();
                    foreach (var item in res)
                    {
                        if (item.Rate >= 0.01)
                            item.DisplayRate = (item.Rate).ToString("#,0.0000");
                        else if (item.Rate >= 0.001)
                            item.DisplayRate = "(x10) " + (item.Rate * 10).ToString("#,0.0000");
                        else if (item.Rate >= 0.0001)
                            item.DisplayRate = "(x100) " + (item.Rate * 100).ToString("#,0.0000");
                        else if (item.Rate >= 0.00001)
                            item.DisplayRate = "(x1000) " + (item.Rate * 1000).ToString("#,0.0000");
                    }
                    return res;
                }
                else
                {
                    var res = db.Table<Instrument>().Where(i => i.Id != UserCur).ToList();
                    var UserCurRate = GetRate(UserCur);
                    foreach (var item in res)
                    {
                        if (item.Rate / UserCurRate >= 0.01)
                            item.DisplayRate = (item.Rate / UserCurRate).ToString("#,0.0000");
                        else if (item.Rate / UserCurRate >= 0.001)
                            item.DisplayRate = "(x10) " + (item.Rate * 10 / UserCurRate).ToString("#,0.0000");
                        else if (item.Rate / UserCurRate >= 0.0001)
                            item.DisplayRate = "(x100) " + (item.Rate * 100 / UserCurRate).ToString("#,0.0000");
                        else if (item.Rate / UserCurRate >= 0.00001)
                            item.DisplayRate = "(x1000) " + (item.Rate * 1000 / UserCurRate).ToString("#,0.0000");
                    }
                    return res;
                }
            }
        }

        public static double GetRate(int ID)
        {
            using (SQLiteConnection db = new SQLiteConnection(DBInitializer.DB_PATH))
                return db.Get<Instrument>(ID).Rate;
        }

        public static string GetSymbol(int ID)
        {
            using (SQLiteConnection db = new SQLiteConnection(DBInitializer.DB_PATH))
                return db.Get<Instrument>(ID).Symbol;
        }

        public static void UpdateRates(List<Instrument> items)
        {
            using (SQLiteConnection db = new SQLiteConnection(DBInitializer.DB_PATH))
            {
                List<Instrument> rates = db.Table<Instrument>().ToList();
                foreach (Instrument item in items)
                {
                    if (rates.Where(i => i.Id == item.Id).Any())
                    {
                        rates.FirstOrDefault(i => i.Id == item.Id).Rate = item.Rate;
                    }
                    else if (!string.IsNullOrEmpty(item.Title) && !string.IsNullOrEmpty(item.ShortTitle))
                    {
                        rates.Add(item);
                    }
                }
                db.RunInTransaction(() =>
                {
                    foreach (var x in rates)
                        db.InsertOrReplace(x);
                });
            }
        }
    }
}