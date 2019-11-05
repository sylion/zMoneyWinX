using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using zMoneyWinX.Model;
using zMoneyWinX.Client;
using System.Threading.Tasks;

namespace zMoneyWinX.ViewModel
{
    public class MerchantViewModel
    {
        private ObservableCollection<Merchant> _merchants = null;
        public static List<Merchant> MerhcantsList => Merchant.Merchants().ToList();
        public ObservableCollection<Merchant> Merchants
        {
            get
            {
                if (_merchants == null)
                    _merchants = Merchant.Merchants();
                return _merchants;
            }
            set
            {
                _merchants = value;
            }
        }

        public void Insert(Merchant Item)
        {
            DBObject.Insert(Item);
            Refresh();
        }
        public void Insert(List<Merchant> items)
        {
            DBObject.Insert(items);
            Refresh();
        }
        public void Delete(Merchant item)
        {
            Merchant res = Merchants.SingleOrDefault(i => i.Id == item.Id);
            Merchants.Remove(res);
            item.isDeleted = true;
            item.Changed = SettingsManager.toUnixTime(DateTime.UtcNow);
            DBObject.Insert(item);
        }
        public async Task Update(Merchant item)
        {
            DBObject.Insert(item);
            await App.Provider.VMTransaction.Refresh();
            Refresh();
        }
        public void Delete(Guid ID)
        {
            Merchant res = Merchants.SingleOrDefault(i => i.Id == ID);
            Merchants.Remove(res);
            DBObject.Delete(ID, typeof(Merchant));
        }
        private void Refresh()
        {
            Merchants.Clear();
            foreach (Merchant x in Merchant.Merchants())
                Merchants.Add(x);
        }
    }
}
