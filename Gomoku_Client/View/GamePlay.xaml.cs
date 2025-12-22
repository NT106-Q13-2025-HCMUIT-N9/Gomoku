using System;
using System.Collections.Generic;
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
        private NetworkStream stream;
        private Thread receiveThread;
        private bool isConnected = false;
        private MainGameUI mainWindow;

        public GamePlay(TcpClient client, string username, char symbol, string opponent, MainGameUI window)
        {
            InitializeComponent();

            this.mainWindow = window;

            if (client == null || !client.Connected)
            {
                MessageBox.Show("Kết nối bị mất. Vui lòng thử lại.");
                NavigationService?.GoBack();
                return;
            }

            this.tcpClient = client;
            this.stream = client.GetStream();
            this.player1Name = username;
            this.playerSymbol = symbol;
            this.opponentName = opponent;
            this.player2Name = opponent;

            this.isPlayerTurn = (symbol == 'X');
            this.isConnected = true;
            this.isGameOver = false;

            InitializeGame();
            DrawBoard();
            SetupTimers();
            UpdateGameStatus();

            receiveThread = new Thread(ReceiveFromServer);
            receiveThread.IsBackground = true;
            receiveThread.Start();

            Dispatcher.Invoke(() =>
            {
                Player1NameText.Text = $"{player1Name} ({playerSymbol})";
                Player2NameText.Text = $"{player2Name} ({GetOpponentSymbol()})";

                if (isPlayerTurn)
                {
                    player1Timer.Start();
                    GameStatusText.Text = $"Lượt của bạn ({playerSymbol})";
                }
                else
                {
                    player2Timer.Start();
                    GameStatusText.Text = $"Lượt của đối thủ ({GetOpponentSymbol()})";
                }

                SendReadyToServer();
            });

            Console.WriteLine($"[INIT] Player: {username}, Symbol: {symbol}, IsPlayerTurn: {isPlayerTurn}, IsGameOver: {isGameOver}");
        }

        private char GetOpponentSymbol()
        {
            return playerSymbol == 'X' ? 'O' : 'X';
        }

        private void SendReadyToServer()
        {
            try
            {
                string message = $"[READY];{player1Name}";
                byte[] data = Encoding.UTF8.GetBytes(message);
                stream.Write(data, 0, data.Length);
                stream.Flush();
                Console.WriteLine($"[SEND] {message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] SendReadyToServer: {ex.Message}");
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            Disconnect();
        }

        private void Disconnect()
        {
            try
            {
                isConnected = false;
                isGameOver = true;

                player1Timer?.Stop();
                player2Timer?.Stop();

                if (receiveThread != null && receiveThread.IsAlive)
                {
                    receiveThread.Join(100);
                }

                if (stream != null)
                    stream.Close();

                if (tcpClient != null)
                    tcpClient.Close();
            }
            catch { }
        }

        private void BoardCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Console.WriteLine($"[CLICK] isGameOver: {isGameOver}, isPlayerTurn: {isPlayerTurn}");

            if (isGameOver)
            {
                Console.WriteLine("[CLICK] Game is over, click ignored");
                return;
            }

            if (!isPlayerTurn)
            {
                Console.WriteLine("[CLICK] Not player's turn, click ignored");
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
                Console.WriteLine("[CLICK] Cell already occupied");
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
                SendMatchEnd();
                isConnected = false;
            }

            player1Timer?.Stop();
            player2Timer?.Stop();
            NavigationService?.GoBack();
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

        private void SendChatMessage()
        {
            string message = tb_Message.Text.Trim();

            if (string.IsNullOrEmpty(message))
                return;

            DisplayChatMessage(player1Name, message, true);

            try
            {
                string chatMessage = $"[CHAT];{player1Name};{message}";
                byte[] data = Encoding.UTF8.GetBytes(chatMessage);
                stream.Write(data, 0, data.Length);
                stream.Flush();
                Console.WriteLine($"[SEND] {chatMessage}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] SendChatMessage: {ex.Message}");
            }

            tb_Message.Clear();
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
            player1Timer.Tick += Player1Timer_Tick;

            player2Timer = new DispatcherTimer();
            player2Timer.Interval = TimeSpan.FromSeconds(1);
            player2Timer.Tick += Player2Timer_Tick;

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

        private TextBlock FindTimerTextBlock(bool isPlayer1)
        {
            var textBlocks = FindVisualChildren<TextBlock>(this);

            foreach (var tb in textBlocks)
            {
                if (tb.FontFamily.Source == "Consolas" && tb.FontSize == 30)
                {
                    if (isPlayer1 && tb.Foreground is SolidColorBrush brush &&
                        brush.Color.ToString() == "#FF00D946")
                    {
                        return tb;
                    }
                    else if (!isPlayer1 && tb.Foreground is SolidColorBrush brush2 &&
                             brush2.Color.ToString() == "#FFECECEC")
                    {
                        return tb;
                    }
                }
            }

            return null;
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

        private void SwitchTurn()
        {
            Dispatcher.Invoke(() =>
            {
                isPlayerTurn = !isPlayerTurn;

                if (isPlayerTurn)
                {
                    player2Timer.Stop();
                    player1Timer.Start();
                }
                else
                {
                    player1Timer.Stop();
                    player2Timer.Start();
                }

                UpdateGameStatus();
                Console.WriteLine($"[SWITCH] isPlayerTurn: {isPlayerTurn}");
            });
        }

        private void UpdateGameStatus()
        {
            Dispatcher.Invoke(() =>
            {
                string currentPlayer = isPlayerTurn ? player1Name : player2Name;
                char currentSymbol = isPlayerTurn ? playerSymbol : GetOpponentSymbol();
                GameStatusText.Text = $"Lượt của {currentPlayer} ({currentSymbol})";
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
                MessageBoxResult result = MessageBox.Show(
                    message + "\n\nThoát về màn hình chính?",
                    "Kết thúc trận đấu",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Information
                );

                if (result == MessageBoxResult.Yes)
                {
                    NavigationService?.GoBack();
                }
            });
        }

        private void Player1Timer_Tick(object sender, EventArgs e)
        {
            player1TimeLeft = player1TimeLeft.Subtract(TimeSpan.FromSeconds(1));
            UpdatePlayer1TimerDisplay();

            if (player1TimeLeft.TotalSeconds <= 0)
            {
                player1Timer.Stop();
                GameOver(false, $"{player2Name} thắng do {player1Name} hết thời gian!");
            }
        }

        private void Player2Timer_Tick(object sender, EventArgs e)
        {
            player2TimeLeft = player2TimeLeft.Subtract(TimeSpan.FromSeconds(1));
            UpdatePlayer2TimerDisplay();

            if (player2TimeLeft.TotalSeconds <= 0)
            {
                player2Timer.Stop();
                GameOver(true, $"{player1Name} thắng do {player2Name} hết thời gian!");
            }
        }

        private void SendMoveToServer(int row, int col)
        {
            try
            {
                string message = $"[MOVE];{row};{col}";
                byte[] data = Encoding.UTF8.GetBytes(message);
                stream.Write(data, 0, data.Length);
                stream.Flush();
                Console.WriteLine($"[SEND] {message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] SendMoveToServer: {ex.Message}");
            }
        }

        private void SendResignToServer()
        {
            try
            {
                string message = "[RESIGN]";
                byte[] data = Encoding.UTF8.GetBytes(message);
                stream.Write(data, 0, data.Length);
                stream.Flush();
                Console.WriteLine($"[SEND] {message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] SendResignToServer: {ex.Message}");
            }
        }

        private void SendMatchEnd()
        {
            try
            {
                string message = $"[MATCH_END];{player1Name}";
                byte[] data = Encoding.UTF8.GetBytes(message);
                stream.Write(data, 0, data.Length);
                stream.Flush();
                Console.WriteLine($"[SEND] {message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] SendMatchEnd: {ex.Message}");
            }
        }

        private void ReceiveFromServer()
        {
            byte[] buffer = new byte[4096];

            try
            {
                while (isConnected && tcpClient.Connected)
                {
                    if (!stream.DataAvailable)
                    {
                        Thread.Sleep(10);
                        continue;
                    }

                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break;

                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                    Console.WriteLine($"[RECV] {message}");
                    ProcessServerMessage(message);
                }
            }
            catch (Exception ex)
            {
                if (isConnected)
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
                case "[MOVE1]":
                    if (parts.Length >= 3)
                    {
                        int row = int.Parse(parts[1]);
                        int col = int.Parse(parts[2]);
                        Console.WriteLine($"[PROCESS] MOVE1 at ({row},{col})");
                        Dispatcher.Invoke(() =>
                        {
                            PlaceStone(row, col, true);
                            if (CheckWin(row, col, 1))
                            {
                                string winnerName = (playerSymbol == 'X') ? player1Name : player2Name;
                                GameOver(playerSymbol == 'X', $"{winnerName} thắng!");
                            }
                            else if (moveCount >= boardSize * boardSize)
                            {
                                GameOver(null, "Hòa! Bàn cờ đã đầy!");
                            }
                            else
                            {
                                SwitchTurn();
                            }
                        });
                    }
                    break;

                case "[MOVE2]":
                    if (parts.Length >= 3)
                    {
                        int row = int.Parse(parts[1]);
                        int col = int.Parse(parts[2]);
                        Console.WriteLine($"[PROCESS] MOVE2 at ({row},{col})");
                        Dispatcher.Invoke(() =>
                        {
                            PlaceStone(row, col, false);
                            if (CheckWin(row, col, 2))
                            {
                                string winnerName = (playerSymbol == 'O') ? player1Name : player2Name;
                                GameOver(playerSymbol == 'O', $"{winnerName} thắng!");
                            }
                            else if (moveCount >= boardSize * boardSize)
                            {
                                GameOver(null, "Hòa! Bàn cờ đã đầy!");
                            }
                            else
                            {
                                SwitchTurn();
                            }
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
                        MessageBox.Show("Trận đấu đã kết thúc", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        NavigationService?.GoBack();
                    });
                    break;

                case "[TIME1]":
                    if (parts.Length >= 2)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            if (int.TryParse(parts[1], out int time))
                            {
                                player1TimeLeft = TimeSpan.FromSeconds(time);
                                UpdatePlayer1TimerDisplay();
                            }
                        });
                    }
                    break;

                case "[TIME2]":
                    if (parts.Length >= 2)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            if (int.TryParse(parts[1], out int time))
                            {
                                player2TimeLeft = TimeSpan.FromSeconds(time);
                                UpdatePlayer2TimerDisplay();
                            }
                        });
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