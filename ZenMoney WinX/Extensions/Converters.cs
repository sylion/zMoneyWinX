using System;
using System.Globalization;
using System.Linq;
using Windows.ApplicationModel.Resources;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using zMoneyWinX.Client;
using zMoneyWinX.ViewModel;
using static zMoneyWinX.Client.SettingsManager;

namespace zMoneyWinX.Model
{
    public class AmountToStr : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value != null)
            {
                return ((double)value).ToString("#,0.##");
            }
            return "0";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value != null && !string.IsNullOrEmpty((string)value))
            {
                double res;
                if (!double.TryParse((string)value, NumberStyles.Any, CultureInfo.CurrentCulture, out res))
                    double.TryParse((string)value, NumberStyles.Any, CultureInfo.InvariantCulture, out res);
                return res;
            }
            return 0;
        }
    }

    public class CheckedWeekColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if ((bool)value)
                return new SolidColorBrush(Color.FromArgb(0xFF, 0xF0, 0x41, 0x37));
            else
                return new SolidColorBrush(Colors.Black);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class VisIfTrue : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value != null && (bool)value == true)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class VisIfFalse : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value != null && (bool)value == true)
                return Visibility.Collapsed;
            else
                return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class ScheduledColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value != null && (bool)value == true)
                return new SolidColorBrush(Colors.LightYellow);
            else
                return new SolidColorBrush(Colors.White);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class BudgetTagToWeight : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null || (Guid)value != Guid.Empty)
                return FontWeights.Normal;
            else
                return FontWeights.SemiBold;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class PivotidxToBoolean : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return System.Convert.ToInt32((bool)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return System.Convert.ToBoolean((int)value);
        }
    }

    public class OwnerToVis : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (!string.IsNullOrEmpty((string)value))
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class nullToVis : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if ((TransactionFilter)value != null)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class HideIfEmpty : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value != null)
            {
                if (!string.IsNullOrEmpty((string)value) && !string.IsNullOrWhiteSpace((string)value))
                {
                    return Visibility.Visible;
                }
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class HideIfEmptyIncome : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value != null)
            {
                if (!string.IsNullOrEmpty((string)value))// && !string.IsNullOrWhiteSpace((string)value))
                {
                    return Visibility.Visible;
                }
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class emptyGUID : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value != null && (Guid)value != Guid.Empty)
                return value;
            else
                return null;
        }
    }

    public class tagHasChild : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value != null)
            {
                int res = TagViewModel.Tags.Where(i => i.Parent == ((TagEdit)value).Id).Count();
                if (res > 0)
                    return Visibility.Collapsed;
                else
                    return Visibility.Visible;

            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class ShowArrow : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value != null)
            {
                if ((TransactionType)value == TransactionType.Transfer)
                {
                    return Visibility.Visible;
                }
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class stringToToggled : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value != null)
            {
                if (!string.IsNullOrEmpty((string)value))
                {
                    return true;
                }
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value != null)
            {
                if (!(bool)value)
                    return "";
                Signals.invokeSetupPIN();
                return true;
            }
            return "";
        }
    }

    public class LastSyncTime : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value != null)
            {
                if ((long)value > 0)
                {
                    return ResourceLoader.GetForViewIndependentUse().GetString("SyncTime") + " " + SettingsManager.fromUnixTime((long)value).ToLocalTime().ToString();
                }
                else
                    return ResourceLoader.GetForViewIndependentUse().GetString("SyncTimeNever");
            }
            return ResourceLoader.GetForViewIndependentUse().GetString("SyncTimeNever");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class formulaToResult : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value != null)
            {
                return ((double)value).ToString("#0.##");
            }
            return "0";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value != null && !string.IsNullOrEmpty((string)value))
            {
                double res;
                if (!double.TryParse((string)value, NumberStyles.Any, CultureInfo.CurrentCulture, out res))
                    double.TryParse((string)value, NumberStyles.Any, CultureInfo.InvariantCulture, out res);
                return res;
            }
            return 0;
        }
    }

    public class formulaToResultInteger : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value != null)
            {
                return ((int)value).ToString();
            }
            return "0";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value != null && !string.IsNullOrEmpty((string)value))
            {
                int res;
                if (int.TryParse((string)value, out res))
                    return res;
            }
            return 0;
        }
    }

    public class formulaToResultCurrency : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value != null)
            {
                return ((double)value).ToString("#0.######");
            }
            return "0";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value != null && !string.IsNullOrEmpty((string)value))
            {
                double res;
                if (!double.TryParse((string)value, NumberStyles.Any, CultureInfo.CurrentCulture, out res))
                    double.TryParse((string)value, NumberStyles.Any, CultureInfo.InvariantCulture, out res);
                return res;
            }
            return 0;
        }
    }

    public class stringToTransDate : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value != null)
            {
                return new DateTimeOffset(DateTime.Parse((string)value));
            }
            return new DateTimeOffset(DateTime.Now);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            try
            {
                return ((DateTimeOffset)value).DateTime.ToString("yyyy-MM-dd");
            }
            catch { return null; }
        }
    }

    public class filterDate : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value != null)
            {
                try
                {
                    return new DateTimeOffset((DateTime)value);
                }
                catch
                {
                    return new DateTimeOffset(DateTime.Now.Date);
                }
            }
            return new DateTimeOffset(DateTime.Now.Date);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            try
            {
                return ((DateTimeOffset)value).Date;
            }
            catch { return null; }
        }
    }

    public class periodEnabled : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value != null)
            {
                return (TransactionPeriod)value == TransactionPeriod.Period;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class PivotToAccDisplayMode : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                return 0;

            AccountsDisplayMode val = (AccountsDisplayMode)value;
            return (int)val;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                return AccountsDisplayMode.Active;

            int val = (int)value;
            return (AccountsDisplayMode)val;
        }
    }

    public class pivotToTransType : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                return 0;

            TransactionType val = (TransactionType)value;
            return (int)val;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                return TransactionType.Outcome;

            int val = (int)value;
            return (TransactionType)val;
        }
    }

    public class pivotToTransPeriod : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                return 0;

            TransactionPeriod val = (TransactionPeriod)value;
            return (int)val;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                return TransactionType.Outcome;

            int val = (int)value;
            return (TransactionPeriod)val;
        }
    }

    public class pivotToTransTypeFilter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                return 0;

            TransactionTypeFilter val = (TransactionTypeFilter)value;
            return (int)val;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                return TransactionType.Outcome;

            int val = (int)value;
            return (TransactionTypeFilter)val;
        }
    }

    public class CheckedTextColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if ((bool)value)
                return new SolidColorBrush(Colors.White);
            else
                return new SolidColorBrush(Colors.Black);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class CheckedBackColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if ((bool)value)
                return new SolidColorBrush(Color.FromArgb(0xFF, 0xF0, 0x41, 0x37));
            else
                return new SolidColorBrush(Colors.Transparent);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class HistoryMode : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (int)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return (SettingsManager.HistoryMode)value;
        }
    }

    public class LanguageID : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if ((string)value == "en-US")
                return 1;
            if ((string)value == "ru")
                return 2;
            if ((string)value == "uk")
                return 3;
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if ((int)value == 1)
                return "en-US";
            if ((int)value == 2)
                return "ru";
            if ((int)value == 3)
                return "uk";
            return "";
        }
    }

    public class tagImage : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (!string.IsNullOrEmpty((string)value))
            {
                try
                {
                    if (System.IO.File.Exists("Assets/tags/" + (string)value + ".png"))
                        return new Uri("ms-appx:///Assets/tags/" + (string)value + ".png");
                    else
                        return new Uri("ms-appx:///Assets/tags/8001_question.png");
                }
                catch
                {
                    return new Uri("ms-appx:///Assets/tags/0000_none.png");
                }
            }
            return new Uri("ms-appx:///Assets/tags/0000_none.png");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class tagLetter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (!string.IsNullOrEmpty((string)value))
            {
                return char.ConvertFromUtf32(char.ConvertToUtf32(((string)value), 0));
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class tagImageVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (!string.IsNullOrEmpty((string)value))
            {
                if ((string)value != "0000_none.png")
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class tagLetterVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (!string.IsNullOrEmpty((string)value))
            {
                if ((string)value != "0000_none.png")
                    return Visibility.Collapsed;
                else
                    return Visibility.Visible;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
