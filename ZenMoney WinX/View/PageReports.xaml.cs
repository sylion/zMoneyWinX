using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using zMoneyWinX.Model;
using zMoneyWinX.Reports;

namespace zMoneyWinX.View
{
    public sealed partial class PageReports : Page
    {
        private static bool loaded = false;
        public CategoryReport categoryReport = new CategoryReport();
        public IncomeDistributionReport DistributionReport = new IncomeDistributionReport();
        public SpendingsCalendarReport CalendarReport = new SpendingsCalendarReport();
        public PageReports()
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Required;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await RefreshDistributionReport();
            if (!loaded)
                await init();
            loaded = true;
        }

        private async Task init()
        {
            IncomeDistributionReportPanel.DataContext = DistributionReport.Report;
            CategoryReportPanel.DataContext = categoryReport;
            CalendarReportPanel.DataContext = CalendarReport;
            await RefreshCategoryReport();
            await RefreshCalendarReport();
        }

        private async Task RefreshCategoryReport()
        {
            Signals.invokeSyncStarted();
            await categoryReport.Refresh();
            CategoryReportChart.ItemsSource = categoryReport.Report;
            Signals.invokeSyncEnded();
        }

        private async Task RefreshDistributionReport()
        {
            Signals.invokeSyncStarted();
            await DistributionReport.Refresh();
            IncomeDistributionMessages.ItemsSource = DistributionReport.Report.Msg;
            Signals.invokeSyncEnded();
        }

        private async Task RefreshCalendarReport()
        {
            Signals.invokeSyncStarted();
            await CalendarReport.Refresh();
            CalendarReportChart.ItemsSource = CalendarReport.Report;
            Signals.invokeSyncEnded();
        }

        private async void btnNext_Click(object sender, RoutedEventArgs e)
        {
            categoryReport.NextPeriod();
            await RefreshCategoryReport();
        }

        private async void btnPrev_Click(object sender, RoutedEventArgs e)
        {
            categoryReport.PrevPeriod();
            await RefreshCategoryReport();
        }

        private async void CategoryReportBtnMode_Click(object sender, RoutedEventArgs e)
        {
            var s = sender as ToggleMenuFlyoutItem;
            switch (s.Tag as string)
            {
                case "Collapse":
                    CategoryReportBtnCollapsed.IsChecked = true;
                    CategoryReportBtnExpanded.IsChecked = false;
                    categoryReport.Expanded = false;
                    break;
                case "Expand":
                    CategoryReportBtnCollapsed.IsChecked = false;
                    CategoryReportBtnExpanded.IsChecked = true;
                    categoryReport.Expanded = true;
                    break;

                case "Outcome":
                    CategoryReportBtnOutcome.IsChecked = true;
                    CategoryReportBtnIncome.IsChecked = false;
                    categoryReport.transType = TransactionType.Outcome;
                    break;
                case "Income":
                    CategoryReportBtnOutcome.IsChecked = false;
                    CategoryReportBtnIncome.IsChecked = true;
                    categoryReport.transType = TransactionType.Income;
                    break;

                default:
                    break;
            }
            await RefreshCategoryReport();
        }

        private void IncomeDistrSettings_Click(object sender, RoutedEventArgs e)
        {
            Signals.IncomeDistrSettingsInvoke();
        }

        private void CategoryReportChart_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = (CategoryReportModel)e.ClickedItem;

            TransactionTypeFilter transtype = TransactionTypeFilter.Outcome;
            if (categoryReport.transType == TransactionType.Outcome)
                transtype = TransactionTypeFilter.Outcome;
            if (categoryReport.transType == TransactionType.Income)
                transtype = TransactionTypeFilter.Income;

            if (item.TagId != null && item.TagId != Guid.Empty)
            {
                var filterData = new System.Collections.Generic.List<Guid>();
                if (categoryReport.Expanded)
                    filterData = Model.Tag.Tags.Where(i => i.Id == item.TagId).Select(i => i.Id).ToList();
                else
                    filterData = Model.Tag.Tags.Where(i => i.Id == item.TagId || i.Parent == item.TagId).Select(i => i.Id).ToList();
                App.Provider.VMTransaction.Filter = new TransactionFilter() { Tags = filterData, TransType = transtype, DatePeriod = TransactionPeriod.Period, DateFrom = categoryReport.GetDateFrom, DateTo = categoryReport.GetDateTo };
            }
            else
                App.Provider.VMTransaction.Filter = new TransactionFilter() { TransType = transtype, DatePeriod = TransactionPeriod.Period, DateFrom = categoryReport.GetDateFrom, DateTo = categoryReport.GetDateTo };

            App.FilterFromReport = true;

            App.shell.ViewModel.SelectedMenuItem = App.shell.ViewModel.MenuItems.First();
        }

        private async void btnCalendarPrev_Click(object sender, RoutedEventArgs e)
        {
            CalendarReport.PrevPeriod();
            await RefreshCalendarReport();
        }

        private async void btnCalendarNext_Click(object sender, RoutedEventArgs e)
        {
            CalendarReport.NextPeriod();
            await RefreshCalendarReport();
        }

        private void CalendarItem_Click(object sender, RoutedEventArgs e)
        {
            var item = (SpendingsCalendarModel)((Button)sender).DataContext;
            if (!item.inCurrentMonth)
                return;

            if (!item.isPlanned && item.Sum != 0)
            {
                App.Provider.VMTransaction.Filter = new TransactionFilter()
                {
                    TransType = TransactionTypeFilter.Outcome,
                    DatePeriod = TransactionPeriod.Period,
                    DateFrom = item.CellDate,
                    DateTo = item.CellDate
                };
                App.FilterFromReport = true;
                App.shell.ViewModel.SelectedMenuItem = App.shell.ViewModel.MenuItems.First();
            }
        }
    }
}

