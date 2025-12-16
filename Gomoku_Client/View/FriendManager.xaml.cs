using Gomoku_Client.Model;
using Gomoku_Client.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

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

        public FriendManager(MainGameUI mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;

            DispatcherTimer _timer;
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(2);
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void BackButton_Checked(object sender, RoutedEventArgs e)
        {
            if (_mainWindow == null)
            {
                MessageBox.Show("Không tìm thấy cửa sổ chính.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            _mainWindow.ShowMenuWithAnimation();
        }

        private void SendFriendRequest_Click(object sender, RoutedEventArgs e)
        {
            // Send friend request
        }


        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
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

        }

        private void UnfriendButton_Click(object sender, RoutedEventArgs e)
        {
            Button? sender_handle = sender as Button;
            _pendingUnfriendUsername = sender_handle?.Name;

            UnfriendConfirmationOverlay.Visibility = Visibility.Visible;
            var border = (Border)((Grid)UnfriendConfirmationOverlay).Children[0];
            Storyboard fadeIn = (Storyboard)FindResource("PopupFadeIn");
            fadeIn.Begin(border);
        }

        private async void Timer_Tick(object? sender, EventArgs e)
        {
            try
            {
                string username = FirebaseInfo.AuthClient.User.Info.DisplayName;
                List<string>? new_friend_request = await FireStoreHelper.GetNewFriendReqest(username, curr_friend_request);
                List<string>? new_friend_list = await FireStoreHelper.GetNewFriend(username, curr_friend_list);

                foreach(string request in new_friend_request ?? new List<string>())
                {
                    curr_friend_request.Add(request);
                    FriendRequestsPanel.Children.Add(UIUtils.CreateFriendRequestCard(request, AcceptButton_Click, RefuseButton_Click, this.Resources));
                }

                foreach (string friend in new_friend_list ?? new List<string>())
                {
                    curr_friend_list.Add(friend);
                    FriendsListPanel.Children.Add(UIUtils.CreateFriendCard(friend, ChallengeButton_Click, UnfriendButton_Click, this.Resources));
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Failed to load friends and friend_request");
            }
        }

        private async Task DeleteRequestCard(string name)
        {
            var borderToDelete = FriendRequestsPanel.Children
            .OfType<Border>()
            .FirstOrDefault(b => b.Name == name);

            if (borderToDelete != null)
            {
                FriendRequestsPanel.Children.Remove(borderToDelete);
            }
        }
    }
}
