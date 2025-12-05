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
  /// Interaction logic for FriendManager.xaml
  /// </summary>
  public partial class FriendManager : Page
  {
    private MainGameUI _mainWindow;
    public FriendManager(MainGameUI mainWindow)
    {
      InitializeComponent();
      _mainWindow = mainWindow;
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
