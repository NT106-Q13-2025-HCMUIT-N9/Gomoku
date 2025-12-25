using Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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
        bool matchEnded = false;
        int moveCount = 0;
        const int MAX_MOVES = 15 * 15;
        int total_duration = 0;

        char[,] table = new char[15, 15];

        public int prev_mark_at_dir(int start_row, int start_col, int delta_row, int delta_col, char player)
        {
            int count = 0;
            int col = start_col + delta_col;
            int row = start_row + delta_row;
            while (col >= 0 && row >= 0 && col < 15 && row < 15)
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

            return (top_left + bot_right + 1 >= 5) ||
                   (top_mid + bot_mid + 1 >= 5) ||
                   (top_right + bot_left + 1 >= 5) ||
                   (mid_left + mid_right + 1 >= 5);
        }

        public void StartClock()
        {
            try
            {
                DateTime lastTick = DateTime.Now;
                DateTime lastUpdatedTick;
                Logger.Log($"[CLOCK] Thread started for {name1} vs {name2}");

                Thread.Sleep(100);

                while (!matchEnded)
                {
                    if ((DateTime.Now - lastTick).TotalSeconds >= 1)
                    {
                        lastTick = DateTime.Now;
                        total_duration += 1;

                        lock (turn_lock)
                        {
                            if (matchEnded) break;

                            string message = "";

                            if (current_turn == PlayerTurn.player1 && clock1 > 0)
                            {
                                clock1--;
                                message = $"[TIME1];{clock1}";
                                if (clock1 <= 0)
                                {
                                    ServerUtils.SendMessage(player1.Client, "[TIMEOUT1]");
                                    ServerUtils.SendMessage(player2.Client, "[TIMEOUT1]");
                                    FirestoreHelper.AddMatchInfo(false, new List<string> { name1, name2 }, total_duration, name2);
                                    FirestoreHelper.IncWinUser(name2);
                                    FirestoreHelper.IncLoseUser(name1);
                                    EndMatch();
                                    break;
                                }
                            }
                            else if (current_turn == PlayerTurn.player2 && clock2 > 0)
                            {
                                clock2--;
                                message = $"[TIME2];{clock2}";
                                if (clock2 <= 0)
                                {
                                    ServerUtils.SendMessage(player1.Client, "[TIMEOUT2]");
                                    ServerUtils.SendMessage(player2.Client, "[TIMEOUT2]");
                                    FirestoreHelper.AddMatchInfo(false, new List<string> { name1, name2 }, total_duration, name1);
                                    FirestoreHelper.IncWinUser(name1);
                                    FirestoreHelper.IncLoseUser(name2);
                                    EndMatch();
                                    break;
                                }
                            }


                            bool p1Sent = ServerUtils.SendMessage(player1.Client, message);
                            bool p2Sent = ServerUtils.SendMessage(player2.Client, message);

                            if (!p1Sent)
                            {
                                Logger.Log($"[DISCONNECT] Player 1 ({name1}) lost connection.");

                                if (p2Sent) ServerUtils.SendMessage(player2.Client, $"[OPPONENT_DISCONNECTED];{name2}");

                                FirestoreHelper.AddMatchInfo(false, new List<string> { name1, name2 }, total_duration, name2);
                                FirestoreHelper.IncWinUser(name2);
                                FirestoreHelper.IncLoseUser(name1);

                                EndMatch();
                                break;
                            }

                            if (!p2Sent)
                            {
                                Logger.Log($"[DISCONNECT] Player 2 ({name2}) lost connection.");

                                if (p1Sent) ServerUtils.SendMessage(player1.Client, $"[OPPONENT_DISCONNECTED];{name2}");

                                FirestoreHelper.AddMatchInfo(false, new List<string> { name1, name2 }, total_duration, name1);
                                FirestoreHelper.IncWinUser(name1);
                                FirestoreHelper.IncLoseUser(name2);

                                EndMatch();
                                break;
                            }
                        }
                    }

                    Thread.Sleep(100);
                }
                Logger.Log($"[CLOCK] Thread ended: {name1} - {name2}");
            }
            catch (Exception ex)
            {
                Logger.Log($"[ERROR] StartClock: {ex}");
                if (!matchEnded)
                    EndMatch();
            }
        }

        public void SwitchPlayer()
        {
            lock (turn_lock)
            {
                if (current_turn == PlayerTurn.player1)
                {
                    current_turn = PlayerTurn.player2;
                    Logger.Log($"[SWITCH] Turn is now Player2 ({name2})");
                }
                else
                {
                    current_turn = PlayerTurn.player1;
                    Logger.Log($"[SWITCH] Turn is now Player1 ({name1})");
                }
            }
        }

        public void Handle_Player1()
        {
            byte[] buffer = new byte[4096];

            try
            {
                while (!matchEnded && ServerUtils.StillConnected(player1.Client))
                {
                    if (player1.Client.Available == 0)
                    {
                        Thread.Sleep(5);
                        continue;
                    }

                    int byte_read = player1.Client.Receive(buffer);
                    if (byte_read == 0) break;

                    string message_str = Encoding.UTF8.GetString(buffer, 0, byte_read).Trim();
                    string[] parameter = message_str.Split(';');

                    Logger.Log($"[RECV] Player1 ({name1}): {message_str}");

                    // if (parameter[0] == "[READY]")
                    // {
                    //     lock (ready_lock)
                    //     {
                    //         readyCount++;
                    //         Logger.Log($"[READY] {name1} ready ({readyCount}/2)");
                    //     }
                    //     continue;
                    // }

                    if (parameter[0] == "[CHAT]" && parameter.Length >= 3)
                    {
                        string senderName = parameter[1];
                        string chatMessage = string.Join(";", parameter.Skip(2));
                        string forwardMessage = $"[CHAT];{senderName};{chatMessage}";
                        Logger.Log($"[CHAT] Forwarding to Player2: {forwardMessage}");
                        ServerUtils.SendMessage(player2.Client, forwardMessage);
                        continue;
                    }

                    if (parameter[0] == "[MATCH_END]")
                    {
                        Logger.Log($"[MATCH_END] {name1} requested match end");
                        EndMatch();
                        break;
                    }

                    lock (turn_lock)
                    {
                        if (matchEnded) break;

                        if (parameter[0] == "[SWITCH]" && current_turn == PlayerTurn.player1)
                        {
                            SwitchPlayer();
                        }
                        else if (parameter[0] == "[MOVE]")
                        {
                            if (current_turn != PlayerTurn.player1)
                            {
                                Logger.Log($"[REJECT] Not Player1's turn");
                                ServerUtils.SendMessage(player1.Client, "[INVALID_MOVE];Không phải lượt của bạn");
                                continue;
                            }

                            if (parameter.Length < 3) continue;

                            int row = int.Parse(parameter[1]);
                            int col = int.Parse(parameter[2]);

                            Logger.Log($"[MOVE] Player1 ({name1}) move to ({row},{col})");

                            if (row < 0 || row >= 15 || col < 0 || col >= 15)
                            {
                                ServerUtils.SendMessage(player1.Client, "[INVALID_MOVE];Tọa độ không hợp lệ");
                                continue;
                            }

                            if (table[row, col] != '\0')
                            {
                                Logger.Log($"[REJECT] Cell ({row},{col}) already occupied");
                                ServerUtils.SendMessage(player1.Client, "[INVALID_MOVE];Ô đã có quân cờ");
                                continue;
                            }

                            table[row, col] = 'X';
                            moveCount++;

                            string moveMessage = $"[MOVE1];{row};{col}";
                            Logger.Log($"[BROADCAST] {moveMessage}");
                            ServerUtils.SendMessage(player1.Client, moveMessage);
                            ServerUtils.SendMessage(player2.Client, moveMessage);

                            if (CheckWin(row, col, 'X'))
                            {
                                Logger.Log($"[WIN] {name1} wins!");
                                ServerUtils.SendMessage(player1.Client, "[WIN1]");
                                ServerUtils.SendMessage(player2.Client, "[WIN1]");
                                EndMatch();

                                FirestoreHelper.AddMatchInfo(false, new List<string> { name1, name2 }, total_duration, name1);
                                FirestoreHelper.IncWinUser(name1);
                                FirestoreHelper.IncLoseUser(name2);
                                break;
                            }

                            if (moveCount >= MAX_MOVES)
                            {
                                Logger.Log($"[DRAW] Board full");
                                ServerUtils.SendMessage(player1.Client, "[DRAW]");
                                ServerUtils.SendMessage(player2.Client, "[DRAW]");
                                EndMatch();

                                FirestoreHelper.AddMatchInfo(true, new List<string> { name1, name2 }, total_duration);
                                FirestoreHelper.IncDrawUser(name1);
                                FirestoreHelper.IncDrawUser(name2);
                                break;
                            }

                            SwitchPlayer();
                        }
                        else if (parameter[0] == "[RESIGN]")
                        {
                            Logger.Log($"[RESIGN] {name1} resigned");
                            ServerUtils.SendMessage(player1.Client, "[RESIGN1]");
                            ServerUtils.SendMessage(player2.Client, "[RESIGN1]");
                            EndMatch();

                            FirestoreHelper.AddMatchInfo(false, new List<string> { name1, name2 }, total_duration, name2);
                            FirestoreHelper.IncWinUser(name2);
                            FirestoreHelper.IncLoseUser(name1);
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"[ERROR] Handle_Player1 ({name1}): {ex.Message}");
                if (!matchEnded)
                {
                    ServerUtils.SendMessage(player2.Client, $"[OPPONENT_DISCONNECTED];{name1}");
                    EndMatch();

                    FirestoreHelper.IncWinUser(name2);
                    FirestoreHelper.IncLoseUser(name1);
                }
            }
        }

        public void Handle_Player2()
        {
            byte[] buffer = new byte[4096];

            try
            {
                while (!matchEnded && ServerUtils.StillConnected(player2.Client))
                {
                    if (player2.Client.Available == 0)
                    {
                        Thread.Sleep(5);
                        continue;
                    }

                    int byte_read = player2.Client.Receive(buffer);
                    if (byte_read == 0) break;

                    string message_str = Encoding.UTF8.GetString(buffer, 0, byte_read).Trim();
                    string[] parameter = message_str.Split(';');

                    Logger.Log($"[RECV] Player2 ({name2}): {message_str}");

                    // if (parameter[0] == "[READY]")
                    // {
                    //     lock (ready_lock)
                    //     {
                    //         readyCount++;
                    //         Logger.Log($"[READY] {name2} ready ({readyCount}/2)");
                    //     }
                    //     continue;
                    // }

                    if (parameter[0] == "[CHAT]" && parameter.Length >= 3)
                    {
                        string senderName = parameter[1];
                        string chatMessage = string.Join(";", parameter.Skip(2));
                        string forwardMessage = $"[CHAT];{senderName};{chatMessage}";
                        Logger.Log($"[CHAT] Forwarding to Player1: {forwardMessage}");
                        ServerUtils.SendMessage(player1.Client, forwardMessage);
                        continue;
                    }

                    if (parameter[0] == "[MATCH_END]")
                    {
                        Logger.Log($"[MATCH_END] {name2} requested match end");
                        EndMatch();
                        break;
                    }

                    lock (turn_lock)
                    {
                        if (matchEnded) break;

                        if (parameter[0] == "[SWITCH]" && current_turn == PlayerTurn.player2)
                        {
                            SwitchPlayer();
                        }
                        else if (parameter[0] == "[MOVE]")
                        {
                            if (current_turn != PlayerTurn.player2)
                            {
                                Logger.Log($"[REJECT] Not Player2's turn");
                                ServerUtils.SendMessage(player2.Client, "[INVALID_MOVE];Không phải lượt của bạn");
                                continue;
                            }

                            if (parameter.Length < 3) continue;

                            int row = int.Parse(parameter[1]);
                            int col = int.Parse(parameter[2]);

                            Logger.Log($"[MOVE] Player2 ({name2}) move to ({row},{col})");

                            if (row < 0 || row >= 15 || col < 0 || col >= 15)
                            {
                                ServerUtils.SendMessage(player2.Client, "[INVALID_MOVE];Tọa độ không hợp lệ");
                                continue;
                            }

                            if (table[row, col] != '\0')
                            {
                                Logger.Log($"[REJECT] Cell ({row},{col}) already occupied");
                                ServerUtils.SendMessage(player2.Client, "[INVALID_MOVE];Ô đã có quân cờ");
                                continue;
                            }

                            table[row, col] = 'O';
                            moveCount++;

                            string moveMessage = $"[MOVE2];{row};{col}";
                            Logger.Log($"[BROADCAST] {moveMessage}");
                            ServerUtils.SendMessage(player1.Client, moveMessage);
                            ServerUtils.SendMessage(player2.Client, moveMessage);

                            if (CheckWin(row, col, 'O'))
                            {
                                Logger.Log($"[WIN] {name2} wins!");
                                ServerUtils.SendMessage(player1.Client, "[WIN2]");
                                ServerUtils.SendMessage(player2.Client, "[WIN2]");
                                EndMatch();

                                FirestoreHelper.AddMatchInfo(false, new List<string> { name1, name2 }, total_duration, name2);
                                FirestoreHelper.IncWinUser(name2);
                                FirestoreHelper.IncLoseUser(name1);
                                break;
                            }

                            if (moveCount >= MAX_MOVES)
                            {
                                Logger.Log($"[DRAW] Board full");
                                ServerUtils.SendMessage(player1.Client, "[DRAW]");
                                ServerUtils.SendMessage(player2.Client, "[DRAW]");
                                EndMatch();

                                FirestoreHelper.AddMatchInfo(true, new List<string> { name1, name2 }, total_duration);
                                FirestoreHelper.IncDrawUser(name1);
                                FirestoreHelper.IncDrawUser(name2);
                                break;
                            }

                            SwitchPlayer();
                        }
                        else if (parameter[0] == "[RESIGN]")
                        {
                            Logger.Log($"[RESIGN] {name2} resigned");
                            ServerUtils.SendMessage(player1.Client, "[RESIGN2]");
                            ServerUtils.SendMessage(player2.Client, "[RESIGN2]");
                            EndMatch();

                            FirestoreHelper.AddMatchInfo(false, new List<string> { name1, name2 }, total_duration, name1);
                            FirestoreHelper.IncWinUser(name1);
                            FirestoreHelper.IncLoseUser(name2);
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"[ERROR] Handle_Player2 ({name2}): {ex.Message}");
                if (!matchEnded)
                {
                    ServerUtils.SendMessage(player1.Client, $"[OPPONENT_DISCONNECTED];{name2}");
                    EndMatch();

                    FirestoreHelper.IncWinUser(name1);
                    FirestoreHelper.IncLoseUser(name2);
                }
            }
        }

        public void EndMatch()
        {
            try
            {
                lock (turn_lock)
                {
                    if (matchEnded) return;
                    matchEnded = true;
                }

                Logger.Log($"[END] Match ended: {name1} vs {name2}");

                ServerUtils.SendMessage(player1.Client, "[MATCH_END]");
                ServerUtils.SendMessage(player2.Client, "[MATCH_END]");

                try
                {
                    Server.RemoveActiveMatchDisplay(name1, name2);
                }
                catch (Exception ex)
                {
                    Logger.Log($"[ERROR] Removing active match display: {ex.Message}");
                }

                Thread.Sleep(100);
            }
            catch (Exception ex)
            {
                Logger.Log($"[ERROR] EndMatch: {ex.Message}");
            }
            finally
            {
                try { player1.Close(); } catch { }
                try { player2.Close(); } catch { }
                Gomoku_Server.Server.inMatch.TryRemove(name1, out _);
                Gomoku_Server.Server.inMatch.TryRemove(name2, out _);
            }
        }

        public MatchHandle(TcpClient player1, TcpClient player2, int clock1, int clock2, string name1, string name2)
        {
            this.player1 = player1;
            this.player2 = player2;
            this.clock1 = clock1;
            this.clock2 = clock2;
            this.name1 = name1;
            this.name2 = name2;

            Logger.Log($"[INIT] Match created: {name1} (X) vs {name2} (O)");
            Logger.Log($"[INIT] Starting turn: Player1 ({name1})");
        }
    }
}