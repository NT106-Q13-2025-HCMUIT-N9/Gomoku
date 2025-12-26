using Gomoku_Server;
using Microsoft.VisualBasic.FileIO;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Security.Permissions;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Server
{
    internal class MainClass()
    {
        static void Main(string[] args)
        {
            try
            {
                FirebaseInfo.AppInit();
                Logger.Log("[LOG]: Server Init Firebase successfully");
            }
            catch (Exception e)
            {
                Logger.Log("[CRASH]: Server cannot connect to Firebase please for the love of god turn on your wifi or somthing");
                Logger.Log($"[CRASH]: {e.Message}");
                return;
            }

            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;
            Gomoku_Server.Server server = new Gomoku_Server.Server();
            server.Start(9999);
            Console.WriteLine("Press Ctrl + C to disconnect the server");
            Thread.Sleep(Timeout.Infinite);
        }
    }
}