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
using System.Media;
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

namespace Gomoku_Client
{
    /// <summary>
    /// Interaction logic for MainGameUI.xaml
    /// </summary>
    public partial class MainGameUI : Window
    {
        FirestoreChangeListener? listener;
        public MediaPlayer MainBGM = new MediaPlayer();
        public MediaPlayer ButtonClick = new MediaPlayer();
        public MediaPlayer Keyboard = new MediaPlayer();

        public static MainGameUI? Instance { get; private set; }
        public MainGameUI()
        {
            InitializeComponent();
            Instance = this;
            UpdateActualBGM();
            SoundStart();
        }

        public double MasterVolValue = 0.5;
        public double BGMVolValue = 0.15;
        public double SFXVolValue = 1;

        public void UpdateActualBGM()
        {
            MainBGM.Volume = MasterVolValue * BGMVolValue;
            ButtonClick.Volume = SFXVolValue * MasterVolValue;
            Keyboard.Volume = SFXVolValue * MasterVolValue;
        }

        void SoundStart()
        {

            List<string> BGM = new List<string>();
            BGM.Add(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Sounds", "meow.mp3"));
            BGM.Add(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Sounds", "frog.mp3"));
            BGM.Add(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Sounds", "chess.mp3"));
            BGM.Add(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Sounds", "doodle.mp3"));
            BGM.Add(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Sounds", "Joy.mp3"));
            BGM.Add(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Sounds", "Matchmakers.mp3"));
            int BGMNumber = Random.Shared.Next(0, 6);

            string buttonPath = System.IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Assets",
                "Sounds",
                "ButtonHover.wav"
            );

            string keyboardPath = System.IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Assets",
                "Sounds",
                "Keyboard.wav"
            );


            MainBGM.MediaOpened += (s, e) =>
            {
                MainBGM.Play();
            };

            MainBGM.MediaEnded += (s, e) =>
            {
                BGMNumber = Random.Shared.Next(0, 6);
                MainBGM.Open(new Uri(BGM[BGMNumber], UriKind.Absolute));
                //MainBGM.Position = TimeSpan.Zero;
                MainBGM.Play();
            };

            MainBGM.MediaFailed += (s, e) =>
            {
                MessageBox.Show(e.ErrorException.Message);
            };

            MainBGM.Open(new Uri(BGM[BGMNumber], UriKind.Absolute));
        
            ButtonClick.Open(new Uri(buttonPath, UriKind.Absolute));

            Keyboard.Open(new Uri(keyboardPath, UriKind.Absolute));
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

        public void NavigateWithAnimation(Page page)
        {
            if (MainFrame.Content != null)
            {
                if (MainFrame.RenderTransform == null || !(MainFrame.RenderTransform is TranslateTransform))
                {
                    MainFrame.RenderTransform = new TranslateTransform();
                }

                var transform = (TranslateTransform)MainFrame.RenderTransform;
                /*
                var slideOutAnimation = new DoubleAnimation
                {
                  From = 0,
                  To = -this.ActualWidth,
                  Duration = TimeSpan.FromSeconds(0.3),
                  EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
                };
                */

                MainFrame.Navigate(page);

                var slideInAnimation = new DoubleAnimation
                {
                    From = this.ActualWidth,
                    To = 0,
                    Duration = TimeSpan.FromSeconds(0.6),
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };

                transform.BeginAnimation(TranslateTransform.XProperty, slideInAnimation);

                //transform.BeginAnimation(TranslateTransform.XProperty, slideOutAnimation);
            }
            else
            {
                MainFrame.Navigate(page);

                if (MainFrame.RenderTransform == null || !(MainFrame.RenderTransform is TranslateTransform))
                {
                    MainFrame.RenderTransform = new TranslateTransform();
                }

                var transform = (TranslateTransform)MainFrame.RenderTransform;

                var slideAnimation = new DoubleAnimation
                {
                    From = this.ActualWidth,
                    To = 0,
                    Duration = TimeSpan.FromSeconds(0.5),
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };

                transform.BeginAnimation(TranslateTransform.XProperty, slideAnimation);
            }
        }

        private void SignOut_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ButtonClick.Stop();
                ButtonClick.Play();
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
            ButtonClick.Stop();
            ButtonClick.Play();
            NavigateWithSlideAnimation(new Lobby(this), (RadioButton)sender);
        }

        private void HistoryButton_Checked(object sender, RoutedEventArgs e)
        {
            ButtonClick.Stop();
            ButtonClick.Play();
            NavigateWithSlideAnimation(new History(this), (RadioButton)sender);
        }

        private void FriendManagerButton_Checked(object sender, RoutedEventArgs e)
        {
            ButtonClick.Stop();
            ButtonClick.Play();
            NavigateWithSlideAnimation(new FriendManager(this), (RadioButton)sender);
        }

        private void SettingButton_Checked(object sender, RoutedEventArgs e)
        {
            ButtonClick.Stop();
            ButtonClick.Play();
            NavigateWithSlideAnimation(new Setting(this), (RadioButton)sender);
        }

        private void ExitButton_Checked(object sender, RoutedEventArgs e)
        {
            ButtonClick.Stop();
            ButtonClick.Play();
            QuitConfirmationOverlay.Visibility = Visibility.Visible;

            var storyboard = (Storyboard)this.Resources["FadeInStoryboard"];
            var border = (Border)((Grid)QuitConfirmationOverlay).Children[0];
            storyboard.Begin(border);

            ExitButton.IsChecked = false;
        }
        private void ConfirmQuitButton_Click(object sender, RoutedEventArgs e)
        {
            ButtonClick.Stop();
            ButtonClick.Play();
            Application.Current.Shutdown();
        }

        private void CancelQuitButton_Click(object sender, RoutedEventArgs e)
        {
            ButtonClick.Stop();
            ButtonClick.Play();
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

            listener = doc_ref.Listen(doc_snap => {
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

        private async void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            if (listener != null)
            {
                await listener.StopAsync();
                listener = null;
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            ButtonClick.Stop();
            ButtonClick.Play();
            WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            ButtonClick.Stop();
            ButtonClick.Play();
            QuitConfirmationOverlay.Visibility = Visibility.Visible;

            var storyboard = (Storyboard)this.Resources["FadeInStoryboard"];
            var border = (Border)((Grid)QuitConfirmationOverlay).Children[0];
            storyboard.Begin(border);
        }
    }
}
