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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Gomoku_Client.View
{
  /// <summary>
  /// Interaction logic for Lobby.xaml
  /// </summary>
  public partial class Lobby : Page
  {
    // Truyền tham số MainGameUI để có thể quay lại bằng BackButton
    private MainGameUI _mainWindow;
    public Lobby(MainGameUI mainGameUI)
    {
      InitializeComponent();
      _mainWindow = mainGameUI;
            new GamePlay().Show();
        }

    private void BackButton_Checked(object sender, RoutedEventArgs e)
    {
      if (_mainWindow == null)
      {
        MessageBox.Show("Không tìm thấy cửa sổ chính.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        return;
      }
      _mainWindow.ShowMenuWithAnimation();
    }
  }
}
