using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media;

namespace zMoneyWinX.Model
{
    public enum TransactionType
    {
        Outcome = 0,
        Income = 1,
        Transfer = 2,
        mDept = 3,
        pDept = 4
    }

    public class Extensions
    {
        public static async Task<bool> ReallyDelete(ItemType T)
        {
            string caption;
            switch(T)
            {
                case ItemType.Transaction:
                    caption = ResourceLoader.GetForViewIndependentUse().GetString("DlgMsgDeleteTransaction");
                    break;
                case ItemType.Tag:
                    caption = ResourceLoader.GetForViewIndependentUse().GetString("DlgMsgDeleteTag");
                    break;
                case ItemType.Merchant:
                    caption = ResourceLoader.GetForViewIndependentUse().GetString("DlgMsgDeleteMerchant");
                    break;
                case ItemType.Account:
                    caption = ResourceLoader.GetForViewIndependentUse().GetString("DlgMsgDeleteAccount");
                    break;
                default:
                    caption = ResourceLoader.GetForViewIndependentUse().GetString("DlgMsgDeleteTransaction");
                    break;
            }
            MessageDialog dialog = new MessageDialog(caption);

            dialog.Commands.Add(new UICommand(ResourceLoader.GetForViewIndependentUse().GetString("DialogYes")) { Id = 0 });
            dialog.Commands.Add(new UICommand(ResourceLoader.GetForViewIndependentUse().GetString("DialogNo")) { Id = 1 });
            dialog.DefaultCommandIndex = 0;
            dialog.CancelCommandIndex = 1;

            var result = await dialog.ShowAsync();

            if ((int)result.Id == 0)
                return true;
            else
                return false;
        }

        public enum ItemType
        {
            Transaction = 0,
            Tag = 1,
            Account = 2,
            Merchant = 3
        }

        public static List<SolidColorBrush> TagColors => new List<SolidColorBrush>()
        {
            new SolidColorBrush(Colors.Gray),
            new SolidColorBrush(Colors.Orange),
            new SolidColorBrush(Colors.SkyBlue),
            new SolidColorBrush(Colors.Purple),
            new SolidColorBrush(Colors.Gold),
            new SolidColorBrush(Colors.Red),
            new SolidColorBrush(Colors.DarkBlue),
            new SolidColorBrush(Colors.Violet),
            new SolidColorBrush(Colors.Olive),
            new SolidColorBrush(Colors.GreenYellow),
            new SolidColorBrush(Colors.Brown)
        };

        public static Uri AccountIcon(string TransType)
        {
            if (TransType == "ccard")
                return new Uri("ms-appx:///Assets/Icons/ccard.png");
            else if (TransType == "debt" || string.IsNullOrEmpty(TransType))
                return new Uri("ms-appx:///Assets/Icons/debt.png");
            else if (TransType == "deposit")
                return new Uri("ms-appx:///Assets/Icons/deposit.png");
            else if (TransType == "loan")
                return new Uri("ms-appx:///Assets/Icons/credit.png");
            else if (TransType == "checking")
                return new Uri("ms-appx:///Assets/Icons/checking.png");
            else
                return new Uri("ms-appx:///Assets/Icons/wallet.png");
        }

        public static Color GetColorFromHex(string hexString)
        {
            if (string.IsNullOrEmpty(hexString) || string.IsNullOrWhiteSpace(hexString))
                return Colors.Gray;

            hexString = hexString.Replace("#", string.Empty);
            byte a = byte.Parse(hexString.Substring(0, 2), NumberStyles.HexNumber);
            byte r = byte.Parse(hexString.Substring(2, 2), NumberStyles.HexNumber);
            byte g = byte.Parse(hexString.Substring(4, 2), NumberStyles.HexNumber);
            byte b = byte.Parse(hexString.Substring(6, 2), NumberStyles.HexNumber);
            return Color.FromArgb(a, r, g, b);
        }
    }
}
