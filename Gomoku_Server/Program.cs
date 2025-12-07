using Gomoku_Server;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server
{
    class Server
    {
        private void StartMatch(TcpClient player1, TcpClient player2)
        {
            int clock1 = 600;
            int clock2 = 600;

            MatchHandle matchHandle = new MatchHandle(player1, player2, clock1, clock2);

            try
            {
                ServerUtils.SendMessage(player1.Client, $"[INNIT];{clock1};{clock2};X");
                ServerUtils.SendMessage(player2.Client, $"[INNIT];{clock1};{clock2};O");

                Thread clock_thread = new Thread(() => {
                    matchHandle.StartClock();
                });
                clock_thread.Start();

                Thread player1_thread = new Thread(() => {
                    matchHandle.Handle_Player1();
                });
                player1_thread.Start();

                Thread player2_thread = new Thread(() => {
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
                                }else if (!ServerUtils.StillConnected(player2.Client))
                                {
                                    waiting_queue.Enqueue(player1);
                                    continue;
                                }

                                StartMatch(player1, player2);
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

            Console.WriteLine("[LOG]: Server is ready to listen for client");
            listen_thread.Join();
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