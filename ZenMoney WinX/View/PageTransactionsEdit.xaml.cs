using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using zMoneyWinX.Model;
using zMoneyWinX.ViewModel;

namespace zMoneyWinX.View
{
    public sealed partial class PageTransactionsEdit : Page
    {
        private Transaction item { get; set; }
        private TransactionEdit editItem { get; set; }

        private List<Merchant> merchants { get; set; }
        private TransactionTagHelper TagHelper { get; set; }
        private TransactionScheduleHelper ScheduleHelper { get; set; }

        private bool Create = false;
        private Guid DebtID;

        public PageTransactionsEdit(Transaction _item = null)
        {
            InitializeComponent();
            if (_item != null && _item.ReminderMarker != null && _item.ReminderMarker != Guid.Empty)
            {
                item = _item;
                Create = true;
                CmdSchedule.Visibility = Visibility.Collapsed;
            }
            else if (_item != null)
            {
                item = _item;
                CmdDelete.Visibility = Visibility.Visible;
                CmdSchedule.Visibility = Visibility.Collapsed;
            }
            else
            {
                item = new Transaction();
                Create = true;
                CmdSchedule.Visibility = Visibility.Visible;
            }
        }
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadProgress.Visibility = Visibility.Visible;
            editItem = new TransactionEdit(item);

            editItem.RebindIncomeAccountsEvent -= EditItem_RebindIncomeAccountsEvent;
            editItem.RebindIncomeAccountsEvent += EditItem_RebindIncomeAccountsEvent;
            editItem.RebindOutcomeAccountsEvent -= EditItem_RebindOutcomeAccountsEvent;
            editItem.RebindOutcomeAccountsEvent += EditItem_RebindOutcomeAccountsEvent;

            await init();

            LoadProgress.Visibility = Visibility.Collapsed;
        }

        private void EditItem_RebindOutcomeAccountsEvent()
        {
            Account.SetBinding(ItemsControl.ItemsSourceProperty, new Binding() { Source = editItem.OutcomeAccountList });
            Account2.SetBinding(ItemsControl.ItemsSourceProperty, new Binding() { Source = editItem.OutcomeAccountList });
            Account4.SetBinding(ItemsControl.ItemsSourceProperty, new Binding() { Source = editItem.OutcomeAccountList });
        }

        private void EditItem_RebindIncomeAccountsEvent()
        {
            Account1.SetBinding(ItemsControl.ItemsSourceProperty, new Binding() { Source = editItem.IncomeAccountList });
            Account3.SetBinding(ItemsControl.ItemsSourceProperty, new Binding() { Source = editItem.IncomeAccountList });
            Account5.SetBinding(ItemsControl.ItemsSourceProperty, new Binding() { Source = editItem.IncomeAccountList });
        }

        private async Task init()
        {
            merchants = MerchantViewModel.MerhcantsList;
            TagHelper = new TransactionTagHelper(item.Tags);

            ScheduleHelper = new TransactionScheduleHelper();

            DebtID = AccountViewModel.DebtID;

            TagsOutcome.SetBinding(ItemsControl.ItemsSourceProperty, new Binding() { Source = TagHelper.OutcomeTags });
            TagsIncome.SetBinding(ItemsControl.ItemsSourceProperty, new Binding() { Source = TagHelper.IncomeTags });
            TagIncomeChildren.SetBinding(ItemsControl.ItemsSourceProperty, new Binding() { Source = TagHelper.OutcomeTagsChild });
            TagOutcomeChildren.SetBinding(ItemsControl.ItemsSourceProperty, new Binding() { Source = TagHelper.IncomeTagsChild });

            EditItem_RebindOutcomeAccountsEvent();
            EditItem_RebindIncomeAccountsEvent();

            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { Root.DataContext = editItem; });
        }

        private async void CmdClick(object sender, RoutedEventArgs e)
        {
            switch (((AppBarButton)sender).Tag.ToString())
            {
                case "Schedule":
                    await ScheduleOperation();
                    break;
                case "Save":
                    await save();
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

        private async Task ScheduleOperation()
        {
            ContentSchedule con = new ContentSchedule();
            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
                con.MaxWidth = ActualWidth;
            var tmp = JsonConvert.SerializeObject(ScheduleHelper);
            con.setHelper = JsonConvert.DeserializeObject<TransactionScheduleHelper>(tmp);
            await con.ShowAsync();

            try
            {
                if (con.Content != null && con.Content is TransactionScheduleHelper)
                    ScheduleHelper = (TransactionScheduleHelper)con.Content;
            }
            catch { return; }
        }

        private void Tags_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (editItem.editTransType == TransactionType.Income)
                TagHelper.IncomeClicked((Tag)e.ClickedItem);
            if (editItem.editTransType == TransactionType.Outcome)
                TagHelper.OutcomeClicked((Tag)e.ClickedItem);
        }
        private void TagChildren_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (editItem.editTransType == TransactionType.Income)
                TagHelper.IncomeChildClicked((Tag)e.ClickedItem);
            if (editItem.editTransType == TransactionType.Outcome)
                TagHelper.OutcomeChildClicked((Tag)e.ClickedItem);
        }

        private async Task delete()
        {
            if (await Model.Extensions.ReallyDelete(Model.Extensions.ItemType.Transaction))
            {
                LoadProgress.Visibility = Visibility.Visible;
                App.Provider.VMTransaction.Delete(item);
                Signals.CloseMeInvoke();
                LoadProgress.Visibility = Visibility.Collapsed;
            }
        }

        private async Task save()
        {
            LoadProgress.Visibility = Visibility.Visible;
            var valid = editItem.validate();

            if (string.IsNullOrWhiteSpace(valid))
            {
                await item.Update(editItem, TagHelper.GetTransTags(editItem.editTransType));

                if (Create && ScheduleHelper != null && ScheduleHelper.HelperEnabled)
                {
                    ScheduleHelper.makeReminder(item);
                }
                else if (Create && DateTime.Parse(item.Date).Date > DateTime.Now.Date)
                {
                    Reminder.CreateSingle(item);
                }
                else if (!App.StartdedForQuickAdd)
                {
                    if (Create)
                        App.Provider.VMTransaction.Insert(item);
                    else
                        App.Provider.VMTransaction.Update(item);

                    if (item.ReminderMarker != null && item.ReminderMarker != Guid.Empty)
                    {
                        ReminderMarker.Processed((Guid)item.ReminderMarker);
                        App.Provider.VMTransaction.Delete((Guid)item.ReminderMarker);
                    }
                }
                else
                {
                    DBObject.Insert(item);
                }
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

        private void Merchant_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            var source = new List<Merchant>();
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                source = merchants.FindAll(i => i.Title.ToLower().StartsWith(sender.Text.ToLower()));
                if (source != null && source.Count > 0)
                    sender.ItemsSource = source;
                else
                    sender.ItemsSource = null;
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
                    editItem.editDate = ((DateTimeOffset)args.NewDate).DateTime.ToString("yyyy-MM-dd");
                else if (args.OldDate != null)
                    editItem.editDate = ((DateTimeOffset)args.OldDate).DateTime.ToString("yyyy-MM-dd");
                else
                    editItem.editDate = DateTime.Now.ToString("yyyy-MM-dd");
            }
        }
    }
}