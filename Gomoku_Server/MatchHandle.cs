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
        string name1;
        string name2;
        int clock1;
        int clock2;
        object turn_lock = new object();

        char[,] table = new char[15, 15];

        public int prev_mark_at_dir(int start_row, int start_col, int delta_row, int delta_col, char player)
        {
            int count = 0;
            int col = start_col + delta_col;
            int row = start_row + delta_row;
            while (col >= 0 && row >= 0)
            {
                if (table[row, col] != player)
                {
                    break;
                }

                count += 1;
                col += delta_col;
                row += delta_row;
            }

            return count;
        }

        public bool CheckWin(int start_row, int start_col, char player)
        {
            int top_left = prev_mark_at_dir(start_row, start_col, -1, -1, player);
            int top_mid = prev_mark_at_dir(start_row, start_col, -1, 0, player);
            int top_right = prev_mark_at_dir(start_row, start_col, -1, 1, player);
            int mid_left = prev_mark_at_dir(start_row, start_col, 0, -1, player);
            int mid_right = prev_mark_at_dir(start_row, start_col, 0, 1, player);
            int bot_left = prev_mark_at_dir(start_row, start_col, 1, -1, player);
            int bot_mid = prev_mark_at_dir(start_row, start_col, 1, 0, player);
            int bot_right = prev_mark_at_dir(start_row, start_col, 1, 1, player);

            return (top_left + bot_right + 1 >= 5) || (top_mid + bot_mid + 1 >= 5) || (top_right + bot_left + 1 >= 5) || (mid_left + mid_right + 1 >= 5);
        }

        public void StartClock()
        {
            try
            {
                DateTime lastTick = DateTime.Now;
                while (ServerUtils.StillConnected(player1.Client) && ServerUtils.StillConnected(player2.Client))
                {
                    lock (turn_lock)
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
                }
                Console.WriteLine($"[LOG]: Match ended : {name1} - {name2}");
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

                    lock (turn_lock) {
                        if(current_turn == PlayerTurn.player1)
                        {
                            if (parameter[0] == "[SWITCH]")
                            {
                                SwitchPlayer();
                            }
                            else if (parameter[0] == "[MOVE]")
                            {
                                int row = int.Parse(parameter[1]);
                                int col = int.Parse(parameter[2]);

                                lock (table)
                                {
                                    if (table[row, col] == '\0')
                                    {
                                        table[row, col] = 'X';
                                        player1.Client.Send(Encoding.ASCII.GetBytes($"[MOVE1];{row};{col}"));
                                        player2.Client.Send(Encoding.ASCII.GetBytes($"[MOVE1];{row};{col}"));
                                    }
                                }

                                if (CheckWin(row, col, 'X'))
                                {
                                    player1.Client.Send(Encoding.ASCII.GetBytes("[WIN1]"));
                                    player2.Client.Send(Encoding.ASCII.GetBytes("[WIN1]"));
                                    EndMatch();
                                }
                            }
                        }
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

                    lock (turn_lock)
                    {
                        if(current_turn == PlayerTurn.player2)
                        {
                            if (parameter[0] == "[SWITCH]")
                            {
                                SwitchPlayer();
                            }
                            else if (parameter[0] == "[MOVE]")
                            {
                                int row = int.Parse(parameter[1]);
                                int col = int.Parse(parameter[2]);

                                lock (table)
                                {
                                    if (table[row, col] == '\0')
                                    {
                                        table[row, col] = 'O';
                                        player1.Client.Send(Encoding.ASCII.GetBytes($"[MOVE2];{row};{col}"));
                                        player2.Client.Send(Encoding.ASCII.GetBytes($"[MOVE2];{row};{col}"));
                                    }
                                }

                                if (CheckWin(row, col, 'O'))
                                {
                                    player1.Client.Send(Encoding.ASCII.GetBytes("[WIN2]"));
                                    player2.Client.Send(Encoding.ASCII.GetBytes("[WIN2]"));
                                    EndMatch();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[ERROR]: " + ex.ToString());
            }
        }

        public void EndMatch()
        {
            try
            {
                player1.Close();
                player2.Close();
            }
            catch { }
        }


        public MatchHandle(TcpClient player1, TcpClient player2, int clock1, int clock2, string name1, string name2)
        {
            this.player1 = player1;
            this.player2 = player2;
            this.clock1 = clock1;
            this.clock2 = clock2;
            this.name1 = name1;
            this.name2 = name2;
        }
    }
}
