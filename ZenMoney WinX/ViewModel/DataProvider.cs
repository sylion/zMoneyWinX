using System.Collections.Generic;
using System.Threading.Tasks;
using zMoneyWinX.Model;

namespace zMoneyWinX.ViewModel
{
    public class DataProvider
    {
        public DataProvider()
        {
            VMTransaction = new TransactionViewModel();
            VMTag = new TagViewModel();
            VMMerchant = new MerchantViewModel();
            VMAccount = new AccountViewModel();
        }

        public async Task Init()
        {
            await VMTransaction.Init();
            await VMAccount.Init();
            //Init accounts page to avoid UI freezes when navigate
            new View.PageAccounts();
        }
        public TransactionViewModel VMTransaction = new TransactionViewModel();
        public TagViewModel VMTag = new TagViewModel();
        public MerchantViewModel VMMerchant = new MerchantViewModel();
        public AccountViewModel VMAccount = new AccountViewModel();

        public void Deletions(List<Deletion> items)
        {
            foreach (Deletion item in items)
            {
                switch (item.Object.ToLower())
                {
                    case "transaction":
                        VMTransaction.Delete(item.Id);
                        break;
                    case "account":
                        VMAccount.Delete(item.Id);
                        break;
                    case "tag":
                        VMTag.Delete(item.Id);
                        break;
                    case "merchant":
                        VMMerchant.Delete(item.Id);
                        break;
                    case "reminder":
                        Reminder.DeletePermament(item.Id);
                        break;
                    default:
                        break;
                }
            }
        }
    }
}