using System;
using System.Collections.Generic;
using System.IO;
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

namespace Gomoku_Client.View
{
    /// <summary>
    /// Interaction logic for Lobby.xaml
    /// </summary>
    public partial class Lobby : Page
    {
        // Truyền tham số MainGameUI để có thể quay lại bằng BackButton
        private MainGameUI _mainWindow;
        private bool _isNavigating = false;
        public Lobby(MainGameUI mainGameUI)
        {
            InitializeComponent();
            _mainWindow = mainGameUI;
            this.Loaded += Page_Loaded;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _isNavigating = false;
            if (MatchMakingButton != null)
            {
                MatchMakingButton.IsEnabled = true;
            }
        }

        private void BackButton_Checked(object sender, RoutedEventArgs e)
        {
            _mainWindow.ButtonClick.Stop();
            _mainWindow.ButtonClick.Play();
            if (_mainWindow == null)
            {
                MessageBox.Show("Không tìm thấy cửa sổ chính.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            _mainWindow.ShowMenuWithAnimation();
        }

        private void MatchMakingButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isNavigating) return;

            _isNavigating = true;
            MatchMakingButton.IsEnabled = false;
            _mainWindow.ButtonClick.Stop();
            _mainWindow.ButtonClick.Play();
            if (NavigationService != null)
            {
                var matchmakingPage = new Matchmaking(_mainWindow);

                // Create slide out animation for current page
                var slideOutAnimation = new DoubleAnimation
                {
                    From = 0,
                    To = -_mainWindow.ActualWidth,
                    Duration = TimeSpan.FromSeconds(0.5),
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
                };

                // Setup RenderTransform if needed
                if (this.RenderTransform == null || !(this.RenderTransform is TranslateTransform))
                {
                    this.RenderTransform = new TranslateTransform();
                }

                var transform = (TranslateTransform)this.RenderTransform;

                slideOutAnimation.Completed += (s, args) =>
                {
                    if (NavigationService != null)
                    {
                        NavigationService.Navigate(matchmakingPage);

                        if (matchmakingPage.RenderTransform == null || !(matchmakingPage.RenderTransform is TranslateTransform))
                        {
                            matchmakingPage.RenderTransform = new TranslateTransform();
                        }

                        var matchmakingTransform = (TranslateTransform)matchmakingPage.RenderTransform;

                        var slideInAnimation = new DoubleAnimation
                        {
                            From = _mainWindow.ActualWidth,
                            To = 0,
                            Duration = TimeSpan.FromSeconds(0.5),
                            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                        };

                        matchmakingTransform.BeginAnimation(TranslateTransform.XProperty, slideInAnimation);
                    }
                };
                transform.BeginAnimation(TranslateTransform.XProperty, slideOutAnimation);
            }
            else
            {
                _isNavigating = false;
                MatchMakingButton.IsEnabled = true;
            }
        }

        private void AIButton_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.ButtonClick.Stop();
            _mainWindow.ButtonClick.Play();
        }
        private void ServerBrowserButton_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.ButtonClick.Stop();
            _mainWindow.ButtonClick.Play();

            NotificationManager.Instance.ShowNotification("KHÔNG KHẢ DỤNG", "Chức năng này đang được phát triển, quay lại sau nhé.", Notification.NotificationType.Info);
        }
        private void RankedButton_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.ButtonClick.Stop();
            _mainWindow.ButtonClick.Play();
        }
    }
}