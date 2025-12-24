
using Gomoku_Client.Model;


using System.Configuration;

using System.Data;


using System.Runtime.InteropServices;

using System.Windows;




namespace Gomoku_Client

{




    public partial class App : Application

    {


        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool FreeConsole();
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            FirebaseInfo.AppInit();

            AllocConsole();
            Console.WriteLine("=== GOMOKU CLIENT DEBUG CONSOLE ===");
            Console.WriteLine($"Started at: {DateTime.Now}");
        }
        protected override void OnExit(ExitEventArgs e)
        {
            FreeConsole();
            base.OnExit(e);
        }
    }
}