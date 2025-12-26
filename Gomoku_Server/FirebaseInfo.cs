using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Gomoku_Server
{
    public static class FirebaseInfo
    {
        private static FirebaseApp? _app = null;
        private static string _projectId = "gomoku-ltmcb";
        private static FirestoreDb? _db = null;
        private static object lock_db = new object();

        public static FirestoreDb DB
        {
            get
            {
                lock (lock_db)
                {
                    if (_db == null)
                    {
                        AppInit();
                    }
                    return _db!;
                }
            }
        }

        private static Stream GetEmbeddedStream(string filename)
        {
            var assembly = Assembly.GetExecutingAssembly();

            string? resourceName = assembly.GetManifestResourceNames()
                .FirstOrDefault(str => str.EndsWith(filename));

            if (string.IsNullOrEmpty(resourceName))
            {
                var existing = string.Join("\n", assembly.GetManifestResourceNames());
                throw new Exception($"FATAL: Không tìm thấy Embedded Resource '{filename}'.\n" +
                                    $"Hãy chắc chắn bạn đã set Build Action là 'Embedded Resource'.\n" +
                                    $"Danh sách Resource hiện có:\n{existing}");
            }

            return assembly.GetManifestResourceStream(resourceName)!;
        }

        public static void AppInit()
        {
            if (_db != null) return;

            try
            {
                using (var stream = GetEmbeddedStream("firebase_key.json"))
                {
                    Logger.Log($"[DEBUG] stream: {stream != null}");
                    var serviceAccount = CredentialFactory.FromStream<ServiceAccountCredential>(stream);
                    GoogleCredential credential = serviceAccount.ToGoogleCredential();

                    if (FirebaseApp.DefaultInstance == null)
                    {
                        _app = FirebaseApp.Create(new AppOptions()
                        {
                            Credential = credential,
                            ProjectId = _projectId
                        });
                    }
                    else
                    {
                        _app = FirebaseApp.DefaultInstance;
                    }

                    FirestoreDbBuilder builder = new FirestoreDbBuilder
                    {
                        ProjectId = _projectId,
                        Credential = credential
                    };

                    _db = builder.Build();
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"[FIREBASE ERROR] {ex.Message}");
                throw;
            }
        }
    }
}