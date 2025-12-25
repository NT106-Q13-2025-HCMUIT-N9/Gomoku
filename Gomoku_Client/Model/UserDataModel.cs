using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gomoku_Client.Model
{
    [FirestoreData]
    public class UserStatsModel
    {
        [FirestoreProperty]
        public int Draws { get; set; }

        [FirestoreProperty]
        public int Losses { get; set; }

        [FirestoreProperty]
        public int Wins { get; set; }

        [FirestoreProperty]
        public int total_match { get; set; }
    }

    [FirestoreData]
    public class UserDataModel
    {
        [FirestoreProperty]
        public string Username { get; set; }

        [FirestoreProperty]
        public string Email { get; set; }

        [FirestoreProperty]
        public DocumentReference UserStats { get; set; }

        [FirestoreProperty]
        public List<string> Friends { get; set; }

        [FirestoreProperty]
        public List<string> FriendsRequests { get; set; }

        [FirestoreProperty]
        public List<string> MatchRequests { get; set; }

        [FirestoreProperty]
        public string ImagePath { get; set; }
    }

    [FirestoreData]
    public class MatchInfoModel
    {
        [FirestoreProperty]
        public List<string> Players { get; set; }

        [FirestoreProperty]
        public bool isDraw { get; set; }

        [FirestoreProperty]
        public string? Winner { get; set; }

        [FirestoreProperty]
        public int Duration { get; set; }

        [FirestoreProperty]
        public object? CreateTime { get; set; }
    }
}
