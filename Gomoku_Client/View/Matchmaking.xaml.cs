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

                UserStatsModel? user_stats = await FireStoreHelper.GetUserStats(tb_PlayerName.Text);

                if (user_stats != null)
                {
                    lb_matches.Text = user_stats.total_match.ToString();
                    tb_WinRate.Text = user_stats.total_match > 0
                        ? $"{(user_stats.Wins / (double)user_stats.total_match * 100):F1}%"
                        : "0%";
                }

                await Task.Delay(2000);

                ConnectToServerAndRequestMatch();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load user data: {ex.Message}");
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ConnectToServerAndRequestMatch()
        {
            try
            {
                _tcpClient = new TcpClient();
                _tcpClient.Connect("127.0.0.1", 9999);
                _stream = _tcpClient.GetStream();
                _isConnected = true;

                _receiveThread = new Thread(ReceiveFromServer);
                _receiveThread.IsBackground = true;
                _receiveThread.Start();

                SendMatchRequest();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể kết nối đến server: {ex.Message}");
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
                        string opponentName = (playerSymbol == 'X') ? "OPPONENT" : "OPPONENT";

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
                        MessageBox.Show("Bạn đang trong một trận đấu khác", "Thông báo",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        BackButton_Checked(null, null);
                    });
                    break;

                case "[INVALID_REQUEST]":
                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show("Yêu cầu không hợp lệ", "Lỗi",
                            MessageBoxButton.OK, MessageBoxImage.Error);
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
                _queueTimer.Stop();
                _stopwatch.Reset();
                _movingDotStoryboard?.Stop();

                Console.WriteLine("[MATCHMAKING] Stopping receive thread before navigation");
                _isConnected = false;

                if (_receiveThread != null && _receiveThread.IsAlive)
                {
                    _receiveThread.Join(500);
                    Console.WriteLine("[MATCHMAKING] Receive thread stopped");
                }

                sp_FindingOpponent.Visibility = Visibility.Collapsed;
                BackButton.Visibility = Visibility.Collapsed;
                grid_OpponentFound.Visibility = Visibility.Visible;

                if (!(grid_OpponentFound.RenderTransform is TranslateTransform))
                {
                    grid_OpponentFound.RenderTransform = new TranslateTransform();
                }
                var translateTransform = (TranslateTransform)grid_OpponentFound.RenderTransform;

                var slideInAnimation = new DoubleAnimation
                {
                    From = 100,
                    To = 0,
                    Duration = TimeSpan.FromSeconds(0.4),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                translateTransform.BeginAnimation(TranslateTransform.XProperty, slideInAnimation);

                UserStatsModel? opponent_stats = await FireStoreHelper.GetUserStats(opponent_name);

                if (opponent_stats != null)
                {
                    tb_OpponentName.Text = opponent_name;
                    lb_OpponentMatches.Text = opponent_stats.total_match.ToString();
                    tb_OpponentWinRate.Text = opponent_stats.total_match > 0
                        ? $"{(opponent_stats.Wins / (double)opponent_stats.total_match * 100):F1}%"
                        : "0%";
                }
                else
                {
                    tb_OpponentName.Text = opponent_name;
                    lb_OpponentMatches.Text = "0";
                    tb_OpponentWinRate.Text = "0%";
                }

                await Task.Delay(2000);

                var slideOut = new DoubleAnimation
                {
                    From = 0,
                    To = -_mainWindow.ActualWidth,
                    Duration = TimeSpan.FromSeconds(0.5),
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
                };

                if (this.RenderTransform == null || !(this.RenderTransform is TranslateTransform))
                {
                    this.RenderTransform = new TranslateTransform();
                }

                var transform = (TranslateTransform)this.RenderTransform;

                slideOut.Completed += (s, args) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        Console.WriteLine("[MATCHMAKING] Navigating to GamePlay");
                        Console.WriteLine($"[MATCHMAKING] TcpClient.Connected: {_tcpClient?.Connected}");
                        Console.WriteLine($"[MATCHMAKING] Stream.CanRead: {_stream?.CanRead}");

                        GamePlay gamePlayPage = new GamePlay(_tcpClient, _username, playerSymbol, opponent_name, _mainWindow);

                        var slideIn = new DoubleAnimation
                        {
                            From = _mainWindow.ActualWidth,
                            To = 0,
                            Duration = TimeSpan.FromSeconds(0.5),
                            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                        };

                        gamePlayPage.RenderTransform = new TranslateTransform(_mainWindow.ActualWidth, 0);
                        NavigationService.Navigate(gamePlayPage);

                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            var gameTransform = (TranslateTransform)gamePlayPage.RenderTransform;
                            gameTransform.BeginAnimation(TranslateTransform.XProperty, slideIn);
                        }), DispatcherPriority.Loaded);
                    });
                };

                transform.BeginAnimation(TranslateTransform.XProperty, slideOut);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load opponent data: {ex.Message}");
                NavigationService?.GoBack();
            }
        }
    }
}