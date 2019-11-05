using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;
using zMoneyWinX.Client;
using zMoneyWinX.Model;
using zMoneyWinX.ViewModel;

namespace zMoneyWinX
{
    sealed partial class App : Application
    {
        public static Shell shell { get; private set; }

        private static View.PagePIN pin;

        public static bool FilterFromReport { get; set; } = false;

        private static View.PageAuth auth { get; set; }

        public static DataProvider Provider { get; private set; } = new DataProvider();

        //Secondary Tile ID
        public const string SecondTileTileId = "zMoneyQuickAdd";
        public static bool StartdedForQuickAdd { get; private set; } = false;

        public static string Error { get; set; }
        public static async System.Threading.Tasks.Task showError()
        {
            Windows.ApplicationModel.DataTransfer.DataPackage dataPackage = new Windows.ApplicationModel.DataTransfer.DataPackage();
            dataPackage.SetText(App.Error);
            Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dataPackage);
            await new Windows.UI.Popups.MessageDialog("Some critical error occured.\nError description and tracing data was copied to the clipboard. Send it to the developer...", "Error").ShowAsync();
            Error = string.Empty;
        }

        public static bool AutoClose()
        {
            try
            {
                return shell.autoclose;
            }
            catch
            {
                return true;
            }
        }

        public App()
        {
            Microsoft.ApplicationInsights.WindowsAppInitializer.InitializeAsync(
                Microsoft.ApplicationInsights.WindowsCollectors.Metadata |
                Microsoft.ApplicationInsights.WindowsCollectors.Session);
            InitializeComponent();
            Suspending += OnSuspending;

            Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = SettingsManager.languageID;
            initSlots();
        }

        private void initSlots()
        {
            Signals.AuthOKEvent -= Signals_AuthOKEvent;
            Signals.AuthOKEvent += Signals_AuthOKEvent;

            Signals.PinOkEvent -= Signals_PinOkEvent;
            Signals.PinOkEvent += Signals_PinOkEvent;

            Signals.SetupPINEvent -= Signals_SetupPINEvent;
            Signals.SetupPINEvent += Signals_SetupPINEvent;

            Signals.TransactionBeginEditEvent -= Signals_TransactionEditEvent;
            Signals.TransactionBeginEditEvent += Signals_TransactionEditEvent;

            Signals.MerchantBeginEditEvent -= Signals_MerchantBeginEditEvent;
            Signals.MerchantBeginEditEvent += Signals_MerchantBeginEditEvent;

            Signals.AccountBeginEditEvent -= Signals_AccountBeginEditEvent;
            Signals.AccountBeginEditEvent += Signals_AccountBeginEditEvent;

            Signals.TagBeginEditEvent -= Signals_TagBeginEditEvent;
            Signals.TagBeginEditEvent += Signals_TagBeginEditEvent;

            Signals.FilterEvent -= Signals_FilterEvent;
            Signals.FilterEvent += Signals_FilterEvent;

            Signals.CloseMeEvent -= Signals_CloseMeEvent;
            Signals.CloseMeEvent += Signals_CloseMeEvent;

            Signals.LogOutEvent -= Signals_LogOutEvent;
            Signals.LogOutEvent += Signals_LogOutEvent;

            Signals.IncomeDistrSettingsEvent -= Signals_IncomeDistrSettingsEvent;
            Signals.IncomeDistrSettingsEvent += Signals_IncomeDistrSettingsEvent;

            Signals.BudgetOutBeginEditEvent -= Signals_BudgetOutBeginEditEvent;
            Signals.BudgetOutBeginEditEvent += Signals_BudgetOutBeginEditEvent;

            Signals.BudgetInBeginEditEvent -= Signals_BudgetInBeginEditEvent;
            Signals.BudgetInBeginEditEvent += Signals_BudgetInBeginEditEvent;

            Signals.ShowPlannedEvent -= Signals_ShowPlannedEvent;
            Signals.ShowPlannedEvent += Signals_ShowPlannedEvent;

            Signals.ShowPlannedGuidEvent -= Signals_ShowPlannedGuidEvent;
            Signals.ShowPlannedGuidEvent += Signals_ShowPlannedGuidEvent;

            Signals.ShowPlannedLateEvent -= Signals_ShowPlannedLateEvent;
            Signals.ShowPlannedLateEvent += Signals_ShowPlannedLateEvent;
        }

        #region Slots
        private void Signals_ShowPlannedLateEvent(bool item)
        {
            Window.Current.Content = new View.PageTransactionsPlanned(item);
            UpdateBackButtonVisibility();
        }

        private void Signals_ShowPlannedGuidEvent(Guid item)
        {
            Window.Current.Content = new View.PageTransactionsPlanned(item);
            UpdateBackButtonVisibility();
        }

        private void Signals_ShowPlannedEvent()
        {
            Window.Current.Content = new View.PageTransactionsPlanned();
            UpdateBackButtonVisibility();
        }

        private void Signals_IncomeDistrSettingsEvent()
        {
            Window.Current.Content = new View.PageIncomeDistributionSettings();
            UpdateBackButtonVisibility();
        }

        private void Signals_FilterEvent()
        {
            Window.Current.Content = new View.PageTransactionsFilter();
            UpdateBackButtonVisibility();
        }

        private void Signals_AccountBeginEditEvent(Account item)
        {
            Window.Current.Content = new View.PageAccountsEdit(item);
            UpdateBackButtonVisibility();
        }

        private void Signals_TagBeginEditEvent(Tag item)
        {
            Window.Current.Content = new View.PageTagsEdit(item);
            UpdateBackButtonVisibility();
        }
        private void Signals_MerchantBeginEditEvent(Merchant item)
        {
            Window.Current.Content = new View.PageMerchantsEdit(item);
            UpdateBackButtonVisibility();
        }

        private void Signals_TransactionEditEvent(Transaction item)
        {
            Window.Current.Content = new View.PageTransactionsEdit(item);
            UpdateBackButtonVisibility();
        }

        private void Signals_BudgetInBeginEditEvent(Budget item)
        {
            Window.Current.Content = new View.PageBudgetEdit(item, TransactionType.Income);
            UpdateBackButtonVisibility();
        }

        private void Signals_BudgetOutBeginEditEvent(Budget item)
        {
            Window.Current.Content = new View.PageBudgetEdit(item, TransactionType.Outcome);
            UpdateBackButtonVisibility();
        }

        private void Signals_CloseMeEvent()
        {
            if (StartdedForQuickAdd)
            {
                Current.Exit();
                return;
            }

            Window.Current.Content = shell;
            UpdateBackButtonVisibility();
        }

        private void Signals_SetupPINEvent()
        {
            pin = new View.PagePIN(true);
            Window.Current.Content = pin;
            UpdateBackButtonVisibility();
        }

        private void Signals_PinOkEvent()
        {
            if (StartdedForQuickAdd)
            {
                Window.Current.Content = new View.PageTransactionsEdit();
                UpdateBackButtonVisibility();
                return;
            }
            if (shell == null)
            {
                shell = new Shell();
                shell.RootFrame.NavigationFailed -= OnNavigationFailed;
                shell.RootFrame.NavigationFailed += OnNavigationFailed;
                shell.RootFrame.Navigated -= OnNavigated;
                shell.RootFrame.Navigated += OnNavigated;
            }
            Window.Current.Content = shell;
            UpdateBackButtonVisibility();
        }

        private void Signals_AuthOKEvent()
        {
            if (!StartdedForQuickAdd && shell == null)
            {
                shell = new Shell();
                shell.RootFrame.NavigationFailed -= OnNavigationFailed;
                shell.RootFrame.NavigationFailed += OnNavigationFailed;
                shell.RootFrame.Navigated -= OnNavigated;
                shell.RootFrame.Navigated += OnNavigated;
            }

            if (!string.IsNullOrEmpty(SettingsManager.PIN))
            {
                pin = new View.PagePIN();
                Window.Current.Content = pin;
                UpdateBackButtonVisibility();
            }
            else
            {
                Window.Current.Content = shell;
                UpdateBackButtonVisibility();
            }
        }

        private void Signals_LogOutEvent()
        {
            shell.RootFrame.NavigationFailed -= OnNavigationFailed;
            shell.RootFrame.Navigated -= OnNavigated;

            SettingsManager.deleteCredentials();
            DBInitializer.DropDB();
            Provider = new DataProvider();
            auth = new View.PageAuth();
            shell = new Shell();
            Window.Current.Content = auth;

            shell.RootFrame.NavigationFailed += OnNavigationFailed;
            shell.RootFrame.Navigated += OnNavigated;
            UpdateBackButtonVisibility();
        }
        #endregion

        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            if (e.PrelaunchActivated)
                return;

            if (Window.Current.Content != null)
                return;

            if (e.Arguments == SecondTileTileId)
                StartdedForQuickAdd = true;

            //SY Set statusbar color for mobile devices
            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
            {
                var statusBar = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();
                statusBar.BackgroundColor = Windows.UI.Color.FromArgb(0xFF, 0xF0, 0x41, 0x37);
                statusBar.ForegroundColor = Windows.UI.Colors.White;
                statusBar.BackgroundOpacity = 1;
            }
            //SY Set titleBar color for desktop devices
            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Desktop")
            {
                var titleBar = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar;
                if (titleBar != null)
                {
                    titleBar.ButtonBackgroundColor = Windows.UI.Color.FromArgb(0xFF, 0xF0, 0x41, 0x37);
                    titleBar.ButtonForegroundColor = Windows.UI.Colors.White;
                    titleBar.BackgroundColor = Windows.UI.Color.FromArgb(0xFF, 0xF0, 0x41, 0x37);
                    titleBar.ForegroundColor = Windows.UI.Colors.White;
                }
            }

            Window.Current.VisibilityChanged += Current_VisibilityChanged;
            SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;

            auth = new View.PageAuth();
            Window.Current.Content = auth;

            Window.Current.Activate();
            UpdateBackButtonVisibility();
        }

        private void Current_VisibilityChanged(object sender, VisibilityChangedEventArgs e)
        {
            if (!StartdedForQuickAdd && !e.Visible && !string.IsNullOrEmpty(SettingsManager.PIN))
            {
                pin = new View.PagePIN();
                Window.Current.Content = pin;
                UpdateBackButtonVisibility();
            }
        }

        // handle software back button press
        private DateTime needClose = DateTime.Now.AddSeconds(-10);
        void OnBackRequested(object sender, BackRequestedEventArgs e)
        {
            if (StartdedForQuickAdd)
            {
                Current.Exit();
                return;
            }

            if (Window.Current.Content.GetType() == typeof(View.PageTagsEdit) &&
                !((View.PageTagsEdit)Window.Current.Content).CanGoBack())
            {
                e.Handled = true;
                return;
            }

            if (Window.Current.Content.GetType() == typeof(View.PageTransactionsPlanned) &&
                !((View.PageTransactionsPlanned)Window.Current.Content).CanGoBack())
            {
                e.Handled = true;
                return;
            }

            if (Window.Current.Content.GetType() == typeof(Shell))
            {
                var shell = (Shell)Window.Current.Content;

                if (shell.RootFrame.Content.GetType() == typeof(View.PageAccounts) &&
                    !((View.PageAccounts)shell.RootFrame.Content).CanGoBack())
                {
                    e.Handled = true;
                    return;
                }

                if (AutoClose() && shell.isPaneOpen)
                {
                    shell.isPaneOpen = false;
                    e.Handled = true;
                    return;
                }
                if (shell.RootFrame.Content.GetType() == typeof(View.PageTransactions) && Provider.VMTransaction.Filter != null)
                {
                    Provider.VMTransaction.Filter = null;
                    if (!FilterFromReport)
                    {
                        e.Handled = true;
                        return;
                    }
                    FilterFromReport = false;
                }
                if (shell.RootFrame.CanGoBack)
                {
                    e.Handled = true;
                    shell.RootFrame.GoBack();
                    return;
                }
            }
            else if (Window.Current.Content.GetType() != typeof(View.PageAuth) && Window.Current.Content.GetType() != typeof(View.PagePIN))
            {
                e.Handled = true;
                Window.Current.Content = shell;
                UpdateBackButtonVisibility();
                return;
            }

            if (needClose.Hour == DateTime.Now.Hour && needClose.Minute == DateTime.Now.Minute && DateTime.Now.Second - needClose.Second <= 2)
                Application.Current.Exit();
            else
            {
                needClose = DateTime.Now;
                Signals.invokeNeedClose();
                e.Handled = true;
                return;
            }
        }

        void OnNavigated(object sender, NavigationEventArgs e)
        {
            UpdateBackButtonVisibility();
        }

        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            deferral.Complete();
        }

        private void UpdateBackButtonVisibility()
        {
            if (StartdedForQuickAdd)
            {
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
                return;
            }

            if (Window.Current.Content.GetType() == typeof(Shell))
            {
                var shell = (Shell)Window.Current.Content;
                var visibility = AppViewBackButtonVisibility.Collapsed;
                if (shell.RootFrame.CanGoBack)
                    visibility = AppViewBackButtonVisibility.Visible;
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = visibility;
            }
            else if (Window.Current.Content.GetType() != typeof(View.PageAuth) && Window.Current.Content.GetType() != typeof(View.PagePIN))
            {
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            }
            else
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
        }
    }
}
