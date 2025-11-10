using Firebase.Auth;
using Gomoku_Client.Model;
using Google.Cloud.Firestore;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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
using Gomoku_Client.ViewModel;

namespace Gomoku_Client
{
    /// <summary>
    /// Interaction logic for SignUpUserInterface.xaml
    /// </summary>
    public partial class SignUpUserInterface : Window
    {
        public SignUpUserInterface()
        {
            InitializeComponent();
        }

        private void UsernameBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (UsernameBox.Text == "Tên người dùng")
            {
                UsernameBox.Text = "";
                UsernameBox.Foreground = Brushes.Black;
            }
        }

        private void UsernameBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(UsernameBox.Text))
            {
                UsernameBox.Text = "Tên người dùng";
                UsernameBox.Foreground = Brushes.Gray;
            }
        }

        private void EmailBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (EmailBox.Text == "Email")
            {
                EmailBox.Text = "";
                EmailBox.Foreground = Brushes.Black;
            }
        }

        private void EmailBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(EmailBox.Text))
            {
                EmailBox.Text = "Email";
                EmailBox.Foreground = Brushes.Gray;
            }
        }

        private void GoBack_Click(object sender, RoutedEventArgs e)
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

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (PasswordBox.Password.Length > 0)
                PasswordPlaceholder.Visibility = Visibility.Collapsed;
            else
                PasswordPlaceholder.Visibility = Visibility.Visible;
        }

        private void PasswordConfirmBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (PasswordConfirmBox.Password.Length > 0)
                PasswordConfirmPlaceholder.Visibility = Visibility.Collapsed;
            else
                PasswordConfirmPlaceholder.Visibility = Visibility.Visible;
        }

        private async void DangKi_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string username = UsernameBox.Text;
                string password = PasswordBox.Password;
                string email = EmailBox.Text;
                string re_password = PasswordConfirmBox.Password;

                if(password != re_password)
                {
                    MessageBox.Show($"Lỗi: Nhập lại mật khẩu không chính xác", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (string.IsNullOrEmpty(username))
                {
                    MessageBox.Show($"Lỗi: Vui lòng nhập tên người dùng", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (string.IsNullOrEmpty(email))
                {
                    MessageBox.Show($"Lỗi: Vui lòng nhập một email", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (string.IsNullOrEmpty(password))
                {
                    MessageBox.Show($"Lỗi: Vui lòng nhập mật khẩu", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (string.IsNullOrEmpty(re_password))
                {
                    MessageBox.Show($"Lỗi: Vui lòng nhập lại mật khẩu", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var temp = await FirebaseInfo.AuthClient.CreateUserWithEmailAndPasswordAsync(email, password, username);

                CollectionReference user_collection = FirebaseInfo.DB.Collection("UserInfo");
                UserDataModel doc = new UserDataModel
                {
                    Username = username,
                    Email = email,
                    Password = HashFunc.HashString(password)
                };
                await user_collection.AddAsync(doc);
                MessageBox.Show("Đăng kí thành công", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                MainWindow main = new MainWindow();
                main.Left = this.Left;
                main.Top = this.Top;
                main.Width = this.Width;
                main.Height = this.Height;
                main.WindowState = this.WindowState;
                this.Hide();
                main.Show();
                this.Close();
            }
            catch (FirebaseAuthException ex)
            {
                MessageBox.Show($"Lỗi: {ex.Reason}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
