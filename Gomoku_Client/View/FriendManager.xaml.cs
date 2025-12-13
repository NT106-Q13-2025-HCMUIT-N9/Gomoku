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
using System.Windows.Media.Animation;
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

    private void SendFriendRequest_Click(object sender, RoutedEventArgs e)
    {
        // Send friend request
    }
 
        
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            var border = (Border)((Grid)UnfriendConfirmationOverlay).Children[0];
            Storyboard fadeOut = (Storyboard)FindResource("PopupFadeOut");
            fadeOut.Begin(border);
            fadeOut.Completed += (s, args) =>
            {
                UnfriendConfirmationOverlay.Visibility = Visibility.Collapsed;
            };

            fadeOut.Begin(border);
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            // Send unfriend request to server
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            // Accept friend request
        }

        private void RefuseButton_Click(object sender, RoutedEventArgs e)
        {
            // Refuse friend request
        }

        private void ChallengeButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void UnfriendButton_Click(object sender, RoutedEventArgs e)
        {
            UnfriendConfirmationOverlay.Visibility = Visibility.Visible;
            var border = (Border)((Grid)UnfriendConfirmationOverlay).Children[0];
            Storyboard fadeIn = (Storyboard)FindResource("PopupFadeIn");
            fadeIn.Begin(border);
        }


}
}
