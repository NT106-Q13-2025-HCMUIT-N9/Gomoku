using Gomoku_Client.Model;
using Gomoku_Client.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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
    public partial class Matchmaking : Page
    {
        private MainGameUI _mainWindow;
        private DispatcherTimer _queueTimer;
        private Stopwatch _stopwatch;
        private Storyboard _movingDotStoryboard;

        private TcpClient _tcpClient;
        private NetworkStream _stream;
        private Thread _receiveThread;
        private bool _isConnected = false;
        private string _username;

        public Matchmaking(MainGameUI mainGameUI)
        {
            InitializeComponent();
            _mainWindow = mainGameUI;

            _stopwatch = new Stopwatch();
            _queueTimer = new DispatcherTimer();
            _queueTimer.Interval = TimeSpan.FromSeconds(1);
            _queueTimer.Tick += QueueTimer_Tick;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var user = FirebaseInfo.AuthClient.User;
                if (user?.Info == null)
                {
                    throw new InvalidOperationException("User is not authenticated.");
                }

                _username = user.Info.DisplayName;
                tb_PlayerName.Text = _username;

                _stopwatch.Start();
                _queueTimer.Start();
                _movingDotStoryboard = (Storyboard)this.Resources["MovingDotStoryboard"];
                _movingDotStoryboard?.Begin();

                UserStatsModel? user_stats = await FireStoreHelper.GetUserStats(_username);
                UserDataModel? user_data = await FireStoreHelper.GetUserInfo(_username);
                img_PlayerAvatar.Source = BitmapFrame.Create(new Uri(user_data.ImagePath));

                if (user_stats != null)
                {
                    lb_matches.Text = user_stats.total_match.ToString();
                    tb_WinRate.Text = user_stats.total_match > 0
                        ? $"{(user_stats.Wins / (double)user_stats.total_match * 100):F1}%"
                        : "0%";
                }

                await Task.Delay(4000);
                BackButton.Visibility = Visibility.Hidden;
                await Task.Delay(1000);
                ConnectToServerAndRequestMatch();
            }
            catch (Exception ex)
            {
                NotificationManager.Instance.ShowNotification(
                                "Lỗi",
                                "Tải dữ liệu người dùng thất bại",
                                Notification.NotificationType.Info,
                                3000
                            );
                NavigationService?.GoBack();
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _queueTimer.Stop();
            _stopwatch.Reset();
            _movingDotStoryboard?.Stop();

            DisconnectFromServer();
        }

        private void QueueTimer_Tick(object sender, EventArgs e)
        {
            tb_QueueTime.Text = _stopwatch.Elapsed.ToString(@"mm\:ss");
        }

        private void AnimationGrid_Loaded(object sender, RoutedEventArgs e)
        {
            var storyboard = (Storyboard)this.Resources["PulsingBorderStoryboard"];
            storyboard?.Begin();
        }

        private void BackButton_Checked(object sender, RoutedEventArgs e)
        {
            DisconnectFromServer();

            if (NavigationService != null && NavigationService.CanGoBack)
            {
                var slideOutAnimation = new DoubleAnimation
                {
                    From = 0,
                    To = _mainWindow.ActualWidth,
                    Duration = TimeSpan.FromSeconds(0.5),
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
                };

                if (this.RenderTransform == null || !(this.RenderTransform is TranslateTransform))
                {
                    this.RenderTransform = new TranslateTransform();
                }

                var transform = (TranslateTransform)this.RenderTransform;

                slideOutAnimation.Completed += (s, args) =>
                {
                    var navService = NavigationService;
                    navService.GoBack();

                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        if (navService.Content is Page previousPage)
                        {
                            previousPage.RenderTransform = new TranslateTransform(-_mainWindow.ActualWidth, 0);

                            var previousTransform = (TranslateTransform)previousPage.RenderTransform;

                            var slideInAnimation = new DoubleAnimation
                            {
                                From = -_mainWindow.ActualWidth,
                                To = 0,
                                Duration = TimeSpan.FromSeconds(0.5),
                                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                            };

                            previousTransform.BeginAnimation(TranslateTransform.XProperty, slideInAnimation);
                        }
                    }), System.Windows.Threading.DispatcherPriority.Loaded);
                };

                transform.BeginAnimation(TranslateTransform.XProperty, slideOutAnimation);
            }
        }

        private void ConnectToServerAndRequestMatch()
        {
            try
            {
                if (this.IsLoaded)
                {
                    _tcpClient = new TcpClient();
                    _tcpClient.Connect("34.68.212.10", 9999);
                    _stream = _tcpClient.GetStream();
                    _isConnected = true;

                    _receiveThread = new Thread(ReceiveFromServer);
                    _receiveThread.IsBackground = true;
                    _receiveThread.Start();

                    SendMatchRequest();
                }
                else
                {
                    Console.WriteLine("[DEBUG] Matchmaking page not loaded, not connecting to the server!");
                }
            }
            catch (Exception ex)
            {
                NotificationManager.Instance.ShowNotification(
                    "Lỗi",    
                    "Không thể kết nối đến server",
                    Notification.NotificationType.Info,
                    3000
                );
                BackButton_Checked(null, null);
            }
        }

        private void SendMatchRequest()
        {
            try
            {
                string message = $"[MATCH_REQUEST];{_username}";
                byte[] data = Encoding.UTF8.GetBytes(message);
                _stream.Write(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] SendMatchRequest: {ex.Message}");
            }
        }

        private void ReceiveFromServer()
        {
            byte[] buffer = new byte[1024];

            try
            {
                while (_isConnected && _tcpClient.Connected)
                {
                    if (!_stream.DataAvailable)
                    {
                        Thread.Sleep(10);
                        continue;
                    }

                    int bytesRead = _stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break;

                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                    ProcessServerMessage(message);
                }
            }
            catch (Exception ex)
            {
                if (_isConnected)
                    Console.WriteLine($"[ERROR] ReceiveFromServer: {ex.Message}");
            }
        }

        private void ProcessServerMessage(string message)
        {
            string[] parts = message.Split(';');
            if (parts.Length < 1) return;

            string command = parts[0];

            switch (command)
            {
                case "[INIT]":
                    if (parts.Length >= 4)
                    {
                        int clock1 = int.Parse(parts[1]);
                        int clock2 = int.Parse(parts[2]);
                        char playerSymbol = parts[3][0];
                        string opponentName = parts[4];

                        Dispatcher.Invoke(() =>
                        {
                            ShowOpponentFound(opponentName, playerSymbol);
                        });
                    }
                    break;

                case "[MATCH_FOUND]":
                    if (parts.Length >= 2)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            Console.WriteLine($"[LOG] Đã tìm thấy đối thủ: {parts[1]}");
                        });
                    }
                    break;

                case "[ALREADY_IN_MATCH]":
                    Dispatcher.Invoke(() =>
                    {
                        NotificationManager.Instance.ShowNotification(
                                "Lỗi",
                                "Bạn đang ở trong trận đấu khác",
                                Notification.NotificationType.Info,
                                3000
                            );
                        BackButton_Checked(null, null);
                    });
                    break;

                case "[INVALID_REQUEST]":
                    Dispatcher.Invoke(() =>
                    {
                        NotificationManager.Instance.ShowNotification(
                                "Lỗi",
                                "Yêu cầu không hợp lệ",
                                Notification.NotificationType.Info,
                                3000
                            );
                        BackButton_Checked(null, null);
                    });
                    break;

                default:
                    Console.WriteLine($"[UNKNOWN] {message}");
                    break;
            }
        }

        private void DisconnectFromServer()
        {
            try
            {
                _isConnected = false;

                if (sp_FindingOpponent?.Visibility == Visibility.Visible)
                {
                    Console.WriteLine("[MATCHMAKING] Closing connection (user cancelled)");

                    if (_stream != null)
                    {
                        _stream.Close();
                        _stream = null;
                    }

                    if (_tcpClient != null)
                    {
                        _tcpClient.Close();
                        _tcpClient = null;
                    }
                }
                else
                {
                    Console.WriteLine("[MATCHMAKING] Keeping connection alive for GamePlay");
                }

                if (_receiveThread != null && _receiveThread.IsAlive)
                    _receiveThread.Join(100);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] DisconnectFromServer: {ex.Message}");
            }
        }

        public async Task ShowOpponentFound(string opponent_name, char playerSymbol)
        {
            try
            {
                Console.WriteLine("[MATCHMAKING] Stopping receive thread before navigation");
                _isConnected = false;

                if (_receiveThread != null && _receiveThread.IsAlive)
                {
                    _receiveThread.Join(500);
                }

                grid_OpponentFound.Opacity = 0;
                grid_OpponentFound.Visibility = Visibility.Visible;

                UserStatsModel? opponent_stats = await FireStoreHelper.GetUserStats(opponent_name);
                UserDataModel? opponent_data = await FireStoreHelper.GetUserInfo(opponent_name);

                if (opponent_stats != null)
                {
                    tb_OpponentName.Text = opponent_name;
                    lb_OpponentMatches.Text = opponent_stats.total_match.ToString();
                    tb_OpponentWinRate.Text = opponent_stats.total_match > 0
                        ? $"{(opponent_stats.Wins / (double)opponent_stats.total_match * 100):F1}%"
                        : "0%";
                    img_OpponentAvatar.Source = BitmapFrame.Create(new Uri(opponent_data.ImagePath));
                }
                else
                {
                    tb_OpponentName.Text = opponent_name;
                    lb_OpponentMatches.Text = "0";
                    tb_OpponentWinRate.Text = "0%";
                }

                _queueTimer.Stop();
                _stopwatch.Reset();
                _movingDotStoryboard?.Stop();

                sp_FindingOpponent.Visibility = Visibility.Collapsed;
                BackButton.Visibility = Visibility.Collapsed;

                var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.3))
                {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                grid_OpponentFound.BeginAnimation(OpacityProperty, fadeIn);

                if (!(grid_OpponentFound.RenderTransform is TranslateTransform))
                {
                    grid_OpponentFound.RenderTransform = new TranslateTransform();
                }
                var translateTransform = (TranslateTransform)grid_OpponentFound.RenderTransform;
                var slideInAnimation = new DoubleAnimation(100, 0, TimeSpan.FromSeconds(0.4))
                {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                translateTransform.BeginAnimation(TranslateTransform.XProperty, slideInAnimation);

                await Task.Delay(3000);

                Border blackOverlay = new Border
                {
                    Background = Brushes.Black,
                    Opacity = 0,
                    Visibility = Visibility.Visible
                };

                if (this.Content is Grid rootGrid)
                {
                    Grid.SetRowSpan(blackOverlay, 100);
                    Grid.SetColumnSpan(blackOverlay, 100);
                    Panel.SetZIndex(blackOverlay, 9999);
                    rootGrid.Children.Add(blackOverlay);
                }

                var fadeToBlack = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.8));

                fadeToBlack.Completed += (s, args) =>
                {

                    Dispatcher.Invoke(() =>
                    {

                        _mainWindow.MainBGM.Stop();

                        Console.WriteLine("[MATCHMAKING] Opening GamePlay");

                        var gamePlayWindow = new GamePlay(_tcpClient, _username, playerSymbol, opponent_name, _mainWindow) {
                            Owner = _mainWindow,
                            WindowStartupLocation = WindowStartupLocation.Manual,
                            Left = _mainWindow.Left,
                            Top = _mainWindow.Top
                        };

                        gamePlayWindow.Closed += (sender, e) =>
                        {
                            bool isWinner = gamePlayWindow.FinalResult_IsLocalPlayerWinner;
                            bool isDraw = gamePlayWindow.FinalResult_IsDraw;
                            string p1 = gamePlayWindow.player1Name;
                            string p2 = gamePlayWindow.player2Name;

                            _mainWindow.Left = gamePlayWindow.Left;
                            _mainWindow.Top = gamePlayWindow.Top;

                            Border mainOverlay = new Border
                            {
                                Background = Brushes.Black,
                                Opacity = 1,
                                Visibility = Visibility.Visible
                            };

                            Grid? mainRoot = null;
                            try
                            {
                                mainRoot = _mainWindow.FindName("MainGrid") as Grid;
                            }
                            catch { mainRoot = null; }

                            if (mainRoot == null && _mainWindow.Content is Grid g) mainRoot = g;

                            if (mainRoot != null)
                            {
                                Grid.SetRowSpan(mainOverlay, 100);
                                Grid.SetColumnSpan(mainOverlay, 100);
                                Panel.SetZIndex(mainOverlay, 99999);
                                mainRoot.Children.Add(mainOverlay);
                            }

                            _mainWindow.Visibility = Visibility.Visible;

                            var reveal = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.8))
                            {
                                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                            };

                            reveal.Completed += (s2, e2) =>
                            {
                                try
                                {
                                    if (mainRoot != null && mainRoot.Children.Contains(mainOverlay))
                                        mainRoot.Children.Remove(mainOverlay);
                                }
                                catch { }

                                _mainWindow.StackPanelMenu.Visibility = Visibility.Collapsed;
                                _mainWindow.MainFrame.Visibility = Visibility.Visible;

                                MatchResult resultPage = new MatchResult(isWinner, p1, p2, _mainWindow, isDraw);
                                _mainWindow.MainFrame.Navigate(resultPage);

                                if (_mainWindow.MainBGM.Source != null) _mainWindow.MainBGM.Play();
                            };

                            if (mainOverlay != null)
                                mainOverlay.BeginAnimation(UIElement.OpacityProperty, reveal);
                            else
                            {
                                _mainWindow.StackPanelMenu.Visibility = Visibility.Collapsed;
                                _mainWindow.MainFrame.Visibility = Visibility.Visible;

                                MatchResult resultPage = new MatchResult(isWinner, p1, p2, _mainWindow, isDraw);
                                _mainWindow.MainFrame.Navigate(resultPage);

                                if (_mainWindow.MainBGM.Source != null) _mainWindow.MainBGM.Play();
                            }
                        };

                        gamePlayWindow.Show();
                        _mainWindow.Visibility = Visibility.Collapsed;
                    });
                };

                blackOverlay.BeginAnimation(OpacityProperty, fadeToBlack);
            }
            catch (Exception ex)
            {
                NotificationManager.Instance.ShowNotification(
                    "Lỗi",
                    "Tải dữ liệu thất bại",
                    Notification.NotificationType.Info,
                    3000
                );
                NavigationService?.GoBack();
            }
        }
    }
}