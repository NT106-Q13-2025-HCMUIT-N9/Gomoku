using Gomoku_Client.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Cloud.Firestore;

namespace Gomoku_Client.ViewModel
{
    public class FireStoreHelper
    {
        public static async Task AddUser(UserDataModel user_data)
        {
            UserStatsModel userStatsModel = new UserStatsModel
            {
                UserHandle = user_data.Username,
                Wins = 0,
                Draws = 0,
                Losses = 0,
                total_match = 0
            };

            DocumentReference doc_ref = FirebaseInfo.DB.Collection("UserStats").Document(user_data.Username);
            await doc_ref.SetAsync(userStatsModel);

            user_data.UserStats = doc_ref;
            DocumentReference user_collection = FirebaseInfo.DB.Collection("UserInfo").Document(user_data.Username);
            await user_collection.SetAsync(user_data);
        }
    }
}
