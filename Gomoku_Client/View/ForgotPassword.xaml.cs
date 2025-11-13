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
    public partial class ForgotPasswordUI : Window
    {
        public ForgotPasswordUI()
        {
            InitializeComponent();
        }

        private void Email_GotFocus(object sender, RoutedEventArgs e)
        {
            if (EmailBox.Text == "Email khôi phục")
            {
                EmailBox.Text = "";
                EmailBox.Foreground = Brushes.Black;
            }
        }

        private void Email_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(EmailBox.Text))
            {
                EmailBox.Text = "Email khôi phục";
                EmailBox.Foreground = Brushes.Gray;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MainWindow main = new MainWindow();
            // Sao chép vị trí và kích thước
            main.Left = this.Left;
            main.Top = this.Top;
            main.Width = this.Width;
            main.Height = this.Height;
            main.WindowState = this.WindowState;

            // 1. Ẩn Window hiện tại ngay lập tức
            this.Hide();

            // 2. Hiển thị Window mới
            main.Show();

            // 3. Đóng Window cũ sau khi Window mới đã được hiển thị
            this.Close();
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

        private void Back_From_OPTGrid_Click(object sender, RoutedEventArgs e)
        {
            GetOTPGrid.Visibility = Visibility.Collapsed;
            GetEmailGrid.Visibility = Visibility.Visible;
        }

        private void Set_Password_Click(object sender, RoutedEventArgs e)
        {
            MainWindow main = new MainWindow();
            // Sao chép vị trí và kích thước
            main.Left = this.Left;
            main.Top = this.Top;
            main.Width = this.Width;
            main.Height = this.Height;
            main.WindowState = this.WindowState;

            // 1. Ẩn Window hiện tại ngay lập tức
            this.Hide();

            // 2. Hiển thị Window mới
            main.Show();

            // 3. Đóng Window cũ sau khi Window mới đã được hiển thị
            this.Close();
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
