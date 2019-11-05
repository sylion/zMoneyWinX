using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using zMoneyWinX.Client;
using zMoneyWinX.Model;

namespace zMoneyWinX.View
{
    public sealed partial class PageMerchantsEdit : Page
    {
        private Merchant item { get; set; }
        private MerchantEdit editItem { get; set; }

        private bool Create = false;

        public PageMerchantsEdit(Merchant _item = null)
        {
            this.InitializeComponent();
            if (_item != null)
            {
                item = _item;
                CmdDelete.Visibility = Visibility.Visible;
            }
            else
            {
                item = new Merchant();
                Create = true;
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadProgress.Visibility = Visibility.Visible;
            editItem = new MerchantEdit(item);
            Root.DataContext = editItem;
            LoadProgress.Visibility = Visibility.Collapsed;
        }

        private async void CmdClick(object sender, RoutedEventArgs e)
        {
            switch (((AppBarButton)sender).Tag.ToString())
            {
                case "Save":
                    save();
                    return;
                case "Delete":
                    await delete();
                    return;
                case "Cancel":
                    Signals.CloseMeInvoke();
                    return;
                default:
                    break;
            }
        }

        private async Task delete()
        {
            if (await Model.Extensions.ReallyDelete(Model.Extensions.ItemType.Merchant))
            {
                LoadProgress.Visibility = Visibility.Visible;
                App.Provider.VMMerchant.Delete(item);
                Signals.CloseMeInvoke();
                LoadProgress.Visibility = Visibility.Collapsed;
            }
        }
        private async void save()
        {
            LoadProgress.Visibility = Visibility.Visible;
            if (!string.IsNullOrWhiteSpace(editItem.Title))
            {
                if (Create)
                    App.Provider.VMMerchant.Insert(Merchant.newMerchant(editItem.Title));
                else
                {
                    item.Title = editItem.Title;
                    item.isChanged = true;
                    item.Changed = SettingsManager.toUnixTime(DateTime.UtcNow);
                    await App.Provider.VMMerchant.Update(item);
                }
                Signals.CloseMeInvoke();
            }
            LoadProgress.Visibility = Visibility.Collapsed;
        }
    }
}
