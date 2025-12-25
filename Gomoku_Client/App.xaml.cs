using System.Configuration;
using System.Data;
using System.Windows;
using Gomoku_Client.Model;

namespace Gomoku_Client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>


    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            FirebaseInfo.AppInit();
        }
    }
}