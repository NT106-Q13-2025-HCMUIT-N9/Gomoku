using Firebase.Auth;
using Gomoku_Client.Model;
using Gomoku_Client.ViewModel;
using Google.Cloud.Firestore;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Gomoku_Client
{
    /// <summary>
    /// Interaction logic for SignUpUserInterface.xaml
    /// </summary>
    public partial class SignUpUserInterface : Page
    {
        private MainWindow _mainWindow;
        public SignUpUserInterface(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
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
        private async void UsernameBox_LostFocus(object sender, RoutedEventArgs e)
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
            if (UsernameBox.Text == "Tên người dùng")
            {
                UsernameMsg.Text = "Vui lòng nhập vào một username";
                UsernameBorder.BorderBrush = new SolidColorBrush(Colors.Red);
                UsernameMsg.Visibility = Visibility.Visible;
                if (!isWrongUsername) MainBorder.Height += 15;
                isWrongUsername = true;
            }
            else
            {
                try
                {
                    if (await Validate.IsUsernamExists(UsernameBox.Text))
                    {
                        UsernameMsg.Text = "Username đã tồn tại. Vui lòng chọn một username khác";
                        UsernameBorder.BorderBrush = new SolidColorBrush(Colors.Red);
                        UsernameMsg.Visibility = Visibility.Visible;
                        if (!isWrongUsername) MainBorder.Height += 15;
                        isWrongUsername = true;
                    }
                    else
                    {
                        UsernameBorder.BorderBrush = new SolidColorBrush(Colors.Gray);
                        UsernameMsg.Visibility = Visibility.Collapsed;
                        if (isWrongUsername == true)
                        {
                            MainBorder.Height -= 15;
                            isWrongUsername = false;
                        }
                    }
                }catch (Exception ex)
                {
                    MessageBox.Show($"Critical-Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Information);

                    UsernameMsg.Text = "Xảy ra lỗi không biết rõ";
                    UsernameBorder.BorderBrush = new SolidColorBrush(Colors.Red);
                    UsernameMsg.Visibility = Visibility.Visible;
                    if (!isWrongUsername) MainBorder.Height += 15;
                    
                    isWrongUsername = true;
                }
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
        private async void EmailBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(EmailBox.Text) || EmailBox.Text == "Email")
            {
                EmailBox.Text = "Email";
                EmailBox.Foreground = Brushes.Gray;
                EmailBorder.BorderBrush = new SolidColorBrush(Colors.Red);
                EmailMsg.Text = "Email không được để trống";
                EmailMsg.Visibility = Visibility.Visible;
                if (!isWrongEmail) MainBorder.Height += 15;
                isWrongEmail = true;
            }
            else if (!Validate.IsValidEmail(EmailBox.Text))
            {
                EmailMsg.Text = "Vui lòng nhập đúng định dạng email";
                EmailBorder.BorderBrush = new SolidColorBrush(Colors.Red);
                EmailMsg.Visibility = Visibility.Visible;
                if (!isWrongEmail) MainBorder.Height += 15;
                isWrongEmail = true;
            }
            else
            {
                try
                {
                    CollectionReference user_collection = FirebaseInfo.DB.Collection("UserInfo");
                    QuerySnapshot query_result = await user_collection.WhereEqualTo("Email", EmailBox.Text).GetSnapshotAsync();
                    if (query_result.Count != 0)
                    {
                        EmailMsg.Text = "Email đã liên kết với một tài khoản khác. Vui lòng nhập email khác";
                        EmailBorder.BorderBrush = new SolidColorBrush(Colors.Red);
                        EmailMsg.Visibility = Visibility.Visible;
                        if (!isWrongEmail) MainBorder.Height += 15;
                        isWrongEmail = true;
                    }
                    else
                    {
                        EmailBorder.BorderBrush = new SolidColorBrush(Colors.Gray);
                        EmailMsg.Visibility = Visibility.Collapsed;
                        if (isWrongEmail == true)
                        {
                            MainBorder.Height -= 15;
                            isWrongEmail = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Critical-Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Information);

                    EmailMsg.Text = "Xảy ra lỗi không biết rõ";
                    EmailBorder.BorderBrush = new SolidColorBrush(Colors.Red);
                    EmailMsg.Visibility = Visibility.Visible;
                    if (!isWrongEmail) MainBorder.Height += 15;
                    isWrongEmail = true;
                }
            }
        }

        private void GoBack_Click(object sender, RoutedEventArgs e)
        {
            if (_mainWindow == null)
            {
                MessageBox.Show("Không tìm thấy cửa sổ chính.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            _mainWindow.MainFrame.Visibility = Visibility.Collapsed;
            _mainWindow.MainBorder.Visibility = Visibility.Visible;
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

                UsernameBox_LostFocus(sender, e);
                EmailBox_LostFocus(sender, e);
                PasswordBox_LostFocus(sender, e);
                PasswordConfirmBox_LostFocus(sender, e);

                if(string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(re_password))
                {
                    Debug.WriteLine("Empty");
                    return;
                }

                if(isWrongEmail || isWrongPassword || isWrongUsername || isWrongConfirmPass)
                {
                    Debug.WriteLine("Invail sign up");
                    return;
                }

                var temp = await FirebaseInfo.AuthClient.CreateUserWithEmailAndPasswordAsync(email, password, username);
                await UserAction.SendVeriAsync(await temp.User.GetIdTokenAsync());

                CollectionReference user_collection = FirebaseInfo.DB.Collection("UserInfo");
                CollectionReference user_stat_collection = FirebaseInfo.DB.Collection("UserStats");
                UserDataModel doc = new UserDataModel
                {
                    Username = username,
                    Email = email
                };
                await user_collection.AddAsync(doc);

                MessageBox.Show("Đăng kí thành công. Vui lòng kiểm tra email về thông tin đăng nhập", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                _mainWindow.MainFrame.Visibility = Visibility.Collapsed;
                _mainWindow.MainBorder.Visibility = Visibility.Visible;
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

        private void PasswordVisible_LostFocus(object sender, RoutedEventArgs e)
        {
            if (PasswordBox.Password.Length < 6)
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

            if (PasswordBox.Password != PasswordConfirmBox.Password && PasswordConfirmBox.Password.Length != 0)
            {
                PasswordConfirmBorder.BorderBrush = new SolidColorBrush(Colors.Red);
                if (!isWrongConfirmPass)
                {
                    MainBorder.Height += 15;
                    isWrongConfirmPass = true;
                }
                PasswordConfirmMsg.Text = "Mật khẩu không khớp";
                PasswordConfirmMsg.Visibility = Visibility.Visible;
            }
            else
            {
                PasswordConfirmBorder.BorderBrush = new SolidColorBrush(Colors.Gray);
                PasswordConfirmMsg.Visibility = Visibility.Collapsed;
                if (isWrongConfirmPass == true)
                {
                    MainBorder.Height -= 15;
                }
                isWrongConfirmPass = false;
            }
        }

        private void PasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (PasswordBox.Password.Length < 6)
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

            if (PasswordBox.Password != PasswordConfirmBox.Password && PasswordConfirmBox.Password.Length != 0)
            {
                PasswordConfirmBorder.BorderBrush = new SolidColorBrush(Colors.Red);
                if (!isWrongConfirmPass)
                {
                    MainBorder.Height += 15;
                    isWrongConfirmPass = true;
                }
                PasswordConfirmMsg.Text = "Mật khẩu không khớp";
                PasswordConfirmMsg.Visibility = Visibility.Visible;
            }
            else
            {
                PasswordConfirmBorder.BorderBrush = new SolidColorBrush(Colors.Gray);
                PasswordConfirmMsg.Visibility = Visibility.Collapsed;
                if (isWrongConfirmPass == true)
                {
                    MainBorder.Height -= 15;
                }
                isWrongConfirmPass = false;
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

        private void PasswordConfirmVisible_LostFocus(object sender, RoutedEventArgs e)
        {
            if (PasswordConfirmVisible.Text.Length == 0)
            {
                PasswordConfirmBorder.BorderBrush = new SolidColorBrush(Colors.Red);
                PasswordConfirmMsg.Text = "Xác nhận mật khẩu không được để trống";
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
                }
                isWrongConfirmPass = false;
            }

            if (PasswordConfirmBox.Password != PasswordBox.Password)
            {
                PasswordConfirmBorder.BorderBrush = new SolidColorBrush(Colors.Red);
                PasswordConfirmMsg.Text = "Mâtk khẩu không khớp";
                PasswordConfirmMsg.Visibility = Visibility.Visible;
                if (!isWrongConfirmPass) MainBorder.Height += 15;
                isWrongConfirmPass = true;
            }
        }

        private void PasswordConfirmBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (PasswordConfirmBox.Password.Length == 0 && PasswordConfirmVisible.Text.Length == 0)
            {
                PasswordConfirmBorder.BorderBrush = new SolidColorBrush(Colors.Red);
                PasswordConfirmMsg.Text = "Xác nhận mật khẩu không được để trống";
                PasswordConfirmMsg.Visibility = Visibility.Visible;
                if (!isWrongConfirmPass) MainBorder.Height += 15;
                isWrongConfirmPass = true;
            }
            else if(PasswordConfirmBox.Password != PasswordBox.Password)
            {
                PasswordConfirmBorder.BorderBrush = new SolidColorBrush(Colors.Red);
                PasswordConfirmMsg.Text = "Mật khẩu không khớp";
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
                }
                isWrongConfirmPass = false;
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

        private void PasswordVisible_TextChanged(object sender, TextChangedEventArgs e)
        {
            PasswordBox.Password = PasswordVisible.Text;
            if (PasswordBox.Password.Length > 0)
                PasswordPlaceholder.Visibility = Visibility.Collapsed;
            else
                PasswordPlaceholder.Visibility = Visibility.Visible;
        }

        private void PasswordConfirmVisible_TextChanged(object sender, TextChangedEventArgs e)
        {
            PasswordConfirmBox.Password = PasswordConfirmVisible.Text;
            if (PasswordConfirmBox.Password.Length > 0)
                PasswordConfirmPlaceholder.Visibility = Visibility.Collapsed;
            else
                PasswordConfirmPlaceholder.Visibility = Visibility.Visible;
        }
    }
}
