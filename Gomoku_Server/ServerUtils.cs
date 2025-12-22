using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Gomoku_Server
{
    public class ServerUtils
    {
        public static bool StillConnected(Socket socket)
        {
            try
            {
                if (socket == null) return false;

                if ((socket.Poll(1000, SelectMode.SelectRead) && socket.Available == 0) || !socket.Connected)
                    return false;
                else
                    return true;
            }
            catch (SocketException e)
            {
                Console.WriteLine($"[ERROR] StillConnected: {e.Message}");
                return false;
            }
            catch (ObjectDisposedException e)
            {
                Console.WriteLine($"[ERROR] StillConnected - Socket disposed: {e.Message}");
                return false;
            }
        }

        public static bool SendMessage(Socket socket, string message)
        {
            try
            {
                if (socket == null || !StillConnected(socket))
                    return false;

                byte[] data = Encoding.UTF8.GetBytes(message);
                int bytesSent = socket.Send(data);

                return true;
            }
            catch (SocketException e)
            {
                Console.WriteLine($"[ERROR] SendMessage SocketException: {e.Message}");
                return false;
            }
            catch (ObjectDisposedException e)
            {
                Console.WriteLine($"[ERROR] SendMessage - Socket disposed: {e.Message}");
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine($"[ERROR] SendMessage: {e.Message}");
                return false;
            }
        }

        public static string? ReceiveMessage(Socket socket, int bufferSize = 1024)
        {
            try
            {
                if (socket == null || !StillConnected(socket))
                    return null;

                byte[] buffer = new byte[bufferSize];
                int bytesRead = socket.Receive(buffer);

                if (bytesRead == 0)
                    return null;

                return Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
            }
            catch (SocketException e)
            {
                Console.WriteLine($"[ERROR] ReceiveMessage: {e.Message}");
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine($"[ERROR] ReceiveMessage: {e.Message}");
                return null;
            }
        }
    }
}