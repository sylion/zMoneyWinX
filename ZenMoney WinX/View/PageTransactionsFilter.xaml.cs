using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using zMoneyWinX.Model;
using zMoneyWinX.ViewModel;

namespace zMoneyWinX.View
{
    public sealed partial class PageTransactionsFilter : Page
    {
        private TransactionFilterEdit editFilter;
        private List<Account> Accounts;
        private List<Tag> Tags;
        private List<Merchant> Merchants;
        public PageTransactionsFilter()
        {
            InitializeComponent();

            if (App.Provider.VMTransaction.Filter != null)
                editFilter = App.Provider.VMTransaction.Filter;
            else
                editFilter = new TransactionFilter();

            Accounts = AccountViewModel.Accounts.OrderBy(i => i.Archive).Select(i => { i.isChecked = false; return i; }).ToList();
            FilterAccounts.ItemsSource = Accounts;

            Tags = TagViewModel.Tags.OrderBy(i => i.Title).Select(i => { i.isChecked = false; return i; }).ToList();
            FilterTags.ItemsSource = Tags;

            if (editFilter.Account != null && editFilter.Account.Count > 0)
                foreach (var x in editFilter.Account)
                    Accounts.FirstOrDefault(i => editFilter.Account.Contains(i.Id)).isChecked = true;

            if (editFilter.Tags != null && editFilter.Tags.Count > 0)
                foreach (var x in editFilter.Tags)
                    Tags.FirstOrDefault(i => editFilter.Tags.Contains(i.Id)).isChecked = true;

            Merchants = MerchantViewModel.MerhcantsList;
            if (editFilter.Merchant != null && editFilter.Merchant != Guid.Empty)
            {
                try
                {
                    FilterMerchant.Text = Merchants.FirstOrDefault(i => i.Id == editFilter.Merchant).Title;
                }
                catch { }
            }

            Root.DataContext = editFilter;
        }
        private void CmdClick(object sender, RoutedEventArgs e)
        {
            switch (((AppBarButton)sender).Tag.ToString())
            {
                case "Save":
                    save();
                    return;
                case "Cancel":
                    Signals.CloseMeInvoke();
                    return;
                default:
                    break;
            }
        }
        private void save()
        {
            editFilter.Account = Accounts.Where(i => i.isChecked).Select(i => i.Id).ToList();
            editFilter.Tags = Tags.Where(i => i.isChecked).Select(i => i.Id).ToList();
            if (!string.IsNullOrWhiteSpace(FilterMerchant.Text))
            {
                try
                {
                    editFilter.Merchant = Merchants.FirstOrDefault(i => i.Title == FilterMerchant.Text).Id;
                }
                catch { }
            }
            else
            {
                editFilter.Merchant = null;
            }
            App.Provider.VMTransaction.Filter = (TransactionFilter)editFilter;
            Signals.CloseMeInvoke();
        }

        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (editFilter != null)
                switch (editFilter.DatePeriod)
                {
                    case TransactionPeriod.Today:
                        editFilter.DateFrom = DateTime.Now.Date;
                        editFilter.DateTo = DateTime.Now.Date;
                        break;
                    case TransactionPeriod.Yesterday:
                        editFilter.DateFrom = DateTime.Now.Date.AddDays(-1);
                        editFilter.DateTo = DateTime.Now.Date.AddDays(-1);
                        break;
                    case TransactionPeriod.ThisWeek:
                        editFilter.DateFrom = DateTime.Today.AddDays(-1 * (int)(DateTime.Today.DayOfWeek) + 1).Date;
                        editFilter.DateTo = DateTime.Today.AddDays(-1 * ((int)(DateTime.Today.DayOfWeek) - 7) + 1).Date.AddSeconds(-1);
                        break;
                    case TransactionPeriod.ThisMonth:
                        editFilter.DateFrom = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).Date;
                        editFilter.DateTo = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).Date.AddMonths(1).AddSeconds(-1);
                        break;
                    default:
                        break;
                }
        }

        private void FilterAccounts_ItemClick(object sender, ItemClickEventArgs e)
        {
            ((Account)e.ClickedItem).isChecked = !((Account)e.ClickedItem).isChecked;
        }

        private void FilterTags_ItemClick(object sender, ItemClickEventArgs e)
        {
            ((Tag)e.ClickedItem).isChecked = !((Tag)e.ClickedItem).isChecked;
        }

        private void Merchant_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            var source = new List<Merchant>();
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                source = Merchants.FindAll(i => i.Title.ToLower().StartsWith(sender.Text.ToLower()));
                if (source != null && source.Count > 0)
                    sender.ItemsSource = source;
                else
                    sender.ItemsSource = null;
            }
        }

        private void CalendarDatePickerStart_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
        {
            if (editFilter == null)
                return;

            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
            {
                if (args.NewDate == args.OldDate)
                    return;

                if (args.NewDate != null)
                    editFilter.DateFrom = ((DateTimeOffset)args.NewDate).DateTime.Date;
                else if (args.OldDate != null)
                    editFilter.DateFrom = ((DateTimeOffset)args.OldDate).DateTime.Date;
                else
                    editFilter.DateFrom = DateTime.Now.Date;
            }
        }
        private void CalendarDatePickerEnd_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
        {
            if (editFilter == null)
                return;

            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
            {
                if (args.NewDate == args.OldDate)
                    return;

                if (args.NewDate != null)
                    editFilter.DateTo = ((DateTimeOffset)args.NewDate).DateTime;
                else if (args.OldDate != null)
                    editFilter.DateTo = ((DateTimeOffset)args.OldDate).DateTime;
                else
                    editFilter.DateTo = DateTime.Now.Date;
            }
        }
    }
}
