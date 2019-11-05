using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Navigation;
using zMoneyWinX.Model;
using zMoneyWinX.ViewModel;

namespace zMoneyWinX.View
{
    public sealed partial class PageTransactionsPlanned : Page
    {
        Guid Id = Guid.Empty;
        bool Late = false;
        public PageTransactionsPlanned()
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Required;
        }

        public PageTransactionsPlanned(Guid Id)
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Required;
            this.Id = Id;
        }

        public PageTransactionsPlanned(bool ShowLate)
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Required;
            Late = ShowLate;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            init();
        }

        ScheduledOperationsHelper helper = new ScheduledOperationsHelper();
        private async void init()
        {
            Signals.invokeSyncStarted();
            await Window.Current.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                if (Id == Guid.Empty && !Late)
                    helper.init();
                else if (Late)
                    helper.init(true);
                else
                    helper.init(Id);
                DataContext = helper;
                Transactions.Source = helper.Operations;
            });

            CmdSettings.Visibility = Late ? Visibility.Collapsed : Visibility.Visible;
            Signals.invokeSyncEnded();
        }

        private void CmdClick(object sender, RoutedEventArgs e)
        {
            switch (((AppBarButton)sender).Tag.ToString())
            {
                case "Cancel":
                    Signals.CloseMeInvoke();
                    return;
                default:
                    break;
            }
        }

        private void TransactionsList_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (((ReminderMarker)e.ClickedItem).ShowPlannedMenu == Visibility.Visible)
                Signals.TransactionBeginEditInvoke(((ReminderMarker)e.ClickedItem).Edit());
        }

        ReminderMarker item = null;
        private void Grid_Holding(object sender, Windows.UI.Xaml.Input.HoldingRoutedEventArgs e)
        {
            item = ((FrameworkElement)sender).DataContext as ReminderMarker;
            FlyoutBase.ShowAttachedFlyout(sender as FrameworkElement);
        }

        private void Grid_RightTapped(object sender, Windows.UI.Xaml.Input.RightTappedRoutedEventArgs e)
        {
            item = ((FrameworkElement)sender).DataContext as ReminderMarker;
            FlyoutBase.ShowAttachedFlyout(sender as FrameworkElement);
        }

        public bool CanGoBack()
        {
            if (Id == Guid.Empty)
                return true;
            else
            {
                Id = Guid.Empty;
                init();
                return false;
            }
        }

        private async void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            if (item == null)
                return;

            switch (((MenuFlyoutItem)sender).Tag.ToString())
            {
                case "Write":
                    App.Provider.VMTransaction.Insert(item.Write());
                    init();
                    return;
                case "Change":
                    Signals.TransactionBeginEditInvoke(item.Edit());
                    return;
                case "Delete":
                    if (await Model.Extensions.ReallyDelete(Model.Extensions.ItemType.Transaction))
                    {
                        App.Provider.VMTransaction.DeleteMarker(item.Id);
                        item.Delete();
                    }
                    init();
                    return;
                case "DeleteChain":
                    if (await Model.Extensions.ReallyDelete(Model.Extensions.ItemType.Transaction))
                    {
                        App.Provider.VMTransaction.DeleteMarkerChain(item.Reminder);
                        Reminder.Delete(item.Reminder);
                    }
                    init();
                    return;
                case "ShowChain":
                    Id = item.Reminder;
                    Late = false;
                    CmdSettings.Visibility = Visibility.Visible;
                    init();
                    return;
                default:
                    break;
            }
        }
    }
}
