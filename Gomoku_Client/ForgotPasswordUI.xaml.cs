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

        private void SendOTP_Click(object sender, RoutedEventArgs e)
        {
            GetEmailGrid.Visibility = Visibility.Collapsed;
            GetOTPGrid.Visibility = Visibility.Visible;
        }

        private void Back_From_OPTGrid_Click(object sender, RoutedEventArgs e)
        {
            GetOTPGrid.Visibility = Visibility.Collapsed;
            GetEmailGrid.Visibility = Visibility.Visible;
        }

        private void NewPasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (NewPasswordBox.Text == "Mật khẩu mới")
            {
                NewPasswordBox.Text = "";
                NewPasswordBox.Foreground = Brushes.Black;
            }
        }

        private void NewPasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(OTPBox.Text))
            {
                NewPasswordBox.Text = "Mật khẩu mới";
                NewPasswordBox.Foreground = Brushes.Gray;
            }
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
    }
}
