using Firebase.Auth;
using Gomoku_Client.Model;
using Gomoku_Client.View;
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
  /// Interaction logic for MainGameUI.xaml
  /// </summary>
  public partial class MainGameUI : Window
  {
    public MainGameUI()
    {
      InitializeComponent();
    }

    private void SignOut_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        FirebaseInfo.AuthClient.SignOut();

        MainWindow main = new MainWindow();
        // Sao chép vị trí và kích thước
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

        private void PlayButton_Checked(object sender, RoutedEventArgs e)
        {
            // 1. Tải trang Lobby vào Frame
            MainFrame.Navigate(new Lobby(this));

            // 2. Ẩn Menu Chính
            StackPanelMenu.Visibility = Visibility.Collapsed;

            // 3. Hiển thị Frame nội dung
            MainFrame.Visibility = Visibility.Visible;

            // 4. Đặt lại trạng thái RadioButton
            ((RadioButton)sender).IsChecked = false;
        }
    }
}
