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
                EmailBox.BorderBrush = new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString("#2A2A2A")
                );
                EmailNotFoundText.Visibility = Visibility.Collapsed;
                MainBorder.Height -= 15;
                failedLogin = false;
            }

            if (EmailBox.Text == "Email")
            {
                EmailBox.Text = "";
                EmailBox.Foreground = Brushes.White;
            }
        }

        bool isWrongEmail = false;
        private async void EmailBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(EmailBox.Text))
            {
                EmailBox.Text = "Email";
                EmailBox.Foreground = Brushes.Gray;
                EmailBorder.BorderBrush = new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString("#2A2A2A")
                );
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
                EmailBorder.BorderBrush = new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString("#FF4655")
                );
                EmailNotFoundText.Visibility = Visibility.Visible;
                if(isWrongEmail == false) MainBorder.Height += 15;
                // Email không tồn tại → viền đỏ
                EmailBorder.BorderBrush = new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString("#FF4655")
                );

                isWrongEmail = true;
            }
            else
            {
                try
                {
                    if (!await Validate.IsEmailExists(EmailBox.Text))
                    {
                        EmailNotFoundText.Text = "Không tìm thấy tài khoản với email này";
                        EmailBorder.BorderBrush = Brushes.Red;
                        EmailNotFoundText.Visibility = Visibility.Visible;
                        if (isWrongEmail == false) MainBorder.Height += 15;
                        // Email không tồn tại → viền đỏ
                        EmailBorder.BorderBrush = new SolidColorBrush(
                            (Color)ColorConverter.ConvertFromString("#FF4655")
                        );

                        isWrongEmail = true;
                    }
                    else
                    {
                        EmailBorder.BorderBrush = new SolidColorBrush(
                            (Color)ColorConverter.ConvertFromString("#2A2A2A")
                        );
                        EmailNotFoundText.Visibility = Visibility.Collapsed;
                        if (isWrongEmail == true)
                        {
                            MainBorder.Height -= 15;
                            isWrongEmail = false;
                        }
                        return;
                    }
                }catch (Exception ex)
                {
                    MessageBox.Show($"Critical-Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Information);

                    EmailNotFoundText.Text = "Xảy ra lỗi không biết rõ";
                    EmailBox.BorderBrush = Brushes.Red;
                    EmailNotFoundText.Visibility = Visibility.Visible;
                    if (isWrongEmail == false) MainBorder.Height += 15;
                    // Email không tồn tại → viền đỏ
                    EmailBorder.BorderBrush = new SolidColorBrush(
                        (Color)ColorConverter.ConvertFromString("#FF4655")
                    );

                    isWrongEmail = true;
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

        

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ForgotPasswordUI(this));

            MainBorder.Visibility = Visibility.Collapsed;

            MainFrame.Visibility = Visibility.Visible;



            //ForgotPasswordUI ForgotUI = new ForgotPasswordUI();
            //// Sao chép vị trí và kích thước
            //ForgotUI.Left = this.Left;
            //ForgotUI.Top = this.Top;
            //ForgotUI.Width = this.Width;
            //ForgotUI.Height = this.Height;
            //ForgotUI.WindowState = this.WindowState;

            //// 1. Ẩn Window hiện tại ngay lập tức
            //this.Hide();

            //// 2. Hiển thị Window mới
            //ForgotUI.Show();

            //// 3. Đóng Window cũ sau khi Window mới đã được hiển thị
            //this.Close();
        }

        private void buttonDisable()
        {
            LoginButton.IsHitTestVisible = false;
            ExitButton.IsHitTestVisible = false;
            TogglePasswordBtn.IsHitTestVisible = false;
            ForgotPasswordText.IsHitTestVisible = false;
            SignUpText.IsHitTestVisible = false;
        }

        private void buttonEnable()
        {
            LoginButton.IsHitTestVisible = true;
            ExitButton.IsHitTestVisible = true;
            TogglePasswordBtn.IsHitTestVisible = true;
            ForgotPasswordText.IsHitTestVisible = true;
            SignUpText.IsHitTestVisible = true;
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string password = PasswordBox.Password;
            string email = EmailBox.Text;
            buttonDisable();
            

            if (failedLogin)
            {
                EmailBorder.BorderBrush = new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString("#2A2A2A")
                );
                EmailNotFoundText.Visibility = Visibility.Collapsed;
                MainBorder.Height -= 15;
                failedLogin = false;

            }

            try
            {
                EmailBox_LostFocus(sender, e);

                if (string.IsNullOrEmpty(password))
                {
                    PasswordBorder.BorderBrush = new SolidColorBrush(
                        (Color)ColorConverter.ConvertFromString("#FF4655")
                    );
                    WrongPassText.Visibility = Visibility.Visible;
                    if (isWrongPassword == false) MainBorder.Height += 15;
                    buttonEnable();
                    LoginButton.Content = "Đăng nhập";
                    LoadingCircle.Visibility = Visibility.Collapsed;
                    isWrongPassword = true;
                }

                if (isWrongEmail || string.IsNullOrEmpty(password))
                {
                    buttonEnable();
                    LoginButton.Content = "Đăng nhập";
                    LoadingCircle.Visibility = Visibility.Collapsed;
                    return;
                }

                LoadingCircle.Visibility = Visibility.Visible;
                LoginButton.Content = "";

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
                        EmailNotFoundText.Visibility = Visibility.Visible;
                        if (isWrongEmail == false) MainBorder.Height += 15;
                        // Email không tồn tại → viền đỏ
                        EmailBox.BorderBrush = new SolidColorBrush(
                            (Color)ColorConverter.ConvertFromString("#FF4655")
                        );
                        buttonEnable();
                        failedLogin = true;
                        LoginButton.Content = "Đăng nhập";
                        LoadingCircle.Visibility = Visibility.Collapsed;
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

            MainFrame.Navigate(new SignUpUserInterface(this));

            MainBorder.Visibility = Visibility.Collapsed;

            MainFrame.Visibility = Visibility.Visible;
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

        bool isWrongPassword = false;
        private void PasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (true)
            {
                PasswordBorder.BorderBrush = new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString("#2A2A2A")
                );
                WrongPassText.Visibility = Visibility.Collapsed;
                if (isWrongPassword == true)
                {
                    MainBorder.Height -= 15;
                    isWrongPassword = false;
                }
            }

            if(string.IsNullOrEmpty(PasswordBox.Password))
            {
                PasswordPlaceholder.Visibility = Visibility.Visible;
            }
        }

        private void PasswordVisible_TextChanged(object sender, TextChangedEventArgs e)
        {
            PasswordBox.Password = PasswordVisible.Text;
        }

        private void EmailBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down || e.Key == Key.Enter)
            {
                PasswordBox.Focus();
            }
        }

        private void EmailBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }

        private void PasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if(PasswordPlaceholder.Visibility == Visibility.Visible)
            {
                PasswordPlaceholder.Visibility = Visibility.Collapsed;
            }
        }

        private void PasswordVisible_GotFocus(object sender, RoutedEventArgs e)
        {
            if (PasswordPlaceholder.Visibility == Visibility.Visible)
            {
                PasswordPlaceholder.Visibility = Visibility.Collapsed;
            }
        }
    }
}