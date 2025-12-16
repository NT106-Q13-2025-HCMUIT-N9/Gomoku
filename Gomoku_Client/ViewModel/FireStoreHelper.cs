using Gomoku_Client.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Cloud.Firestore;
using System.Linq.Expressions;

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

        public static async Task<UserDataModel?> GetUserInfo(string username)
        {
            DocumentReference doc_ref = FirebaseInfo.DB.Collection("UserInfo").Document(username);
            DocumentSnapshot doc_snap = await doc_ref.GetSnapshotAsync();
            if (doc_snap.Exists)
            {
                UserDataModel user_data = doc_snap.ConvertTo<UserDataModel>();
                return user_data;
            }
            else
            {
                return null;
            }
        }

        public static async Task<bool> AcceptFriendRequest(string user1, string user2)
        {
            try
            {
                DocumentReference doc1_ref = FirebaseInfo.DB.Collection("UserInfo").Document(user1);
                DocumentSnapshot user1_doc_snap = await doc1_ref.GetSnapshotAsync();

                DocumentReference doc2_ref = FirebaseInfo.DB.Collection("UserInfo").Document(user2);
                DocumentSnapshot user2_doc_snap = await doc2_ref.GetSnapshotAsync();
                if (user2_doc_snap.Exists && user1_doc_snap.Exists)
                {
                    UserDataModel user1_data = user1_doc_snap.ConvertTo<UserDataModel>();
                    user1_data.Friends.Add(user2);
                    await doc1_ref.SetAsync(user1_data);

                    UserDataModel user2_data = user2_doc_snap.ConvertTo<UserDataModel>();
                    user2_data.Friends.Add(user1);
                    await doc2_ref.SetAsync(user2_data);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static async Task<bool> DeleteFriendRequest(string user_have_req, string user_sent_req)
        {
            try
            {
                DocumentReference doc_ref = FirebaseInfo.DB.Collection("UserInfo").Document(user_have_req);
                DocumentSnapshot doc_snap = await doc_ref.GetSnapshotAsync();
                if (doc_snap.Exists)
                {
                    UserDataModel user_data = doc_snap.ConvertTo<UserDataModel>();

                    user_data.FriendsRequests.Remove(user_sent_req);
                    await doc_ref.SetAsync(user_data);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static async Task<List<string>?> GetNewFriendReqest(string username, List<string> old_friend_request)
        {
            DocumentReference doc_ref = FirebaseInfo.DB.Collection("UserInfo").Document(username);
            DocumentSnapshot doc_snap = await doc_ref.GetSnapshotAsync();
            if (doc_snap.Exists)
            {
                UserDataModel user_data = doc_snap.ConvertTo<UserDataModel>();

                List<string> diff = user_data.FriendsRequests.Except(old_friend_request).ToList();
                return diff;
            }
            else
            {
                return null;
            }
        }

        public static async Task<List<string>?> GetNewFriend(string username, List<string> old_friend_list)
        {
            DocumentReference doc_ref = FirebaseInfo.DB.Collection("UserInfo").Document(username);
            DocumentSnapshot doc_snap = await doc_ref.GetSnapshotAsync();
            if (doc_snap.Exists)
            {
                UserDataModel user_data = doc_snap.ConvertTo<UserDataModel>();

                List<string> diff = user_data.Friends.Except(old_friend_list).ToList();
                return diff;
            }
            else
            {
                return null;
            }
        }

        public static async Task<bool> Unfriend(string user1, string user2)
        {
            try
            {
                DocumentReference doc1_ref = FirebaseInfo.DB.Collection("UserInfo").Document(user1);
                DocumentSnapshot user1_doc_snap = await doc1_ref.GetSnapshotAsync();

                DocumentReference doc2_ref = FirebaseInfo.DB.Collection("UserInfo").Document(user2);
                DocumentSnapshot user2_doc_snap = await doc2_ref.GetSnapshotAsync();
                if (user2_doc_snap.Exists && user1_doc_snap.Exists)
                {
                    UserDataModel user1_data = user1_doc_snap.ConvertTo<UserDataModel>();
                    user1_data.Friends.Remove(user2);
                    await doc1_ref.SetAsync(user1_data);

                    UserDataModel user2_data = user2_doc_snap.ConvertTo<UserDataModel>();
                    user2_data.Friends.Remove(user1);
                    await doc2_ref.SetAsync(user2_data);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static async Task<UserStatsModel?> GetUserStats(string username)
        {
            DocumentReference doc_ref = FirebaseInfo.DB.Collection("UserStats").Document(username);
            DocumentSnapshot doc_snap = await doc_ref.GetSnapshotAsync();
            if (doc_snap.Exists)
            {
                UserStatsModel user_stats = doc_snap.ConvertTo<UserStatsModel>();
                return user_stats;
            }
            else
            {
                return null;
            }
        }
    }
}
