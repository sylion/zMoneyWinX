using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;

namespace zMoneyWinX.Client
{
    public static class OAuth
    {
        private const string CONSUMER_KEY = "g71fe65f09a6c2c0cbe80906e2fbba";
        private const string CONSUMER_SECRET = "c7b07571a8";
        private const string API_BASE_URI = "http://api.zenmoney.ru";
        private const string API_REQUEST_TOKEN_ENDPOINT = "http://api.zenmoney.ru/oauth/request_token";
        private const string API_ACCESS_TOKEN_ENDPOINT = "http://api.zenmoney.ru/oauth/access_token";
        public static readonly Uri DiffUri = new Uri(API_BASE_URI + "/v7/diff");
        private static Uri BuildUri(string method) => new Uri(API_BASE_URI + "/" + method);

        private static string OAuth_token;
        private static string OAuth_token_secret;

        public static async Task<AccessToken> getToken(string Login, string Password)
        {
            //Get request token
            var parm = OAuth.GetOAuthParameters();
            var sig = OAuth.CalculateOAuthSignedUrl(parm, API_REQUEST_TOKEN_ENDPOINT, false);
            var resp = await OAuth.GetResponseFromWeb(sig);
            OAuth.SetRequestToken(resp);
            string verificationCode;

            //Get verifier
            using (var client = new HttpClient())
            {
                var pinRequestUrl = BuildUri("access").ToString() + "?oauth_token=" + OAuth.OAuth_token + "&mobile";

                using (var query = new FormUrlEncodedContent(new Collection<KeyValuePair<string, string>>()
                    {
                        new KeyValuePair<string, string>("login", Login),
                        new KeyValuePair<string, string>("password", Password)
                    }))
                {
                    using (var request = await client.PostAsync(pinRequestUrl, query))
                    {
                        if (request.StatusCode != HttpStatusCode.OK)
                            throw new UnauthorizedAccessException("Bad Auth, status code is not OK");

                        var args = ParseQueryString(request.RequestMessage.RequestUri.ToString());

                        if (args.ContainsKey("oauth_verifier"))
                        {
                            verificationCode = Uri.UnescapeDataString(args["oauth_verifier"]);
                        }
                        else
                        {
                            throw new UnauthorizedAccessException("Empty verification code");
                        }
                    }
                }
            }

            //Get access token
            parm = OAuth.GetOAuthParameters(verificationCode);
            sig = OAuth.CalculateOAuthSignedUrl(parm, API_ACCESS_TOKEN_ENDPOINT, true);
            resp = await OAuth.GetResponseFromWeb(sig);
            OAuth.SetRequestToken(resp);

            var OAuth_token = OAuth.OAuth_token;
            var OAuth_token_secret = OAuth.OAuth_token_secret;

            OAuth.OAuth_token = string.Empty;
            OAuth.OAuth_token_secret = string.Empty;

            return new AccessToken(OAuth_token, OAuth_token_secret);
        }

        private static IDictionary<string, string> ParseQueryString(string uri)
        {
            var substring = uri.Substring(((uri.LastIndexOf('?') == -1) ? 0 : uri.LastIndexOf('?') + 1));
            var pairs = substring.Trim('&', 'm', 'o', 'b', 'i', 'l', 'e').Split('&');
            return pairs.Select(piece => piece.Split('=')).ToDictionary(pair => pair[0], pair => pair[1]);
        }

        private static void SetRequestToken(string response)
        {
            string[] keyValPairs = response.Split('&');
            for (int i = 0; i < keyValPairs.Length; i++)
            {
                String[] splits = keyValPairs[i].Split('=');
                switch (splits[0])
                {
                    case "oauth_token":
                        {
                            OAuth_token = splits[1];
                            break;
                        }
                    case "oauth_token_secret":
                        {
                            OAuth_token_secret = splits[1];
                            break;
                        }
                }
            }
        }

        private async static Task<string> GetResponseFromWeb(string url)
        {
            HttpWebRequest Request = (HttpWebRequest)WebRequest.Create(url);
            string httpResponse = null;
            Request.Method = "GET";
            using (HttpWebResponse response = (HttpWebResponse)await Request.GetResponseAsync())
            {
                if (response != null)
                {
                    StreamReader data = new StreamReader(response.GetResponseStream());
                    httpResponse = await data.ReadToEndAsync();
                }
            }
            return httpResponse;
        }

        public static string getOAuthSign(AccessToken token)
        {
            SortedDictionary<string, string> sortedParams = OAuth.GetOAuthParameters();
            StringBuilder baseString = new StringBuilder();
            string baseStringForSig;

            foreach (var param in sortedParams)
            {
                baseString.Append(param.Key);
                baseString.Append("=");
                baseString.Append("\"" + Uri.EscapeDataString(param.Value) + "\"");
                baseString.Append(",");
            }

            var baseStr = baseString.ToString().TrimEnd(',');
            baseStringForSig = Uri.EscapeDataString(baseStr);

            IBuffer dataString = CryptographicBuffer.ConvertStringToBinary(baseStringForSig, BinaryStringEncoding.Utf8);

            return baseStr + "oauth_token=\"" + token.Key + "\",oauth_signature=\"" +
                Uri.EscapeDataString(CryptographicBuffer.EncodeToBase64String(CryptographicEngine.Sign(getCryptoKey(token.Secret), dataString))) + "\"";
        }

        private static string CalculateOAuthSignedUrl(SortedDictionary<string, string> sortedParams, string url, bool exchangeStep)
        {
            StringBuilder baseString = new StringBuilder();
            string baseStringForSig;

            foreach (var param in sortedParams)
            {
                baseString.Append(param.Key);
                baseString.Append("=");
                baseString.Append(Uri.EscapeDataString(param.Value));
                baseString.Append("&");
            }

            var baseStr = baseString.ToString().TrimEnd('&');
            baseStringForSig = "GET&" + Uri.EscapeDataString(url) + "&" + Uri.EscapeDataString(baseStr);

            IBuffer dataString = CryptographicBuffer.ConvertStringToBinary(baseStringForSig, BinaryStringEncoding.Utf8);
            if (exchangeStep)
                return url + "?" + baseStr + "&oauth_signature=" +
                Uri.EscapeDataString(CryptographicBuffer.EncodeToBase64String(CryptographicEngine.Sign(getCryptoKey(OAuth_token_secret), dataString)));
            else
                return url + "?" + baseStr + "&oauth_signature=" +
                Uri.EscapeDataString(CryptographicBuffer.EncodeToBase64String(CryptographicEngine.Sign(getCryptoKey(), dataString)));
        }

        private static CryptographicKey getCryptoKey(string secret = null)
        {
            MacAlgorithmProvider HmacSha1Provider = MacAlgorithmProvider.OpenAlgorithm("HMAC_SHA1");
            if (!string.IsNullOrEmpty(secret))
                return HmacSha1Provider.CreateKey(CryptographicBuffer.ConvertStringToBinary(CONSUMER_SECRET + "&" + secret, BinaryStringEncoding.Utf8));
            else
                return HmacSha1Provider.CreateKey(CryptographicBuffer.ConvertStringToBinary(CONSUMER_SECRET + "&", BinaryStringEncoding.Utf8));
        }

        private static SortedDictionary<string, string> GetOAuthParameters(string verifier = "")
        {
            SortedDictionary<string, string> parameters = new SortedDictionary<string, string>();
            parameters.Add("oauth_nonce", Nonce);
            parameters.Add("oauth_timestamp", SettingsManager.toUnixTime(DateTime.UtcNow).ToString());
            parameters.Add("oauth_consumer_key", CONSUMER_KEY);
            parameters.Add("oauth_signature_method", "HMAC-SHA1");
            if (!string.IsNullOrEmpty(verifier))
            {
                parameters.Add("oauth_token", OAuth_token);
                parameters.Add("oauth_verifier", verifier);
            }
            parameters.Add("oauth_version", "1.0");
            return parameters;
        }

        private static string Nonce
        {
            get
            {
                return new Random().Next(1000).ToString();
            }
        }
    }

    public sealed class AccessToken
    {
        public AccessToken(string key, string secret)
        {
            Key = key;
            Secret = secret;
        }

        public bool valid()
        {
            if (string.IsNullOrEmpty(Key) || string.IsNullOrEmpty(Secret))
                return false;
            else
                return true;
        }

        public readonly string Key;
        public readonly string Secret;
    }
}
