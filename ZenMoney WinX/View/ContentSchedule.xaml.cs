using System;
using System.Text.RegularExpressions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using zMoneyWinX.ViewModel;

namespace zMoneyWinX.View
{
    public sealed partial class ContentSchedule : ContentDialog
    {
        TransactionScheduleHelper helper;
        public ContentSchedule()
        {
            InitializeComponent();
        }

        public TransactionScheduleHelper setHelper
        {
            set
            {
                helper = value;
                Schdule.DataContext = helper;
            }
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Content = helper;
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Content = null;
        }

        private void TextBox_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            if (!string.IsNullOrEmpty(sender.Text) && !Regex.IsMatch(sender.Text, "^[0-9]+$"))
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
            switch (((Button)sender).Tag.ToString())
            {
                case "Week1":
                    helper.Week1 = !helper.Week1;
                    break;
                case "Week2":
                    helper.Week2 = !helper.Week2;
                    break;
                case "Week3":
                    helper.Week3 = !helper.Week3;
                    break;
                case "Week4":
                    helper.Week4 = !helper.Week4;
                    break;
                case "Week5":
                    helper.Week5 = !helper.Week5;
                    break;
                case "Week6":
                    helper.Week6 = !helper.Week6;
                    break;
                case "Week7":
                    helper.Week7 = !helper.Week7;
                    break;
                default:
                    break;
            }
        }

        private void CalendarDatePicker_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
        {
            if (helper == null)
                return;
            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
            {
                if (args.NewDate == args.OldDate)
                    return;

                if (args.NewDate != null)
                    helper.EndDate = ((DateTimeOffset)args.NewDate).DateTime.ToString("yyyy-MM-dd");
                else if (args.OldDate != null)
                    helper.EndDate = ((DateTimeOffset)args.OldDate).DateTime.ToString("yyyy-MM-dd");
                else
                    helper.EndDate = DateTime.Now.ToString("yyyy-MM-dd");
            }
        }
    }
}
