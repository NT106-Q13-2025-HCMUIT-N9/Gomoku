using Gomoku_Client.Model;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Gomoku_Client.View
{
    public partial class GamePlay : Page
    {
        private const int boardSize = 15;
        private const double cellSize = 46.5;

        private int[,] board;
        private bool isPlayerTurn = false;
        private bool isGameOver = false;
        private int moveCount = 0;
        private char playerSymbol = ' ';
        private string opponentName = "OPPONENT";

        private DispatcherTimer player1Timer;
        private DispatcherTimer player2Timer;
        private TimeSpan player1TimeLeft = TimeSpan.FromMinutes(5);
        private TimeSpan player2TimeLeft = TimeSpan.FromMinutes(5);

        private string player1Name = "YOU";
        private string player2Name = "OPPONENT";

        private TcpClient tcpClient;
        private Thread receiveThread;
        private bool isConnected = false;
        private MainGameUI mainWindow;

        //SoundMaker
        public MediaPlayer MainBGM = new MediaPlayer();
        public MediaPlayer ButtonClick = new MediaPlayer();
        public MediaPlayer Keyboard = new MediaPlayer();
        public GamePlay(TcpClient client, string username, char symbol, string opponent, MainGameUI window)
        {
            InitializeComponent();

            this.mainWindow = window;

            this.tcpClient = client;

            this.player1Name = username;
            this.playerSymbol = symbol;
            this.opponentName = opponent;
            this.player2Name = opponent;

            this.Loaded += GamePlay_Loaded;
        }

        private void GamePlay_Loaded(object sender, RoutedEventArgs e)
        {
            if (tcpClient == null || !tcpClient.Connected)
            {
                MessageBox.Show("Kết nối bị mất. Vui lòng thử lại.");
                ExitToHome();
                return;
            }

            this.isConnected = true;
            this.isPlayerTurn = (playerSymbol == 'X');
            this.isGameOver = false;

<<<<<<< Updated upstream
=======
            SetAvatar(player1Name, opponentName);

>>>>>>> Stashed changes
            InitializeGame();
            DrawBoard();
            SetupTimers();
            UpdateGameStatus();

            Player1NameText.Text = $"{player1Name} (BẠN)";
            Player2NameText.Text = $"{player2Name}";

            GameStatusText.Text = isPlayerTurn
                ? $"Lượt của bạn "
                : $"Lượt của {player2Name}";

            if (receiveThread == null || !receiveThread.IsAlive)
            {
                receiveThread = new Thread(ReceiveFromServer);
                receiveThread.IsBackground = true;
                receiveThread.Start();
            }

<<<<<<< Updated upstream
            receiveThread = new Thread(ReceiveFromServer);
            receiveThread.IsBackground = true;
            receiveThread.Start();
            Console.WriteLine("[INIT] Receive thread started immediately");
            StarSound();
=======
            try
            {
                StartSound();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi âm thanh: " + ex.Message);
            }
>>>>>>> Stashed changes
        }

        private void StarSound()
        {
            List<string> BGM = new List<string>();
            BGM.Add(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Sounds", "LOLTheme.mp3"));
            BGM.Add(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Sounds", "Awaken.mp3"));
            BGM.Add(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Sounds", "LegendsNeverDie.mp3"));

            int BGMNumber = Random.Shared.Next(0, 3);

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

            double BGMVolume = mainWindow.MasterVolValue * mainWindow.BGMVolValue;
            double SFXVolume = mainWindow.MasterVolValue * mainWindow.SFXVolValue;

            MainBGM.Volume = BGMVolume;
            ButtonClick.Volume = SFXVolume;
            Keyboard.Volume = SFXVolume;

            MainBGM.MediaOpened += (s, e) =>
            {
                MainBGM.Play();
            };

            MainBGM.MediaEnded += (s, e) =>
            {
                BGMNumber = Random.Shared.Next(0, 3);
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

        private char GetOpponentSymbol()
        {
            return playerSymbol == 'X' ? 'O' : 'X';
        }


        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            Disconnect();
        }

        private async Task Disconnect()
        {
            UserState.currentState = State.Ready;
            try
            {
                isConnected = false;
                isGameOver = true;

                player1Timer?.Stop();
                player2Timer?.Stop();

                if (receiveThread != null && receiveThread.IsAlive)
                {
                    receiveThread.Join(300);
                    await Task.Delay(300);
                    Console.WriteLine($"[DEBUG] Receive thread status: {receiveThread != null} and IsAlive: {receiveThread.IsAlive}");
                }

                if (tcpClient != null)
                {
                    try { tcpClient.Close(); } catch { }
                    tcpClient = null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Disconnect: {ex.Message}");
            }
        }

        private void BoardCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Console.WriteLine($"[CLICK] isGameOver: {isGameOver}, isPlayerTurn: {isPlayerTurn}, playerSymbol: {playerSymbol}");

            if (isGameOver)
            {
                Console.WriteLine("[CLICK] Game is over, click ignored");
                return;
            }

            if (!isPlayerTurn)
            {
                Console.WriteLine($"[CLICK] Not player's turn (isPlayerTurn={isPlayerTurn}), click ignored");
                return;
            }

            Point clickPos = e.GetPosition(BoardCanvas);

            int col = (int)Math.Round((clickPos.X - cellSize / 2.0) / cellSize);
            int row = (int)Math.Round((clickPos.Y - cellSize / 2.0) / cellSize);

            Console.WriteLine($"[CLICK] Position - Row: {row}, Col: {col}");

            if (row < 0 || row >= boardSize || col < 0 || col >= boardSize)
            {
                Console.WriteLine("[CLICK] Out of bounds");
                return;
            }

            if (board[row, col] != 0)
            {
                Console.WriteLine($"[CLICK] Cell already occupied: board[{row},{col}] = {board[row, col]}");
                return;
            }

            Console.WriteLine($"[CLICK] Valid move, sending to server");
            SendMoveToServer(row, col);
        }
        private void SendMessage_Click(object sender, RoutedEventArgs e)
        {
            SendChatMessage();
        }

        private void SurrenderButton_Click(object sender, RoutedEventArgs e)
        {
            SurrenderConfirmationOverlay.Visibility = Visibility.Visible;
            var storyboard = (Storyboard)this.Resources["FadeInStoryboard"];
            var border = (Border)((Grid)SurrenderConfirmationOverlay).Children[0];
            storyboard.Begin(border);
        }

        private void ConfirmSurrenderButton_Click(object sender, RoutedEventArgs e)
        {
            SendResignToServer();
            SurrenderConfirmationOverlay.Visibility = Visibility.Collapsed;
        }

        private void CancelSurrenderButton_Click(object sender, RoutedEventArgs e)
        {
            var storyboard = (Storyboard)this.Resources["FadeOutStoryboard"];
            var border = (Border)((Grid)SurrenderConfirmationOverlay).Children[0];
            storyboard.Completed += (s, args) =>
            {
                SurrenderConfirmationOverlay.Visibility = Visibility.Collapsed;
            };
            storyboard.Begin(border);
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            QuitConfirmationOverlay.Visibility = Visibility.Visible;
            var storyboard = (Storyboard)this.Resources["FadeInStoryboard"];
            var border = (Border)((Grid)QuitConfirmationOverlay).Children[0];
            storyboard.Begin(border);
        }

        private void ConfirmQuitButton_Click(object sender, RoutedEventArgs e)
        {
            if (isConnected)
            {
                SendResignToServer();
                isConnected = false;
            }

            player1Timer?.Stop();
            player2Timer?.Stop();
            ExitToHome();
            return;
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

        private void ChatInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendMessage_Click(sender, e);
                e.Handled = true;
            }

            if (e.Key == Key.Escape)
            {
                tb_Message.Clear();
                e.Handled = true;
            }
        }

        private void DisplayChatMessage(string senderName, string message, bool isOwnMessage)
        {
            Dispatcher.Invoke(() =>
            {
                Border messageBorder = new Border
                {
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0F0F1E")),
                    CornerRadius = new CornerRadius(5),
                    Padding = new Thickness(8),
                    Margin = new Thickness(0, 0, 0, 6)
                };

                TextBlock messageText = new TextBlock
                {
                    Text = $"{senderName}: {message}",
                    FontSize = 10,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isOwnMessage ? "#00D946" : "#ECECEC")),
                    TextWrapping = TextWrapping.Wrap
                };

                messageBorder.Child = messageText;
                ChatMessagesPanel.Children.Add(messageBorder);


            });
        }

        public void InitializeGame()
        {
            board = new int[boardSize, boardSize];
            isGameOver = false;
            moveCount = 0;

            player1TimeLeft = TimeSpan.FromMinutes(5);
            player2TimeLeft = TimeSpan.FromMinutes(5);
        }

        public void DrawBoard()
        {
            BoardCanvas.Children.Clear();

            double startX = cellSize / 2.0;
            double startY = cellSize / 2.0;
            double endPos = cellSize * boardSize - cellSize / 2.0;

            for (int i = 0; i < boardSize; i++)
            {
                Line line = new Line
                {
                    X1 = startX,
                    Y1 = startY + i * cellSize,
                    X2 = endPos,
                    Y2 = startY + i * cellSize,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1.5
                };
                BoardCanvas.Children.Add(line);
            }

            for (int i = 0; i < boardSize; i++)
            {
                Line line = new Line
                {
                    X1 = startX + i * cellSize,
                    Y1 = startY,
                    X2 = startX + i * cellSize,
                    Y2 = endPos,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1.5
                };
                BoardCanvas.Children.Add(line);
            }

            DrawStarPoint(3, 3);
            DrawStarPoint(3, 11);
            DrawStarPoint(7, 7);
            DrawStarPoint(11, 3);
            DrawStarPoint(11, 11);
        }

        private void DrawStarPoint(int row, int col)
        {
            Ellipse dot = new Ellipse
            {
                Width = 6,
                Height = 6,
                Fill = Brushes.Black
            };

            double x = cellSize / 2.0 + col * cellSize - 3;
            double y = cellSize / 2.0 + row * cellSize - 3;

            Canvas.SetLeft(dot, x);
            Canvas.SetTop(dot, y);
            BoardCanvas.Children.Add(dot);
        }

        private void SetupTimers()
        {
            player1Timer = new DispatcherTimer();
            player1Timer.Interval = TimeSpan.FromSeconds(1);

            player2Timer = new DispatcherTimer();
            player2Timer.Interval = TimeSpan.FromSeconds(1);

            player1TimeLeft = TimeSpan.FromMinutes(5);
            player2TimeLeft = TimeSpan.FromMinutes(5);

            UpdatePlayer1TimerDisplay();
            UpdatePlayer2TimerDisplay();
        }

        private void UpdatePlayer1TimerDisplay()
        {
            Player1TimerText.Text = player1TimeLeft.ToString(@"mm\:ss");
            if (player1TimeLeft.TotalSeconds <= 30)
            {
                Player1TimerText.Foreground = new SolidColorBrush(Colors.Red);
            }
            else
            {
                Player1TimerText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00D946"));
            }
        }

        private void UpdatePlayer2TimerDisplay()
        {
            Player2TimerText.Text = player2TimeLeft.ToString(@"mm\:ss");
            if (player2TimeLeft.TotalSeconds <= 30)
            {
                Player2TimerText.Foreground = new SolidColorBrush(Colors.Red);
            }
            else
            {
                Player2TimerText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ECECEC"));
            }
        }

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        private void PlaceStone(int row, int col, bool isBlack)
        {
            board[row, col] = isBlack ? 1 : 2;
            moveCount++;

            DrawStone(row, col, isBlack);
        }

        private void DrawStone(int row, int col, bool isBlack)
        {
            Dispatcher.Invoke(() =>
            {
                Ellipse stone = new Ellipse();

                if (isBlack)
                    stone.Style = (Style)FindResource("BlackStone");
                else
                    stone.Style = (Style)FindResource("WhiteStone");

                double x = cellSize / 2.0 + col * cellSize - stone.Width / 2;
                double y = cellSize / 2.0 + row * cellSize - stone.Height / 2;

                Canvas.SetLeft(stone, x);
                Canvas.SetTop(stone, y);

                BoardCanvas.Children.Add(stone);
            });
        }

        private void UpdateGameStatus()
        {
            Dispatcher.Invoke(() =>
            {
                UpdateTurnUI();
                GameStatusText.Text = isPlayerTurn ? "Lượt của bạn" : $"Lượt của {player2Name}";
            });
        }

        private bool CheckWin(int row, int col, int player)
        {
            return CheckDirection(row, col, 0, 1, player) ||
                   CheckDirection(row, col, 1, 0, player) ||
                   CheckDirection(row, col, 1, 1, player) ||
                   CheckDirection(row, col, 1, -1, player);
        }

        private bool CheckDirection(int row, int col, int dRow, int dCol, int player)
        {
            int count = 1;
            count += CountStones(row, col, dRow, dCol, player);
            count += CountStones(row, col, -dRow, -dCol, player);
            return count >= 5;
        }

        private int CountStones(int row, int col, int dRow, int dCol, int player)
        {
            int count = 0;
            int r = row + dRow;
            int c = col + dCol;

            while (r >= 0 && r < boardSize && c >= 0 && c < boardSize && board[r, c] == player)
            {
                count++;
                r += dRow;
                c += dCol;
            }

            return count;
        }

        private void GameOver(bool? player1Wins, string message)
        {
            isGameOver = true;
            player1Timer.Stop();
            player2Timer.Stop();

            Dispatcher.Invoke(() =>
            {
                bool isLocalPlayerWinner = player1Wins.HasValue && player1Wins.Value;
                bool isDraw = !player1Wins.HasValue;

                Border blackOverlay = new Border
                {
                    Background = new SolidColorBrush(Colors.Black),
                    Opacity = 0,
                    Width = mainWindow.ActualWidth,
                    Height = mainWindow.ActualHeight
                };

                var rootGrid = this.Content as Grid;
                if (rootGrid != null)
                {
                    rootGrid.Children.Add(blackOverlay);
                    Panel.SetZIndex(blackOverlay, 9999);
                }

                var fadeToBlack = new DoubleAnimation
                {
                    From = 0,
                    To = 1,
                    Duration = TimeSpan.FromSeconds(0.5),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
                };

                fadeToBlack.Completed += (s, args) =>
                {
                    MatchResult resultPage = new MatchResult(
                        isLocalPlayerWinner,
                        player1Name,
                        player2Name,
                        mainWindow,
                        isDraw
                    );

                    resultPage.Opacity = 0;

                    if (mainWindow != null)
                    {
                        try
                        {
                            mainWindow.NavigateWithAnimation(resultPage);

                            Dispatcher.BeginInvoke(new Action(() =>
                            {
                                var fadeIn = new DoubleAnimation
                                {
                                    From = 0,
                                    To = 1,
                                    Duration = TimeSpan.FromSeconds(0.5),
                                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                                };
                                resultPage.BeginAnimation(OpacityProperty, fadeIn);
                            }), DispatcherPriority.Loaded);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[ERROR] Navigation failed: {ex.Message}");
                            MessageBox.Show(message);
                            ExitToHome();
                        }
                    }
                    else
                    {
                        MessageBox.Show(message);
                        ExitToHome();
                    }
                };

                blackOverlay.BeginAnimation(OpacityProperty, fadeToBlack);
            });
        }


        private void SendMoveToServer(int row, int col)
        {
            try
            {
                if (!isConnected || tcpClient == null || !tcpClient.Connected)
                {
                    Console.WriteLine("[ERROR] Not connected to server");
                    return;
                }

                Socket socket = tcpClient.Client;
                if (socket == null || !socket.Connected)
                {
                    Console.WriteLine("[ERROR] Socket is not available for sending");
                    return;
                }

                string message = $"[MOVE];{row};{col}\n";
                byte[] data = Encoding.UTF8.GetBytes(message);
                socket.Send(data);
                Console.WriteLine($"[SEND] {message.TrimEnd()}");
            }
            catch (ObjectDisposedException ex)
            {
                Console.WriteLine($"[ERROR] SendMoveToServer: Socket disposed - {ex.Message}");
                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show("Mất kết nối với server.", "Lỗi kết nối", MessageBoxButton.OK, MessageBoxImage.Error);
                    ExitToHome();
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] SendMoveToServer: {ex.Message}");
            }
        }

        private void SendChatMessage()
        {
            string message = tb_Message.Text.Trim();

            if (string.IsNullOrEmpty(message))
                return;

            DisplayChatMessage(player1Name, message, true);

            try
            {
                Socket socket = tcpClient?.Client;
                if (socket == null || !socket.Connected)
                {
                    Console.WriteLine("[ERROR] Socket not available for chat");
                    return;
                }

                string chatMessage = $"[CHAT];{player1Name};{message}\n";
                byte[] data = Encoding.UTF8.GetBytes(chatMessage);
                socket.Send(data);
                Console.WriteLine($"[SEND] {chatMessage.TrimEnd()}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] SendChatMessage: {ex.Message}");
            }

            tb_Message.Clear();
        }

        private void SendResignToServer()
        {
            try
            {
                Socket socket = tcpClient?.Client;
                if (socket == null) return;

                string message = "[RESIGN]\n";
                byte[] data = Encoding.UTF8.GetBytes(message);
                socket.Send(data);
                Console.WriteLine($"[SEND] {message.TrimEnd()}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] SendResignToServer: {ex.Message}");
            }
        }

        private void ExitToHome()
        {
            try
            {
                isGameOver = true;
                player1Timer?.Stop();
                player2Timer?.Stop();

                if (isConnected)
                {
                    SendMatchEnd();
                    isConnected = false;
                }
                Disconnect();

                Dispatcher.Invoke(() =>
                {
                    mainWindow?.NavigateToLobby();
                });
            }
            catch (Exception ex)
            {
                Window.GetWindow(this)?.Close();
            }
        }

        private void UpdateTurnUI()
        {
            Dispatcher.Invoke(() =>
            {
                var activeColor = (Color)ColorConverter.ConvertFromString("#00D946");
                var inactiveColor = (Color)ColorConverter.ConvertFromString("#ECECEC");

                bool p1Active = isPlayerTurn;
                bool p2Active = !isPlayerTurn;

                AvatarBorder1.BorderBrush = new SolidColorBrush(p1Active ? activeColor : inactiveColor);
                AvatarShadow1.Color = p1Active ? activeColor : inactiveColor;
                Player1NameText.Foreground = new SolidColorBrush(p1Active ? activeColor : inactiveColor);
                Player1TimerText.Foreground = new SolidColorBrush(p1Active ? activeColor : inactiveColor);
                BackgroundTag1.Background = new SolidColorBrush(p1Active ? activeColor : inactiveColor);
                ShadowColorPlayer1.Color = p1Active ? activeColor : inactiveColor;

                AvatarBorder2.BorderBrush = new SolidColorBrush(p2Active ? activeColor : inactiveColor);
                AvatarShadow2.Color = p2Active ? activeColor : inactiveColor;
                Player2NameText.Foreground = new SolidColorBrush(p2Active ? activeColor : inactiveColor);
                Player2TimerText.Foreground = new SolidColorBrush(p2Active ? activeColor : inactiveColor);
                BackgroundTag2.Background = new SolidColorBrush(p2Active ? activeColor : inactiveColor);
                ShadowColorPlayer2.Color = p2Active ? activeColor : inactiveColor;
            });
        }

        private void SendMatchEnd()
        {
            try
            {
                Socket socket = tcpClient?.Client;
                if (socket == null) return;

                string message = $"[MATCH_END];{player1Name}\n";
                byte[] data = Encoding.UTF8.GetBytes(message);
                socket.Send(data);
                Console.WriteLine($"[SEND] {message.TrimEnd()}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] SendMatchEnd: {ex.Message}");
            }
        }

        private void ReceiveFromServer()
        {
            byte[] buffer = new byte[4096];
            StringBuilder messageBuffer = new StringBuilder();
            DateTime lastMessageTime = DateTime.Now;
            const int TIMEOUT_SECONDS = 15;

            Console.WriteLine("[RECEIVE] Thread started");

            try
            {
                Thread.Sleep(100);
                Console.WriteLine("[RECEIVE] Starting main receive loop");

                while (isConnected && tcpClient != null)
                {
                    if (!isConnected || isGameOver) break;
                    try
                    {
                        Socket socket = tcpClient.Client;

                        if (socket == null)
                        {
                            Console.WriteLine("[ERROR] Socket is null");
                            break;
                        }

                        if (!socket.Connected)
                        {
                            Console.WriteLine("[ERROR] Socket not connected, checking if data available...");
                            if (socket.Available == 0)
                            {
                                break;
                            }
                        }



                        TimeSpan timeSinceLastMessage = DateTime.Now - lastMessageTime;
                        if (timeSinceLastMessage.TotalSeconds > TIMEOUT_SECONDS)
                        {
                            if (!isConnected || isGameOver)
                            {
                                Console.WriteLine("[TIMEOUT] Ignored because user is exiting.");
                                break;
                            }
                            Console.WriteLine($"[TIMEOUT] No message for {TIMEOUT_SECONDS}s");

                            try
                            {
                                Dispatcher.BeginInvoke(() =>
                                {
                                    if (!isConnected || isGameOver) return;

                                    NotificationManager.Instance.ShowNotification(
                                        "Lỗi đường truyền",
                                        "Hết thời gian chờ! Bạn đã bị mất kết nối với server.",
                                        Notification.NotificationType.Info,
                                        5000
                                    );
                                    mainWindow?.NavigateToLobby();
                                });
                            }
                            catch { }
                            break;
                        }

                        if (socket.Available == 0)
                        {
                            Thread.Sleep(50);
                            continue;
                        }

                        int bytesRead = socket.Receive(buffer);

                        if (bytesRead == 0)
                        {
                            Console.WriteLine("[INFO] Server closed connection (0 bytes)");
                            break;
                        }

                        lastMessageTime = DateTime.Now;
                        Console.WriteLine($"[RECEIVE] Got {bytesRead} bytes, timeout reset");

                        string data = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        messageBuffer.Append(data);

                        string bufferedData = messageBuffer.ToString();
                        string[] lines = bufferedData.Split('\n');

                        for (int i = 0; i < lines.Length - 1; i++)
                        {
                            string msg = lines[i].Trim();
                            if (!string.IsNullOrEmpty(msg))
                            {
                                Console.WriteLine($"[RECV] {msg}");
                                try
                                {
                                    ProcessServerMessage(msg);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"[ERROR] ProcessServerMessage: {ex.Message}");
                                }
                            }
                        }

                        messageBuffer.Clear();
                        if (lines.Length > 0)
                        {
                            string lastLine = lines[lines.Length - 1];
                            if (!string.IsNullOrEmpty(lastLine) && !bufferedData.EndsWith("\n"))
                            {
                                messageBuffer.Append(lastLine);
                            }
                        }
                    }
                    catch (SocketException sockEx)
                    {
                        if (isConnected)
                            Console.WriteLine($"[ERROR] SocketException: {sockEx.ErrorCode} - {sockEx.Message}");
                        break;
                    }
                    catch (IOException ioEx)
                    {
                        if (isConnected)
                            Console.WriteLine($"[ERROR] IOException: {ioEx.Message}");
                        break;
                    }
                }
            }
            catch (ObjectDisposedException ex)
            {
                Console.WriteLine($"[ERROR] Socket disposed: {ex.Message}");
            }
            catch (Exception ex)
            {
                if (isConnected)
                    Console.WriteLine($"[ERROR] ReceiveFromServer: {ex.Message}");
            }
            finally
            {
                Console.WriteLine("[RECEIVE] Thread ended");

                if (isConnected && !isGameOver)
                {
                    try
                    {
                        Dispatcher.BeginInvoke(() =>
                        {
                            MessageBox.Show("Mất kết nối với server.", "Lỗi kết nối",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                            ExitToHome();
                        });
                    }
                    catch { }
                }
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
                    Console.WriteLine($"[PROCESS] INIT received - Match started");
                    Dispatcher.Invoke(() =>
                    {
                        UpdateGameStatus();
                    });
                    break;

                case "[MOVE1]":
                    if (parts.Length >= 3)
                    {
                        int row = int.Parse(parts[1]);
                        int col = int.Parse(parts[2]);
                        Console.WriteLine($"[PROCESS] MOVE1 at ({row},{col}), mySymbol: {playerSymbol}");

                        Dispatcher.Invoke(() =>
                        {
                            PlaceStone(row, col, true);

                            if (playerSymbol == 'X')
                            {
                                isPlayerTurn = false;
                                Console.WriteLine("[PROCESS] My move (X), turn OFF");
                            }
                            else
                            {
                                isPlayerTurn = true;
                                Console.WriteLine("[PROCESS] Opponent move (X), turn ON");
                            }

                            UpdateGameStatus();
                        });
                    }
                    break;

                case "[MOVE2]":
                    if (parts.Length >= 3)
                    {
                        int row = int.Parse(parts[1]);
                        int col = int.Parse(parts[2]);
                        Console.WriteLine($"[PROCESS] MOVE2 at ({row},{col}), mySymbol: {playerSymbol}");

                        Dispatcher.Invoke(() =>
                        {
                            PlaceStone(row, col, false);

                            if (playerSymbol == 'O')
                            {
                                isPlayerTurn = false;
                                Console.WriteLine("[PROCESS] My move (O), turn OFF");
                            }
                            else
                            {
                                isPlayerTurn = true;
                                Console.WriteLine("[PROCESS] Opponent move (O), turn ON");
                            }

                            UpdateGameStatus();
                        });
                    }
                    break;

                case "[WIN1]":
                    Dispatcher.Invoke(() =>
                    {
                        string winnerName = (playerSymbol == 'X') ? player1Name : player2Name;
                        GameOver(playerSymbol == 'X', $"{winnerName} thắng!");
                    });
                    break;

                case "[WIN2]":
                    Dispatcher.Invoke(() =>
                    {
                        string winnerName = (playerSymbol == 'O') ? player1Name : player2Name;
                        GameOver(playerSymbol == 'O', $"{winnerName} thắng!");
                    });
                    break;

                case "[DRAW]":
                    Dispatcher.Invoke(() =>
                    {
                        GameOver(null, "Hòa!");
                    });
                    break;

                case "[TIMEOUT1]":
                    Dispatcher.Invoke(() =>
                    {
                        string loserName = (playerSymbol == 'X') ? player1Name : player2Name;
                        string winnerName = (playerSymbol == 'X') ? player2Name : player1Name;
                        GameOver(playerSymbol == 'O', $"{winnerName} thắng do {loserName} hết thời gian!");
                    });
                    break;

                case "[TIMEOUT2]":
                    Dispatcher.Invoke(() =>
                    {
                        string loserName = (playerSymbol == 'O') ? player1Name : player2Name;
                        string winnerName = (playerSymbol == 'O') ? player2Name : player1Name;
                        GameOver(playerSymbol == 'X', $"{winnerName} thắng do {loserName} hết thời gian!");
                    });
                    break;

                case "[RESIGN1]":
                    Dispatcher.Invoke(() =>
                    {
                        string loserName = (playerSymbol == 'X') ? player1Name : player2Name;
                        string winnerName = (playerSymbol == 'X') ? player2Name : player1Name;
                        GameOver(playerSymbol == 'O', $"{winnerName} thắng do {loserName} đầu hàng!");
                    });
                    break;

                case "[RESIGN2]":
                    Dispatcher.Invoke(() =>
                    {
                        string loserName = (playerSymbol == 'O') ? player1Name : player2Name;
                        string winnerName = (playerSymbol == 'O') ? player2Name : player1Name;
                        GameOver(playerSymbol == 'X', $"{winnerName} thắng do {loserName} đầu hàng!");
                    });
                    break;

                case "[OPPONENT_DISCONNECTED]":
                    if (parts.Length >= 2)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            GameOver(true, $"Bạn thắng do đối thủ thoát!");
                        });
                    }
                    break;

                case "[MATCH_END]":
                    Dispatcher.Invoke(() =>
                    {
                        isGameOver = true;
                        player1Timer.Stop();
                        player2Timer.Stop();
                    });
                    break;

                case "[TIME1]":
                    if (parts.Length >= 2)
                    {
                        if (int.TryParse(parts[1], out int time))
                        {
                            Dispatcher.Invoke(() =>
                            {
                                if (playerSymbol == 'X')
                                {
                                    player1TimeLeft = TimeSpan.FromSeconds(time);
                                    UpdatePlayer1TimerDisplay();
                                }
                                else
                                {
                                    player2TimeLeft = TimeSpan.FromSeconds(time);
                                    UpdatePlayer2TimerDisplay();
                                }
                            });
                        }
                    }
                    break;

                case "[TIME2]":
                    if (parts.Length >= 2)
                    {
                        if (int.TryParse(parts[1], out int time))
                        {
                            Dispatcher.Invoke(() =>
                            {
                                if (playerSymbol == 'O')
                                {
                                    player1TimeLeft = TimeSpan.FromSeconds(time);
                                    UpdatePlayer1TimerDisplay();
                                }
                                else
                                {
                                    player2TimeLeft = TimeSpan.FromSeconds(time);
                                    UpdatePlayer2TimerDisplay();
                                }
                            });
                        }
                    }
                    break;

                case "[INVALID_MOVE]":
                    if (parts.Length >= 2)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            isPlayerTurn = true;
                            UpdateGameStatus();
                            MessageBox.Show(parts[1], "Nước đi không hợp lệ", MessageBoxButton.OK, MessageBoxImage.Warning);
                        });
                    }
                    break;

                case "[CHAT]":
                    if (parts.Length >= 3)
                    {
                        string senderName = parts[1];
                        string chatMessage = string.Join(";", parts.Skip(2));
                        Console.WriteLine($"[PROCESS] CHAT from {senderName}: {chatMessage}");
                        DisplayChatMessage(senderName, chatMessage, false);
                    }
                    break;

                default:
                    Console.WriteLine($"[UNKNOWN] {message}");
                    break;
            }
        }
    }
}