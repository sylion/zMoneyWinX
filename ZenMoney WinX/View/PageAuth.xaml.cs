using System;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using zMoneyWinX.Client;
using zMoneyWinX.Model;

namespace zMoneyWinX.View
{
    public sealed partial class PageAuth : Page
    {
        private AccessToken token;
        private bool loaded = false;
        public PageAuth()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Window.Current.CoreWindow.KeyDown -= CoreWindow_KeyDown;
            Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown;

            Signals.NeedCloseEvent -= Signals_NeedCloseEvent;
            Signals.NeedCloseEvent += Signals_NeedCloseEvent;

            if (!loaded)
            {
                CheckDBVersion();
                loaded = true;
            }
        }

        private async void CheckDBVersion()
        {
            try
            {
                if (!File.Exists(DBInitializer.DB_PATH))
                    SettingsManager.lastsynctime = 0;
            }
            catch
            {
                SettingsManager.lastsynctime = 0;
            }


            foreach (var item in DBInitializer.OldDBs)
            {
                try
                {
                    var filename = Path.Combine(ApplicationData.Current.LocalFolder.Path, item);
                    if (File.Exists(filename))
                    {
                        var store = await ApplicationData.Current.LocalFolder.GetFileAsync(item);
                        await store.DeleteAsync();
                    }
                }
                catch { }
            }
        }

        private async void Signals_NeedCloseEvent()
        {
            NeedClose.Visibility = Visibility.Visible;
            await Task.Delay(TimeSpan.FromSeconds(2));
            NeedClose.Visibility = Visibility.Collapsed;
        }

        private void CoreWindow_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
            switch (args.VirtualKey)
            {
                case VirtualKey.Enter:
                    if (usr.Text.Trim() != "" && pwd.Password.Trim() != "")
                        btnLogin_Click(this, new RoutedEventArgs());
                    else if (usr.Text.Trim() != "")
                        usr.Focus(FocusState.Programmatic);
                    else if (pwd.Password.Trim() != "")
                        pwd.Focus(FocusState.Programmatic);
                    break;
                default:
                    break;
            }
        }

        private async void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            if (usr.Text.Trim() == "")
            {
                usr.Focus(FocusState.Programmatic);
                return;
            }
            if (pwd.Password.Trim() == "")
            {
                pwd.Focus(FocusState.Programmatic);
                return;
            }

            invertUI();

            LogErr.Visibility = Visibility.Collapsed;
            ConnErr.Visibility = Visibility.Collapsed;
            try
            {
                token = await OAuth.getToken(usr.Text, pwd.Password);
            }
            catch (UnauthorizedAccessException)
            {
                LogErr.Visibility = Visibility.Visible;
                invertUI();
                pwd.Focus(FocusState.Programmatic);
                return;
            }
            catch
            {
                ConnErr.Visibility = Visibility.Visible;
                invertUI();
                return;
            }

            SettingsManager.saveCredentials(usr.Text, pwd.Password, token);

            if (await SyncNow())
            {
                var dialog = new MessageDialog(ResourceLoader.GetForViewIndependentUse().GetString("DialogSetupPin"));
                dialog.Commands.Add(new UICommand(ResourceLoader.GetForViewIndependentUse().GetString("DialogYes")) { Id = 0 });
                dialog.Commands.Add(new UICommand(ResourceLoader.GetForViewIndependentUse().GetString("DialogNo")) { Id = 1 });
                dialog.DefaultCommandIndex = 0;
                dialog.CancelCommandIndex = 1;

                var result = await dialog.ShowAsync();

                if ((int)result.Id == 0)
                    Signals.invokeSetupPIN();
                else
                    Signals.invokeAuthOK();
            }
            else
            {
                await SyncError();
                return;
            }
        }

        private async Task<bool> SyncNow()
        {
            Sync.Visibility = Visibility.Visible;
            return await SyncManager.Sync(true);
        }

        private void invertUI()
        {
            LoginBox.Visibility = (LoginBox.Visibility == Visibility.Visible) ? Visibility.Collapsed : Visibility.Visible;
            LoadBox.Visibility = (LoadBox.Visibility == Visibility.Visible) ? Visibility.Collapsed : Visibility.Visible;
            Sync.Visibility = Visibility.Collapsed;
        }

        private async void WhenLoaded(object sender, RoutedEventArgs e)
        {
            var coreDispatcher = Window.Current.Dispatcher;
            await coreDispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                if (string.IsNullOrEmpty(SettingsManager.login) || string.IsNullOrEmpty(SettingsManager.password))
                {
                    invertUI();
                }
                else
                {
                    if (SettingsManager.lastsynctime <= 0)
                    {
                        if (!await SyncNow())
                        {
                            await SyncError();
                            return;
                        }
                    }
                    else if (!App.StartdedForQuickAdd)
                    {
                        await App.Provider.Init();
                    }
                    if (App.StartdedForQuickAdd && (!SettingsManager.quickAddPIN || string.IsNullOrEmpty(SettingsManager.PIN)))
                        Signals.invokePinOk();
                    else
                        Signals.invokeAuthOK();
                }
            });
        }

        private async Task SyncError()
        {
            if (string.IsNullOrEmpty(App.Error))
                await new MessageDialog(ResourceLoader.GetForViewIndependentUse().GetString("ErrorFirstSync")).ShowAsync();
            else
                await App.showError();

            App.Current.Exit();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            Window.Current.CoreWindow.KeyDown -= CoreWindow_KeyDown;
            Signals.NeedCloseEvent -= Signals_NeedCloseEvent;
        }
    }
}
