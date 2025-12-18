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
    class Server
    {

        /* Client message : 
         * 
         * Random match : MATCH_REQUEST|username
         * 
         * Challenge Match : CHALLENGE_REQUEST|username1|username2
         * 
         * Accept Challenge : CHALLENGE_ACCEPT|username1|username2
         * 
         * Decline Challenge : CHALLENGE_DECLINE|username1|username2
         * 
         * note : "username1|username2" is the room key of each challenge match that username1 is the challenger
         */


        ConcurrentDictionary<string, TcpClient> challenges = new ConcurrentDictionary<string, TcpClient>();
        Queue<TcpClient> waiting_queue = new Queue<TcpClient>();
        ConcurrentDictionary<TcpClient, string> names = new ConcurrentDictionary<TcpClient, string>();


        private void StartMatch(TcpClient player1, TcpClient player2, string name1, string name2)
        {
            int clock1 = 600;
            int clock2 = 600;

            MatchHandle matchHandle = new MatchHandle(player1, player2, clock1, clock2, name1, name2);

            try
            {
                ServerUtils.SendMessage(player1.Client, $"[INIT];{clock1};{clock2};X");
                ServerUtils.SendMessage(player2.Client, $"[INIT];{clock1};{clock2};O");

                Thread clock_thread = new Thread(() =>
                {
                    matchHandle.StartClock();
                });
                clock_thread.Start();

                Thread player1_thread = new Thread(() =>
                {
                    matchHandle.Handle_Player1();
                });
                player1_thread.Start();

                Thread player2_thread = new Thread(() =>
                {
                    matchHandle.Handle_Player2();
                });
                player2_thread.Start();

                player1_thread.Join();
                player2_thread.Join();
                clock_thread.Join();
            }
            catch (Exception ex)
            {
                Console.WriteLine("[ERROR]: " + ex.ToString());
            }
        }



        public Server()
        {
            TcpListener listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 9999);
            listener.Start();

            new Thread(() =>
            {
                StartChallengeServer();
            }).Start();
        
            Thread listen_thread = new Thread(() =>
            {
                try
                {
                    while (true)
                    {
                        TcpClient client = listener.AcceptTcpClient();
                        var stream = client.GetStream();
                        byte[] buffer = new byte[1024];

                        int byteRead = stream.Read(buffer, 0, buffer.Length);
                        if (byteRead == 0) continue;

                        string message = Encoding.UTF8.GetString(buffer, 0, byteRead).Trim();
                        string[] parts = message.Split('|');
                        if (parts.Length < 2) continue;

                        TcpClient? player1 = null;
                        TcpClient? player2 = null;
                        lock (waiting_queue)
                        {
                            waiting_queue.Enqueue(client);
                            names.TryAdd(client, parts[1]);
                            Console.WriteLine($"[LOG]: Match request from {parts[1]}");

                            if (waiting_queue.Count >= 2)
                            {
                                player1 = waiting_queue.Dequeue();
                                player2 = waiting_queue.Dequeue();
                            }
                        }

                        if ( player1 != null && player2 != null )
                        {
                            if (!ServerUtils.StillConnected(player1.Client))
                            {
                                waiting_queue.Enqueue(player2);
                                continue;
                            }
                            else if (!ServerUtils.StillConnected(player2.Client))
                            {
                                waiting_queue.Enqueue(player1);
                                continue;
                            }
                            new Thread(() =>
                            {
                                StartMatch(player1, player2, names[player1], names[player2]);
                            }).Start();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[ERROR]: " + ex.ToString());
                }
            });
            listen_thread.Start();

            Console.WriteLine("[LOG]: Server is running on 9999 (Random Match)");
            listen_thread.Join();
        }

        public void StartChallengeServer()
        {
            TcpListener challengeListener = new TcpListener(IPAddress.Parse("127.0.0.1"), 8888);
            challengeListener.Start();
            Console.WriteLine("[LOG]: Server is running on 8888 (Challenged Match)");
            new Thread(() =>
            {
                while (true)
                {
                    TcpClient client = challengeListener.AcceptTcpClient();
                    new Thread(() =>
                    {
                        HandleChallenge(client);
                    }).Start();
                }
            }).Start();
        }

        public void HandleChallenge(TcpClient client)
        {
            string? room = null;
            var stream = client.GetStream();
            byte[] data = new byte[1024];

            try
            {
                while (true)
                {
                    int byteRead = stream.Read(data, 0, data.Length);
                    if ( byteRead == 0) break;


                    string message = Encoding.UTF8.GetString(data, 0, byteRead).Trim();
                    string[] parts = message.Split('|');
                    if (parts.Length < 3) continue;


                    string command = parts[0];
                    room = parts[1] + "|" + parts[2];

                    switch (command)
                    {
                        case "CHALLENGE_REQUEST":
                            challenges.TryAdd(room, client);
                            names.TryAdd(client, parts[1]);
                            Console.WriteLine($"[LOG]: {parts[1]} is challenging {parts[2]}");
                            break;

                        case "CHALLENGE_ACCEPT":
                            if (challenges.TryRemove(room, out TcpClient? challenger))
                            {
                                names.TryAdd(client, parts[2]);
                                Console.WriteLine($"[LOG]: {parts[2]} accepted {parts[1]}'s challenge");
                                // Remove all other challenges created by the challenger
                                new Thread(() =>
                                {
                                    StartMatch(challenger, client, names[challenger], names[client]);
                                }).Start();

                            } 
                            else
                            {
                                Console.WriteLine($"[LOG]: {parts[2]} accepted {parts[1]}'s challenge but {parts[1]} is in match or offline now");
                                ServerUtils.SendMessage(client.Client, $"CHALLENGE_CANCELED|{room}");
                            }
                            break;

                        case "CHALLENGE_DECLINE":
                            Console.WriteLine($"[LOG]: {parts[2]} declined {parts[1]}'s challenge");
                            challenges.TryRemove(room, out TcpClient? removedClient);
                            break;

                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[ERROR]: " + ex.ToString());
            }
            finally
            {
                if (room != null)
                    challenges.TryRemove(room, out _);

                client.Close();
            }
        }

    }


    internal class MainClass()
    {
        static void Main(string[] args)
        {
            Server server = new Server();
        }
    }
}