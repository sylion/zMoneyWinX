using System;
using System.Text.RegularExpressions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using zMoneyWinX.Client;
using zMoneyWinX.Model;

namespace zMoneyWinX.View
{
    public sealed partial class PageBudgetEdit : Page
    {
        public string CurrencySymbol;
        Budget Item;
        BudgetEdit editItem;
        public PageBudgetEdit(Budget item, TransactionType type)
        {
            InitializeComponent();
            CurrencySymbol = Instrument.GetSymbol(User.GetDefaultCurrency());
            Item = item;
            editItem = new BudgetEdit(Item, type);
            Root.DataContext = editItem;
        }

        private void CmdClick(object sender, RoutedEventArgs e)
        {
            switch (((AppBarButton)sender).Tag.ToString())
            {
                case "Save":
                    Save();
                    Signals.CloseMeInvoke();
                    return;
                case "Cancel":
                    Signals.CloseMeInvoke();
                    return;
                default:
                    break;
            }
        }

        private void Save()
        {
            Item.Income = editItem.Income;
            Item.Outcome = editItem.Outcome;
            Item.Changed = SettingsManager.toUnixTime(DateTime.Now);
            Item.isChanged = true;
            Budget.Insert(Item);
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
    }
}
