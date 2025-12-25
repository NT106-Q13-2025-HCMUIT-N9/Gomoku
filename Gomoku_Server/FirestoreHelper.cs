using Gomoku_Server.Model;
using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gomoku_Server
{
    public static class FirestoreHelper
    {
        public static async void AddMatchInfo(bool isDraw, List<string> Players, int duration, string? winner = null)
        {
            DocumentReference new_doc_ref = FirebaseInfo.DB.Collection("MatchInfo").Document();

            MatchInfoModel matchInfoModel = new MatchInfoModel()
            {
                isDraw = isDraw,
                Players = Players,
                Duration = duration,
                CreateTime = FieldValue.ServerTimestamp
            };

            if (isDraw)
            {
                await new_doc_ref.SetAsync(matchInfoModel);
            }
            else
            {
                matchInfoModel.Winner = winner;
                await new_doc_ref.SetAsync(matchInfoModel);
            }
        }

        public static async void IncWinUser(string username)
        {
            DocumentReference doc_ref = FirebaseInfo.DB.Collection("UserStats").Document(username);
            await doc_ref.UpdateAsync("Wins", FieldValue.Increment(1));
            await doc_ref.UpdateAsync("total_match", FieldValue.Increment(1));
        }

        public static async void IncLoseUser(string username)
        {
            DocumentReference doc_ref = FirebaseInfo.DB.Collection("UserStats").Document(username);
            await doc_ref.UpdateAsync("Losses", FieldValue.Increment(1));
            await doc_ref.UpdateAsync("total_match", FieldValue.Increment(1));
        }

        public static async void IncDrawUser(string username)
        {
            DocumentReference doc_ref = FirebaseInfo.DB.Collection("UserStats").Document(username);
            await doc_ref.UpdateAsync("Draws", FieldValue.Increment(1));
            await doc_ref.UpdateAsync("total_match", FieldValue.Increment(1));
        }
    }
}
