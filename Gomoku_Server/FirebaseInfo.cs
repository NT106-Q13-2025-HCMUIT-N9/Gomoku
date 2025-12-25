using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                        _db = FirestoreDb.Create(_projectId);
                    }
                    return _db;
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

            _db = FirestoreDb.Create(_projectId);
        }
    }
}
