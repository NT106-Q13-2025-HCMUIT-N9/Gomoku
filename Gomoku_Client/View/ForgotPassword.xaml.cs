using Gomoku_Client.Helpers;
using Gomoku_Client.Model;
using Gomoku_Client.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Media;
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

namespace Gomoku_Client
{
    /// <summary>
    /// Interaction logic for ForgotPasswordUI.xaml
    /// </summary>
    public partial class ForgotPasswordUI : Page
    {
        private MediaPlayer ButtonClick = new MediaPlayer();
        private MainWindow _mainWindow;
        public ForgotPasswordUI(MainWindow mainWindow)
        {
            InitializeComponent();
            StartSound();
            _mainWindow = mainWindow;
        }

        void StartSound()
        {
            string buttonPath = AudioHelper.ExtractResourceToTemp("Assets/Sounds/ButtonHover.wav");

            if (buttonPath != null)
                ButtonClick.Open(new Uri(buttonPath));
        }

        private void Email_GotFocus(object sender, RoutedEventArgs e)
        {
            if (EmailBox.Text == "Email khôi phục")
            {
                EmailBox.Text = "";
                EmailBox.Foreground = Brushes.White;
            }
        }

        private async void Email_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(EmailBox.Text) && textChanged == true)
            {
                EmailNotFoundText.Text = "Email không hợp lệ";
                EmailBox.Text = "Email khôi phục";
                EmailBox.Foreground = Brushes.Gray;
                EmailNotFoundText.Visibility = Visibility.Visible;
                GetEmailGrid.Height = 220;
                EmailBorder.BorderBrush = new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString("#FF4655")
                );
                return;
            }

            //Kiểm tra định dạng email hợp lệ 
            else if (!Validate.IsValidEmail(EmailBox.Text))
            {
                EmailNotFoundText.Text = "Email không hợp lệ";
                EmailNotFoundText.Visibility = Visibility.Visible;
                GetEmailGrid.Height = 220;
                EmailBorder.BorderBrush = new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString("#FF4655")
                );
                return;
            }

            //Kiểm tra email có tồn tại trong hệ thống không 
            else
            {
                try
                {
                    if (!await Validate.IsEmailExists(EmailBox.Text))
                    {
                        EmailNotFoundText.Text = "Không tìm thấy tài khoản với email này";
                        EmailNotFoundText.Visibility = Visibility.Visible;
                        GetEmailGrid.Height = 220;
                        EmailBorder.BorderBrush = new SolidColorBrush(
                            (Color)ColorConverter.ConvertFromString("#FF4655")
                        );
                    }
                    else
                    {
                        EmailBorder.BorderBrush = new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString("#2A2A2A")
                );
                        EmailNotFoundText.Visibility = Visibility.Collapsed;
                        GetEmailGrid.Height = 205;
                    }
                }
                catch (Exception ex)
                {
                    EmailNotFoundText.Text = "Xảy ra lỗi không biết rõ";
                    EmailNotFoundText.Visibility = Visibility.Visible;
                    GetEmailGrid.Height = 220;
                    EmailBorder.BorderBrush = new SolidColorBrush(
                        (Color)ColorConverter.ConvertFromString("#FF4655")
                    );
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ButtonClick.Stop();
            ButtonClick?.Play();
            _mainWindow.MainFrame.Visibility = Visibility.Collapsed;
            _mainWindow.MainBorder.Visibility = Visibility.Visible;
        }

        private void buttonDisable()
        {
            BackButton.IsHitTestVisible = false;
            SendOTPButton.IsHitTestVisible = false;
            EmailBox.IsHitTestVisible = false;
        }

        private void buttonEnable()
        {
            BackButton.IsHitTestVisible = true;
            SendOTPButton.IsHitTestVisible = true;
            EmailBox.IsHitTestVisible = true;
        }

        private async void SendOTP_Click(object sender, RoutedEventArgs e)
        {
            ButtonClick.Stop();
            ButtonClick?.Play();
            LoadingCircle.Visibility = Visibility.Visible;
            buttonDisable();
            SendOTPButton.Content = "";

            Email_LostFocus(sender, e);

            // Thêm điều kiển kiểm tra email hợp lệ và tồn tại trong hệ thống ở đây
            if (!Validate.IsValidEmail(EmailBox.Text) || !await Validate.IsEmailExists(EmailBox.Text))
            {
                //Loaded
                LoadingCircle.Visibility = Visibility.Collapsed;
                buttonEnable();
                SendOTPButton.Content = "Gửi mã xác thực";
                return;
            }

            else
            {
                //Loading

                try
                {
                    await UserAction.SendResetAsync(EmailBox.Text);
                    //MessageBox.Show($"Đã gửi link để cập nhập mật khẩu vào email: {EmailBox.Text}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                    //Hiển thị overlay xác nhận
                    ConfirmationOverlay.Visibility = Visibility.Visible;

                    var storyboard = (Storyboard)this.Resources["PopupFadeIn"];
                    var border = (Border)((Grid)ConfirmationOverlay).Children[0];
                    storyboard.Begin(border);



                    //Loaded
                    LoadingCircle.Visibility = Visibility.Collapsed;
                    buttonEnable();
                    SendOTPButton.Content = "Gửi mã xác thực";



                    storyboard.Begin(border);
                }
                catch (AuthException auth_ex)
                {
                    AuthException a = auth_ex;

                    LoadingCircle.Visibility = Visibility.Collapsed;
                    buttonEnable();
                    SendOTPButton.Content = "Gửi mã xác thực";

                    //Hiển thị overlay xác nhận thất bại
                    ConfirmFailedOverlay.Visibility = Visibility.Visible;

                    var storyboard = (Storyboard)this.Resources["PopupFadeIn"];
                    var border = (Border)((Grid)ConfirmFailedOverlay).Children[0];
                    storyboard.Begin(border);
                }
            }
        }

        private void Set_Password_Click(object sender, RoutedEventArgs e)
        {
            ButtonClick.Stop();
            ButtonClick?.Play();
            if (_mainWindow == null)
            {
                return;
            }
            _mainWindow.MainFrame.Visibility = Visibility.Collapsed;
            _mainWindow.MainBorder.Visibility = Visibility.Visible;
        }

        private CancellationTokenSource? _emailTypingCts; // CancellationTokenSource để hủy bỏ tác vụ kiểm tra email khi người dùng tiếp tục gõ
        private bool textChanged = false;
        private async void EmailBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {

            _mainWindow.Keyboard.Stop();
            _mainWindow.Keyboard?.Play();
            textChanged = true;
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }

            EmailBox.Foreground = Brushes.White;
            EmailBorder.BorderBrush = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString("#2A2A2A")
            );
            EmailNotFoundText.Visibility = Visibility.Collapsed;
            GetEmailGrid.Height = 205;

            // Hủy delay cũ nếu user vẫn đang gõ
            _emailTypingCts?.Cancel();
            _emailTypingCts = new CancellationTokenSource();

            try
            {
                await Task.Delay(1500, _emailTypingCts.Token);
                if (string.IsNullOrWhiteSpace(EmailBox.Text))
                {
                    EmailNotFoundText.Text = "Email không hợp lệ";
                    EmailBox.Foreground = Brushes.Gray;
                    EmailNotFoundText.Visibility = Visibility.Visible;
                    GetEmailGrid.Height = 220;
                    EmailBorder.BorderBrush = new SolidColorBrush(
                        (Color)ColorConverter.ConvertFromString("#FF4655")
                    );
                    return;
                }

                else if (!Validate.IsValidEmail(EmailBox.Text))
                {
                    EmailNotFoundText.Text = "Email không hợp lệ";
                    EmailNotFoundText.Visibility = Visibility.Visible;
                    GetEmailGrid.Height = 220;
                    EmailBorder.BorderBrush = new SolidColorBrush(
                        (Color)ColorConverter.ConvertFromString("#FF4655")
                    );
                    return;
                }

                else
                {
                    try
                    {
                        if (!await Validate.IsEmailExists(EmailBox.Text))
                        {
                            EmailNotFoundText.Text = "Không tìm thấy tài khoản với email này";
                            EmailNotFoundText.Visibility = Visibility.Visible;
                            GetEmailGrid.Height = 220;
                            EmailBorder.BorderBrush = new SolidColorBrush(
                                (Color)ColorConverter.ConvertFromString("#FF4655")
                            );
                        }
                        else
                        {
                            EmailBox.Foreground = Brushes.Gray;
                            EmailBorder.BorderBrush = new SolidColorBrush(
                        (Color)ColorConverter.ConvertFromString("#2A2A2A")
                    );
                            EmailNotFoundText.Visibility = Visibility.Collapsed;
                            GetEmailGrid.Height = 205;
                        }
                    }
                    catch (Exception ex)
                    {
                        EmailNotFoundText.Text = "Xảy ra lỗi không biết rõ";
                        EmailNotFoundText.Visibility = Visibility.Visible;
                        GetEmailGrid.Height = 220;
                        EmailBorder.BorderBrush = new SolidColorBrush(
                            (Color)ColorConverter.ConvertFromString("#FF4655")
                        );
                    }
                }
            }
            catch (TaskCanceledException)
            {

            }
        }

        private void ConfirmationButton_Click(object sender, RoutedEventArgs e)
        {
            ButtonClick.Stop();
            ButtonClick?.Play();
            _mainWindow.MainFrame.Visibility = Visibility.Collapsed;
            _mainWindow.MainBorder.Visibility = Visibility.Visible;
        }

        private void ConfirmationFailedButton_Click(object sender, RoutedEventArgs e)
        {
            ButtonClick.Stop();
            ButtonClick?.Play();
            var storyboard = (Storyboard)this.Resources["PopupFadeOut"];
            var border = (Border)((Grid)ConfirmFailedOverlay).Children[0];
            storyboard.Completed += (s, args) =>
            {
                ConfirmFailedOverlay.Visibility = Visibility.Collapsed;
            };

            storyboard.Begin(border);
        }
    }
}