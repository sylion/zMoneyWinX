using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Navigation;
using zMoneyWinX.Model;

namespace zMoneyWinX.View
{
    public sealed partial class PageTransactions : Page
    {
        public PageTransactions()
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Required;
            init();
        }

        private async void init()
        {
            Signals.invokeSyncStarted();
            await Window.Current.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                Transactions.Source = App.Provider.VMTransaction.GroupedTransactions;
                FilterTotals.DataContext = App.Provider.VMTransaction;
                FilterTotals.SetBinding(VisibilityProperty, new Binding() { Path = new PropertyPath("Filter"), Converter = new nullToVis() });
                Scheduled.Visibility = ReminderMarker.HasLatePayments() ? Visibility.Visible : Visibility.Collapsed;
            });
            Signals.invokeSyncEnded();
        }

        private async void TransactionsList_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is Transaction)
            {
                var item = (Transaction)e.ClickedItem;
                await Task.Delay(250);
                Signals.TransactionBeginEditInvoke(item);
            }
            else if (e.ClickedItem is ReminderMarker)
            {
                var item = (ReminderMarker)e.ClickedItem;
                await Task.Delay(250);
                Signals.TransactionBeginEditInvoke(item.Edit());
            }
        }

        private void FilterResetButton_Click(object sender, RoutedEventArgs e)
        {
            App.Provider.VMTransaction.Filter = null;
        }

        //Reminders
        ReminderMarker markeritem = null;
        Transaction transitem = null;
        private void Grid_Holding(object sender, Windows.UI.Xaml.Input.HoldingRoutedEventArgs e)
        {
            if (((FrameworkElement)sender).DataContext is ReminderMarker)
                markeritem = ((FrameworkElement)sender).DataContext as ReminderMarker;
            else
                markeritem = null;

            FlyoutBase.ShowAttachedFlyout(sender as FrameworkElement);
        }

        private void Grid_RightTapped(object sender, Windows.UI.Xaml.Input.RightTappedRoutedEventArgs e)
        {
            if (((FrameworkElement)sender).DataContext is ReminderMarker)
                markeritem = ((FrameworkElement)sender).DataContext as ReminderMarker;
            else
                markeritem = null;

            if (((FrameworkElement)sender).DataContext is Transaction)
                transitem = ((FrameworkElement)sender).DataContext as Transaction;
            else
                transitem = null;

            FlyoutBase.ShowAttachedFlyout(sender as FrameworkElement);
        }

        private async void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            if (markeritem == null && transitem == null)
                return;

            switch (((MenuFlyoutItem)sender).Tag.ToString())
            {
                case "Write":
                    App.Provider.VMTransaction.Insert(markeritem.Write());
                    App.Provider.VMTransaction.Delete(markeritem.Id);
                    return;
                case "Change":
                    Signals.TransactionBeginEditInvoke(markeritem.Edit());
                    return;
                case "Delete":
                    if (await Model.Extensions.ReallyDelete(Model.Extensions.ItemType.Transaction))
                    {
                        if (markeritem != null)
                        {
                            markeritem.Delete();
                            App.Provider.VMTransaction.Delete(markeritem.Id);
                        }
                        else
                            App.Provider.VMTransaction.Delete(transitem);
                    }
                    return;
                default:
                    break;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(((Button)sender).Tag.ToString() == "ScheduledShow")
            {
                Signals.ShowPlannedLateInvoke();
            }
            Scheduled.Visibility = Visibility.Collapsed;
        }
    }
}
