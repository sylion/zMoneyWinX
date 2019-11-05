using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Media;
using zMoneyWinX.Model;

namespace zMoneyWinX.Reports
{
    class AccountCurrencyReport
    {
        public string Name { get; set; }
        public double AmountMST { get; set; }
        public string AmountMSTSymbol { get; set; }
        public double Amount { get; set; }
        public string PercentStr { get; set; }
        public double Percent { get; set; }
        public Brush Color { get; set; }

        public static async Task<List<AccountCurrencyReport>> Report()
        {
            List<AccountCurrencyReport> res = new List<AccountCurrencyReport>();
            var Currency = User.GetDefaultCurrency();
            await Task.Run(() =>
            {
                int CurrentUserIDStr = User.CurrentUserID;
                using (var db = new SQLiteConnection(DBInitializer.DB_PATH))
                {
                    var list = db.Table<Account>().Where(i => !i.Archive && i.Type != "debt" && !i.isDeleted && (i.Role == null || i.Role == CurrentUserIDStr) && i.Balance > 0).GroupBy(i => i.Instrument);

                    foreach (var item in list)
                    {
                        var sum = item.Sum(i => i.Balance);
                        double summst = 0;
                        if (item.Key != Currency)
                        {
                            if (item.Key == 2)
                                summst = sum * Instrument.GetRate(item.Key);
                            else
                                summst = sum * Instrument.GetRate(item.Key) / Instrument.GetRate(Currency);
                        }
                        if (summst == 0)
                            summst = sum;
                        res.Add(new AccountCurrencyReport { Name = db.Get<Instrument>(item.Key).ShortTitle, AmountMST = Math.Round(summst), Amount = sum });
                    }
                }
            });

            res = res.OrderByDescending(i => i.AmountMST).ToList();
            double summ = res.Sum(i => i.AmountMST);
            var symbol = Instrument.GetSymbol(Currency);
            int iter = 0;
            foreach (var item in res)
            {
                item.Color = ColorModeBrushes[iter];
                item.AmountMSTSymbol = symbol;
                item.Percent = item.AmountMST / summ * 100;
                item.PercentStr = (item.AmountMST / summ * 100).ToString("#0.#") + "%";
                iter++;
            }

            return res;
        }

        public static List<Brush> ColorModeBrushes => new List<Brush>()
        {
            new SolidColorBrush(Colors.Orange),
            new SolidColorBrush(Colors.SkyBlue),
            new SolidColorBrush(Colors.Purple),
            new SolidColorBrush(Colors.Gold),
            new SolidColorBrush(Colors.Red),
            new SolidColorBrush(Colors.Silver),
            new SolidColorBrush(Colors.Gray),
            new SolidColorBrush(Colors.DarkBlue),
            new SolidColorBrush(Colors.Violet),
            new SolidColorBrush(Colors.Olive),
            new SolidColorBrush(Colors.GreenYellow),
            new SolidColorBrush(Colors.Brown)
        };
    }
}
