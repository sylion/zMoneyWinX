using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using zMoneyWinX.Model;
using zMoneyWinX.ViewModel;

namespace zMoneyWinX.View
{
    public sealed partial class PageBudget : Page
    {
        BudgetViewModel vmbudget = new BudgetViewModel();
        public PageBudget()
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Required;
            Root.DataContext = vmbudget;
            BudgetIncomeList.ItemsSource = vmbudget.budgetIncome;
            BudgetOutcomeList.ItemsSource = vmbudget.budgetOutcome;
            BudgetTags.ItemsSource = vmbudget.budgetTags;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await Refresh();
        }

        public async Task Refresh()
        {
            Signals.invokeSyncStarted();
            await vmbudget.Refresh();
            Signals.invokeSyncEnded();
        }

        private async void btnPrev_Click(object sender, RoutedEventArgs e)
        {
            if (vmbudget.PrevMonth())
                await Refresh();
        }

        private async void btnNext_Click(object sender, RoutedEventArgs e)
        {
            if (vmbudget.NextMonth())
                await Refresh();
        }

        private async void BudgetIncomeList_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = (Budget)e.ClickedItem;
            await Task.Delay(250);
            Signals.BudgetInBeginEditInvoke(item);
        }

        private async void BudgetOutcomeList_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = (Budget)e.ClickedItem;
            await Task.Delay(250);
            Signals.BudgetOutBeginEditInvoke(item);
        }

        private async void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(vmbudget.NeedUpdate)
            {
                vmbudget.NeedUpdate = false;
                await Refresh();
            }
        }
    }
}
