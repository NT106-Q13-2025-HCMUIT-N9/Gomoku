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
using System.Windows.Shapes;

namespace Gomoku_Client.View
{
    /// <summary>
    /// Interaction logic for GamePlay.xaml
    /// </summary>
    public partial class GamePlay : Window
    {
        public GamePlay()
        {
            InitializeComponent();
        }

        private void BoardCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void SendMessage_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SurrenderButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ChatInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendMessage_Click(sender, e);

                e.Handled = true;
            }

            if ( e.Key == Key.Escape)
            {
                tb_Message.Clear();
                e.Handled = true;
            }

        }
    }
}
