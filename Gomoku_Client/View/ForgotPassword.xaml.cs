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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Gomoku_Client
{
    /// <summary>
    /// Interaction logic for ForgotPasswordUI.xaml
    /// </summary>
    public partial class ForgotPasswordUI : Page
    {
        private MainWindow _mainWindow;
        public ForgotPasswordUI(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
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
            if(string.IsNullOrWhiteSpace(EmailBox.Text) && textChanged == true)
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
                    MessageBox.Show($"Critical-Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Information);

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
            _mainWindow.MainFrame.Visibility = Visibility.Collapsed;
            _mainWindow.MainBorder.Visibility = Visibility.Visible;
        }

        private async void SendOTP_Click(object sender, RoutedEventArgs e)
        {
            Email_LostFocus(sender, e);

            // Thêm điều kiển kiểm tra email hợp lệ và tồn tại trong hệ thống ở đây
            if (!Validate.IsValidEmail(EmailBox.Text) || !await Validate.IsEmailExists(EmailBox.Text))
            {
                return;
            }

            else
            {
                try
                {
                    await UserAction.SendResetAsync(EmailBox.Text);
                    MessageBox.Show($"Đã gửi link để cập nhập mật khẩu vào email: {EmailBox.Text}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (AuthException auth_ex)
                {
                    MessageBox.Show($"Lỗi: {auth_ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Set_Password_Click(object sender, RoutedEventArgs e)
        {
            if (_mainWindow == null)
            {
                MessageBox.Show("Không tìm thấy cửa sổ chính.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            _mainWindow.MainFrame.Visibility = Visibility.Collapsed;
            _mainWindow.MainBorder.Visibility = Visibility.Visible;
        }

        private CancellationTokenSource? _emailTypingCts; // CancellationTokenSource để hủy bỏ tác vụ kiểm tra email khi người dùng tiếp tục gõ
        private bool textChanged = false;
        private async void EmailBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
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
                        MessageBox.Show($"Critical-Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Information);

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
    }
}
