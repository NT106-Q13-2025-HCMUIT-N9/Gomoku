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
using System.Runtime.CompilerServices;

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

        bool isWrongUsername = false;
        private void UsernameBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(UsernameBox.Text))
            {
                UsernameBox.Text = "Tên người dùng";
                UsernameBox.Foreground = Brushes.Gray;
                UsernameBorder.BorderBrush = new SolidColorBrush(Colors.Gray);
                UsernameMsg.Visibility = Visibility.Collapsed;
                if (isWrongUsername == true)
                {
                    MainBorder.Height -= 15;
                    isWrongUsername = false;
                }
            }

            else if(true)
            {
                UsernameBorder.BorderBrush = new SolidColorBrush(Colors.Red);
                UsernameMsg.Visibility = Visibility.Visible;
                if(!isWrongUsername) MainBorder.Height += 15;
                isWrongUsername = true;
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

        bool isWrongEmail = false;
        private void EmailBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(EmailBox.Text))
            {
                EmailBox.Text = "Email";
                EmailBox.Foreground = Brushes.Gray;
                EmailBorder.BorderBrush = new SolidColorBrush(Colors.Gray);
                EmailMsg.Visibility = Visibility.Collapsed;
                if (isWrongEmail == true)
                {
                    MainBorder.Height -= 15;
                    isWrongEmail = false;
                }
            }

            else if (true)
            {
                EmailMsg.Text = "Email không hợp lệ";
                EmailBorder.BorderBrush = new SolidColorBrush(Colors.Red);
                EmailMsg.Visibility = Visibility.Visible;
                if (!isWrongEmail) MainBorder.Height += 15;
                isWrongEmail = true;
            }

            else if (true)
            {
                EmailMsg.Text = "Email đã liên kết với một tài khoản";
                EmailBorder.BorderBrush = new SolidColorBrush(Colors.Red);
                EmailMsg.Visibility = Visibility.Visible;
                if (!isWrongEmail) MainBorder.Height += 15;
                isWrongEmail = true;
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
                await UserAction.SendVeriAsync(await temp.User.GetIdTokenAsync());

                CollectionReference user_collection = FirebaseInfo.DB.Collection("UserInfo");
                UserDataModel doc = new UserDataModel
                {
                    Username = username,
                    Email = email,
                    Password = HashFunc.HashString(password)
                };
                await user_collection.AddAsync(doc);

                MessageBox.Show("Đăng kí thành công. Vui lòng kiểm tra email về thông tin đăng nhập", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

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
            catch (AuthException ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        bool isWrongPassword = false;
        private void PasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (PasswordBox.Password.Length == 0)
            {
                PasswordBorder.BorderBrush = new SolidColorBrush(Colors.Gray);
                PasswordMsg.Visibility = Visibility.Collapsed;
                if (isWrongPassword == true)
                {
                    MainBorder.Height -= 15;
                    isWrongPassword = false;
                }
            }
            else if (PasswordBox.Password.Length < 6)
            {
                PasswordBorder.BorderBrush = new SolidColorBrush(Colors.Red);
                PasswordMsg.Visibility = Visibility.Visible;
                if (!isWrongPassword) MainBorder.Height += 15;
                isWrongPassword = true;
            }

            else
            {
                PasswordBorder.BorderBrush = new SolidColorBrush(Colors.Gray);
                PasswordMsg.Visibility = Visibility.Collapsed;
                if (isWrongPassword == true)
                {
                    MainBorder.Height -= 15;
                    isWrongPassword = false;
                }
            }
        }

        private void PasswordBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true; 
            }
        }

        private bool isShowingPass = false;
        private void TogglePasswordBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!isShowingPass)
            {
                // Hiện mật khẩu
                PasswordVisible.Text = PasswordBox.Password;
                PasswordVisible.Visibility = Visibility.Visible;
                PasswordBox.Visibility = Visibility.Collapsed;

                TogglePasswordBtn.Content = "🙈";
            }
            else
            {
                // Ẩn mật khẩu
                PasswordBox.Password = PasswordVisible.Text;
                PasswordBox.Visibility = Visibility.Visible;
                PasswordVisible.Visibility = Visibility.Collapsed;

                TogglePasswordBtn.Content = "👁";
            }

            isShowingPass = !isShowingPass;
        }

        private void PasswordVisible_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }

        bool isWrongConfirmPass = false;
        private void PasswordConfirmBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (PasswordConfirmBox.Password.Length == 0)
            {
                PasswordConfirmBorder.BorderBrush = new SolidColorBrush(Colors.Gray);
                PasswordConfirmMsg.Visibility = Visibility.Collapsed;
                if (isWrongConfirmPass == true)
                {
                    MainBorder.Height -= 15;
                    isWrongConfirmPass = false;
                }
            }

            else if(PasswordConfirmBox.Password != PasswordBox.Password)
            {
                PasswordConfirmBorder.BorderBrush = new SolidColorBrush(Colors.Red);
                PasswordConfirmMsg.Visibility = Visibility.Visible;
                if (!isWrongConfirmPass) MainBorder.Height += 15;
                isWrongConfirmPass = true;
            }

            else
            {
                PasswordConfirmBorder.BorderBrush = new SolidColorBrush(Colors.Gray);
                PasswordConfirmMsg.Visibility = Visibility.Collapsed;
                if (isWrongConfirmPass == true)
                {
                    MainBorder.Height -= 15;
                    isWrongConfirmPass = false;
                }
            }
        }

        private void PasswordConfirmBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }

        private void PasswordConfirmVisible_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }

        bool isShowingConfirmPass = false;
        private void TogglePasswordConfirmBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!isShowingConfirmPass)
            {
                // Hiện mật khẩu
                PasswordConfirmVisible.Text = PasswordConfirmBox.Password;
                PasswordConfirmVisible.Visibility = Visibility.Visible;
                PasswordConfirmBox.Visibility = Visibility.Collapsed;

                TogglePasswordConfirmBtn.Content = "🙈";
            }
            else
            {
                // Ẩn mật khẩu
                PasswordConfirmBox.Password = PasswordConfirmVisible.Text;
                PasswordConfirmBox.Visibility = Visibility.Visible;
                PasswordConfirmVisible.Visibility = Visibility.Collapsed;

                TogglePasswordConfirmBtn.Content = "👁";
            }

            isShowingConfirmPass = !isShowingConfirmPass;
        }
    }
}
