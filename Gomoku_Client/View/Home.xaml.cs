using Firebase.Auth;
using Gomoku_Client.Model;
using Gomoku_Client.View;
using Gomoku_Client.ViewModel;
using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Media;
using System.Net;
using System.Net.Sockets;
using System.Printing.IndexedProperties;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
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

        public int secretAvatar = 0;

        public static MainGameUI? Instance { get; private set; }

        public class AvatarItem
        {
            public string? Image { get; set; }
            public string? Name { get; set; }
        }

        public ObservableCollection<AvatarItem>? Avatars { get; set; }
        public ObservableCollection<AvatarItem>? SecretAvatars { get; set; }

        FirestoreChangeListener? match_listener;
        List<string> old_match_request = new List<string>();
        public MainGameUI()
        {
            InitializeComponent();
            Instance = this;
            UpdateActualBGM();
            SoundStart();

            Avatars = new ObservableCollection<AvatarItem>
            {
            new AvatarItem { Image="pack://application:,,,/Assets/Avatar/T1_6sao.jpg", Name="T1|Logo" },
            new AvatarItem { Image="pack://application:,,,/Assets/Avatar/T1 Faker.jpg", Name="T1|Faker" },
            new AvatarItem { Image="pack://application:,,,/Assets/Avatar/T1 Gumayusi.jpg", Name="T1|Gumayasi" },
            new AvatarItem { Image="pack://application:,,,/Assets/Avatar/T1 Keria.jpg", Name="T1|Keria" },
            new AvatarItem { Image="pack://application:,,,/Assets/Avatar/T1 Oner.jpg", Name="T1|Oner" },
            new AvatarItem { Image="pack://application:,,,/Assets/Avatar/T1 Zeus.jpg", Name="T1|Zeus" },

            new AvatarItem { Image="pack://application:,,,/Assets/Avatar/GenG Logo.jpg", Name="GenG|Logo" },
            new AvatarItem { Image="pack://application:,,,/Assets/Avatar/GenG Canyon.jpg", Name="GenG|Canyon" },
            new AvatarItem { Image="pack://application:,,,/Assets/Avatar/GenG Chovy.jpg", Name="GenG|Chovy" },
            new AvatarItem { Image="pack://application:,,,/Assets/Avatar/GenG Kiin.jpg", Name="GenG|Kiin" },
            new AvatarItem { Image="pack://application:,,,/Assets/Avatar/GenG Lehends.jpg", Name="GenG|Lehends" },
            new AvatarItem { Image="pack://application:,,,/Assets/Avatar/GenG Ruler.jpg", Name="GenG|Ruler" },

            new AvatarItem { Image="pack://application:,,,/Assets/Avatar/KT Logo.jpg", Name="KT|Logo" },
            new AvatarItem { Image="pack://application:,,,/Assets/Avatar/KT Flash.jpg", Name="KT|Flash" },
            new AvatarItem { Image="pack://application:,,,/Assets/Avatar/KT Bdd.jpg", Name="KT|Bdd" },
            new AvatarItem { Image="pack://application:,,,/Assets/Avatar/KT Aiming.jpg", Name="KT|Aiming" },
            new AvatarItem { Image="pack://application:,,,/Assets/Avatar/KT Smeb.jpg", Name="KT|Smeb" },
            new AvatarItem { Image="pack://application:,,,/Assets/Avatar/KT Score.jpg", Name="KT|Score" },

            new AvatarItem { Image="pack://application:,,,/Assets/Avatar/HLE Logo.jpg", Name="HLE|Logo" },
            new AvatarItem { Image="pack://application:,,,/Assets/Avatar/HLE Kanavi.jpg", Name="HLE|Kanavi" },
            new AvatarItem { Image="pack://application:,,,/Assets/Avatar/HLE Peanut.jpg", Name="HLE|Peanut" },
            new AvatarItem { Image="pack://application:,,,/Assets/Avatar/HLE Viper.jpg", Name="HLE|Viper" },
            new AvatarItem { Image="pack://application:,,,/Assets/Avatar/HLE Zeka.jpg", Name="HLE|Zeka" },
            new AvatarItem { Image="pack://application:,,,/Assets/Avatar/HLE Zeus.jpg", Name="HLE|Zeus" },
            };

            SecretAvatars = new ObservableCollection<AvatarItem>
            {
                new AvatarItem { Image="pack://application:,,,/Assets/Avatar/Random Tus.jpg", Name="TusMeme" },
                new AvatarItem { Image="pack://application:,,,/Assets/Avatar/Random Tuanas.jpg", Name="GeiLord" },
                new AvatarItem { Image="pack://application:,,,/Assets/Avatar/Random Truo.jpg", Name="MasterWibu" },
                new AvatarItem { Image="pack://application:,,,/Assets/Avatar/Random J97.jpg", Name="Jack Đồ Tể" },
                new AvatarItem { Image="pack://application:,,,/Assets/Avatar/Random J97v2.jpg", Name="Jack Chúa Quỷ" },
                new AvatarItem { Image="pack://application:,,,/Assets/Avatar/Random Baka.jpg", Name="Baka" },
                new AvatarItem { Image="pack://application:,,,/Assets/Avatar/Random Larry.jpg", Name="Larry" },
                new AvatarItem { Image="pack://application:,,,/Assets/Avatar/Random Yippe.jpg", Name="Yippe" },
                new AvatarItem { Image="pack://application:,,,/Assets/Avatar/Random Miku.jpg", Name="Miku" },
                new AvatarItem { Image="pack://application:,,,/Assets/Avatar/Random Teto.jpg", Name="Teto" },
                new AvatarItem { Image="pack://application:,,,/Assets/Avatar/Random Charlie.jpg", Name="Charlie" },
                new AvatarItem { Image="pack://application:,,,/Assets/Avatar/Random DonaldTrump.jpg", Name="Đô Năm Trăm" },
                new AvatarItem { Image="pack://application:,,,/Assets/Avatar/Random GermanArtist.jpg", Name="Họa sĩ Dức" },
                new AvatarItem { Image="pack://application:,,,/Assets/Avatar/Random George Floyd.jpg", Name="George Floyd" },
                new AvatarItem { Image="pack://application:,,,/Assets/Avatar/Random George Droid.jpg", Name="George Droid" },
                new AvatarItem { Image="pack://application:,,,/Assets/Avatar/Random Epstein.jpg", Name="Kẻ bí ẩn" },
                new AvatarItem { Image="pack://application:,,,/Assets/Avatar/Random IShowMeat.jpg", Name="IShowMeat" },
                new AvatarItem { Image="pack://application:,,,/Assets/Avatar/Random BigChungus.jpg", Name="Big Chungus" },
                new AvatarItem { Image="pack://application:,,,/Assets/Avatar/Random Freddy.jpg", Name="Freddy" },
                new AvatarItem { Image="pack://application:,,,/Assets/Avatar/Random CoolerFreddy.jpg", Name="CoolerFreddy" },
                new AvatarItem { Image="pack://application:,,,/Assets/Avatar/Random DirtBlock.jpg", Name="Minecraft Dirt" },
            };

            DataContext = this;
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

        public void NavigateToLobby()
        {
            MainFrame.Navigate(new Lobby(this));
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
            string username = FirebaseInfo.AuthClient.User.Info.DisplayName;
            tb_PlayerName.Text = username;

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

            Google.Cloud.Firestore.DocumentReference match_ref = FirebaseInfo.DB.Collection("UserInfo").Document(username);
            match_listener = match_ref.Listen(snapshot => {
                if (snapshot.Exists)
                {
                    UserDataModel user_data = snapshot.ConvertTo<UserDataModel>();
                    if(old_match_request.Count >= 1)
                    {
                        List<string> deleted_request = old_match_request.Except(user_data.MatchRequests).ToList();
                        foreach (string del in deleted_request)
                        {
                            old_match_request.Remove(del);
                        }
                    }

                    List<string> diff_request = user_data.MatchRequests.Except(old_match_request).ToList();
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        AvatarBrush.ImageSource = BitmapFrame.Create(new Uri(user_data.ImagePath));
                        ColorChangeBaseOnTeam(AvatarBrush.ImageSource.ToString());

                        foreach (string request in diff_request)
                        {
                            NotificationManager.Instance.ShowNotification(
                                $"{request} muốn thách đấu bạn",
                                "Bạn sợ à ?",
                                Notification.NotificationType.YesNo,
                                15000,
                                onAccept: (s, e) =>
                                {
                                    respondChallenge("[CHALLENGE_ACCEPT]", request, username);                                  
                                },
                                onDecline: (s, e) =>
                                {
                                    respondChallenge("[CHALLENGE_DECLINE]", request, username);
                                }
                            );
                        }
                    });
                }
            });
        }

        private TcpClient client;

        void respondChallenge(string response, string challenger, string me)
        {
            client = new TcpClient();

            try
            {
                client.Connect(IPAddress.Parse("127.0.0.1"), 8888);
            }
            catch
            {
                NotificationManager.Instance.ShowNotification(
                        "Lỗi",
                        "Server có thể đang không hoạt động",
                        Notification.NotificationType.Info,
                        5000
                    );
                return;
            }

            if (!client.Connected) return;
            switch (response)
            {
                case "[CHALLENGE_ACCEPT]":
                    acceptChallenge(client, response, challenger, me);
                    break;

                case "[CHALLENGE_DECLINE]":
                    declineChallenge(client, response, challenger, me);
                    break;

                default:
                    break;
            }

        }

        void acceptChallenge(TcpClient client, string response, string challenger, string me) 
        {
            if ( UserState.currentState == State.InMatch)
            {
                NotificationManager.Instance.ShowNotification(
                    "Chấp nhận thách đấu thất bại",
                    "Hiện giờ bạn không thể tham gia trận đấu khác",
                    Notification.NotificationType.Info,
                    5000
                    );
                return;
            }
            UserState.currentState = State.InMatch;
            var stream = client.GetStream();
            byte[] data = Encoding.UTF8.GetBytes($"{response};{challenger};{me}");
            stream.Write(data, 0, data.Length);

            NotificationManager.Instance.ShowNotification(
                    $"Đã chấp nhận lời thách đấu của {challenger}",
                    "Chuẩn bị vào trận đấu!",
                    Notification.NotificationType.Info,
                    5000
                    );
        }

        void declineChallenge(TcpClient client, string response, string challenger, string me)
        {
            var stream = client.GetStream();
            byte[] data = Encoding.UTF8.GetBytes($"{response};{challenger};{me}");
            stream.Write(data, 0, data.Length);

            NotificationManager.Instance.ShowNotification(
                    $"Đã từ chối lời thách đấu của {challenger}",
                    "Không đủ trình!",
                    Notification.NotificationType.Info,
                    5000
                    );
            stream.Close();
            client.Close();
            client = null;
            return;
        }

        private async void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            if (listener != null)
            {
                await listener.StopAsync();
                listener = null;
            }

            if(match_listener != null)
            {
                await match_listener.StopAsync();
                match_listener = null;
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

        private void EditAvatarButton_Click(object sender, RoutedEventArgs e)
        {
            ButtonClick.Stop();
            ButtonClick.Play();
            ScrollAvatar.ScrollToTop();
            if (secretAvatar < 2)
            {
                AvatarOverlay.Visibility = Visibility.Visible;

                var storyboard = (Storyboard)this.Resources["FadeInStoryboard"];
                var border = (Border)((Grid)AvatarOverlay).Children[0];
                storyboard.Begin(border);
                secretAvatar++;
            }
            else
            {
                AvatarSecretOverlay.Visibility = Visibility.Visible;

                var storyboard = (Storyboard)this.Resources["FadeInStoryboard"];
                var border = (Border)((Grid)AvatarSecretOverlay).Children[0];
                storyboard.Begin(border);
                secretAvatar = 0;
            }
        }
        private void ColorChangeUI(string color1, string color2)
        {
            Color customColor = (Color)ColorConverter.ConvertFromString(color1);
            Color customColor2 = (Color)ColorConverter.ConvertFromString(color2);
            Color YellowMain = (Color)ColorConverter.ConvertFromString("#333333");

            var animation = new ColorAnimation
            {
                To = customColor,
                Duration = TimeSpan.FromMilliseconds(400),
                FillBehavior = FillBehavior.HoldEnd
            };

            var animation2 = new ColorAnimation
            {
                To = customColor2,
                Duration = TimeSpan.FromMilliseconds(400),
                FillBehavior = FillBehavior.HoldEnd
            };

            // Glow
            GlowEffect.BeginAnimation(DropShadowEffect.ColorProperty, animation);

            // Text RadioButton
            PlayButton.Foreground.BeginAnimation(SolidColorBrush.ColorProperty, animation);

            // Avatar
            Avatar.BorderBrush.BeginAnimation(SolidColorBrush.ColorProperty, animation2);
            AvatarGlow.BeginAnimation(DropShadowEffect.ColorProperty, animation2);

            // Border EditAvatarButton
            EditAvatarButton.Background.BeginAnimation(SolidColorBrush.ColorProperty, animation2);

            // Label Winrate
            lb_winrate.Foreground.BeginAnimation(SolidColorBrush.ColorProperty, animation);

            //QuitConfirmOverlay
            QuitBorder.BorderBrush.BeginAnimation(SolidColorBrush.ColorProperty, animation);
            QuitBorderGlow.BeginAnimation(DropShadowEffect.ColorProperty, animation);
            QuitIconGlow.BeginAnimation(DropShadowEffect.ColorProperty, animation);

            ExitDraw.Fill.BeginAnimation(SolidColorBrush.ColorProperty, animation);

            ConfirmQuitButton.Background.BeginAnimation(SolidColorBrush.ColorProperty, animation);

            if (color1 == "#FFD700" || color1 == "#FFFAF0")
            {
                if (ConfirmQuitButton.Foreground is SolidColorBrush currentBrush)
                {
                    SolidColorBrush animatedBrush = new SolidColorBrush(currentBrush.Color);

                    ConfirmQuitButton.Foreground = animatedBrush;

                    ColorAnimation colorAnim = new ColorAnimation
                    {
                        To = (Color)ColorConverter.ConvertFromString("#2C3E50"), 
                        Duration = TimeSpan.FromMilliseconds(400),
                        FillBehavior = FillBehavior.HoldEnd
                    };

                    animatedBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnim);
                }
            }
            else
            {
                if (ConfirmQuitButton.Foreground is SolidColorBrush currentBrush)
                {
                    SolidColorBrush animatedBrush = new SolidColorBrush(currentBrush.Color);

                    ConfirmQuitButton.Foreground = animatedBrush;

                    ColorAnimation colorAnim = new ColorAnimation
                    {
                        To = Colors.White,
                        Duration = TimeSpan.FromMilliseconds(400),
                        FillBehavior = FillBehavior.HoldEnd
                    };

                    animatedBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnim);
                }
            }

            //AvatarChooseGlow
            AvatarChooseGlow.BeginAnimation(DropShadowEffect.ColorProperty, animation);
            AvatarSecretChooseGlow.BeginAnimation(DropShadowEffect.ColorProperty, animation);

            var border = ConfirmQuitButton.Template.FindName("ButtonBorder", ConfirmQuitButton) as Border;

            if (border != null && border.Effect is DropShadowEffect shadow)
            {
                shadow.BeginAnimation(DropShadowEffect.ColorProperty, animation);
            }
        }

        private void ColorChangeBaseOnTeam(string path)
        {
            if (path.Contains("T1") == true)
            {
                ColorChangeUI("#FF4655", "#FF4655");
            }
            else if (path.Contains("GenG") == true)
            {
                ColorChangeUI("#FFD700", "#121212");
            }else if (path.Contains("KT") == true)
            {
                ColorChangeUI("#FFFAF0", "#121212");
            }else if (path.Contains("HLE") == true)
            {
                ColorChangeUI("#FFFFA500", "#FFFFA500");
            }
            else
            {
                ColorChangeUI("#FF4655", "#FF4655");
            }
        }

        private async void Border_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ButtonClick.Stop();
            ButtonClick.Play();

            secretAvatar = 0;

            var border = sender as Border;
            if (border?.DataContext is AvatarItem selectedAvatar)
            {
                AvatarBrush.ImageSource = BitmapFrame.Create(new Uri(selectedAvatar.Image ?? "", UriKind.RelativeOrAbsolute));
                await FireStoreHelper.SetUserAvatar(FirebaseInfo.AuthClient.User.Info.DisplayName, AvatarBrush.ImageSource.ToString());

                ColorChangeBaseOnTeam(AvatarBrush.ImageSource.ToString());

                //if (selectedAvatar.Name?.Contains("T1|") == true)
                //    ColorChangeUI("#FF4655", "#FF4655");
                //else if (selectedAvatar.Name?.Contains("GenG|") == true)
                //    ColorChangeUI("#FFD700", "#121212");
                //else if (selectedAvatar.Name?.Contains("KT|") == true)
                //    ColorChangeUI("#FFFAF0", "#121212");
                //else if (selectedAvatar.Name?.Contains("HLE|") == true)
                //    ColorChangeUI("#FFFFA500", "#FFFFA500");
                //else ColorChangeUI("#FF4655", "#FF4655");

                if (AvatarOverlay.Visibility == Visibility.Visible)
                {
                    var storyboard = (Storyboard)this.Resources["FadeOutStoryboard"];
                    var sborder = (Border)((Grid)AvatarOverlay).Children[0];
                    storyboard.Completed += (s, args) =>
                    {
                        AvatarOverlay.Visibility = Visibility.Collapsed;
                    };

                    storyboard.Begin(sborder);
                }

                else if(AvatarSecretOverlay.Visibility == Visibility.Visible)
                {
                    var storyboard = (Storyboard)this.Resources["FadeOutStoryboard"];
                    var sborder = (Border)((Grid)AvatarSecretOverlay).Children[0];
                    storyboard.Completed += (s, args) =>
                    {
                        AvatarSecretOverlay.Visibility = Visibility.Collapsed;
                    };

                    storyboard.Begin(sborder);
                }
            }
        }

        private void CloseAvatarOverlay_Click(object sender, RoutedEventArgs e)
        {
            ButtonClick.Stop();
            ButtonClick.Play();

            var storyboard = (Storyboard)this.Resources["FadeOutStoryboard"];
            var border = (Border)((Grid)AvatarOverlay).Children[0];
            storyboard.Completed += (s, args) =>
            {
                AvatarOverlay.Visibility = Visibility.Collapsed;
            };

            storyboard.Begin(border);
        }

        private void CloseAvatarSecretOverlay_Click(object sender, RoutedEventArgs e)
        {
            ButtonClick.Stop();
            ButtonClick.Play();

            secretAvatar = 0;

            var storyboard = (Storyboard)this.Resources["FadeOutStoryboard"];
            var border = (Border)((Grid)AvatarSecretOverlay).Children[0];
            storyboard.Completed += (s, args) =>
            {
                AvatarSecretOverlay.Visibility = Visibility.Collapsed;
            };

            storyboard.Begin(border);
        }
    }
}
