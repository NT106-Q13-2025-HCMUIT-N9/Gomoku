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
        private const string AppUid = "5C3C3EA0-AB5E-47EC-AC79-BF6CCB1F895D";
        private static Mutex? _mutex = null;

        protected override void OnStartup(StartupEventArgs e)
        {
            bool isNew;

            _mutex = new Mutex(true, AppUid, out isNew);

            if (isNew)
            {
                base.OnStartup(e);
                FirebaseInfo.AppInit();
            }
            else
            {
                Shutdown();
                return;
            }
        }
    }
}