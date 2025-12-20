using Gomoku_Client.Model;
using Gomoku_Client.ViewModel;
using Google.Cloud.Firestore;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using static Google.Rpc.Context.AttributeContext.Types;

namespace Gomoku_Client.View
{
    /// <summary>
    /// Interaction logic for FriendManager.xaml
    /// </summary>
    public partial class FriendManager : Page
    {
        private MainGameUI _mainWindow;
        private List<string> curr_friend_request = new List<string>();
        private List<string> curr_friend_list = new List<string>();
        private string? _pendingUnfriendUsername;

        FirestoreChangeListener? listener;

        public FriendManager(MainGameUI mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
        }

        private void BackButton_Checked(object sender, RoutedEventArgs e)
        {
            _mainWindow.ButtonClick.Stop();
            _mainWindow.ButtonClick.Play();
            if (_mainWindow == null)
            {
                MessageBox.Show("Không tìm thấy cửa sổ chính.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            _mainWindow.ShowMenuWithAnimation();
        }

        private async void SendFriendRequest_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.ButtonClick.Stop();
            _mainWindow.ButtonClick.Play();
            if (string.IsNullOrEmpty(FriendUsernameInput.Text))
            {
                MessageBox.Show("Vui long nhap username ban muon ket ban");
                return;
            }

            if(!await Validate.IsUsernamExists(FriendUsernameInput.Text))
            {
                MessageBox.Show("Username khong ton tai");
                return;
            }

            string username = FirebaseInfo.AuthClient.User.Info.DisplayName;
            await FireStoreHelper.SendFriendRequest(username, FriendUsernameInput.Text);
            MessageBox.Show("Thanks i sent it");
        }


        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.ButtonClick.Stop();
            _mainWindow.ButtonClick.Play();
            var border = (Border)((Grid)UnfriendConfirmationOverlay).Children[0];
            Storyboard fadeOut = (Storyboard)FindResource("PopupFadeOut");
            fadeOut.Begin(border);
            fadeOut.Completed += (s, args) =>
            {
                UnfriendConfirmationOverlay.Visibility = Visibility.Collapsed;
            };

            fadeOut.Begin(border);
        }

        private async void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.ButtonClick.Stop();
            _mainWindow.ButtonClick.Play();
            if (string.IsNullOrEmpty(_pendingUnfriendUsername)) return;

            string username = FirebaseInfo.AuthClient.User.Info.DisplayName;

            bool success = await FireStoreHelper.Unfriend(username, _pendingUnfriendUsername);

            if (success)
            {
                curr_friend_list.Remove(_pendingUnfriendUsername);

                var cardToDelete = FriendsListPanel.Children
                    .OfType<FrameworkElement>()
                    .FirstOrDefault(c => c.Name == _pendingUnfriendUsername);

                if (cardToDelete != null)
                {
                    FriendsListPanel.Children.Remove(cardToDelete);
                }

                CancelButton_Click(sender, e);
                _pendingUnfriendUsername = null;
            }
        }

        private async void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.ButtonClick.Stop();
            _mainWindow.ButtonClick.Play();
            Button? sender_handle = sender as Button;
            string username = FirebaseInfo.AuthClient.User.Info.DisplayName;

            if (sender_handle != null)
            {
                bool successed = await FireStoreHelper.AcceptFriendRequest(username, sender_handle.Name);
                if (successed)
                {
                    await DeleteRequestCard(sender_handle.Name);
                    await FireStoreHelper.DeleteFriendRequest(username, sender_handle.Name);
                    curr_friend_request.Remove(sender_handle.Name);
                }
            }
        }

        private async void RefuseButton_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.ButtonClick.Stop();
            _mainWindow.ButtonClick.Play();
            Button? sender_handle = sender as Button;
            string username = FirebaseInfo.AuthClient.User.Info.DisplayName;

            if (sender_handle != null)
            {
                await DeleteRequestCard(sender_handle.Name);
                await FireStoreHelper.DeleteFriendRequest(username, sender_handle.Name);
                curr_friend_request.Remove(sender_handle.Name);
            }
        }

        private void ChallengeButton_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.ButtonClick.Stop();
            _mainWindow.ButtonClick.Play();
        }

        private void UnfriendButton_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.ButtonClick.Stop();
            _mainWindow.ButtonClick.Play();
            Button? sender_handle = sender as Button;
            _pendingUnfriendUsername = sender_handle?.Name;

            UnfriendConfirmationOverlay.Visibility = Visibility.Visible;
            var border = (Border)((Grid)UnfriendConfirmationOverlay).Children[0];
            Storyboard fadeIn = (Storyboard)FindResource("PopupFadeIn");
            fadeIn.Begin(border);
        }

        private async Task DeleteRequestCard(string name)
        {
            await Task.Yield();
            var borderToDelete = FriendRequestsPanel.Children
            .OfType<Border>()
            .FirstOrDefault(b => b.Name == name);

            if (borderToDelete != null)
            {
                FriendRequestsPanel.Children.Remove(borderToDelete);
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            curr_friend_request.Clear();
            curr_friend_list.Clear();
            FriendRequestsPanel.Children.Clear();
            FriendsListPanel.Children.Clear();

            string username = FirebaseInfo.AuthClient.User.Info.DisplayName;
            Google.Cloud.Firestore.DocumentReference doc_ref = FirebaseInfo.DB.Collection("UserInfo").Document(username);
            listener = doc_ref.Listen(doc_snap => {
                if (doc_snap.Exists)
                {
                    UserDataModel user_data = doc_snap.ConvertTo<UserDataModel>();

                    List<string> diff_request = user_data.FriendsRequests.Except(curr_friend_request).ToList();
                    List<string> diff_friend = user_data.Friends.Except(curr_friend_list).ToList();

                    App.Current.Dispatcher.Invoke(() =>
                    {
                        foreach (string request in diff_request)
                        {
                            curr_friend_request.Add(request);
                            FriendRequestsPanel.Children.Add(UIUtils.CreateFriendRequestCard(request, AcceptButton_Click, RefuseButton_Click, this.Resources));
                        }

                        foreach (string friend in diff_friend)
                        {
                            curr_friend_list.Add(friend);
                            FriendsListPanel.Children.Add(UIUtils.CreateFriendCard(friend, ChallengeButton_Click, UnfriendButton_Click, this.Resources));
                        }

                        TotalFriendsCount.Text = curr_friend_list.Count.ToString() + " bạn bè";
                    });
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

        private void FriendUsernameInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            _mainWindow.Keyboard.Stop();
            _mainWindow.Keyboard.Play();
        }
    }
}
