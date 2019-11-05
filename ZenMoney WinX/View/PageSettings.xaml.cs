using System;
using Windows.ApplicationModel.Resources;
using Windows.UI.StartScreen;
using Windows.UI.Xaml.Controls;
using zMoneyWinX.Client;
using zMoneyWinX.Model;

namespace zMoneyWinX.View
{
    public sealed partial class PageSettings : Page
    {
        public class CurrencyHelper : INotifyProperty
        {
            public void init(int CurID)
            {
                _CurrentCurrency = CurID;
                NotifyPropertyChanged();
            }
            private int _CurrentCurrency { get; set; }
            public int CurrentCurrency
            {
                get { return _CurrentCurrency; }
                set
                {
                    _CurrentCurrency = value;
                    NotifyPropertyChanged();
                    User.UpdateCurrency(_CurrentCurrency);
                    App.Provider.VMAccount.UpdateBalance();
                    Signals.UpdateBalanceInvoke();
                }
            }
        }
        CurrencyHelper currency = new CurrencyHelper();
        public PageSettings()
        {
            InitializeComponent();
            SettingsArea.DataContext = new SettingsManager();
            UserCurrency.DataContext = currency;
            UserCurrency.ItemsSource = Instrument.Instruments();
            FamilyUsers.ItemsSource = User.Users();
        }

        public void PinButtoninit()
        {
            if (SecondaryTile.Exists(App.SecondTileTileId))
            {
                PinUnpin.Content = new SymbolIcon(Symbol.UnPin);
                PinUnpinLbl.Text = ResourceLoader.GetForViewIndependentUse().GetString("QuickAddUnPin");
            }
            else
            {
                PinUnpin.Content = new SymbolIcon(Symbol.Pin);
                PinUnpinLbl.Text = ResourceLoader.GetForViewIndependentUse().GetString("QuickAddPin");
            }
        }

        private void Logout_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Signals.invokeLogOut();
        }

        private async void PinUnpin_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (SecondaryTile.Exists(App.SecondTileTileId))
            {
                SecondaryTile secondaryTile = new SecondaryTile(App.SecondTileTileId);
                await secondaryTile.RequestDeleteAsync();
            }
            else
            {
                Uri square150x150Logo = new Uri("ms-appx:///Assets/SecSquare150x150Logo.png");
                string tileActivationArguments = App.SecondTileTileId;
                SecondaryTile secondaryTile = new SecondaryTile(App.SecondTileTileId, "Add operation", tileActivationArguments, square150x150Logo, TileSize.Square150x150);
                secondaryTile.VisualElements.ShowNameOnSquare150x150Logo = false;
                secondaryTile.RoamingEnabled = false;
                await secondaryTile.RequestCreateAsync();
            }
            PinButtoninit();
        }

        private void Page_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            PinButtoninit();
            currency.init(User.GetDefaultCurrency());
        }
    }
}
