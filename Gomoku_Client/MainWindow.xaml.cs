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
        EmailBox.Text = "Tên người dùng";
        EmailBox.Foreground = Brushes.Gray;
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
      try
      {
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
          MessageBox.Show($"Lỗi: {auth_ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
      }
    }
  }
}