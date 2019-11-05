using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Graphics.Display;
using Windows.System;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media.Animation;
using zMoneyWinX.Client;
using zMoneyWinX.Model;
using zMoneyWinX.Presentation;
using zMoneyWinX.View;

namespace zMoneyWinX
{
    public sealed partial class Shell : Page
    {
        public Shell()
        {
            InitializeComponent();

            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
            {
                DisplayInformation.GetForCurrentView().OrientationChanged += Shell_OrientationChanged;
                Shell_OrientationChanged(DisplayInformation.GetForCurrentView(), new object());
            }

            initSlots();

            var vm = new ShellViewModel();

            vm.MenuItems.Add(new MenuItem { Icon = "", Title = ResourceLoader.GetForViewIndependentUse().GetString("PageTransactions"), PageType = typeof(PageTransactions) });
            vm.MenuItems.Add(new MenuItem { Icon = "", Title = ResourceLoader.GetForViewIndependentUse().GetString("PageAccounts"), PageType = typeof(PageAccounts) });
            vm.MenuItems.Add(new MenuItem { Icon = "", Title = ResourceLoader.GetForViewIndependentUse().GetString("PageBudget"), PageType = typeof(PageBudget) });
            vm.MenuItems.Add(new MenuItem { Icon = "", Title = ResourceLoader.GetForViewIndependentUse().GetString("PageReports"), PageType = typeof(PageReports) });
            vm.MenuItems.Add(new MenuItem { Icon = "", Title = ResourceLoader.GetForViewIndependentUse().GetString("PageTags"), PageType = typeof(PageTags) });
            vm.MenuItems.Add(new MenuItem { Icon = "", Title = ResourceLoader.GetForViewIndependentUse().GetString("PageMerchants"), PageType = typeof(PageMerchants) });
            vm.MenuItems.Add(new MenuItem { Icon = "", Title = ResourceLoader.GetForViewIndependentUse().GetString("PageCurrencies"), PageType = typeof(PageCurrencies) });
            vm.MenuItems.Add(new MenuItem { Icon = "", Title = ResourceLoader.GetForViewIndependentUse().GetString("PageSettings"), PageType = typeof(PageSettings) });
            vm.MenuItems.Add(new MenuItem { Icon = "", Title = ResourceLoader.GetForViewIndependentUse().GetString("PageAbout"), PageType = typeof(PageAbout) });

            // select the first menu item
            vm.SelectedMenuItem = vm.MenuItems.First();

            ViewModel = vm;

            // add entry animations
            var transitions = new TransitionCollection { };
            var transition = new NavigationThemeTransition { };
            transitions.Add(transition);
            ShellFrame.ContentTransitions = transitions;
            ShellFrame.Navigated += Frame_Navigated;
        }

        private void Signals_UpdateBalanceEvent()
        {
            Balance.SetBinding(TextBlock.TextProperty, new Binding() { Source = App.Provider.VMAccount.Balance });
        }

        private static bool Inititalized = false;
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Signals_UpdateBalanceEvent();

            #region RateUsDialog
            if (!Inititalized)
            {
                if (!SettingsManager.answer && SettingsManager.runs >= 5)
                {
                    var dialog = new Windows.UI.Popups.MessageDialog(ResourceLoader.GetForCurrentView().GetString("RateUsText"));

                    dialog.Commands.Add(new Windows.UI.Popups.UICommand(ResourceLoader.GetForCurrentView().GetString("RateUsYes")) { Id = 0 });
                    dialog.Commands.Add(new Windows.UI.Popups.UICommand(ResourceLoader.GetForCurrentView().GetString("RateUsNo")) { Id = 1 });

                    dialog.DefaultCommandIndex = 0;
                    dialog.CancelCommandIndex = 1;

                    var result = await dialog.ShowAsync();

                    if ((int)result.Id == 0)
                    {
                        await Launcher.LaunchUriAsync(new Uri(string.Format("ms-windows-store:REVIEW?PFN={0}", Windows.ApplicationModel.Package.Current.Id.FamilyName)));
                        SettingsManager.answer = true;
                        SettingsManager.runs = 0;
                    }
                    else
                        SettingsManager.runs = 0;
                }
                else if (!SettingsManager.answer)
                {
                    SettingsManager.runs++;
                }
                Inititalized = true;
            }
            #endregion       
        }

        private void Frame_Navigated(object sender, Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            if (ShellFrame.Content.GetType() == typeof(PageTransactions))
            {
                CmdAdd.Visibility = Visibility.Visible;
                CmdFilter.Visibility = Visibility.Visible;
                CmdUpdate.Visibility = Visibility.Visible;
            }
            if (ShellFrame.Content.GetType() == typeof(PageAccounts) ||
                ShellFrame.Content.GetType() == typeof(PageTags) ||
                ShellFrame.Content.GetType() == typeof(PageMerchants))
            {
                CmdAdd.Visibility = Visibility.Visible;
                CmdFilter.Visibility = Visibility.Collapsed;
                CmdUpdate.Visibility = Visibility.Visible;
            }
            if (ShellFrame.Content.GetType() == typeof(PageBudget) ||
                ShellFrame.Content.GetType() == typeof(PageCurrencies) ||
                ShellFrame.Content.GetType() == typeof(PageSettings) ||
                ShellFrame.Content.GetType() == typeof(PageAbout) ||
                ShellFrame.Content.GetType() == typeof(PageReports))
            {
                CmdAdd.Visibility = Visibility.Collapsed;
                CmdFilter.Visibility = Visibility.Collapsed;
                CmdUpdate.Visibility = Visibility.Visible;
            }
        }

        private void initSlots()
        {
            Signals.UpdateBalanceEvent -= Signals_UpdateBalanceEvent;
            Signals.UpdateBalanceEvent += Signals_UpdateBalanceEvent;

            Signals.SyncStartedEvent -= Signals_SyncStartedEvent;
            Signals.SyncStartedEvent += Signals_SyncStartedEvent;

            Signals.SyncEndedEvent -= Signals_SyncEndeEvent;
            Signals.SyncEndedEvent += Signals_SyncEndeEvent;

            Signals.NeedCloseEvent -= Signals_NeedCloseEvent;
            Signals.NeedCloseEvent += Signals_NeedCloseEvent;

            Signals.ConnErrEvent -= Signals_ConnErrEvent;
            Signals.ConnErrEvent += Signals_ConnErrEvent;
        }

        private async void Signals_ConnErrEvent()
        {
            ConnectionError.Visibility = Visibility.Visible;
            await Task.Delay(TimeSpan.FromSeconds(5));
            ConnectionError.Visibility = Visibility.Collapsed;
        }

        private async void Signals_NeedCloseEvent()
        {
            NeedClose.Visibility = Visibility.Visible;
            await Task.Delay(TimeSpan.FromSeconds(2));
            NeedClose.Visibility = Visibility.Collapsed;
        }

        private void Signals_SyncEndeEvent()
        {
            LoadProgress.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        private void Signals_SyncStartedEvent()
        {
            LoadProgress.Visibility = Windows.UI.Xaml.Visibility.Visible;
        }

        public ShellViewModel ViewModel { get; private set; }

        private async void Shell_OrientationChanged(DisplayInformation sender, object args)
        {
            var currentOrientation = DisplayInformation.GetForCurrentView().CurrentOrientation;
            var statusBar = StatusBar.GetForCurrentView();

            switch (currentOrientation)
            {
                case DisplayOrientations.Landscape:
                    await statusBar.HideAsync();
                    break;
                case DisplayOrientations.Portrait:
                    await statusBar.ShowAsync();
                    break;
                default:
                    break;
            }
        }

        public bool isPaneOpen
        {
            get
            {
                return SplitView.IsSwipeablePaneOpen;
            }
            set
            {
                SplitView.IsSwipeablePaneOpen = value;
            }
        }

        public bool autoclose
        {
            get
            {
                if (SplitView.DisplayMode == SplitViewDisplayMode.CompactInline)
                    return false;
                else
                    return true;
            }
        }

        public Frame RootFrame
        {
            get
            {
                return ShellFrame;
            }
        }

        private async void CmdClick(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            switch (((AppBarButton)sender).Tag.ToString())
            {
                case "Filter":
                    Signals.invokeFilter();
                    break;
                case "Update":
                    await Sync();
                    Signals_UpdateBalanceEvent();
                    break;
                case "Add":
                    if (RootFrame.Content.GetType() == typeof(PageTransactions))
                        Signals.TransactionBeginEditInvoke(null);
                    if (RootFrame.Content.GetType() == typeof(PageMerchants))
                        Signals.MerchantBeginEditInvoke(null);
                    if (RootFrame.Content.GetType() == typeof(PageTags))
                        Signals.TagBeginEditInvoke(null);
                    if (RootFrame.Content.GetType() == typeof(PageAccounts))
                        Signals.AccountBeginEditInvoke(null);
                    break;
                case "Planned":
                    Signals.ShowPlannedInvoke();
                    break;
                default:
                    break;
            }
        }

        private async Task Sync()
        {
            Signals.invokeSyncStarted();
            try
            {
                await Client.SyncManager.Sync();
            }
            catch { }
            Signals.invokeSyncEnded();
        }
    }
}
