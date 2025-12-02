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

        public void StartClock()
        {
            try
            {
                while (ServerUtils.StillConnected(player1.Client) && ServerUtils.StillConnected(player2.Client))
                {
                    if (current_turn == PlayerTurn.player1)
                    {
                        string message = $"[TIME1];{clock1}";
                        ServerUtils.SendMessage(player1.Client, message);
                        ServerUtils.SendMessage(player2.Client, message);
                        clock1 -= 1;
                    }
                    else if (current_turn == PlayerTurn.player2)
                    {
                        string message = $"[TIME2];{clock2}";
                        ServerUtils.SendMessage(player1.Client, message);
                        ServerUtils.SendMessage(player2.Client, message);
                        clock1 -= 1;
                    }
                    Thread.Sleep(1000);
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
            if(current_turn == PlayerTurn.player1)
            {
                current_turn = PlayerTurn.player2;
            }
            else
            {
                current_turn = PlayerTurn.player1;
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
