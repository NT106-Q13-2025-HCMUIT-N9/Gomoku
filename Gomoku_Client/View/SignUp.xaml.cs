using Firebase.Auth;
using Gomoku_Client.Model;
using Gomoku_Client.ViewModel;
using Google.Cloud.Firestore;
using System.Diagnostics;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Gomoku_Client
{
    /// <summary>
    /// Interaction logic for SignUpUserInterface.xaml
    /// </summary>
    public partial class SignUpUserInterface : Page
    {
        private MediaPlayer ButtonClick = new MediaPlayer();
        private MainWindow _mainWindow;
        private bool isLoading = false;
        public SignUpUserInterface(MainWindow mainWindow)
        {
            isLoading = true;
            InitializeComponent();
            StartSound();
            _mainWindow = mainWindow;
            isLoading = false;
        }

        private void UsernameBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (UsernameBox.Text == "Tên người dùng")
            {
                UsernameBox.Text = "";
                UsernameBox.Foreground = Brushes.White;
            }
        }

        void StartSound()
        {
            string buttonPath = System.IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Assets",
                "Sounds",
                "ButtonHover.wav"
            );

            ButtonClick.Open(new Uri(buttonPath, UriKind.Absolute));
        }

        bool isWrongUsername = false;
        private async void UsernameBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(UsernameBox.Text))
            {
                UsernameBox.Text = "Tên người dùng";
                UsernameBox.Foreground = Brushes.Gray;
                UsernameBorder.BorderBrush = new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString("#2A2A2A")
                );
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
                UsernameBorder.BorderBrush = new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString("#FF4655")
                );
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
                        UsernameMsg.Text = "Username đã tồn tại";
                        UsernameBorder.BorderBrush = new SolidColorBrush(
                            (Color)ColorConverter.ConvertFromString("#FF4655")
                        );
                        UsernameMsg.Visibility = Visibility.Visible;
                        if (!isWrongUsername) MainBorder.Height += 15;
                        isWrongUsername = true;
                    }
                    else
                    {
                        UsernameBorder.BorderBrush = new SolidColorBrush(
                            (Color)ColorConverter.ConvertFromString("#2A2A2A")
                        );
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
                    UsernameBorder.BorderBrush = new SolidColorBrush(
                        (Color)ColorConverter.ConvertFromString("#FF4655")
                    );
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
                EmailBox.Foreground = Brushes.White;
            }
        }

        bool isWrongEmail = false;
        private async void EmailBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(EmailBox.Text) || EmailBox.Text == "Email")
            {
                EmailBox.Text = "Email";
                EmailBox.Foreground = Brushes.Gray;
                EmailBorder.BorderBrush = new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString("#FF4655")
                );
                EmailMsg.Text = "Email không được để trống";
                EmailMsg.Visibility = Visibility.Visible;
                if (!isWrongEmail) MainBorder.Height += 15;
                isWrongEmail = true;
            }
            else if (!Validate.IsValidEmail(EmailBox.Text))
            {
                EmailMsg.Text = "Vui lòng nhập đúng định dạng email";
                EmailBorder.BorderBrush = new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString("#FF4655")
                );
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
                        EmailMsg.Text = "Email đã liên kết với một tài khoản khác";
                        EmailBorder.BorderBrush = new SolidColorBrush(
                            (Color)ColorConverter.ConvertFromString("#FF4655")
                        );
                        EmailMsg.Visibility = Visibility.Visible;
                        if (!isWrongEmail) MainBorder.Height += 15;
                        isWrongEmail = true;
                    }
                    else
                    {
                        EmailBorder.BorderBrush = new SolidColorBrush(
                            (Color)ColorConverter.ConvertFromString("#2A2A2A")
                        );
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
                    EmailBorder.BorderBrush = new SolidColorBrush(
                            (Color)ColorConverter.ConvertFromString("#FF4655")
                    );
                    EmailMsg.Visibility = Visibility.Visible;
                    if (!isWrongEmail) MainBorder.Height += 15;
                    isWrongEmail = true;
                }
            }
        }

        private void GoBack_Click(object sender, RoutedEventArgs e)
        {
            ButtonClick.Stop();
            ButtonClick?.Play();
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
            if (_isInternalChange) return;
            _mainWindow.Keyboard.Stop();
            _mainWindow.Keyboard.Play();
            if (PasswordBox.Password.Length > 0)
                PasswordPlaceholder.Visibility = Visibility.Collapsed;
            else
                PasswordPlaceholder.Visibility = Visibility.Visible;
        }

        private void PasswordConfirmBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (_isInternalChange) return;
            _mainWindow.Keyboard.Stop();
            _mainWindow.Keyboard.Play();
            if (PasswordConfirmBox.Password.Length > 0)
                PasswordConfirmPlaceholder.Visibility = Visibility.Collapsed;
            else
                PasswordConfirmPlaceholder.Visibility = Visibility.Visible;
        }

        private void UsernameBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            _mainWindow.Keyboard.Stop();
            _mainWindow.Keyboard.Play();
            try
            {
                char inputChar = e.Text[0];

                if (!(char.IsLetterOrDigit(inputChar)))
                {
                    e.Handled = true;
                }
            }
            catch (Exception)
            {
                e.Handled = true;
            }
        }

        private void buttonDisable()
        {
            SignUpButton.IsHitTestVisible = false;
            ReturnButton.IsHitTestVisible = false;
            UsernameBox.IsHitTestVisible = false;
            EmailBox.IsHitTestVisible = false;
            PasswordBox.IsHitTestVisible = false;
            PasswordConfirmBox.IsHitTestVisible = false;
        }

        private void buttonEnable()
        {
            SignUpButton.IsHitTestVisible = true;
            ReturnButton.IsHitTestVisible = true;
            UsernameBox.IsHitTestVisible = true;
            EmailBox.IsHitTestVisible = true;
            PasswordBox.IsHitTestVisible = true;
            PasswordConfirmBox.IsHitTestVisible = true;

        }
        private async void DangKi_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ButtonClick.Stop();
                ButtonClick?.Play();

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

                //Loading
                LoadingCircle.Visibility = Visibility.Visible;
                buttonDisable();
                SignUpButton.Content = "";

                var temp = await FirebaseInfo.AuthClient.CreateUserWithEmailAndPasswordAsync(email, password, username);
                await UserAction.SendVeriAsync(await temp.User.GetIdTokenAsync());

                UserDataModel doc = new UserDataModel
                {
                    Username = username,
                    Email = email,
                    Friends = new List<string>(),
                    FriendsRequests = new List<string>(),
                    MatchRequests = new List<string>()
                };

                await FireStoreHelper.AddUser(doc);

                ConfirmationOverlay.Visibility = Visibility.Visible;

                //Loaded
                LoadingCircle.Visibility = Visibility.Collapsed;
                buttonEnable();
                SignUpButton.Content = "Đăng Kí Mới";

                var storyboard = (Storyboard)this.Resources["PopupFadeIn"];
                var border = (Border)((Grid)ConfirmationOverlay).Children[0];
                storyboard.Begin(border);

                
            }
            catch (FirebaseAuthException ex)
            {
                FirebaseAuthException a = ex;
                //MessageBox.Show($"Lỗi: {ex.Reason}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ConfirmFailedOverlay.Visibility = Visibility.Visible;

                var storyboard = (Storyboard)this.Resources["PopupFadeIn"];
                var border = (Border)((Grid)ConfirmFailedOverlay).Children[0];
                storyboard.Begin(border);

                //ConfirmationFailedMessage.Text = $"{ex.Reason}";

                //Loaded
                LoadingCircle.Visibility = Visibility.Collapsed;
                buttonEnable();
                SignUpButton.Content = "Đăng Kí Mới";
            }
            catch (AuthException ex)
            {
                AuthException a = ex;
                //MessageBox.Show($"Lỗi: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ConfirmFailedOverlay.Visibility = Visibility.Visible;

                var storyboard = (Storyboard)this.Resources["PopupFadeIn"];
                var border = (Border)((Grid)ConfirmFailedOverlay).Children[0];
                storyboard.Begin(border);

                //ConfirmationFailedMessage.Text = $"{ex.Message}";

                //Loaded
                LoadingCircle.Visibility = Visibility.Collapsed;
                buttonEnable();
                SignUpButton.Content = "Đăng Kí Mới";
            }
        }

        bool isWrongPassword = false;

        private void PasswordVisible_LostFocus(object sender, RoutedEventArgs e)
        {
            if (PasswordBox.Password.Length < 6)
            {
                PasswordBorder.BorderBrush = new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString("#FF4655")
                );
                PasswordMsg.Visibility = Visibility.Visible;
                if (!isWrongPassword) MainBorder.Height += 15;
                isWrongPassword = true;
            }
            else
            {
                PasswordBorder.BorderBrush = new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString("#2A2A2A")
                );
                PasswordMsg.Visibility = Visibility.Collapsed;
                if (isWrongPassword == true)
                {
                    MainBorder.Height -= 15;
                    isWrongPassword = false;
                }
            }

            if (PasswordBox.Password != PasswordConfirmBox.Password && PasswordConfirmBox.Password.Length != 0)
            {
                PasswordConfirmBorder.BorderBrush = new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString("#FF4655")
                );
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
                PasswordConfirmBorder.BorderBrush = new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString("#2A2A2A")
                );
                PasswordConfirmMsg.Visibility = Visibility.Collapsed;
                if (isWrongConfirmPass == true)
                {
                    MainBorder.Height -= 15;
                }
                isWrongConfirmPass = false;
            }

            if (string.IsNullOrEmpty(PasswordBox.Password))
            {
                PasswordPlaceholder.Visibility = Visibility.Visible;
            }
        }

        private void PasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (PasswordBox.Password.Length < 6)
            {
                PasswordBorder.BorderBrush = new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString("#FF4655")
                );
                PasswordMsg.Visibility = Visibility.Visible;
                if (!isWrongPassword) MainBorder.Height += 15;
                isWrongPassword = true;
            }

            else
            {
                PasswordBorder.BorderBrush = new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString("#2A2A2A")
                );
                PasswordMsg.Visibility = Visibility.Collapsed;
                if (isWrongPassword == true)
                {
                    MainBorder.Height -= 15;
                    isWrongPassword = false;
                }
            }

            if (PasswordBox.Password != PasswordConfirmBox.Password && PasswordConfirmBox.Password.Length != 0)
            {
                PasswordConfirmBorder.BorderBrush = new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString("#FF4655")
                );
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
                PasswordConfirmBorder.BorderBrush = new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString("#2A2A2A")
                );
                PasswordConfirmMsg.Visibility = Visibility.Collapsed;
                if (isWrongConfirmPass == true)
                {
                    MainBorder.Height -= 15;
                }
                isWrongConfirmPass = false;
            }

            if (string.IsNullOrEmpty(PasswordBox.Password))
            {
                PasswordPlaceholder.Visibility = Visibility.Visible;
            }
        }

        private void PasswordBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.Space)
            {
                e.Handled = true; 
            }
            if ((e.Key == Key.Down || e.Key == Key.Enter) && isShowingConfirmPass == false) PasswordConfirmBox.Focus();
            else PasswordConfirmVisible.Focus();
        }

    private bool isShowingPass = false;
        private bool _isInternalChange = false;

        private void TogglePasswordBtn_Click(object sender, RoutedEventArgs e)
    {
            _isInternalChange = true;
            ButtonClick.Stop();
            ButtonClick?.Play();
            if (!isShowingPass)
      {
        // Hiện mật khẩu
        PasswordVisible.Text = PasswordBox.Password;
        PasswordVisible.Visibility = Visibility.Visible;
        PasswordBox.Visibility = Visibility.Collapsed;
        TogglePasswordIcon.Data = (Geometry)FindResource("EyeOffIcon");
      }
      else
      {
        // Ẩn mật khẩu
        PasswordBox.Password = PasswordVisible.Text;
        PasswordBox.Visibility = Visibility.Visible;
        PasswordVisible.Visibility = Visibility.Collapsed;
        TogglePasswordIcon.Data = (Geometry)FindResource("EyeIcon");
      }

      isShowingPass = !isShowingPass;
            _isInternalChange = false;
        }

    private void PasswordVisible_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
            if ((e.Key == Key.Down || e.Key == Key.Enter) && isShowingConfirmPass == false) PasswordConfirmBox.Focus();
            else PasswordConfirmVisible.Focus();
        }

        bool isWrongConfirmPass = false;

        private void PasswordConfirmVisible_LostFocus(object sender, RoutedEventArgs e)
        {
            if (PasswordConfirmVisible.Text.Length == 0)
            {
                PasswordConfirmBorder.BorderBrush = new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString("#FF4655")
                );
                PasswordConfirmMsg.Text = "Xác nhận mật khẩu không được để trống";
                PasswordConfirmMsg.Visibility = Visibility.Visible;
                if (!isWrongConfirmPass) MainBorder.Height += 15;
                isWrongConfirmPass = true;
            }
            else
            {
                PasswordConfirmBorder.BorderBrush = new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString("#2A2A2A")
                );
                PasswordConfirmMsg.Visibility = Visibility.Collapsed;
                if (isWrongConfirmPass == true)
                {
                    MainBorder.Height -= 15;
                }
                isWrongConfirmPass = false;
            }

            if (PasswordConfirmBox.Password != PasswordBox.Password)
            {
                PasswordConfirmBorder.BorderBrush = new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString("#FF4655")
                );
                PasswordConfirmMsg.Text = "Mâtk khẩu không khớp";
                PasswordConfirmMsg.Visibility = Visibility.Visible;
                if (!isWrongConfirmPass) MainBorder.Height += 15;
                isWrongConfirmPass = true;
            }

            if (string.IsNullOrEmpty(PasswordConfirmBox.Password))
            {
                PasswordConfirmPlaceholder.Visibility = Visibility.Visible;
            }
        }

        private void PasswordConfirmBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (PasswordConfirmBox.Password.Length == 0 && PasswordConfirmVisible.Text.Length == 0)
            {
                PasswordConfirmBorder.BorderBrush = new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString("#FF4655")
                );
                PasswordConfirmMsg.Text = "Xác nhận mật khẩu không được để trống";
                PasswordConfirmMsg.Visibility = Visibility.Visible;
                if (!isWrongConfirmPass) MainBorder.Height += 15;
                isWrongConfirmPass = true;
            }
            else if(PasswordConfirmBox.Password != PasswordBox.Password)
            {
                PasswordConfirmBorder.BorderBrush = new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString("#FF4655")
                );
                PasswordConfirmMsg.Text = "Mật khẩu không khớp";
                PasswordConfirmMsg.Visibility = Visibility.Visible;
                if (!isWrongConfirmPass) MainBorder.Height += 15;
                isWrongConfirmPass = true;
            }
            else
            {
                PasswordConfirmBorder.BorderBrush = new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString("#2A2A2A")
                );
                PasswordConfirmMsg.Visibility = Visibility.Collapsed;
                if (isWrongConfirmPass == true)
                {
                    MainBorder.Height -= 15;
                }
                isWrongConfirmPass = false;
            }

            if (string.IsNullOrEmpty(PasswordConfirmBox.Password))
            {
                PasswordConfirmPlaceholder.Visibility = Visibility.Visible;
            }
        }

        private void PasswordConfirmBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (isLoading) return;
            _mainWindow.Keyboard.Stop();
            _mainWindow.Keyboard.Play();
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }

        private void PasswordConfirmVisible_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (isLoading) return;
            _mainWindow.Keyboard.Stop();
            _mainWindow.Keyboard.Play();
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }

    bool isShowingConfirmPass = false;
    private void TogglePasswordConfirmBtn_Click(object sender, RoutedEventArgs e)
    {
            _isInternalChange = true;
            ButtonClick.Stop();
            ButtonClick?.Play();
            if (!isShowingConfirmPass)
      {
        // Hiện mật khẩu
        PasswordConfirmVisible.Text = PasswordConfirmBox.Password;
        PasswordConfirmVisible.Visibility = Visibility.Visible;
        PasswordConfirmBox.Visibility = Visibility.Collapsed;
        TogglePasswordConfirmIcon.Data = (Geometry)FindResource("EyeOffIcon");
      }
      else
      {
        // Ẩn mật khẩu
        PasswordConfirmBox.Password = PasswordConfirmVisible.Text;
        PasswordConfirmBox.Visibility = Visibility.Visible;
        PasswordConfirmVisible.Visibility = Visibility.Collapsed;
        TogglePasswordConfirmIcon.Data = (Geometry)FindResource("EyeIcon");
      }

      isShowingConfirmPass = !isShowingConfirmPass;
            _isInternalChange = false;
        }

    private void PasswordVisible_TextChanged(object sender, TextChangedEventArgs e)
        {
            PasswordBox.Password = PasswordVisible.Text;
        }

        private void PasswordConfirmVisible_TextChanged(object sender, TextChangedEventArgs e)
        {
            PasswordConfirmBox.Password = PasswordConfirmVisible.Text;
        }

        private void UsernameBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Space) e.Handled = true;
            if (e.Key == Key.Down || e.Key == Key.Enter) EmailBox.Focus();
        }

        private void EmailBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (isLoading) return;
            _mainWindow.Keyboard.Stop();
            _mainWindow.Keyboard.Play();
            if (e.Key == Key.Space) e.Handled = true;
            if ((e.Key == Key.Down || e.Key == Key.Enter) && isShowingPass == false) PasswordBox.Focus();
            else PasswordVisible.Focus();
        }

        private void PasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (PasswordPlaceholder.Visibility == Visibility.Visible)
            {
                PasswordPlaceholder.Visibility = Visibility.Collapsed;
            }
        }

        private void PasswordConfirmBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (PasswordConfirmPlaceholder.Visibility == Visibility.Visible)
            {
                PasswordConfirmPlaceholder.Visibility = Visibility.Collapsed;
            }
        }

        private void PasswordVisible_GotFocus(object sender, RoutedEventArgs e)
        {
            if (PasswordPlaceholder.Visibility == Visibility.Visible)
            {
                PasswordPlaceholder.Visibility = Visibility.Collapsed;
            }
        }

        private void PasswordConfirmVisible_GotFocus(object sender, RoutedEventArgs e)
        {
            if (PasswordConfirmPlaceholder.Visibility == Visibility.Visible)
            {
                PasswordConfirmPlaceholder.Visibility = Visibility.Collapsed;
            }
        }

        private void ConfirmationButton_Click(object sender, RoutedEventArgs e)
        {
            /*var storyboard = (Storyboard)this.Resources["PopupFadeOut"];
            var border = (Border)((Grid)ConfirmationOverlay).Children[0];
            storyboard.Completed += (s, args) =>
            {
                ConfirmationOverlay.Visibility = Visibility.Collapsed;
            };

            storyboard.Begin(border);*/
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
