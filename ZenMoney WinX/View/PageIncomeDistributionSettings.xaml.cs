using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using zMoneyWinX.Client;
using zMoneyWinX.Model;

namespace zMoneyWinX.View
{
    public sealed partial class PageIncomeDistributionSettings : Page
    {
        Set set;
        public PageIncomeDistributionSettings()
        {
            InitializeComponent();
            set = new Set();
            Root.DataContext = set;
        }

        private async void CmdClick(object sender, RoutedEventArgs e)
        {
            switch (((AppBarButton)sender).Tag.ToString())
            {
                case "Save":
                    if (await set.Validate())
                    {
                        set.Save();
                        Signals.CloseMeInvoke();
                    }
                    return;
                case "Cancel":
                    Signals.CloseMeInvoke();
                    return;
                default:
                    break;
            }
        }

        private void TextBox_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            if (!string.IsNullOrEmpty(sender.Text) && !Regex.IsMatch(sender.Text, @"[\d]"))
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

        private class Set : INotifyProperty
        {
            public Set()
            {
                mandatoryperc = SettingsManager.mandatoryperc;
                nonmandatoryperc = SettingsManager.nonmandatoryperc;
                debetperc = SettingsManager.debetperc;
            }

            public void Save()
            {
                SettingsManager.mandatoryperc = mandatoryperc;
                SettingsManager.nonmandatoryperc = nonmandatoryperc;
                SettingsManager.debetperc = debetperc;
            }

            public async Task<bool> Validate()
            {
                string validate = "";

                if (_mandatoryperc <= 0 || _nonmandatoryperc <= 0 || debetperc < 0)
                    validate += "\n" + ResourceLoader.GetForViewIndependentUse().GetString("RIDSetErrorMSG0");

                if (_mandatoryperc + _nonmandatoryperc + debetperc != 100)
                    validate += "\n" + ResourceLoader.GetForViewIndependentUse().GetString("RIDSetErrorMSG100");

                if (!string.IsNullOrEmpty(validate))
                {
                    await new MessageDialog(ResourceLoader.GetForViewIndependentUse().GetString("ErrorValidation") + validate).ShowAsync();
                    return false;
                }
                return true;
            }            

            private int _mandatoryperc { get; set; }
            public int mandatoryperc { get { return _mandatoryperc; } set { _mandatoryperc = value; NotifyPropertyChanged(); } }

            private int _nonmandatoryperc { get; set; }
            public int nonmandatoryperc { get { return _nonmandatoryperc; } set { _nonmandatoryperc = value; NotifyPropertyChanged(); } }

            private int _debetperc { get; set; }
            public int debetperc { get { return _debetperc; } set { _debetperc = value; NotifyPropertyChanged(); } }
        }
    }
}
