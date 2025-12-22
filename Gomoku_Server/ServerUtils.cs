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

                return socket.Connected;
            }
            catch (ObjectDisposedException)
            {
                return false;
            }
            catch
            {
                return false;
            }
        }

        public static bool SendMessage(Socket socket, string message)
        {
            try
            {
                if (socket == null)
                {
                    Console.WriteLine($"[SEND_ERROR] Socket is null");
                    return false;
                }

                if (!message.EndsWith("\n"))
                {
                    message += "\n";
                }

                byte[] data = Encoding.UTF8.GetBytes(message);
                int bytesSent = socket.Send(data);

                return bytesSent > 0;
            }
            catch (SocketException e)
            {
                Console.WriteLine($"[SEND_ERROR] SocketException: {e.ErrorCode} - {e.Message}");
                return false;
            }
            catch (ObjectDisposedException e)
            {
                Console.WriteLine($"[SEND_ERROR] Socket disposed: {e.Message}");
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine($"[SEND_ERROR] {e.GetType().Name}: {e.Message}");
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
                Console.WriteLine($"[RECV_ERROR] SocketException: {e.Message}");
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine($"[RECV_ERROR] {e.GetType().Name}: {e.Message}");
                return null;
            }
        }
    }
}