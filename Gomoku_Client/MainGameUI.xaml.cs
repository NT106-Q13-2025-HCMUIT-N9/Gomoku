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
            MainWindow main = new MainWindow();
            // Sao chép vị trí và kích thước
            main.Left = this.Left;
            main.Top = this.Top;
            main.Width = this.Width;
            main.Height = this.Height;
            main.WindowState = this.WindowState;

            // 1. Ẩn Window hiện tại ngay lập tức
            this.Hide();

            // 2. Hiển thị Window mới
            main.Show();

            // 3. Đóng Window cũ sau khi Window mới đã được hiển thị
            this.Close();
        }
    }
}
