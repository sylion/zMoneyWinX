using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using zMoneyWinX.Client;
using zMoneyWinX.Model;

namespace zMoneyWinX.Reports
{
    public class IncomeDistributionReport : INotifyProperty
    {
        private DateTime DateFrom;
        private DateTime DateTo;

        private int MandatoryPerc;
        private int NonMandatoryPerc;
        private int DebetPerc;

        private int FinancicalMonthStarts;

        public IncomeDistributionReportModel Report = new IncomeDistributionReportModel();

        public IncomeDistributionReport()
        {
        }

        private void UpdateSettings()
        {
            FinancicalMonthStarts = 1;
            MandatoryPerc = SettingsManager.mandatoryperc;
            NonMandatoryPerc = SettingsManager.nonmandatoryperc;
            DebetPerc = SettingsManager.debetperc;

            if (DateTime.Now.Day < FinancicalMonthStarts)
            {
                if (FinancicalMonthStarts > DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month - 1))
                    FinancicalMonthStarts = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month - 1);

                DateFrom = new DateTime(DateTime.Now.Year, DateTime.Now.Month - 1, FinancicalMonthStarts).Date;
                DateTo = new DateTime(DateTime.Now.Year, DateTime.Now.Month - 1, FinancicalMonthStarts).AddDays(30);
            }
            else
            {
                if (FinancicalMonthStarts > DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month))
                    FinancicalMonthStarts = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);

                DateFrom = new DateTime(DateTime.Now.Year, DateTime.Now.Month, FinancicalMonthStarts).Date;
                DateTo = new DateTime(DateTime.Now.Year, DateTime.Now.Month, FinancicalMonthStarts).AddDays(30);
            }

            Report.IncomeDistributionInfo = string.Format("{0} {1}/{2}/{3}", ResourceLoader.GetForViewIndependentUse().GetString("IncomeDistributionInfo"), MandatoryPerc, DebetPerc, NonMandatoryPerc);
            Report.IncomeDistributionInfo1 = string.Format("- {1}% {0}", ResourceLoader.GetForViewIndependentUse().GetString("IncomeDistributionInfo1"), MandatoryPerc);
            Report.IncomeDistributionInfo2 = string.Format("- {1}% {0}", ResourceLoader.GetForViewIndependentUse().GetString("IncomeDistributionInfo2"), DebetPerc);
            Report.IncomeDistributionInfo3 = string.Format("- {1}% {0}", ResourceLoader.GetForViewIndependentUse().GetString("IncomeDistributionInfo3"), NonMandatoryPerc);
        }

        public async Task Refresh()
        {
            UpdateSettings();
            await Task.Run(() =>
            {
                var DefCur = User.GetDefaultCurrency();
                var DefCurSymb = Instrument.GetSymbol(DefCur);

                var _from = SettingsManager.toUnixTime(DateFrom);
                var _to = SettingsManager.toUnixTime(DateTo);

                List<Instrument> currencies = Instrument.Instruments();

                List<Budget> budget = Budget.GetBudget(DateFrom).ToList();

                using (var db = new SQLiteConnection(DBInitializer.DB_PATH))
                {
                    var trans = db.Table<Transaction>().Where(i => !i.isDeleted && i.TransDateStamp >= _from && i.TransDateStamp <= _to).ToList();
                    var TagsMandatory = Tag.Tags.Where(i => i.Required != null && i.Required == true).Select(i => i.Id).ToList();

                    var AccountDebets = Account.Accounts()
                                        .Where(i => !i.Archive && !i.isDeleted && ((i.Savings != null && i.Savings == true) || (!i.InBalance && i.Type == "deposit")))
                                        .Select(i => i.Id).ToList();

                    //Calc income summ
                    double Income = 0;
                    Income = budget.Where(i => i.Income > 0).Sum(i => i.Income);
                    Report.IncomeStr = ResourceLoader.GetForViewIndependentUse().GetString("IncomeDistributionReportStr") + Income.ToString("#,0.#") + " " + DefCurSymb;
                    if (Income > 0)
                        Report.IncomeStr += " " + ResourceLoader.GetForViewIndependentUse().GetString("IncomeDistributionReportFromBudget");

                    if (Income <= 0)
                    {
                        foreach (var item in trans.Where(i => i.TransType == TransactionType.Income))
                        {
                            if (item.OperationInstrument == DefCur)
                            {
                                Income += item.OperationAmount;
                            }
                            else
                            {
                                if (item.OperationInstrument == 2)
                                    Income += item.OperationAmount * currencies.FirstOrDefault(i => i.Id == item.OperationInstrument).Rate;
                                else
                                    Income += item.OperationAmount * currencies.FirstOrDefault(i => i.Id == item.OperationInstrument).Rate / currencies.FirstOrDefault(i => i.Id == DefCur).Rate;
                            }
                        }
                        Report.IncomeStr = ResourceLoader.GetForViewIndependentUse().GetString("IncomeDistributionReportStr") + Income.ToString("#,0.#") + " " + DefCurSymb;
                    }
                    Report.Income = Income;

                    //Calc mandatory outcome
                    double OutcomeMandatory = 0;
                    foreach (var item in trans.Where(i => i.TransType == TransactionType.Outcome && i.Tags != null && TagsMandatory.Contains(i.Tags.FirstOrDefault())))
                    {
                        if (item.OperationInstrument == DefCur)
                        {
                            OutcomeMandatory += item.OperationAmount;
                        }
                        else
                        {
                            if (item.OperationInstrument == 2)
                                OutcomeMandatory += item.OperationAmount * currencies.FirstOrDefault(i => i.Id == item.OperationInstrument).Rate;
                            else
                                OutcomeMandatory += item.OperationAmount * currencies.FirstOrDefault(i => i.Id == item.OperationInstrument).Rate / currencies.FirstOrDefault(i => i.Id == DefCur).Rate;
                        }
                    }
                    Report.OutcomeMandatory = Math.Round(OutcomeMandatory, 2);

                    //Calc nonmandatory outcome
                    double OutcomeNonMandatory = 0;
                    foreach (var item in trans.Where(i => i.TransType == TransactionType.Outcome && i.Tags != null && !TagsMandatory.Contains(i.Tags.FirstOrDefault())))
                    {
                        if (item.OperationInstrument == DefCur)
                        {
                            OutcomeNonMandatory += item.OperationAmount;
                        }
                        else
                        {
                            if (item.OperationInstrument == 2)
                                OutcomeNonMandatory += item.OperationAmount * currencies.FirstOrDefault(i => i.Id == item.OperationInstrument).Rate;
                            else
                                OutcomeNonMandatory += item.OperationAmount * currencies.FirstOrDefault(i => i.Id == item.OperationInstrument).Rate / currencies.FirstOrDefault(i => i.Id == DefCur).Rate;
                        }
                    }
                    Report.OutcomeNonMandatory = Math.Round(OutcomeNonMandatory, 2);

                    //Calc debets
                    double Debets = 0;
                    foreach (var item in trans.Where(i => (i.TransType == TransactionType.Transfer || i.TransType == TransactionType.Income)
                                                    && AccountDebets.Contains(i.IncomeAccount)))
                    {
                        if (item.IncomeInstrument == DefCur)
                        {
                            Debets += item.Income;
                        }
                        else
                        {
                            if (item.OperationInstrument == 2)
                                Debets += item.Income * currencies.FirstOrDefault(i => i.Id == item.IncomeInstrument).Rate;
                            else
                                Debets += item.Income * currencies.FirstOrDefault(i => i.Id == item.IncomeInstrument).Rate / currencies.FirstOrDefault(i => i.Id == DefCur).Rate;
                        }
                    }
                    Report.Debets = Math.Round(Debets, 2);

                    //Calc balance and percentage
                    double Balance = 0;
                    if (Income > 0)
                    {
                        Balance = Math.Round(Income - (OutcomeMandatory + OutcomeNonMandatory + Debets), 2);
                        Report.Balance = Balance;

                        Report.BalancePerc = Balance / Income * 100;
                        Report.BalancePercStr = Report.BalancePerc.ToString("0.#") + "%";
                        if (Report.BalancePerc > 100)
                            Report.BalancePerc = 100;
                        if (Report.BalancePerc < 0)
                            Report.BalancePerc = 0;

                        Report.OutcomeMandatoryPerc = OutcomeMandatory / Income * 100;
                        Report.OutcomeMandatoryPercStr = Report.OutcomeMandatoryPerc.ToString("0.#") + "%";
                        if (Report.OutcomeMandatoryPerc > 100)
                            Report.OutcomeMandatoryPerc = 100;
                        if (Report.OutcomeMandatoryPerc < 0)
                            Report.OutcomeMandatoryPerc = 0;

                        Report.OutcomeNonMandatoryPerc = OutcomeNonMandatory / Income * 100;
                        Report.OutcomeNonMandatoryPercStr = Report.OutcomeNonMandatoryPerc.ToString("0.#") + "%";
                        if (Report.OutcomeNonMandatoryPerc > 100)
                            Report.OutcomeNonMandatoryPerc = 100;
                        if (Report.OutcomeNonMandatoryPerc < 0)
                            Report.OutcomeNonMandatoryPerc = 0;

                        if (DebetPerc > 0)
                        {
                            Report.DebetsPerc = Debets / Income * 100;
                            Report.DebetsPercStr = Report.DebetsPerc.ToString("0.#") + "%";
                            if (Report.DebetsPerc > 100)
                                Report.DebetsPerc = 100;
                            if (Report.DebetsPerc < 0)
                                Report.DebetsPerc = 0;
                        }
                        else
                        {
                            Report.DebetsPerc = 100;
                            Report.DebetsPercStr = Report.DebetsPerc.ToString("0.#") + "%";
                        }
                    }
                    else
                    {
                        Report.Balance = 0;
                        Report.BalancePerc = 0;
                        Report.BalancePercStr = "0%";

                        Report.OutcomeMandatoryPerc = 0;
                        Report.OutcomeMandatoryPercStr = "0%";

                        Report.OutcomeNonMandatoryPerc = 0;
                        Report.OutcomeNonMandatoryPercStr = "0%";

                        Report.DebetsPerc = 0;
                        Report.DebetsPercStr = "0%";
                    }

                    //Generate messages
                    Report.Msg = new List<string>();
                    if(budget.Where(i => i.Income > 0).Sum(i => i.Income) <= 0)
                        Report.Msg.Add(ResourceLoader.GetForViewIndependentUse().GetString("IncomeDistributionReportMsgNoBudget"));

                    if (Income <= 0)
                        Report.Msg.Add(ResourceLoader.GetForViewIndependentUse().GetString("IncomeDistributionReportMsgOops"));

                    if (TagsMandatory == null || TagsMandatory.Count <= 0)
                        Report.Msg.Add(ResourceLoader.GetForViewIndependentUse().GetString("IncomeDistributionReportMsgNoTagsMandatory"));

                    if (DebetPerc > 0 && (AccountDebets == null || AccountDebets.Count <= 0))
                        Report.Msg.Add(ResourceLoader.GetForViewIndependentUse().GetString("IncomeDistributionReportMsgNoAccDebets"));

                    if (Income > 0)
                    {
                        double tmp = 0;
                        bool Bad = false;
                        if (Report.OutcomeMandatoryPerc > MandatoryPerc)
                        {
                            tmp = Report.OutcomeMandatory - (Income / 100 * MandatoryPerc);
                            Report.Msg.Add(ResourceLoader.GetForViewIndependentUse().GetString("IncomeDistributionReportMsgMandatoryMore") + tmp.ToString("#,0.##") + DefCurSymb);
                        }

                        if (Report.OutcomeNonMandatoryPerc > NonMandatoryPerc)
                        {
                            tmp = Report.OutcomeNonMandatory - (Income / 100 * NonMandatoryPerc);
                            Report.Msg.Add(ResourceLoader.GetForViewIndependentUse().GetString("IncomeDistributionReportMsgNonMandatoryMore") + tmp.ToString("#,0.##") + DefCurSymb);
                        }

                        if (DebetPerc != 0 && Report.DebetsPerc > DebetPerc)
                        {
                            tmp = Report.Debets - (Income / 100 * DebetPerc);
                            Report.Msg.Add(ResourceLoader.GetForViewIndependentUse().GetString("IncomeDistributionReportMsgDepositMore") + tmp.ToString("#,0.##") + DefCurSymb);
                        }

                        double tmpBalance = Balance;
                        if (Report.OutcomeMandatoryPerc < MandatoryPerc)
                        {
                            if (Income / 100 * MandatoryPerc - Report.OutcomeMandatory <= tmpBalance)
                            {
                                tmp = Income / 100 * MandatoryPerc - Report.OutcomeMandatory;
                                tmpBalance -= tmp;
                                Report.Msg.Add(ResourceLoader.GetForViewIndependentUse().GetString("IncomeDistributionReportMsgMandatoryLess") + tmp.ToString("#,0.##") + DefCurSymb);
                            }
                            else if (tmpBalance > 0)
                            {
                                tmpBalance -= tmpBalance;
                                Report.Msg.Add(ResourceLoader.GetForViewIndependentUse().GetString("IncomeDistributionReportMsgMandatoryLess") + tmp.ToString("#,0.##") + DefCurSymb);
                            }
                            else
                                Bad = true;
                        }

                        if (Report.DebetsPerc < DebetPerc)
                        {
                            if ((Income / 100 * DebetPerc) - Report.Debets <= tmpBalance)
                            {
                                tmp = (Income / 100 * DebetPerc) - Report.Debets;
                                tmpBalance -= tmp;
                                Report.Msg.Add(ResourceLoader.GetForViewIndependentUse().GetString("IncomeDistributionReportMsgDepositLess") + tmp.ToString("#,0.##") + DefCurSymb);
                            }
                            else if (tmpBalance > 0)
                            {
                                tmp = tmpBalance;
                                tmpBalance -= tmpBalance;
                                Report.Msg.Add(ResourceLoader.GetForViewIndependentUse().GetString("IncomeDistributionReportMsgDepositLess") + tmp.ToString("#,0.##") + DefCurSymb);
                            }
                            else
                                Bad = true;
                        }

                        if (Report.OutcomeNonMandatoryPerc < NonMandatoryPerc)
                        {
                            if ((Income / 100 * NonMandatoryPerc) - Report.OutcomeNonMandatory <= tmpBalance)
                            {
                                tmp = (Income / 100 * NonMandatoryPerc) - Report.OutcomeNonMandatory;
                                tmpBalance -= tmp;
                                Report.Msg.Add(ResourceLoader.GetForViewIndependentUse().GetString("IncomeDistributionReportMsgNonMandatoryLess") + tmp.ToString("#,0.##") + DefCurSymb);
                            }
                            else if (tmpBalance > 0)
                            {
                                tmp = tmpBalance;
                                tmpBalance -= tmpBalance;
                                Report.Msg.Add(ResourceLoader.GetForViewIndependentUse().GetString("IncomeDistributionReportMsgNonMandatoryLess") + tmp.ToString("#,0.##") + DefCurSymb);
                            }
                            else
                                Bad = true;
                        }

                        if (Bad)
                        {
                            if (tmpBalance < 0)
                                Report.Msg.Add(ResourceLoader.GetForViewIndependentUse().GetString("IncomeDistributionReportMsgBadBalance"));
                            else
                                Report.Msg.Add(ResourceLoader.GetForViewIndependentUse().GetString("IncomeDistributionReportMsgBad"));
                        }
                    }
                }
            });
            Report.Notify();
        }
    }

    public class IncomeDistributionReportModel : INotifyProperty
    {
        public double Income { get; set; }
        public string IncomeStr { get; set; }

        public double OutcomeMandatory { get; set; }
        public double OutcomeMandatoryPerc { get; set; }
        public string OutcomeMandatoryPercStr { get; set; }

        public double OutcomeNonMandatory { get; set; }
        public double OutcomeNonMandatoryPerc { get; set; }
        public string OutcomeNonMandatoryPercStr { get; set; }

        public double Debets { get; set; }
        public double DebetsPerc { get; set; }
        public string DebetsPercStr { get; set; }

        public double Balance { get; set; }
        public double BalancePerc { get; set; }
        public string BalancePercStr { get; set; }

        public List<string> Msg { get; set; } = new List<string>();

        public string IncomeDistributionInfo { get; set; }
        public string IncomeDistributionInfo1 { get; set; }
        public string IncomeDistributionInfo2 { get; set; }
        public string IncomeDistributionInfo3 { get; set; }
        //public string IncomeDistributionInfo { get { return _IncomeDistributionInfo; } set { _IncomeDistributionInfo = value; NotifyPropertyChanged(); } }
        //public string IncomeDistributionInfo1 { get { return _IncomeDistributionInfo1; } set { _IncomeDistributionInfo1 = value; NotifyPropertyChanged(); } }
        //public string IncomeDistributionInfo2 { get { return _IncomeDistributionInfo2; } set { _IncomeDistributionInfo2 = value; NotifyPropertyChanged(); } }
        //public string IncomeDistributionInfo3 { get { return _IncomeDistributionInfo3; } set { _IncomeDistributionInfo3 = value; NotifyPropertyChanged(); } }

        public void Notify()
        {
            NotifyPropertyChanged();
        }
    }
}
