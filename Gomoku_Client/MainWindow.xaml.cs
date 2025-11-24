using Firebase.Auth;
using Gomoku_Client.Model;
using Gomoku_Client.ViewModel;
using Google.Cloud.Firestore;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Gomoku_Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = e.Uri.AbsoluteUri,
                UseShellExecute = true
            });
            e.Handled = true;
        }

        bool failedLogin = false;
        private void EmailBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (failedLogin)
            {
                EmailBorder.BorderBrush = new SolidColorBrush(Colors.Gray);
                EmailNotFoundText.Visibility = Visibility.Collapsed;
                MainBorder.Height -= 15;
                failedLogin = false;
            }

            if (EmailBox.Text == "Email")
            {
                EmailBox.Text = "";
                EmailBox.Foreground = Brushes.Black;
            }
        }

        bool isWrongEmail = false;
        private async void EmailBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(EmailBox.Text))
            {
                EmailBox.Text = "Email";
                EmailBox.Foreground = Brushes.Gray;
                EmailBorder.BorderBrush = new SolidColorBrush(Colors.Gray);
                EmailNotFoundText.Visibility = Visibility.Collapsed;
                if(isWrongEmail == true)
                {
                    MainBorder.Height -= 15;
                    isWrongEmail = false;
                }
                return;
            }

            if (!Validate.IsValidEmail(EmailBox.Text))
            {
                EmailNotFoundText.Text = "Email không hợp lệ";
                EmailBox.BorderBrush = Brushes.Red;
                EmailNotFoundText.Visibility = Visibility.Visible;
                if(isWrongEmail == false) MainBorder.Height += 15;
                // Email không tồn tại → viền đỏ
                EmailBorder.BorderBrush = new SolidColorBrush(Colors.Red);

                isWrongEmail = true;
            }
            else
            {
                if(!await Validate.IsEmailExists(EmailBox.Text))
                {
                    EmailNotFoundText.Text = "Không tìm thấy tài khoản với email này";
                    EmailBox.BorderBrush = Brushes.Red;
                    EmailNotFoundText.Visibility = Visibility.Visible;
                    if (isWrongEmail == false) MainBorder.Height += 15;
                    // Email không tồn tại → viền đỏ
                    EmailBorder.BorderBrush = new SolidColorBrush(Colors.Red);

                    isWrongEmail = true;
                }
                else
                {
                    EmailBorder.BorderBrush = new SolidColorBrush(Colors.Gray);
                    EmailNotFoundText.Visibility = Visibility.Collapsed;
                    if (isWrongEmail == true)
                    {
                        MainBorder.Height -= 15;
                        isWrongEmail = false;
                    }
                    return;
                }
            }
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (PasswordBox.Password.Length > 0)
                PasswordPlaceholder.Visibility = Visibility.Collapsed;
            else
                PasswordPlaceholder.Visibility = Visibility.Visible;
        }

        private void SignUpButton_Click(object sender, RoutedEventArgs e)
        {
            SignUpUserInterface SignUp = new SignUpUserInterface();
            // Sao chép vị trí và kích thước
            SignUp.Left = this.Left;
            SignUp.Top = this.Top;
            SignUp.Width = this.Width;
            SignUp.Height = this.Height;
            SignUp.WindowState = this.WindowState;

            // 1. Ẩn Window hiện tại ngay lập tức
            this.Hide();

            // 2. Hiển thị Window mới
            SignUp.Show();

            // 3. Đóng Window cũ sau khi Window mới đã được hiển thị
            this.Close();
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            ForgotPasswordUI ForgotUI = new ForgotPasswordUI();
            // Sao chép vị trí và kích thước
            ForgotUI.Left = this.Left;
            ForgotUI.Top = this.Top;
            ForgotUI.Width = this.Width;
            ForgotUI.Height = this.Height;
            ForgotUI.WindowState = this.WindowState;

            // 1. Ẩn Window hiện tại ngay lập tức
            this.Hide();

            // 2. Hiển thị Window mới
            ForgotUI.Show();

            // 3. Đóng Window cũ sau khi Window mới đã được hiển thị
            this.Close();
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string password = PasswordBox.Password;
            string email = EmailBox.Text;

            if (failedLogin)
            {
                EmailBorder.BorderBrush = new SolidColorBrush(Colors.Gray);
                EmailNotFoundText.Visibility = Visibility.Collapsed;
                MainBorder.Height -= 15;
                failedLogin = false;
            }

            try
            {
                EmailBox_LostFocus(sender, e);

                if(isWrongEmail || string.IsNullOrEmpty(password))
                {
                    return;
                }

                var user = await FirebaseInfo.AuthClient.SignInWithEmailAndPasswordAsync(email, password);

                MainGameUI mainGame = new MainGameUI();
                mainGame.Left = this.Left;
                mainGame.Top = this.Top;
                mainGame.Width = this.Width;
                mainGame.Height = this.Height;
                mainGame.WindowState = this.WindowState;
                this.Hide();
                mainGame.Show();
                this.Close();
            }
            catch (FirebaseAuthException)
            {
                try
                {
                    await UserAction.LoginEmailtAsync(email, password);
                }
                catch (AuthException auth_ex)
                {
                    if(auth_ex.Message == "INVALID_LOGIN_CREDENTIALS")
                    {
                        EmailNotFoundText.Text = "Email hoặc mật khẩu không chính xác";
                        EmailBox.BorderBrush = Brushes.Red;
                        EmailNotFoundText.Visibility = Visibility.Visible;
                        if (isWrongEmail == false) MainBorder.Height += 15;
                        // Email không tồn tại → viền đỏ
                        EmailBorder.BorderBrush = new SolidColorBrush(Colors.Red);

                        failedLogin = true;
                    }
                }
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void SignUp_Click(object sender, RoutedEventArgs e)
        {
            SignUpUserInterface SignUp = new SignUpUserInterface();
            SignUp.Left = this.Left;
            SignUp.Top = this.Top;
            SignUp.Width = this.Width;
            SignUp.Height = this.Height;
            SignUp.WindowState = this.WindowState;
            this.Hide();
            SignUp.Show();
            this.Close();
        }

        private bool isShowing = false;
        private void TogglePasswordBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!isShowing)
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

            isShowing = !isShowing;
        }

        private void PasswordBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true; // chặn phím Space
            }
        }

        private void PasswordVisible_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }

        private void PasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            
        }

        private void PasswordVisible_TextChanged(object sender, TextChangedEventArgs e)
        {
            PasswordBox.Password = PasswordVisible.Text;
            if (PasswordBox.Password.Length > 0)
                PasswordPlaceholder.Visibility = Visibility.Collapsed;
            else
                PasswordPlaceholder.Visibility = Visibility.Visible;
        }
    }
}