using System;
using Windows.Storage;

namespace zMoneyWinX.Client
{
    class SettingsManager
    {
        //===Time conversations===
        /// <summary>
        /// Convert DateTime to UnixTimeStamp
        /// </summary>
        /// <param name="date">DateTime to convert</param>
        /// <returns>string UnixTime value</returns>
        public static long toUnixTime(DateTime date)
        {
            var timeSpan = (date - new DateTime(1970, 1, 1, 0, 0, 0));
            return (long)timeSpan.TotalSeconds;
        }
        /// <summary>
        /// Convert UnixTimeStamp to DateTime
        /// </summary>
        /// <param name="timestamp">timeStamp to convert</param>
        /// <returns>DateTime value</returns>
        public static DateTime fromUnixTime(long timestamp)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return origin.AddSeconds(timestamp);
        }

        //===Roaming Settings Storage===
        public static ApplicationDataContainer roamingSettings = ApplicationData.Current.RoamingSettings;
        /// <summary>
        /// Application runs count (stored in roaming settings folder)
        /// </summary>
        public static int runs
        {
            get
            {
                try { return (int)roamingSettings.Values["RunsCount"]; }
                catch { return 0; }
            }
            set
            {
                roamingSettings.Values["RunsCount"] = value;
            }
        }
        /// <summary>
        /// User answer for RateUs dialog
        /// </summary>
        public static bool answer
        {
            get
            {

                try { return (bool)roamingSettings.Values["RateUsAnswer"]; }
                catch { return false; }
            }
            set
            {
                roamingSettings.Values["RateUsAnswer"] = value;
            }
        }

        //===Local Settings Storage===
        public static ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        /// <summary>
        /// user name
        /// </summary>
        public static string login
        {
            get
            {
                try { return (string)localSettings.Values["Login"]; }
                catch { return string.Empty; }
            }
            set
            {
                localSettings.Values["Login"] = value;
            }
        }
        /// <summary>
        /// user password
        /// </summary>
        public static string password
        {
            get
            {
                try { return (string)localSettings.Values["password"]; }
                catch { return string.Empty; }
            }
            set
            {
                localSettings.Values["password"] = value;
            }
        }
        /// <summary>
        /// Token key
        /// </summary>
        public static string tokenkey
        {
            get
            {
                try { return (string)localSettings.Values["tokenkey"]; }
                catch { return string.Empty; }
            }
            set
            {
                localSettings.Values["tokenkey"] = value;
            }
        }
        /// <summary>
        /// Token secret
        /// </summary>
        public static string tokensecret
        {
            get
            {
                try { return (string)localSettings.Values["tokensecret"]; }
                catch { return string.Empty; }
            }
            set
            {
                localSettings.Values["tokensecret"] = value;
            }
        }
        /// <summary>
        /// user PIN code for app
        /// </summary>
        public static string PIN
        {
            get
            {
                try { return (string)localSettings.Values["PIN"]; }
                catch { return string.Empty; }
            }
            set
            {
                localSettings.Values["PIN"] = value;
                if (string.IsNullOrWhiteSpace(value))
                    quickAddPIN = false;
            }
        }

        public static bool quickAddPIN
        {
            get
            {

                try { return (bool)roamingSettings.Values["quickAddPIN"]; }
                catch { return false; }
            }
            set
            {
                roamingSettings.Values["quickAddPIN"] = value;
            }
        }

        /// <summary>
        /// last sync time retrieved from the server
        /// </summary>
        public static long lastsynctime
        {
            get
            {
                try { return (long)localSettings.Values["lastsynctime"]; }
                catch { return 0; }
            }
            set
            {
                localSettings.Values["lastsynctime"] = value;
            }
        }
        /// <summary>
        /// Show full history on transactions page
        /// </summary>
        public static HistoryMode historymode
        {
            get
            {
                try { return (HistoryMode)localSettings.Values["historymode"]; }
                catch { return HistoryMode.Week; }
            }
            set
            {
                localSettings.Values["historymode"] = (int)value;
            }
        }

        /// <summary>
        /// Accounts Display Mode on accounts page
        /// </summary>
        public static bool accountsdisplaymode
        {
            get
            {
                try { return (bool)localSettings.Values["accountsdisplaymode"]; }
                catch { return false; }
            }
            set
            {
                localSettings.Values["accountsdisplaymode"] = value;
                App.Provider.VMAccount.Refresh();
            }
        }

        #region 50/20/30
        public static int mandatoryperc
        {
            get
            {
                try { return (int)localSettings.Values["mandatoryperc"]; }
                catch { return 50; }
            }
            set
            {
                localSettings.Values["mandatoryperc"] = value;
            }
        }
        public static int nonmandatoryperc
        {
            get
            {
                try { return (int)localSettings.Values["nonmandatoryperc"]; }
                catch { return 30; }
            }
            set
            {
                localSettings.Values["nonmandatoryperc"] = value;
            }
        }
        public static int debetperc
        {
            get
            {
                try { return (int)localSettings.Values["debetperc"]; }
                catch { return 20; }
            }
            set
            {
                localSettings.Values["debetperc"] = value;
            }
        }
        #endregion

        public static string languageID
        {
            get
            {
                try
                {
                    string res = (string)localSettings.Values["languageID"];
                    if (string.IsNullOrEmpty(res))
                        res = "";
                    return res;
                }
                catch { return ""; }
            }
            set
            {
                localSettings.Values["languageID"] = value;
            }
        }

        public enum AccountsDisplayMode
        {
            Active = 0,
            Selected = 1,
            All = 2
        }

        public enum HistoryMode
        {
            Week = 0,
            Month = 1,
            HalfYear = 2,
            Year = 3,
            All = 4
        }

        public static AccessToken getToken()
        {
            return new AccessToken(tokenkey, tokensecret);
        }

        public static void saveCredentials(string Login, string Password, AccessToken token)
        {
            tokenkey = token.Key;
            tokensecret = token.Secret;
            login = Login;
            password = Password;
        }

        public static void saveCredentials(AccessToken token)
        {
            tokenkey = token.Key;
            tokensecret = token.Secret;
        }

        public static void deleteCredentials()
        {
            tokenkey = "";
            tokensecret = "";
            login = "";
            password = "";
            PIN = "";
            lastsynctime = 0;
        }
    }
}
