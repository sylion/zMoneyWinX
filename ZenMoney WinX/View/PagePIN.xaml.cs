using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using zMoneyWinX.Client;
using zMoneyWinX.Model;

namespace zMoneyWinX.View
{
    public sealed partial class PagePIN : Page
    {
        private PIN pin;
        public PagePIN(bool Setup = false)
        {
            this.InitializeComponent();

            if (!Setup && string.IsNullOrEmpty(SettingsManager.PIN))
            {
                Signals.invokePinOk();
                LoadProgress.Visibility = Visibility.Visible;
                return;
            }

            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Desktop")
            {
                Window.Current.CoreWindow.KeyDown -= CoreWindow_KeyDown;
                Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown;
            }
            pin = new PIN(Setup);
            PinArea.DataContext = pin;

            Signals.NeedCloseEvent -= Signals_NeedCloseEvent;
            Signals.NeedCloseEvent += Signals_NeedCloseEvent;

            PIN.PinEnterdEvent -= Pin_PinEnterdEvent;
            PIN.PinEnterdEvent += Pin_PinEnterdEvent;
        }

        private void Pin_PinEnterdEvent()
        {
            if (!pin.setup && SettingsManager.PIN == pin.getPIN())
            {
                Signals.invokePinOk();
                LoadProgress.Visibility = Visibility.Visible;
                return;
            }
            else if (!pin.setup && SettingsManager.PIN != pin.getPIN())
            {
                WrongPIN();
                return;
            }

            if (pin.setupOk)
            {
                SettingsManager.PIN = pin.getPIN(true);
                Signals.invokePinOk();
                LoadProgress.Visibility = Visibility.Visible;
                return;
            }
        }

        private async void WrongPIN()
        {
            WrongPin.Visibility = Visibility.Visible;
            pin.Clear();
            await Task.Delay(TimeSpan.FromSeconds(2));
            WrongPin.Visibility = Visibility.Collapsed;
        }

        private async void Signals_NeedCloseEvent()
        {
            NeedClose.Visibility = Visibility.Visible;
            await Task.Delay(TimeSpan.FromSeconds(2));
            NeedClose.Visibility = Visibility.Collapsed;
        }

        private void CoreWindow_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
            switch (args.VirtualKey)
            {
                case VirtualKey.Number0:
                case VirtualKey.NumberPad0:
                    pin.addDigit("0");
                    break;
                case VirtualKey.Number1:
                case VirtualKey.NumberPad1:
                    pin.addDigit("1");
                    break;
                case VirtualKey.Number2:
                case VirtualKey.NumberPad2:
                    pin.addDigit("2");
                    break;
                case VirtualKey.Number3:
                case VirtualKey.NumberPad3:
                    pin.addDigit("3");
                    break;
                case VirtualKey.Number4:
                case VirtualKey.NumberPad4:
                    pin.addDigit("4");
                    break;
                case VirtualKey.Number5:
                case VirtualKey.NumberPad5:
                    pin.addDigit("5");
                    break;
                case VirtualKey.Number6:
                case VirtualKey.NumberPad6:
                    pin.addDigit("6");
                    break;
                case VirtualKey.Number7:
                case VirtualKey.NumberPad7:
                    pin.addDigit("7");
                    break;
                case VirtualKey.Number8:
                case VirtualKey.NumberPad8:
                    pin.addDigit("8");
                    break;
                case VirtualKey.Number9:
                case VirtualKey.NumberPad9:
                    pin.addDigit("9");
                    break;
                case VirtualKey.Back:
                    pin.Correct();
                    break;
                default:
                    break;
            }
        }

        private void btnClick(object sender, RoutedEventArgs e)
        {
            string d = ((Button)sender).Name.ToString();
            pin.addDigit(d.Substring(d.Length - 1, 1));
        }

        private void btnCorrect_Click(object sender, RoutedEventArgs e)
        {
            pin.Correct();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Desktop")
                Window.Current.CoreWindow.KeyDown -= CoreWindow_KeyDown;
            PIN.PinEnterdEvent -= Pin_PinEnterdEvent;
            Signals.NeedCloseEvent -= Signals_NeedCloseEvent;
        }

        private class PIN : INotifyProperty
        {
            public delegate void VoidSignalHandler();
            public static event VoidSignalHandler PinEnterdEvent;

            public bool setup { private set; get; }
            private string tmpPin;
            public PIN(bool _setup = false)
            {
                setup = _setup;
            }

            public string areaText
            {
                get
                {
                    if (!setup)
                        return ResourceLoader.GetForCurrentView().GetString("PINEnter");
                    if (string.IsNullOrEmpty(tmpPin))
                        return ResourceLoader.GetForCurrentView().GetString("PINSetup");
                    else
                        return ResourceLoader.GetForCurrentView().GetString("PINRepeat");
                }
            }

            public void Clear()
            {
                clear(true);
            }

            public bool setupOk
            {
                get
                {
                    if (!string.IsNullOrEmpty(tmpPin))
                    {
                        if (tmpPin == getPIN())
                        {
                            return true;
                        }
                        else
                        {
                            clear(true);
                            return false;
                        }
                    }
                    else
                    {
                        tmpPin = getPIN();
                        clear();
                        return false;
                    }
                }
            }

            public void Correct()
            {
                if (!string.IsNullOrEmpty(digit4))
                {
                    digit4 = "";
                    return;
                }
                if (!string.IsNullOrEmpty(digit3))
                {
                    digit3 = "";
                    return;
                }
                if (!string.IsNullOrEmpty(digit2))
                {
                    digit2 = "";
                    return;
                }
                if (!string.IsNullOrEmpty(digit1))
                {
                    digit1 = "";
                    return;
                }
            }

            private void clear(bool Dispose = false)
            {
                digit1 = string.Empty;
                digit2 = string.Empty;
                digit3 = string.Empty;
                digit4 = string.Empty;
                if (Dispose)
                    tmpPin = string.Empty;
            }

            public string getPIN(bool Dispose = false)
            {
                string res = string.Empty;
                if (PINcount() == 4)
                    res = digit1 + digit2 + digit3 + digit4;

                if (Dispose)
                    clear(Dispose);

                return res;
            }

            public void addDigit(string digit)
            {
                switch (PINcount())
                {
                    case 0:
                        digit1 = digit;
                        break;
                    case 1:
                        digit2 = digit;
                        break;
                    case 2:
                        digit3 = digit;
                        break;
                    case 3:
                        digit4 = digit;
                        break;
                    default:
                        break;
                }
                if (PINcount() == 4 && PinEnterdEvent != null)
                    PinEnterdEvent.Invoke();
            }

            public int PINcount()
            {
                int res = 0;
                if (!string.IsNullOrEmpty(digit1))
                    res++;
                if (!string.IsNullOrEmpty(digit2))
                    res++;
                if (!string.IsNullOrEmpty(digit3))
                    res++;
                if (!string.IsNullOrEmpty(digit4))
                    res++;
                return res;
            }

            private string _digit1;
            private string _digit2;
            private string _digit3;
            private string _digit4;
            private string digit1
            {
                get { return _digit1; }
                set
                {
                    _digit1 = value;
                    NotifyPropertyChanged();
                }
            }
            private string digit2
            {
                get { return _digit2; }
                set
                {
                    _digit2 = value;
                    NotifyPropertyChanged();
                }
            }
            private string digit3
            {
                get { return _digit3; }
                set
                {
                    _digit3 = value;
                    NotifyPropertyChanged();
                }
            }
            private string digit4
            {
                get { return _digit4; }
                set
                {
                    _digit4 = value;
                    NotifyPropertyChanged();
                }
            }

            public string glyphDigit1
            {
                get
                {
                    if (string.IsNullOrEmpty(digit1))
                        return "\uEA3A";
                    else
                        return "\uE91F";
                }
            }
            public string glyphDigit2
            {
                get
                {
                    if (string.IsNullOrEmpty(digit2))
                        return "\uEA3A";
                    else
                        return "\uE91F";
                }
            }
            public string glyphDigit3
            {
                get
                {
                    if (string.IsNullOrEmpty(digit3))
                        return "\uEA3A";
                    else
                        return "\uE91F";
                }
            }
            public string glyphDigit4
            {
                get
                {
                    if (string.IsNullOrEmpty(digit4))
                        return "\uEA3A";
                    else
                        return "\uE91F";
                }
            }
        }
    }
}
