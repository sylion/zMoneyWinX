using Windows.UI.Xaml.Controls;
using zMoneyWinX.Model;

namespace zMoneyWinX.View
{
    public sealed partial class ContentAccountAlias : ContentDialog
    {
        private static Alias alias = new Alias();
        public ContentAccountAlias()
        {
            InitializeComponent();
            AliasStr.DataContext = alias;
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Content = alias.alias;
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Content = null;
        }

        public string setAlias
        {
            set
            {
                alias.alias = value;
            }
        }

        private class Alias : INotifyProperty
        {
            private string _alias;
            public string alias
            {
                get
                {
                    return _alias;
                }
                set
                {
                    _alias = value;
                    NotifyPropertyChanged();
                }
            }
        }
    }
}
