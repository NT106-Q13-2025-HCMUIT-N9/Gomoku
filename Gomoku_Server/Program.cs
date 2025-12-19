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

        /* Client messages : 
         * 
         * Random match: [MATCH_REQUEST];username
         * 
         * Challenge Match: [CHALLENGE_REQUEST];challenger;target
         * 
         * Accept Challenge: [CHALLENGE_ACCEPT];challenger;target
         * 
         * Decline Challenge: [CHALLENGE_DECLINE];challenger;target
         * 
         * End Match: [MATCH_END];username -> server remove user from the "inMatch" dictionary
         * 
         * (Optional)
         * If a challenge match is canceled ( the challenger is in a match or offline and the target accepts the challenge )
         * Then the server will send a message to the target : [CHALLENGE_CANCELED];challenger;target
         * 
         * 
         * 
         * note : "challenger;target" is the room key for each challenge match 
         */


        public ConcurrentDictionary<string, TcpClient> challenges = new ConcurrentDictionary<string, TcpClient>();
        public ConcurrentDictionary<TcpClient, string> names = new ConcurrentDictionary<TcpClient, string>();
        public static ConcurrentDictionary<string, bool> inMatch = new ConcurrentDictionary<string, bool>();
        public Queue<TcpClient> waiting_queue = new Queue<TcpClient>();


        private void StartMatch(TcpClient player1, TcpClient player2, string name1, string name2)
        {
            removeChallengesOf(player1);
            removeChallengesOf(player2);
            inMatch.TryAdd(name1, true);
            inMatch.TryAdd(name2, true);
            Console.WriteLine($"[LOG]: Match started : {name1} - {name2}");
            int clock1 = 600;
            int clock2 = 600;

            MatchHandle matchHandle = new MatchHandle(player1, player2, clock1, clock2, name1, name2);

            try
            {
                ServerUtils.SendMessage(player1.Client, $"[INIT];{clock1};{clock2};X");
                ServerUtils.SendMessage(player2.Client, $"[INIT];{clock1};{clock2};O");

                Thread clockThread = new Thread(matchHandle.StartClock);
                Thread p1Thread = new Thread(matchHandle.Handle_Player1);
                Thread p2Thread = new Thread(matchHandle.Handle_Player2);

                clockThread.IsBackground = true;
                p1Thread.IsBackground = true;
                p2Thread.IsBackground = true;

                clockThread.Start();
                p1Thread.Start();
                p2Thread.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine("[MATCH ERROR]: " + ex.ToString());
            }
        }



        public Server()
        {
            TcpListener listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 9999);
            listener.Start();

            Thread challenge_thread = new Thread(() =>
            {
                StartChallengeServer();
            });
        
            Thread listen_thread = new Thread(() =>
            {
                try
                {
                    while (true)
                    {
                        TcpClient client = listener.AcceptTcpClient();

                        new Thread(() => HandleRandomMatchRequest(client))
                        {
                            IsBackground = true
                        }.Start();

                        
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[ERROR]: " + ex.ToString());
                }
            });
            listen_thread.Start();
            challenge_thread.Start();
            Console.WriteLine("[LOG]: Server is running on port 9999 (Random Match)");
            listen_thread.Join();
            challenge_thread.Join();
        }   

        public void StartChallengeServer()
        {
            TcpListener challengeListener = new TcpListener(IPAddress.Parse("127.0.0.1"), 8888);
            challengeListener.Start();
            Console.WriteLine("[LOG]: Server is running on port 8888 (Challenge Match)");
            while (true)
            {
                TcpClient client = challengeListener.AcceptTcpClient();
                new Thread(() =>
                {
                    HandleChallenge(client);
                }).Start();
            }
        }

        public void HandleRandomMatchRequest(TcpClient client)
        {
            try
            {
                var stream = client.GetStream();
                byte[] buffer = new byte[1024];
                int byteRead = stream.Read(buffer, 0, buffer.Length);
                if (byteRead == 0)
                {
                    client.Close();
                    return;
                }
                string message = Encoding.UTF8.GetString(buffer, 0, byteRead).Trim();
                string[] parts = message.Split(';');
                if (parts.Length < 2 || parts[0] != "[MATCH_REQUEST]")
                {
                    client.Close();
                    return;
                }

                TcpClient? player1 = null;
                TcpClient? player2 = null;

                lock (waiting_queue)
                {
                    if (!waiting_queue.Contains(client))
                    {
                        waiting_queue.Enqueue(client);
                    }

                    names.TryAdd(client, parts[1]);
                    Console.WriteLine($"[LOG]: Random match request from {parts[1]}");

                    if (waiting_queue.Count >= 2)
                    {
                        player1 = waiting_queue.Dequeue();
                        player2 = waiting_queue.Dequeue();
                    }
                }

                if (player1 != null && player2 != null)
                {
                    if (!ServerUtils.StillConnected(player1.Client))
                    {
                        names.TryRemove(player1, out _);
                        lock (waiting_queue)
                        {
                            waiting_queue.Enqueue(player2);
                        }
                        return;
                    }
                    else if (!ServerUtils.StillConnected(player2.Client))
                    {
                        names.TryRemove(player2, out _);
                        lock (waiting_queue)
                        {
                            waiting_queue.Enqueue(player1);
                        }
                        return;
                    }

                    new Thread(() =>
                    {
                        removeChallengesOf(player1);
                        removeChallengesOf(player2);
                        StartMatch(player1, player2, names[player1], names[player2]);
                    })
                    {
                        IsBackground = true
                    }.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] HandleRandomMatchRequest: {ex.Message}");
                try { client?.Close(); } catch { }
            }
        }

        public void HandleChallenge(TcpClient client)
        {
            string? room = null;
            var stream = client.GetStream();
            byte[] data = new byte[1024];

            try
            {
                int byteRead = stream.Read(data, 0, data.Length);
                if ( byteRead == 0 ) return;

                    

                string message = Encoding.UTF8.GetString(data, 0, byteRead).Trim();
                string[] parts = message.Split(';');
                if (parts.Length < 3) return;


                string command = parts[0];
                room = parts[1] + ";" + parts[2];

                switch (command)
                {
                    case "[CHALLENGE_REQUEST]":
                        Console.WriteLine($"[LOG]: {parts[1]} is challenging {parts[2]}");
                        challenges.TryAdd(room, client);
                        names.TryAdd(client, parts[1]);
                        new Thread(() =>
                        {
                            KeepAliveConnection(client, room, TimeSpan.FromSeconds(20));
                        }).Start();
                        break;


                    case "[CHALLENGE_ACCEPT]":
                        //Console.WriteLine($"[DEBUG] Checking challenge accept: {parts[1]} -> {parts[2]}");
                        //Console.WriteLine($"[DEBUG] inMatch dictionary contents:");
                        //foreach (var entry in inMatch)
                        //{
                        //    Console.WriteLine($"  {entry.Key}: {entry.Value}");
                        //}

                        challenges.TryRemove(room, out TcpClient? challenger);
                        bool isChallengerInMatch = inMatch.ContainsKey(parts[1]);
                        bool canStartMatch = (challenger != null && ServerUtils.StillConnected(challenger.Client));

                        if ( isChallengerInMatch )
                        {
                            Console.WriteLine($"[LOG]: {parts[2]} accepted {parts[1]}'s challenge, but {parts[1]} is currently in another match");
                            names.TryRemove(client, out _);
                            ServerUtils.SendMessage(client.Client, $"[CHALLENGE_CANCELED];{room}");
                        }
                        else if ( !canStartMatch )
                        {
                            Console.WriteLine($"[LOG]: {parts[2]} accepted {parts[1]}'s challenge but {parts[1]} disconnected it");
                            names.TryRemove(client, out _);
                            ServerUtils.SendMessage(client.Client, $"[CHALLENGE_CANCELED];{room}");
                        }
                        else
                        {
                            Console.WriteLine($"[LOG]: {parts[2]} accepted {parts[1]}'s challenge");
                            names.TryAdd(client, parts[2]);
                            names.TryAdd(challenger, parts[1]);
                            new Thread(() =>
                            {
                                StartMatch(challenger, client, parts[1], parts[2]);
                            }).Start();
                        }

                        break;


                    case "[CHALLENGE_DECLINE]":
                        Console.WriteLine($"[LOG]: {parts[2]} declined {parts[1]}'s challenge");
                        challenges.TryRemove(room, out _);
                        names.TryRemove(client, out _);
                        break;


                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[ERROR]: " + ex.ToString());
            }
        }

        public void removeChallengesOf(TcpClient client)
        {
            var keysToRemove =  challenges
                                .Where(kvp => kvp.Value == client)
                                .Select(kvp => kvp.Key)
                                .ToList();

            foreach (var key in keysToRemove)
            {
                challenges.TryRemove(key, out _);
                Console.WriteLine($"[LOG]: Deleted challenge request timeout for room: {key}");
            }
        }

        private void KeepAliveConnection(TcpClient client, string room, TimeSpan timeout)
        {
            var startTime = DateTime.Now;

            while (DateTime.Now - startTime < timeout)
            {
                bool stillConnected = client.Client != null && ServerUtils.StillConnected(client.Client);

                if (!stillConnected)
                {
                    Console.WriteLine($"[LOG]: Challenger disconnected for room: {room}");
                    challenges.TryRemove(room, out _);
                    client.Close();
                    return;
                }

                if (!challenges.TryGetValue(room, out _))
                {
                    Console.WriteLine($"[LOG]: Challenge request for room: {room} was deleted");
                    return;
                }

                Thread.Sleep(1000);
            }

            // Timeout
            Console.WriteLine($"[LOG]: Challenge request timeout for room: {room}");
            challenges.TryRemove(room, out _);
            client.Close();
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