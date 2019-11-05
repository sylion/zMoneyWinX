using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using zMoneyWinX.Model;

namespace zMoneyWinX.Client
{
    static class Client
    {
        private static async Task<DiffResponseObject> diff(AccessToken accessToken)
        {
            using (var _sessionHttpClient = new HttpClient())
            {
                _sessionHttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                _sessionHttpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "OAuth " + OAuth.getOAuthSign(accessToken));

                var diffObject = await DBInitializer.getDiffObject();
                var jsonData = JsonConvert.SerializeObject(diffObject);

                using (var content = new StringContent(jsonData, Encoding.UTF8, "application/json"))
                {
                    using (var response = await _sessionHttpClient.PostAsync(OAuth.DiffUri, content))
                    {
                        var json = await response.Content.ReadAsStringAsync();

                        if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                            throw new HttpRequestException("Bad request: " + json + " || Diff object: " + jsonData);

                        if (response.StatusCode != System.Net.HttpStatusCode.OK)
                            throw new UnauthorizedAccessException("Auth exception: " + json);

                        try
                        {
                            //TEST server responce localy
                            //string filepath = @"Assets\data.json";
                            //Windows.Storage.StorageFolder folder = Windows.ApplicationModel.Package.Current.InstalledLocation;
                            //Windows.Storage.StorageFile file = await folder.GetFileAsync(filepath);
                            //json = await Windows.Storage.FileIO.ReadTextAsync(file);

                            return JsonConvert.DeserializeObject<DiffResponseObject>(json);
                        }
                        catch (Exception e)
                        {
                            throw new JsonException("Serialization exception: " + e.Message + " || Server response object: " + json);
                        }
                    }
                }
            }
        }

        public static async Task<DiffResponseObject> diff()
        {
            AccessToken accessToken = SettingsManager.getToken();
            if (accessToken == null || !accessToken.valid())
                throw new UnauthorizedAccessException("Bad or empty access token");
            try
            {
                return await diff(accessToken);
            }
            catch
            {
                try
                {
                    accessToken = await OAuth.getToken(SettingsManager.login, SettingsManager.password);
                    if (accessToken == null || !accessToken.valid())
                        throw new UnauthorizedAccessException("Bad or empty access token");
                    SettingsManager.saveCredentials(accessToken);
                    return await diff(accessToken);
                }
                catch (JsonException error)
                {
                    App.Error = error.Message;
                    throw error;
                }
                catch (HttpRequestException error)
                {
                    App.Error = error.Message;
                    throw error;
                }
                catch (Exception error)
                {
                    throw error;
                }
            }
        }
    }
}
