using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace zMoneyWinX.Controls
{
    public sealed class CurrencyTextBox : TextBox
    {
        public CurrencyTextBox()
        {
            DefaultStyleKey = typeof(CurrencyTextBox);
        }

        public string Symbol
        {
            get { return (string)GetValue(SymbolProperty); }
            set { SetValue(SymbolProperty, value); }
        }

        public static readonly DependencyProperty SymbolProperty = DependencyProperty.Register("Symbol", typeof(string), typeof(CurrencyTextBox), new PropertyMetadata(""));
        //, new PropertyChangedCallback(OnSymbolChanged)));

        //private static void OnSymbolChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        //{
        //    ((CurrencyTextBox)d).Symbol = (string)e.NewValue;
        //}

        //public event PropertyChangedEventHandler PropertyChanged;
        //void SetValueDp(DependencyProperty property, object value, [System.Runtime.CompilerServices.CallerMemberName] string p = null)
        //{
        //    SetValue(property, value);
        //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
        //}
    }
}