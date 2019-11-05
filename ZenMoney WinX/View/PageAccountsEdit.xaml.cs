using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using zMoneyWinX.Model;

namespace zMoneyWinX.View
{
    public sealed partial class PageAccountsEdit : Page
    {
        private Account item { get; set; }
        private AcountEditController editItem { get; set; }

        List<Company> companies { get; set; }

        public PageAccountsEdit(Account _item = null)
        {
            InitializeComponent();
            if (_item != null)
            {
                item = _item;
                editItem = new AcountEditController(item);
                if (item.Type != "debt")
                    CmdDelete.Visibility = Visibility.Visible;
            }
            else
            {
                item = new Account();
                editItem = new AcountEditController();
            }
            init();
        }

        private void init()
        {
            LoadProgress.Visibility = Visibility.Visible;
            AccInstrument.ItemsSource = Instrument.Instruments();
            AccType.ItemsSource = editItem.accountTypes;
            AccNumbers.ItemsSource = editItem.editSyncId;
            Root.DataContext = editItem;
            companies = Company.Companies();
            LoadProgress.Visibility = Visibility.Collapsed;
        }

        private async void CmdClick(object sender, RoutedEventArgs e)
        {
            switch (((AppBarButton)sender).Tag.ToString())
            {
                case "Save":
                    save();
                    return;
                case "Delete":
                    await delete();
                    return;
                case "Cancel":
                    Signals.CloseMeInvoke();
                    return;
                default:
                    break;
            }
        }
        private async Task delete()
        {
            if (await Model.Extensions.ReallyDelete(Model.Extensions.ItemType.Account))
            {
                LoadProgress.Visibility = Visibility.Visible;
                App.Provider.VMAccount.Delete(item);
                Signals.CloseMeInvoke();
                LoadProgress.Visibility = Visibility.Collapsed;
            }
        }
        private async void save()
        {
            LoadProgress.Visibility = Visibility.Visible;
            var valid = editItem.Validate();
            if (string.IsNullOrWhiteSpace(valid))
            {
                editItem.Save(item);
                Signals.CloseMeInvoke();
            }
            else
            {
                await new MessageDialog(ResourceLoader.GetForViewIndependentUse().GetString("ErrorValidation") + valid).ShowAsync();
            }
            LoadProgress.Visibility = Visibility.Collapsed;
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

        private void Companies_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            var source = new List<Company>();
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                source = companies.FindAll(i => i.Title.ToLower().StartsWith(sender.Text.ToLower()));
                if (source != null && source.Count > 0)
                    sender.ItemsSource = source;
                else
                    sender.ItemsSource = null;
            }
        }

        private async void AddAlias_Click(object sender, RoutedEventArgs e)
        {
            ContentAccountAlias con = new ContentAccountAlias();
            con.MaxWidth = ActualWidth;
            con.setAlias = "";
            await con.ShowAsync();

            string res;
            try
            {
                res = (string)con.Content;
            }
            catch { return; }

            if (res != null && !string.IsNullOrWhiteSpace(res))
                editItem.editSyncId.Add(res);
        }

        private async void EditAlias_Click(object sender, ItemClickEventArgs e)
        {
            string source = (string)e.ClickedItem;
            ContentAccountAlias con = new ContentAccountAlias();
            con.MaxWidth = ActualWidth;
            con.setAlias = source;
            await con.ShowAsync();

            string res;
            try
            {
                res = (string)con.Content;
            }
            catch { return; }

            if (res == null)
                editItem.editSyncId.Remove(source);
            else if (!string.IsNullOrWhiteSpace(res))
            {
                int idx = editItem.editSyncId.IndexOf(source);
                editItem.editSyncId.Remove(source);
                editItem.editSyncId.Insert(idx, res);
            }
        }

        private void CalendarDatePicker_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
        {
            if (editItem == null)
                return;
            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
            {
                if (args.NewDate == args.OldDate)
                    return;

                if (args.NewDate != null)
                    editItem.editStartDate = (DateTimeOffset)args.NewDate;
                else if (args.OldDate != null)
                    editItem.editStartDate = (DateTimeOffset)args.OldDate;
                else
                    editItem.editStartDate = DateTime.Now;
            }
        }
    }
}
