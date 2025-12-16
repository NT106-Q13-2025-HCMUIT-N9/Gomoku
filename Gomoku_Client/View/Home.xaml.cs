using Firebase.Auth;
using Gomoku_Client.Model;
using Gomoku_Client.View;
using Gomoku_Client.ViewModel;
using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;
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
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Gomoku_Client
{
    /// <summary>
    /// Interaction logic for MainGameUI.xaml
    /// </summary>
    public partial class MainGameUI : Window
    {
        public MainGameUI()
        {
            InitializeComponent();
        }

        private void AnimateSlideIn()
        {
            var slideIn = (Storyboard)this.Resources["SlideInFromRight"];
            slideIn.Begin(MainFrame);
        }

        public void ShowMenuWithAnimation()
        {
            if (MainFrame.Visibility == Visibility.Visible)
            {
                var slideOut = (Storyboard)this.Resources["SlideOutToLeft"];
                slideOut.Completed += (s, args) =>
                {
                    MainFrame.Visibility = Visibility.Collapsed;
                    MainFrame.Content = null;
                    StackPanelMenu.Visibility = Visibility.Visible;

                    AnimateMenuFadeIn();
                };
                slideOut.Begin(MainFrame);
            }
            else
            {
                StackPanelMenu.Visibility = Visibility.Visible;
            }
        }

        private void AnimateMenuFadeIn()
        {
            StackPanelMenu.Opacity = 0;
            var fadeIn = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            StackPanelMenu.BeginAnimation(UIElement.OpacityProperty, fadeIn);
        }

        private void NavigateWithSlideAnimation(Page page, RadioButton sender)
        {

            if (MainFrame.Visibility == Visibility.Visible)
            {
                var slideOut = (Storyboard)this.Resources["SlideOutToLeft"];
                slideOut.Completed += (s, args) =>
                {
                    MainFrame.Navigate(page);
                    AnimateSlideIn();
                };
                slideOut.Begin(MainFrame);
            }
            else
            {
                // 1. Tải trang Lobby vào Frame
                MainFrame.Navigate(page);
                // 2. Ẩn Menu Chính
                MainFrame.Visibility = Visibility.Visible;
                // 3. Hiển thị Frame nội dung
                StackPanelMenu.Visibility = Visibility.Collapsed;
                // 4. Đặt lại trạng thái PlayButton
                AnimateSlideIn();
            }

            sender.IsChecked = false;
        }

        private void SignOut_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FirebaseInfo.AuthClient.SignOut();

                MainWindow main = new MainWindow();
                // Sao chép vị trí và kích thước
                main.Left = this.Left;
                main.Top = this.Top;
                main.Width = this.Width;
                main.Height = this.Height;
                main.WindowState = this.WindowState;

                this.Hide();
                main.Show();
                this.Close();
            }
            catch (FirebaseAuthException ex)
            {
                MessageBox.Show($"Lỗi: {ex.Reason}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PlayButton_Checked(object sender, RoutedEventArgs e)
        {
            NavigateWithSlideAnimation(new Lobby(this), (RadioButton)sender);
        }

        private void HistoryButton_Checked(object sender, RoutedEventArgs e)
        {
            NavigateWithSlideAnimation(new History(this), (RadioButton)sender);
        }

        private void FriendManagerButton_Checked(object sender, RoutedEventArgs e)
        {
            NavigateWithSlideAnimation(new FriendManager(this), (RadioButton)sender);
        }

        private void SettingButton_Checked(object sender, RoutedEventArgs e)
        {
            NavigateWithSlideAnimation(new Setting(this), (RadioButton)sender);
        }

        private void ExitButton_Checked(object sender, RoutedEventArgs e)
        {
            QuitConfirmationOverlay.Visibility = Visibility.Visible;

            var storyboard = (Storyboard)this.Resources["FadeInStoryboard"];
            var border = (Border)((Grid)QuitConfirmationOverlay).Children[0];
            storyboard.Begin(border);

            ExitButton.IsChecked = false;
        }
        private void ConfirmQuitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void CancelQuitButton_Click(object sender, RoutedEventArgs e)
        {
            var storyboard = (Storyboard)this.Resources["FadeOutStoryboard"];
            var border = (Border)((Grid)QuitConfirmationOverlay).Children[0];
            storyboard.Completed += (s, args) =>
            {
                QuitConfirmationOverlay.Visibility = Visibility.Collapsed;
            };

            storyboard.Begin(border);
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            tb_PlayerName.Text = FirebaseInfo.AuthClient.User.Info.DisplayName;

            string username = FirebaseInfo.AuthClient.User.Info.DisplayName;
            Google.Cloud.Firestore.DocumentReference doc_ref = FirebaseInfo.DB.Collection("UserStats").Document(username);

            FirestoreChangeListener listener = doc_ref.Listen(doc_snap => {
                if (doc_snap.Exists)
                {
                    UserStatsModel user_stats = doc_snap.ConvertTo<UserStatsModel>();

                    App.Current.Dispatcher.Invoke(() =>
                    {
                        lb_matches.Text = user_stats.total_match.ToString();
                        lb_winrate.Text = user_stats.total_match > 0
                            ? ((user_stats.Wins / (double)user_stats.total_match) * 100).ToString("F2") + "%"
                            : "0";
                    });
                }
            });
        }
    }
}
