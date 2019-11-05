using System.Threading.Tasks;
using zMoneyWinX.Model;

namespace zMoneyWinX.ViewModel
{
    public class AccountBalanceHelper : INotifyProperty
    {
        private string _Balance = "---";
        private string _NegativeBalance = "---";
        private string _AvailableBalance = "---";

        private string _AllBalance = "---";
        private string _AllNegativeBalance = "---";
        private string _AllAvailableBalance = "---";

        private string _OweI = "---";
        private string _OweMe = "---";

        public static async Task<AccountBalanceHelper> Report()
        {
            AccountBalanceHelper res = new AccountBalanceHelper();
            await Task.Run(() =>
            {
                res.Balance = Account.GetBalance(Account.BalanceType.All);
                res.NegativeBalance = Account.GetBalance(Account.BalanceType.Negative);
                res.AvailableBalance = Account.GetBalance(Account.BalanceType.Available);

                res.AllBalance = Account.GetBalanceAll(Account.BalanceType.All);
                res.AllNegativeBalance = Account.GetBalanceAll(Account.BalanceType.Negative);
                res.AllAvailableBalance = Account.GetBalanceAll(Account.BalanceType.Available);

                //res.OweI = Account.GetBalanceAll(Account.BalanceType.mDebt);
                //res.OweMe = Account.GetBalanceAll(Account.BalanceType.pDebt);
            });

            return res;
        }

        public string Balance { get { return _Balance; } set { _Balance = value; NotifyPropertyChanged(); } }
        public string NegativeBalance { get { return _NegativeBalance; } set { _NegativeBalance = value; NotifyPropertyChanged(); } }
        public string AvailableBalance { get { return _AvailableBalance; } set { _AvailableBalance = value; NotifyPropertyChanged(); } }
        public string AllBalance { get { return _AllBalance; } set { _AllBalance = value; NotifyPropertyChanged(); } }
        public string AllNegativeBalance { get { return _AllNegativeBalance; } set { _AllNegativeBalance = value; NotifyPropertyChanged(); } }
        public string AllAvailableBalance { get { return _AllAvailableBalance; } set { _AllAvailableBalance = value; NotifyPropertyChanged(); } }
        public string OweI { get { return _OweI; } set { _OweI = value; NotifyPropertyChanged(); } }
        public string OweMe { get { return _OweMe; } set { _OweMe = value; NotifyPropertyChanged(); } }
    }
}