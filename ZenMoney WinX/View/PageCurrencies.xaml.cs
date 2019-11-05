using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Navigation;
using zMoneyWinX.Model;
using static zMoneyWinX.ViewModel.CurrencyHelper;

namespace zMoneyWinX.View
{
    public sealed partial class PageCurrencies : Page
    {
        //private static bool loaded = false;
        CurrencyConverter converter;
        public PageCurrencies()
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Required;

            converter = new CurrencyConverter();
            ConverterPanel.DataContext = converter;
            FromCurrency.SetBinding(ComboBox.ItemsSourceProperty, new Binding() { Source = converter.instruments });
            ToCurrency.SetBinding(ComboBox.ItemsSourceProperty, new Binding() { Source = converter.instruments });
            converter.init();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //if (!loaded)
            await init();
            //loaded = true;
        }

        private async Task init()
        {
            Signals.invokeSyncStarted();
            await Window.Current.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                InstrumentList.ItemsSource = Instrument.RatesList();
            });
            Signals.invokeSyncEnded();
        }

        private void TextBox_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            if (!string.IsNullOrEmpty(sender.Text) && !Regex.IsMatch(sender.Text, "^\\d*[\\.\\,]?\\d*$"))
            {
                int pos = sender.SelectionStart - 1;
                sender.Text = sender.Text.Remove(pos, 1);
                sender.SelectionStart = sender.Text.Length;
            }
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var control = (TextBox)sender;
            control.SelectAll();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            converter.Convert();
        }
    }
}
