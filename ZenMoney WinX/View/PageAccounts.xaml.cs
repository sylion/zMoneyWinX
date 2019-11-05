using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using zMoneyWinX.Client;
using zMoneyWinX.Model;
using zMoneyWinX.ViewModel;

namespace zMoneyWinX.View
{
    public sealed partial class PageAccounts : Page
    {
        private static bool loaded = false;
        public PageAccounts()
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Required;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!loaded)
                await init();
            loaded = true;
        }

        public bool CanGoBack()
        {
            if (AccountList.Visibility == Visibility.Visible)
                return true;

            AccountList.Visibility = Visibility.Visible;
            AccountBalance.Visibility = Visibility.Collapsed;
            return false;
        }

        private async Task init()
        {
            Signals.invokeSyncStarted();
            await Window.Current.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                Accounts.Source = App.Provider.VMAccount.GroupedAccounts;
            });
            DisplayMode.DataContext = new SettingsManager();
            Signals.invokeSyncEnded();
        }

        private void AccountsList_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = (Account)e.ClickedItem;
            if (item.Id != null && item.Id != Guid.Empty)
            {
                var filterData = new System.Collections.Generic.List<Guid>();
                filterData.Add(item.Id);
                App.Provider.VMTransaction.Filter = new TransactionFilter() { Account = filterData };

                App.FilterFromReport = true;
                
                App.shell.ViewModel.SelectedMenuItem = App.shell.ViewModel.MenuItems.First();
            }
        }
        private async void BalanceButton_Click(object sender, RoutedEventArgs e)
        {
            Signals.invokeSyncStarted();
            if (AccountList.Visibility == Visibility.Visible)
                AccountList.Visibility = Visibility.Collapsed;
            else
                AccountList.Visibility = Visibility.Visible;

            if (AccountBalance.Visibility == Visibility.Visible)
                AccountBalance.Visibility = Visibility.Collapsed;
            else
                AccountBalance.Visibility = Visibility.Visible;

            if (AccountBalance.Visibility == Visibility.Visible)
            {
                AccountBalance.DataContext = await AccountBalanceHelper.Report();
                var Report = await Reports.AccountCurrencyReport.Report();
                if (Report.Count > 1)
                {
                    CurrencyReportContainer.ItemsSource = Report;
                    CurrencyReportContainer.Visibility = Visibility.Visible;
                    CurrencyReportLabel.Visibility = Visibility.Visible;
                }
                else
                {
                    CurrencyReportContainer.Visibility = Visibility.Collapsed;
                    CurrencyReportLabel.Visibility = Visibility.Collapsed;
                }
            }
            Signals.invokeSyncEnded();
        }
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var item = (Account)((Button)e.OriginalSource).DataContext;
            Signals.AccountBeginEditInvoke(item);
        }
    }
}
