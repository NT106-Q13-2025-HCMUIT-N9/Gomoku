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
using System.IO;
using System.Linq;
using System.Media;
using System.Net;
using System.Net.Http;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Gomoku_Client.Helpers;

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
                new AvatarItem { Image="pack://application:,,,/Assets/Avatar/Random Tuanas.jpg", Name="TuanKiller" },
                new AvatarItem { Image="pack://application:,,,/Assets/Avatar/Random Truo.jpg", Name="MasterWibu" },
                new AvatarItem { Image="pack://application:,,,/Assets/Avatar/Random J97.jpg", Name="Jack Đồ Tể" },
                new AvatarItem { Image="pack://application:,,,/Assets/Avatar/Random J97v2.jpg", Name="Jack Chúa Quỷ" },
                new AvatarItem { Image="pack://application:,,,/Assets/Avatar/Random DoMiXi.jpg", Name="Độ Mixi" },
                new AvatarItem { Image="pack://application:,,,/Assets/Avatar/Random Baka.jpg", Name="Baka" },
                new AvatarItem { Image="pack://application:,,,/Assets/Avatar/Random Larry.jpg", Name="Larry" },
                new AvatarItem { Image="pack://application:,,,/Assets/Avatar/Random Yippe.jpg", Name="Yippe" },
                new AvatarItem { Image="pack://application:,,,/Assets/Avatar/Random Miku.jpg", Name="Miku" },
                new AvatarItem { Image="pack://application:,,,/Assets/Avatar/Random Teto.jpg", Name="Teto" },
                new AvatarItem { Image="pack://application:,,,/Assets/Avatar/Random Charlie.jpg", Name="Charlie" },
                new AvatarItem { Image="pack://application:,,,/Assets/Avatar/Random DonaldTrump.jpg", Name="Đô Năm Trăm" },
                new AvatarItem { Image="pack://application:,,,/Assets/Avatar/Random GermanArtist.jpg", Name="Họa sĩ Đức" },
                new AvatarItem { Image="pack://application:,,,/Assets/Avatar/Random TheKing.jpg", Name="The GOAT" },
                new AvatarItem { Image="pack://application:,,,/Assets/Avatar/Random DreamyBull.jpg", Name="Trâu Mộng Mơ" },
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

            string path = System.IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "data.txt"
            );

        }

        public static double MasterVolValue;
        public static double BGMVolValue;
        public static double SFXVolValue;

        public void UpdateActualBGM()
        {
            MainBGM.Volume = MasterVolValue * BGMVolValue;
            ButtonClick.Volume = SFXVolValue * MasterVolValue;
            Keyboard.Volume = SFXVolValue * MasterVolValue;

            string volumeFile = System.IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "volume.txt"
            );

            string[] lines =
            {
                MasterVolValue.ToString(),
                BGMVolValue.ToString(),
                SFXVolValue.ToString()
            };

            File.WriteAllLines(volumeFile, lines);
        }

        void SoundStart()
        {
            try
            {
                List<string> BGM = new List<string>();

                string[] bgmFiles = {
                    "meow.mp3", "frog.mp3", "chess.mp3",
                    "doodle.mp3", "Joy.mp3", "Matchmakers.mp3"
                };

                foreach (var file in bgmFiles)
                {
                    string path = AudioHelper.ExtractResourceToTemp($"Assets/Sounds/{file}");
                    if (!string.IsNullOrEmpty(path))
                    {
                        BGM.Add(path);
                    }
                }

                string buttonPath = AudioHelper.ExtractResourceToTemp("Assets/Sounds/ButtonHover.wav");
                string keyboardPath = AudioHelper.ExtractResourceToTemp("Assets/Sounds/Keyboard.wav");

                if (BGM.Count > 0)
                {
                    int BGMNumber = Random.Shared.Next(0, BGM.Count);

                    MainBGM.MediaOpened += (s, e) => MainBGM.Play();

                    MainBGM.MediaEnded += (s, e) =>
                    {
                        BGMNumber = Random.Shared.Next(0, BGM.Count);
                        MainBGM.Open(new Uri(BGM[BGMNumber], UriKind.Absolute));
                        MainBGM.Play();
                    };

                    MainBGM.MediaFailed += (s, e) =>
                    {
                        Debug.WriteLine($"Lỗi phát nhạc: {e.ErrorException.Message}");
                    };

                    MainBGM.Open(new Uri(BGM[BGMNumber], UriKind.Absolute));
                }

                if (!string.IsNullOrEmpty(buttonPath))
                {
                    ButtonClick.Open(new Uri(buttonPath, UriKind.Absolute));
                }

                if (!string.IsNullOrEmpty(keyboardPath))
                {
                    Keyboard.Open(new Uri(keyboardPath, UriKind.Absolute));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi khởi tạo âm thanh MainGameUI: {ex.Message}");
            }
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

        private void PlayButton_Checked(object sender, RoutedEventArgs e)
        {
            ButtonClick.Stop();
            ButtonClick.Play();
            NavigateWithSlideAnimation(new Lobby(this), (RadioButton)sender);
        }

        public void NavigateToLobby()
        {
            MainFrame.Content = null;
            StackPanelMenu.Visibility = Visibility.Visible;
            MainFrame.Visibility = Visibility.Collapsed;

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

                    Dispatcher.Invoke(() =>
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
                    if (old_match_request.Count >= 1)
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

        async void respondChallenge(string response, string challenger, string me)
        {
            client = new TcpClient();
            string username = FirebaseInfo.AuthClient.User.Info.DisplayName;
            Google.Cloud.Firestore.DocumentReference doc_ref = FirebaseInfo.DB.Collection("UserInfo").Document(username);
            try
            {
                client.Connect(IPAddress.Parse("34.68.212.10"), 9999);
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

            if (!client.Connected) {
                NotificationManager.Instance.ShowNotification(
                    "Lỗi",
                    "Bạn chưa được kết nối đến server",
                    Notification.NotificationType.Info,
                    5000
                );
                return;
            } 
            switch (response)
            {
                case "[CHALLENGE_ACCEPT]":
                    await doc_ref.UpdateAsync("MatchRequests", FieldValue.ArrayRemove(challenger));
                    acceptChallenge(client, response, challenger, me);
                    break;

                case "[CHALLENGE_DECLINE]":
                    await doc_ref.UpdateAsync("MatchRequests", FieldValue.ArrayRemove(challenger));
                    declineChallenge(client, response, challenger, me);
                    break;
                default:
                    break;
            }

        }


        void acceptChallenge(TcpClient client, string response, string challenger, string me)
        {
            if (UserState.currentState == State.InMatch)
            {
                NotificationManager.Instance.ShowNotification("Lỗi", "Bạn đang trong trận đấu khác!", Notification.NotificationType.Info, 3000);
                return;
            }

            try
            {
                NetworkStream stream = client.GetStream();
                byte[] data = Encoding.UTF8.GetBytes($"{response};{challenger};{me}\n");
                stream.Write(data, 0, data.Length);

                NotificationManager.Instance.ShowNotification("Thách đấu", $"Đã chấp nhận {challenger}. Đang vào trận...", Notification.NotificationType.Info, 3000);

                Task.Run(() =>
                {
                    try
                    {
                        byte[] buffer = new byte[2048];
                        while (client != null && client.Connected)
                        {
                            int bytesRead = stream.Read(buffer, 0, buffer.Length);
                            if (bytesRead == 0) break;

                            string serverResponse = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                            Console.WriteLine($"[DEBUG] Home Recv: {serverResponse}");

                            string[] messages = serverResponse.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

                            foreach (var msg in messages)
                            {
                                if (msg.StartsWith("[INIT]"))
                                {
                                    string[] parts = msg.Split(';');

                                    if (parts.Length >= 5)
                                    {
                                        char playerSymbol = parts[3][0];
                                        string opponentName = parts[4];

                                        Console.WriteLine($"[DEBUG] Parsed INIT: Symbol={playerSymbol}, Opponent={opponentName}");

                                        Dispatcher.Invoke(() =>
                                        {

                                            this.MainBGM.Stop();

                                            Console.WriteLine("[MATCHMAKING] Opening GamePlay");

                                            var gamePlayWindow = new GamePlay(client, tb_PlayerName.Text, playerSymbol, opponentName, this)
                                            {
                                                Owner = this,
                                                WindowStartupLocation = WindowStartupLocation.Manual,
                                                Left = this.Left,
                                                Top = this.Top
                                            };

                                            gamePlayWindow.Closed += (sender, e) =>
                                            {
                                                bool isWinner = gamePlayWindow.FinalResult_IsLocalPlayerWinner;
                                                bool isDraw = gamePlayWindow.FinalResult_IsDraw;
                                                string p1 = gamePlayWindow.player1Name;
                                                string p2 = gamePlayWindow.player2Name;

                                                this.Left = gamePlayWindow.Left;
                                                this.Top = gamePlayWindow.Top;

                                                Dispatcher.Invoke(() =>
                                                {
                                                    try
                                                    {
                                                        this.Visibility = Visibility.Visible;
                                                    }
                                                    catch { }

                                                    Border mainOverlay = new Border
                                                    {
                                                        Background = Brushes.Black,
                                                        Opacity = 1,
                                                        Visibility = Visibility.Visible
                                                    };

                                                    Grid? mainRoot = null;
                                                    try
                                                    {
                                                        mainRoot = this.FindName("MainGrid") as Grid;
                                                    }
                                                    catch { mainRoot = null; }

                                                    if (mainRoot == null && this.Content is Grid g) mainRoot = g;

                                                    if (mainRoot != null)
                                                    {
                                                        Grid.SetRowSpan(mainOverlay, 100);
                                                        Grid.SetColumnSpan(mainOverlay, 100);
                                                        Panel.SetZIndex(mainOverlay, 99999);
                                                        mainRoot.Children.Add(mainOverlay);
                                                    }

                                                    Action navigateAction = () =>
                                                    {
                                                        try
                                                        {
                                                            if (mainRoot != null && mainRoot.Children.Contains(mainOverlay))
                                                                mainRoot.Children.Remove(mainOverlay);
                                                        }
                                                        catch { }

                                                        this.StackPanelMenu.Visibility = Visibility.Collapsed;
                                                        this.MainFrame.Visibility = Visibility.Visible;

                                                        MatchResult resultPage = new MatchResult(isWinner, p1, p2, this, isDraw);
                                                        try
                                                        {
                                                            this.MainFrame.Navigate(resultPage);
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            Debug.WriteLine($"[ERROR] Navigate resultPage: {ex.Message}");
                                                        }

                                                        try
                                                        {
                                                            if (this.MainBGM.Source != null) this.MainBGM.Play();
                                                        }
                                                        catch { }
                                                    };

                                                    bool navigated = false;

                                                    var reveal = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.8))
                                                    {
                                                        EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                                                    };

                                                    reveal.Completed += (s2, e2) =>
                                                    {
                                                        if (navigated) return;
                                                        navigated = true;
                                                        navigateAction();
                                                    };

                                                    if (mainOverlay != null)
                                                        mainOverlay.BeginAnimation(UIElement.OpacityProperty, reveal);
                                                    else
                                                    {
                                                        navigated = true;
                                                        navigateAction();
                                                    }

                                                    var fallback = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1.5) };
                                                    fallback.Tick += (fs, fe) =>
                                                    {
                                                        fallback.Stop();
                                                        if (!navigated)
                                                        {
                                                            navigated = true;
                                                            navigateAction();
                                                        }
                                                    };
                                                    fallback.Start();
                                                });
                                            };

                                            gamePlayWindow.Show();
                                            this.Visibility = Visibility.Collapsed;
                                        });
                                        return;
                                    }
                                    else
                                    {
                                        Console.WriteLine($"[ERROR] Invalid INIT packet length: {parts.Length}");
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[ERROR] Accept Loop Error: {ex}");
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Setup Accept Error: {ex.Message}");
            }
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

            if (match_listener != null)
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

                else if (AvatarSecretOverlay.Visibility == Visibility.Visible)
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
