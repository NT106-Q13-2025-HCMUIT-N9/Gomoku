using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Gomoku_Server
{
    enum PlayerTurn
    {
        player1,
        player2
    }

    internal class MatchHandle
    {
        PlayerTurn current_turn = PlayerTurn.player1;
        TcpClient player1;
        TcpClient player2;
        int clock1;
        int clock2;
        object turn_lock = new object();

        public void StartClock()
        {
            try
            {
                DateTime lastTick = DateTime.Now;
                while (ServerUtils.StillConnected(player1.Client) && ServerUtils.StillConnected(player2.Client))
                {
                    if ((DateTime.Now - lastTick).TotalSeconds >= 1)
                    {
                        lastTick = DateTime.Now;

                        if (current_turn == PlayerTurn.player1)
                        {
                            string message = $"[TIME1];{clock1}";
                            ServerUtils.SendMessage(player1.Client, message);
                            ServerUtils.SendMessage(player2.Client, message);
                            clock1--;
                        }
                        else if (current_turn == PlayerTurn.player2)
                        {
                            string message = $"[TIME2];{clock2}";
                            ServerUtils.SendMessage(player1.Client, message);
                            ServerUtils.SendMessage(player2.Client, message);
                            clock2--;
                        }
                    }
                }
                Console.WriteLine("[LOG]: A match end");
            }
            catch (Exception ex)
            {
                Console.WriteLine("[ERROR]: " + ex.ToString());
            }
        }

        public void SwitchPlayer()
        {
            lock (turn_lock)
            {
                if (current_turn == PlayerTurn.player1)
                {
                    current_turn = PlayerTurn.player2;
                }
                else
                {
                    current_turn = PlayerTurn.player1;
                }
            }
        }

        public void Handle_Player1()
        {
            byte[] buffer = new byte[1024];

            try
            {
                while (ServerUtils.StillConnected(player1.Client))
                {
                    if (player1.Client.Available == 0)
                    {
                        continue;
                    }

                    int byte_read = player1.Client.Receive(buffer);
                    string message_str = Encoding.UTF8.GetString(buffer, 0, byte_read);
                    string[] parameter = message_str.Split(';');

                    if (parameter[0] == "[SWITCH]" && current_turn == PlayerTurn.player1)
                    {
                        SwitchPlayer();
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("[ERROR]: " + ex.ToString());
            }
        }

        public void Handle_Player2()
        {
            byte[] buffer = new byte[1024];

            try
            {
                while (ServerUtils.StillConnected(player2.Client))
                {
                    if (player2.Client.Available == 0)
                    {
                        continue;
                    }

                    int byte_read = player2.Client.Receive(buffer);
                    string message_str = Encoding.UTF8.GetString(buffer, 0, byte_read);
                    string[] parameter = message_str.Split(';');

                    if (parameter[0] == "[SWITCH]" && current_turn == PlayerTurn.player2)
                    {
                        SwitchPlayer();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[ERROR]: " + ex.ToString());
            }
        }

        public MatchHandle(TcpClient player1, TcpClient player2, int clock1, int clock2)
        {
            this.player1 = player1;
            this.player2 = player2;
            this.clock1 = clock1;
            this.clock2 = clock2;
        }
    }
}
