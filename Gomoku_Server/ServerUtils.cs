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
                if ((socket.Poll(1000, SelectMode.SelectRead) && socket.Available == 0) || !socket.Connected)
                    return false;
                else
                    return true;
            }
            catch (SocketException e)
            {
                return false;
            }
        }

        public static bool SendMessage(Socket socket, string message)
        {
            if(StillConnected(socket))
            {
                socket.Send(Encoding.ASCII.GetBytes(message));
                return true;
            }

            return false;
        }
    }
}
