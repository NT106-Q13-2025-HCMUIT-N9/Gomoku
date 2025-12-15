using Gomoku_Client.Model;
using Google.Cloud.Firestore;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Gomoku_Client.ViewModel
{
    class Validate
    {
        public static bool IsValidEmail(string email)
        {
            string pattern = @"^[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?$";
            return Regex.IsMatch(email, pattern);
        }

        public static async Task<bool> IsUsernamExists(string username)
        {
            DocumentReference doc_ref = FirebaseInfo.DB.Collection("UserInfo").Document(username);
            DocumentSnapshot doc_snap = await doc_ref.GetSnapshotAsync();
            return doc_snap.Exists;
        }

        public static async Task<bool> IsEmailExists(string email)
        {
            CollectionReference user_collection = FirebaseInfo.DB.Collection("UserInfo");
            QuerySnapshot query_result = await user_collection.WhereEqualTo("Email", email).GetSnapshotAsync();
            return query_result.Count != 0;
        }
    }
}
