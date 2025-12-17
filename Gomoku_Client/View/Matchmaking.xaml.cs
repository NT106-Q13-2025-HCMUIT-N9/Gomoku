using Gomoku_Client.Model;
using Gomoku_Client.ViewModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Gomoku_Client.View
{
    /// <summary>
    /// Interaction logic for Matchmaking.xaml
    /// </summary>
    public partial class Matchmaking : Page
    {
        private MainGameUI _mainWindow;
        private DispatcherTimer _queueTimer;
        private Stopwatch _stopwatch;
        private Storyboard _movingDotStoryboard;

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

                tb_PlayerName.Text = user.Info.DisplayName;

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

        private async void btn_Simulate_Click(object sender, RoutedEventArgs e)
        {
            // simulate opponent found (REMOVE LATER)
            string opponent_name = "CharlieKirkPro";
            await ShowOpponentFound(opponent_name);
        }
        public async Task ShowOpponentFound(string opponent_name)
        {
            try
            {
                _queueTimer.Stop();
                _stopwatch.Reset();
                _movingDotStoryboard?.Stop();

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
                    Duration = TimeSpan.FromSeconds(0.4), // Giảm thời gian
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

                await Task.Delay(5000);

                var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.3));
                fadeOut.Completed += (s, e) =>
                {
                    var gamePlayWindow = new GamePlay();
                    gamePlayWindow.Show();
                    _mainWindow.Close();
                };
                _mainWindow.BeginAnimation(Window.OpacityProperty, fadeOut);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load opponent data: {ex.Message}");
                NavigationService?.GoBack();
            }
        }
    }
}