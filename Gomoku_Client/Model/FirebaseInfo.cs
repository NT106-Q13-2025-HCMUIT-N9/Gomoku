using Firebase.Auth;
using Firebase.Auth.Providers;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace Gomoku_Client.Model
{
    internal class FirebaseInfo
    {
        private static FirebaseApp? _app = null;

        private static object lock_app = new object();
        private static object lock_auth = new object();

        private static string _projectId = " Gomoku-ltmcb";
        private static string? _api_key = null;

        private static FirebaseAuthProvider[] _authProvider =
        [
            new GoogleProvider().AddScopes("email"),
            new EmailProvider()
        ];

        private static FirebaseAuthClient? _authClient = null;

        private FirebaseInfo() { }

        public static string ProjectId { get => _projectId; }

        public static FirebaseAuthProvider[] AuthProviders { get => _authProvider; }

        public static FirebaseAuthClient AuthClient
        {
            get
            {
                lock (lock_auth)
                {
                    if (_authClient == null)
                    {
                        FirebaseAuthConfig config = new FirebaseAuthConfig
                        {
                            ApiKey = FirebaseInfo.ApiKey,
                            AuthDomain = "gomoku-ltmcb.firebaseapp.com",
                            Providers = _authProvider
                        };

                        _authClient = new FirebaseAuthClient(config);
                    }
                    return _authClient;
                }
            }
        }

        public static string ApiKey
        {
            get
            {
                if(_api_key == null)
                {
                    string jsonFilePath = @"..\..\..\Assets\api.env";
                    string fullPath = Path.GetFullPath(jsonFilePath);
                    _api_key = File.ReadAllText(fullPath).Trim();
                }
                return _api_key;
            }
        }

        public static FirebaseApp App
        {
            get {
                lock (lock_app)
                {
                    if(_app == null)
                    {
                        AppInit();
                    }
                    return _app;
                }
            }
        }

        public static void AppInit()
        {
            string jsonFilePath = @"..\..\..\Assets\firebase_key.json";
            string fullPath = Path.GetFullPath(jsonFilePath);
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", fullPath);

            _app = FirebaseApp.Create(new AppOptions()
            {
                Credential = CredentialFactory.FromFile<ServiceAccountCredential>(fullPath).ToGoogleCredential()
            });
        }
    }
}
