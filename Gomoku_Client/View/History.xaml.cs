using Gomoku_Client.Model;
using Google.Cloud.Firestore;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Gomoku_Client.View
{
    /// <summary>
    /// Interaction logic for History.xaml
    /// </summary>
    public partial class History : Page
    {
        // Truyền tham số MainGameUI để có thể quay lại bằng BackButton
        private MainGameUI _mainWindow;
        private FirestoreChangeListener? listener = null;

        public History(MainGameUI mainGameUI)
        {
            InitializeComponent();
            _mainWindow = mainGameUI;
        }

        private void BackButton_Checked(object sender, RoutedEventArgs e)
        {
            
            if (_mainWindow == null)
            {
                MessageBox.Show("Không tìm thấy cửa sổ chính.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            _mainWindow.ButtonClick.Stop();
            _mainWindow.ButtonClick.Play();
            _mainWindow.ShowMenuWithAnimation();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            MatchListPanel.Children.Clear();

            CollectionReference match_info_ref = FirebaseInfo.DB.Collection("MatchInfo");
            Query query = match_info_ref.WhereArrayContains("Players", FirebaseInfo.AuthClient.User.Info.DisplayName);
            listener = query.Listen(snapshot => { 
                foreach(DocumentChange change in snapshot.Changes)
                {
                    DocumentSnapshot doc = change.Document;
                    if (doc.Exists)
                    {
                        MatchInfoModel match_info = doc.ConvertTo<MatchInfoModel>();
                        string curr_user = FirebaseInfo.AuthClient.User.Info.DisplayName;
                        string? opponent = match_info.Players.FirstOrDefault(f => f != curr_user);

                        string minute = (match_info.Duration / 60).ToString("D2");
                        string sec = (match_info.Duration % 60).ToString("D2");
                        string duration = minute + " phút " + sec + " giây";

                        if (match_info.isDraw)
                        {
                            Debug.WriteLine(minute);

                            App.Current.Dispatcher.Invoke(() =>
                            {
                                Border drawMatchItem = UIUtils.CreateDrawMatchItem(opponent ?? "NULL", duration);
                                MatchListPanel.Children.Add(drawMatchItem);
                            });
                        }
                        else
                        {
                            if(match_info.Winner == curr_user)
                            {
                                App.Current.Dispatcher.Invoke(() =>
                                {
                                    Border drawMatchItem = UIUtils.CreateWinMatchItem(opponent ?? "NULL", duration);
                                    MatchListPanel.Children.Add(drawMatchItem);
                                });
                            }
                            else
                            {
                                App.Current.Dispatcher.Invoke(() =>
                                {
                                    Border drawMatchItem = UIUtils.CreateLoseMatchItem(opponent ?? "NULL", duration);
                                    MatchListPanel.Children.Add(drawMatchItem);
                                });
                            }
                        }
                    }
                }
            });
        }

        private async void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            if (listener != null)
            {
                await listener.StopAsync();
                listener = null;
            }
        }
    }
}
