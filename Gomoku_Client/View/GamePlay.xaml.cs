using System;
using System.Collections.Generic;
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

namespace Gomoku_Client.View
{
    /// <summary>
    /// Interaction logic for GamePlay.xaml
    /// </summary>
    public partial class GamePlay : Window
    {
        // Constants Variables
        private const int boardSize = 15;
        private const double cellSize = 46.5;

        // Game state
        private int[,] board;
        private bool isPlayerTurn = true;
        private bool isGameOver = true;
        private int moveCount = 0;

        // Timers
        private DispatcherTimer player1Timer;
        private DispatcherTimer player2Timer;
        private TimeSpan player1TimeLeft = TimeSpan.FromMinutes(5);
        private TimeSpan player2TimeLeft = TimeSpan.FromMinutes(5);

        // Player Info
        private string player1Name = "YOU";
        private string player2Name = "OPPONENT";

        //SoundMaker
        public MediaPlayer MainBGM = new MediaPlayer();
        public MediaPlayer ButtonClick = new MediaPlayer();
        public MediaPlayer Keyboard = new MediaPlayer();
        public GamePlay()
        {
            InitializeComponent();
            InitializeGame();
            DrawBoard();
            SetupTimers();
            UpdateGameStatus();
            StarSound();
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
        
        private void BoardCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (isGameOver) return;

            // Lấy vị trí click
            Point clickPos = e.GetPosition(BoardCanvas);

            // Chuyển đổi tọa độ pixel sang tọa độ ô cờ
            int col = (int)Math.Round((clickPos.X - cellSize / 2.0) / cellSize);
            int row = (int)Math.Round((clickPos.Y - cellSize / 2.0) / cellSize);

            // Kiểm tra tọa độ hợp lệ
            if (row < 0 || row >= boardSize || col < 0 || col >= boardSize)
                return;

            // Kiểm tra ô đã có quân chưa
            if (board[row, col] != 0)
            {
                return;
            }

            // Đặt quân cờ
            PlaceStone(row, col);
        }

        private void SendMessage_Click(object sender, RoutedEventArgs e)
        {
            ButtonClick.Stop();
            ButtonClick.Play();
            SendChatMessage();
        }

        private void SurrenderButton_Click(object sender, RoutedEventArgs e)
        {
            ButtonClick.Stop();
            ButtonClick.Play();
            SurrenderConfirmationOverlay.Visibility = Visibility.Visible;
            var storyboard = (Storyboard)this.Resources["FadeInStoryboard"];
            var border = (Border)((Grid)SurrenderConfirmationOverlay).Children[0];
            storyboard.Begin(border);
        }

        private void ConfirmSurrenderButton_Click(object sender, RoutedEventArgs e)
        {
            ButtonClick.Stop();
            ButtonClick.Play();
            player1Timer?.Stop();
            player2Timer?.Stop();
            GameOver(false, $"{player1Name} đã đầu hàng! {player2Name} thắng!");
        }

        private void CancelSurrenderButton_Click(object sender, RoutedEventArgs e)
        {
            ButtonClick.Stop();
            ButtonClick.Play();
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
            ButtonClick.Stop();
            ButtonClick.Play();
            QuitConfirmationOverlay.Visibility = Visibility.Visible;
            var storyboard = (Storyboard)this.Resources["FadeInStoryboard"];
            var border = (Border)((Grid)QuitConfirmationOverlay).Children[0];
            storyboard.Begin(border);
        }

        private void ConfirmQuitButton_Click(object sender, RoutedEventArgs e)
        {
            ButtonClick.Stop();
            ButtonClick.Play();
            player1Timer?.Stop();
            player2Timer?.Stop();
            this.Close();
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

        private void ChatInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendMessage_Click(sender, e);

                e.Handled = true;
                return;
            }

            if ( e.Key == Key.Escape)
            {
                tb_Message.Clear();
                e.Handled = true;
                return;
            }
            Keyboard.Stop();
            Keyboard.Play();

        }

        private void SendChatMessage()
        {
            string message = tb_Message.Text.Trim();

            if (string.IsNullOrEmpty(message))
                return;

            // Tạo message bubble
            Border messageBorder = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0F0F1E")),
                CornerRadius = new CornerRadius(5),
                Padding = new Thickness(8),
                Margin = new Thickness(0, 0, 0, 6)
            };

            TextBlock messageText = new TextBlock
            {
                Text = $"{player1Name}: {message}",
                FontSize = 10,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00D946")),
                TextWrapping = TextWrapping.Wrap
            };

            messageBorder.Child = messageText;
            ChatMessagesPanel.Children.Add(messageBorder);

            // Clear input
            tb_Message.Clear();

            // TODO: Gửi message qua network đến đối thủ
        }

        public void ReceiveChatMessage(string message)
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
                    Text = $"{player2Name}: {message}",
                    FontSize = 10,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ECECEC")),
                    TextWrapping = TextWrapping.Wrap
                };

                messageBorder.Child = messageText;
                ChatMessagesPanel.Children.Add(messageBorder);
            });
        }

        public void InitializeGame()
        {
            board = new int[boardSize, boardSize];
            isPlayerTurn = true;
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

            // Vẽ các đường ngang
            for ( int i = 0; i < boardSize; i++ )
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
            // Vẽ các đường dọc
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

            // Vẽ các điểm đánh dấu (star points)
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
            // Timer cho Player 1
            player1Timer = new DispatcherTimer();
            player1Timer.Interval = TimeSpan.FromSeconds(1);
            player1Timer.Tick += Player1Timer_Tick;

            // Timer cho Player 2
            player2Timer = new DispatcherTimer();
            player2Timer.Interval = TimeSpan.FromSeconds(1);
            player2Timer.Tick += Player2Timer_Tick;

            // Reset thời gian
            player1TimeLeft = TimeSpan.FromMinutes(5);
            player2TimeLeft = TimeSpan.FromMinutes(5);

            // Cập nhật hiển thị ban đầu
            UpdatePlayer1TimerDisplay();
            UpdatePlayer2TimerDisplay();

            // Bắt đầu timer cho người chơi đi trước
            player1Timer.Start();
        }

        private void UpdatePlayer1TimerDisplay()
        {
            Player1TimerText.Text = player1TimeLeft.ToString(@"mm\:ss");
            // Đổi màu cảnh báo khi còn < 30 giây
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
            // Đổi màu cảnh báo khi còn < 30 giây
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
            // Tìm tất cả TextBlock trong Window
            var textBlocks = FindVisualChildren<TextBlock>(this);

            foreach (var tb in textBlocks)
            {
                if (tb.FontFamily.Source == "Consolas" && tb.FontSize == 30)
                {
                    // Kiểm tra màu để phân biệt Player 1 và Player 2
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


        private void PlaceStone(int row, int col)
        {
            board[row, col] = isPlayerTurn ? 1 : 2;
            moveCount++;

            DrawStone(row, col, isPlayerTurn);

            if (CheckWin(row, col))
            {
                string winner = isPlayerTurn ? player1Name : player2Name;
                GameOver(isPlayerTurn, $"{winner} thắng!");
                return;
            }

            if (moveCount >= boardSize * boardSize)
            {
                GameOver(null, "Hòa! Bàn cờ đã đầy!");
                return;
            }

            SwitchTurn();
        }

        private void DrawStone(int row, int col, bool isBlack)
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
        }

        private void SwitchTurn()
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
        }

        private void UpdateGameStatus()
        {
            string currentPlayer = isPlayerTurn ? player1Name : player2Name;
            GameStatusText.Text = $"Lượt của {currentPlayer}";
        }

        private bool CheckWin(int row, int col)
        {
            int player = board[row, col];

            // Kiểm tra 4 hướng: ngang, dọc, chéo chính, chéo phụ
            return CheckDirection(row, col, 0, 1, player) ||  // Ngang
                   CheckDirection(row, col, 1, 0, player) ||  // Dọc
                   CheckDirection(row, col, 1, 1, player) ||  // Chéo chính
                   CheckDirection(row, col, 1, -1, player);   // Chéo phụ
        }

        private bool CheckDirection(int row, int col, int dRow, int dCol, int player)
        {
            int count = 1; 

            // Đếm về phía trước
            count += CountStones(row, col, dRow, dCol, player);

            // Đếm về phía sau
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

            MessageBoxResult result = MessageBox.Show(
                message + "\n\nThoát về màn hình chính?",
                "Kết thúc trận đấu",
                MessageBoxButton.YesNo,
                MessageBoxImage.Information
            );

            if (result == MessageBoxResult.Yes)
            {
                this.Close();
            }
            
        }

        public void ReceiveMove(int row, int col)
        {
            Dispatcher.Invoke(() =>
            {
                if (!isGameOver && board[row, col] == 0)
                {
                    PlaceStone(row, col);
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
    }
}