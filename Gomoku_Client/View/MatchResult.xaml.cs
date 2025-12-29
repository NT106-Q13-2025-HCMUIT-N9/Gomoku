using Gomoku_Client.Model;
using Gomoku_Client.ViewModel;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace Gomoku_Client.View
{
    public partial class MatchResult : Page
    {
        private MainGameUI _mainWindow;
        private bool _isLocalPlayerWinner;
        private bool _isDraw;
        private string _playerName;
        private string _opponentName;

        public MatchResult(bool isLocalPlayerWinner, string playerName, string opponentName, MainGameUI mainWindow, bool isDraw = false)
        {
            InitializeComponent();

            _isLocalPlayerWinner = isLocalPlayerWinner;
            _isDraw = isDraw;
            _playerName = playerName;
            _opponentName = opponentName;
            _mainWindow = mainWindow;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                tb_PlayerName.Text = _playerName;
                UserStatsModel? playerStats = await FireStoreHelper.GetUserStats(_playerName);
                UserDataModel? playerData = await FireStoreHelper.GetUserInfo(_playerName);
                if (playerStats != null)
                {
                    lb_matches.Text = playerStats.total_match.ToString();
                    tb_WinRate.Text = playerStats.total_match > 0
                        ? $"{(playerStats.Wins / (double)playerStats.total_match * 100):F1}%"
                        : "0%";
                }
                if (playerData != null)
                {
                    img_PlayerAvatar.Source = BitmapFrame.Create(new Uri(playerData.ImagePath));
                }

                tb_OpponentName.Text = _opponentName;
                UserStatsModel? opponentStats = await FireStoreHelper.GetUserStats(_opponentName);
                UserDataModel? opponentData = await FireStoreHelper.GetUserInfo(_opponentName);
                if (opponentStats != null)
                {
                    lb_OpponentMatches.Text = opponentStats.total_match.ToString();
                    tb_OpponentWinRate.Text = opponentStats.total_match > 0
                        ? $"{(opponentStats.Wins / (double)opponentStats.total_match * 100):F1}%"
                        : "0%";
                }
                if (opponentData != null)
                {
                    img_OpponentAvatar.Source = BitmapFrame.Create(new Uri(opponentData.ImagePath));
                }

                AnimateResult();
            }
            catch (Exception ex)
            {
                NotificationManager.Instance.ShowNotification(
                    "Lỗi",
                    "Không hiển thị được kết quả trận đấu",
                    Notification.NotificationType.Info,
                    3000
                );
                ReturnToLobby();
            }
        }

        private void AnimateResult()
        {
            var fadeInText = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(1.0),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            //draw
            if (_isDraw)
            {
                tb_ResultText.Text = "HÒA!";
                tb_ResultText.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                tb_ResultText.BeginAnimation(OpacityProperty, fadeInText);
                return;
            }

            // w/l
            if (_isLocalPlayerWinner)
            {
                tb_ResultText.Text = "CHIẾN THẮNG!";
                tb_ResultText.Foreground = new SolidColorBrush(Color.FromRgb(255, 215, 0));
            }
            else
            {
                tb_ResultText.Text = "THẤT BẠI";
                tb_ResultText.Foreground = new SolidColorBrush(Color.FromRgb(255, 70, 85));
            }

            tb_ResultText.BeginAnimation(OpacityProperty, fadeInText);

            var duration = TimeSpan.FromSeconds(1.8);
            var easing = new CubicEase { EasingMode = EasingMode.EaseInOut };

            if (_isLocalPlayerWinner)
            {
                var moveSeparator = new DoubleAnimation
                {
                    From = 0,
                    To = 170,
                    Duration = duration,
                    EasingFunction = easing,
                    BeginTime = TimeSpan.FromSeconds(0.8)
                };
                transform_Separator.BeginAnimation(TranslateTransform.XProperty, moveSeparator);

                var shrinkOpponent = new DoubleAnimation
                {
                    From = 1.0,
                    To = 0.7,
                    Duration = duration,
                    EasingFunction = easing,
                    BeginTime = TimeSpan.FromSeconds(0.8)
                };
                scale_Opponent.BeginAnimation(ScaleTransform.ScaleXProperty, shrinkOpponent);
                scale_Opponent.BeginAnimation(ScaleTransform.ScaleYProperty, shrinkOpponent);

                var moveOpponent = new DoubleAnimation
                {
                    From = 0,
                    To = 115,
                    Duration = duration,
                    EasingFunction = easing,
                    BeginTime = TimeSpan.FromSeconds(0.8)
                };
                translate_Opponent.BeginAnimation(TranslateTransform.XProperty, moveOpponent);

                var colorAnimation = new ColorAnimation
                {
                    To = Color.FromRgb(150, 150, 150),
                    Duration = duration,
                    BeginTime = TimeSpan.FromSeconds(0.8)
                };
                border_Opponent.BorderBrush = new SolidColorBrush(Color.FromRgb(76, 175, 80));
                border_Opponent.BorderBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
            }
            else
            {
                var moveSeparator = new DoubleAnimation
                {
                    From = 0,
                    To = -170,
                    Duration = duration,
                    EasingFunction = easing,
                    BeginTime = TimeSpan.FromSeconds(0.8)
                };
                transform_Separator.BeginAnimation(TranslateTransform.XProperty, moveSeparator);

                var shrinkPlayer = new DoubleAnimation
                {
                    From = 1.0,
                    To = 0.7,
                    Duration = duration,
                    EasingFunction = easing,
                    BeginTime = TimeSpan.FromSeconds(0.8)
                };
                scale_LocalPlayer.BeginAnimation(ScaleTransform.ScaleXProperty, shrinkPlayer);
                scale_LocalPlayer.BeginAnimation(ScaleTransform.ScaleYProperty, shrinkPlayer);

                var movePlayer = new DoubleAnimation
                {
                    From = 0,
                    To = -115,
                    Duration = duration,
                    EasingFunction = easing,
                    BeginTime = TimeSpan.FromSeconds(0.8)
                };
                translate_LocalPlayer.BeginAnimation(TranslateTransform.XProperty, movePlayer);

                var colorAnimation = new ColorAnimation
                {
                    To = Color.FromRgb(150, 150, 150),
                    Duration = duration,
                    BeginTime = TimeSpan.FromSeconds(0.8)
                };
                border_LocalPlayer.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 70, 85));
                border_LocalPlayer.BorderBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
            }
        }

        private void ReturnToLobby()
        {
            try
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
                    _mainWindow?.NavigateToLobby();
                };

                transform.BeginAnimation(TranslateTransform.XProperty, slideOutAnimation);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] ReturnToLobby: {ex.Message}");
                _mainWindow?.NavigateToLobby();
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
        }

        private void BackButton_Checked(object sender, RoutedEventArgs e)
        {
            ReturnToLobby();
        }
    }
}