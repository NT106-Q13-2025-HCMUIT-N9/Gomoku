using Firebase.Auth;
using Gomoku_Client.Model;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Gomoku_Client.ViewModel
{
    public class UserAction
    {
        public static async Task SendVeriAsync(string id_token)
        {
            string oobs_uri = $"https://identitytoolkit.googleapis.com/v1/accounts:sendOobCode?key={FirebaseInfo.ApiKey}";

            using (HttpClient client = new HttpClient())
            {
                var requestBody = new
                {
                    requestType = "VERIFY_EMAIL",
                    idToken = id_token,
                    returnSecureToken = true
                };


                string jsonRequest = JsonConvert.SerializeObject(requestBody);
                StringContent update_content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(oobs_uri, update_content);
                if (!response.IsSuccessStatusCode)
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    JObject errorJson = JObject.Parse(errorContent);
                    string errorMessage = errorJson["error"]?["message"]?.ToString() ?? (response.ReasonPhrase ?? "");

                    throw new AuthException(errorMessage);
                }
            }
        }

        public static async Task SendResetAsync(string email)
        {
            string oobs_uri = $"https://identitytoolkit.googleapis.com/v1/accounts:sendOobCode?key={FirebaseInfo.ApiKey}";

            using (HttpClient client = new HttpClient())
            {
                var requestBody = new
                {
                    requestType = "PASSWORD_RESET",
                    email = email,
                    returnSecureToken = true
                };


                string jsonRequest = JsonConvert.SerializeObject(requestBody);
                StringContent update_content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(oobs_uri, update_content);
                if (!response.IsSuccessStatusCode)
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    JObject errorJson = JObject.Parse(errorContent);
                    string errorMessage = errorJson["error"]?["message"]?.ToString() ?? (response.ReasonPhrase ?? "");

                    throw new AuthException(errorMessage);
                }
            }
        }

        public static async Task LoginEmailtAsync(string email, string password)
        {
            string oobs_uri = $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={FirebaseInfo.ApiKey}";

            using (HttpClient client = new HttpClient())
            {
                var requestBody = new
                {
                    email = email,
                    password = password,
                    returnSecureToken = true
                };


                string jsonRequest = JsonConvert.SerializeObject(requestBody);
                StringContent update_content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(oobs_uri, update_content);
                if (!response.IsSuccessStatusCode)
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    JObject errorJson = JObject.Parse(errorContent);
                    string errorMessage = errorJson["error"]?["message"]?.ToString() ?? (response.ReasonPhrase ?? "");

                    throw new AuthException(errorMessage);
                }
            }
        }
    }
}