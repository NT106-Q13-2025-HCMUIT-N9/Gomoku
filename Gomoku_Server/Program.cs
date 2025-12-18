using Gomoku_Server;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Security.Permissions;
using System.Text;

namespace Server
{
    class Server
    {

        ConcurrentDictionary<string, TcpClient> challenges = new ConcurrentDictionary<string, TcpClient>();


        private void StartMatch(TcpClient player1, TcpClient player2)
        {
            int clock1 = 600;
            int clock2 = 600;

            MatchHandle matchHandle = new MatchHandle(player1, player2, clock1, clock2);

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
            Queue<TcpClient> waiting_queue = new Queue<TcpClient>();

            TcpListener listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 9999);
            listener.Start();

            new Thread(() =>
            {
                StartLobbyServer();
            }).Start();
        
            Thread listen_thread = new Thread(() =>
            {
                try
                {
                    while (true)
                    {
                        TcpClient client = listener.AcceptTcpClient();

                        lock (waiting_queue)
                        {
                            waiting_queue.Enqueue(client);
                            
                            Console.WriteLine($"[LOG]: Connected to {client.Client.RemoteEndPoint?.ToString()}");

                            if (waiting_queue.Count >= 2)
                            {
                                TcpClient player1 = waiting_queue.Dequeue();
                                TcpClient player2 = waiting_queue.Dequeue();

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

                                if ( player1 != null  && player2 != null )
                                {
                                    new Thread(() =>
                                    {
                                        StartMatch(player1, player2);
                                    }).Start();
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[ERROR]: " + ex.ToString());
                }
            });
            listen_thread.Start();

            Console.WriteLine("[LOG]: Server is running on 9999 (Match)");
            listen_thread.Join();
        }

        public void StartLobbyServer()
        {
            TcpListener challengeListener = new TcpListener(IPAddress.Parse("127.0.0.1"), 8888);
            challengeListener.Start();
            Console.WriteLine("[LOG]: Server is running on 8888 (Lobby)");
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
            string room = null;
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
                    room = parts[1] + parts[2];


                    Console.WriteLine(message);
                    switch (command)
                    {
                        case "CHALLENGE_REQUEST":
                            challenges.TryAdd(room, client);
                            break;

                        case "CHALLENGE_ACCEPT":
                            if (challenges.TryRemove(room, out TcpClient challengeClient))
                            {
                                new Thread(() =>
                                {
                                    StartMatch(challengeClient, client);
                                }).Start();

                            }
                            break;

                        case "CHALLENGE_REJECT":
                            challenges.TryRemove(room, out TcpClient removedClient);
                            break;

                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
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