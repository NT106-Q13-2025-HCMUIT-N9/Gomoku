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
                EmailBox.Foreground = Brushes.Black;
            }
        }

        private async void Email_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(EmailBox.Text))
            {
                EmailBox.Text = "Email khôi phục";
                EmailBox.Foreground = Brushes.Gray;
                EmailBorder.BorderBrush = new SolidColorBrush(Colors.Gray);
                EmailNotFoundText.Visibility = Visibility.Collapsed;
                GetEmailGrid.Height = 205;
            }

            //Kiểm tra định dạng email hợp lệ 
            else if(!Validate.IsValidEmail(EmailBox.Text))
            {
                EmailNotFoundText.Text = "Email không hợp lệ";
                EmailBox.BorderBrush = Brushes.Red;
                EmailNotFoundText.Visibility = Visibility.Visible;
                GetEmailGrid.Height = 220;
                EmailBorder.BorderBrush = new SolidColorBrush(Colors.Red);
            }

            //Kiểm tra email có tồn tại trong hệ thống không 
            else
            {
                try
                {
                    if (!await Validate.IsEmailExists(EmailBox.Text))
                    {
                        EmailNotFoundText.Text = "Không tìm thấy tài khoản với email này";
                        EmailBox.BorderBrush = Brushes.Red;
                        EmailNotFoundText.Visibility = Visibility.Visible;
                        GetEmailGrid.Height = 220;
                        EmailBorder.BorderBrush = new SolidColorBrush(Colors.Red);
                    }
                    else
                    {
                        EmailBox.Foreground = Brushes.Gray;
                        EmailBorder.BorderBrush = new SolidColorBrush(Colors.Gray);
                        EmailNotFoundText.Visibility = Visibility.Collapsed;
                        GetEmailGrid.Height = 205;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Critical-Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Information);

                    EmailNotFoundText.Text = "Xảy ra lỗi không biết rõ";
                    EmailBox.BorderBrush = Brushes.Red;
                    EmailNotFoundText.Visibility = Visibility.Visible;
                    GetEmailGrid.Height = 220;
                    EmailBorder.BorderBrush = new SolidColorBrush(Colors.Red);
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.MainFrame.Visibility = Visibility.Collapsed;
            _mainWindow.MainBorder.Visibility = Visibility.Visible;
        }

        private void OTPBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (OTPBox.Text == "Mã OTP")
            {
                OTPBox.Text = "";
                OTPBox.Foreground = Brushes.Black;
            }
        }

        private void OTPBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(OTPBox.Text))
            {
                OTPBox.Text = "Mã OTP";
                OTPBox.Foreground = Brushes.Gray;
            }
        }

        private void OTPConfirm_Click(object sender, RoutedEventArgs e)
        {
            GetOTPGrid.Visibility = Visibility.Collapsed;
            SetPasswordGrid.Visibility = Visibility.Visible;
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

        private void Back_From_OPTGrid_Click(object sender, RoutedEventArgs e)
        {
            GetOTPGrid.Visibility = Visibility.Collapsed;
            GetEmailGrid.Visibility = Visibility.Visible;
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

        private void Back_From_SetPasswordGrid_Click(object sender, RoutedEventArgs e)
        {
            SetPasswordGrid.Visibility = Visibility.Collapsed;
            GetOTPGrid.Visibility = Visibility.Visible;
        }

        private void NewPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (NewPasswordBox.Password.Length > 0)
                NewPasswordPlaceholder.Visibility = Visibility.Collapsed;
            else
                NewPasswordPlaceholder.Visibility = Visibility.Visible;
        }

        private void NewPasswordConfirmBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (NewPasswordConfirmBox.Password.Length > 0)
                NewPasswordConfirmPlaceholder.Visibility = Visibility.Collapsed;
            else
                NewPasswordConfirmPlaceholder.Visibility = Visibility.Visible;
        }
    }
}
