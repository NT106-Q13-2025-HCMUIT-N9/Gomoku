using Firebase.Auth;
using Firebase.Auth.Providers;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Gomoku_Client.Model
{
    internal class FirebaseInfo
    {
        private static FirebaseApp? _app = null;

        private static object lock_app = new object();
        private static object lock_auth = new object();
        private static object lock_db = new object();

        private static string _projectId = "gomoku-ltmcb";
        private static string? _api_key = null;

        private static FirestoreDb? _db = null;

        private static FirebaseAuthClient? _authClient = null;

        private FirebaseInfo() { }

        public static string ProjectId { get => _projectId; }

        private static Stream GetEmbeddedStream(string filename)
        {
            var assembly = Assembly.GetExecutingAssembly();

            string? resourceName = assembly.GetManifestResourceNames()
                                          .FirstOrDefault(str => str.EndsWith(filename));

            if (string.IsNullOrEmpty(resourceName))
            {
                var existing = string.Join("\n", assembly.GetManifestResourceNames());
                throw new Exception($"Không tìm thấy file embedded '{filename}'! Đảm bảo Build Action là Embedded Resource.\nDanh sách có sẵn:\n{existing}");
            }

            return assembly.GetManifestResourceStream(resourceName)!;
        }

        public static string ApiKey
        {
            get
            {
                if (_api_key == null)
                {
                    using (var stream = GetEmbeddedStream("api.env"))
                    using (var reader = new StreamReader(stream))
                    {
                        _api_key = reader.ReadToEnd().Trim();
                    }
                }
                return _api_key;
            }
        }

        public static FirestoreDb DB
        {
            get
            {
                lock (lock_db)
                {
                    if (_db == null)
                    {

                        if (_app == null) AppInit();

                    }
                    return _db!;
                }
            }
        }

        public static FirebaseApp App
        {
            get
            {
                lock (lock_app)
                {
                    if (_app == null)
                    {
                        AppInit();
                    }
                    return _app!;
                }
            }
        }

        public static void AppInit()
        {

            using (var stream = GetEmbeddedStream("firebase_key.json"))
            {
                var serviceAccount = CredentialFactory.FromStream<ServiceAccountCredential>(stream);
                GoogleCredential credential = serviceAccount.ToGoogleCredential();
                if (FirebaseApp.DefaultInstance == null)
                {
                    _app = FirebaseApp.Create(new AppOptions()
                    {
                        Credential = credential
                    });
                }
                else
                {
                    _app = FirebaseApp.DefaultInstance;
                }

                FirestoreClientBuilder builder = new FirestoreClientBuilder
                {
                    Credential = credential
                };

                _db = FirestoreDb.Create(_projectId, builder.Build());
            }
        }

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
                            Providers = new FirebaseAuthProvider[]
                            {
                                new GoogleProvider().AddScopes("email"),
                                new EmailProvider()
                            }
                        };

                        _authClient = new FirebaseAuthClient(config);
                    }
                    return _authClient;
                }
            }
        }
    }
}