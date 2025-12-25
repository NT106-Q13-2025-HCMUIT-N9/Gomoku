using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Gomoku_Server
{
    public class Server
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
         * Server responses and additional messages:
         * - [MATCH_FOUND];opponent_name -> sent to both players when random match is found
         * - [CHALLENGE_TIMEOUT];room_key -> sent to challenger if challenge request times out
         * - [OPPONENT_DISCONNECTED];username -> sent when opponent disconnects during match
         * - [INVALID_REQUEST] -> sent when client sends malformed message
         * - [ALREADY_IN_MATCH];username -> sent when user tries to join queue while already in match
         * - [USER_NOT_FOUND];target_user -> sent when trying to challenge non-existent user
         * 
         * note : "challenger;target" is the room key for each challenge match 
         */


        public static ConcurrentDictionary<string, TcpClient> challenges = new ConcurrentDictionary<string, TcpClient>();
        public static ConcurrentDictionary<TcpClient, string> names = new ConcurrentDictionary<TcpClient, string>();
        public static ConcurrentDictionary<string, bool> inMatch = new ConcurrentDictionary<string, bool>();
        public static Queue<TcpClient> waiting_queue = new Queue<TcpClient>();

        TcpListener TcpListener;

        public Server()
        {

        }

        public void Start(int port)
        {
            try
            {
                TcpListener?.Stop();
            }
            catch { }

            TcpListener = new TcpListener(IPAddress.Parse("127.0.0.1"), port);
            TcpListener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            Thread listenThread = new Thread(ListenForClients);
            listenThread.IsBackground = false;
            listenThread.Start();
        }

        public void Stop()
        {
            try
            {
                TcpListener?.Stop();
                Console.WriteLine("[LOG]: Server stopped");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Stop: {ex.Message}");
            }
        }

        private void ListenForClients()
        {
            try
            {
                TcpListener.Start();
                Console.WriteLine($"[LOG]: Server started successfully");

                while (true)
                {
                    TcpClient client = TcpListener.AcceptTcpClient();
                    Thread t = new Thread(() =>
                    {
                        HandleClient(client);
                    });
                    t.IsBackground = true;
                    t.Start();
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"[ERROR] ListenForClients: {ex.Message}");
                Console.WriteLine("[INFO] Port may already be in use. Try a different port or close other instances.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] ListenForClients: {ex.Message}");
            }
        }

        public void HandleClient(TcpClient client)
        {
            try
            {
                Socket socket = client.Client;
                byte[] buffer = new byte[1024];

                Console.WriteLine($"[HANDLE_CLIENT] Waiting for initial message...");

                int byteRead = socket.Receive(buffer);
                if (byteRead == 0)
                {
                    Console.WriteLine($"[HANDLE_CLIENT] Connection closed (0 bytes)");
                    client.Close();
                    return;
                }

                string message = Encoding.UTF8.GetString(buffer, 0, byteRead).Trim();
                string[] parts = message.Split(';');

                Console.WriteLine($"[HANDLE_CLIENT] Received: {message}");

                if (parts.Length < 2)
                {
                    ServerUtils.SendMessage(socket, "[INVALID_REQUEST]");
                    client.Close();
                    return;
                }

                string command = parts[0];

                switch (command)
                {
                    case "[MATCH_REQUEST]":
                        HandleRandomMatchRequest(client, parts);
                        Console.WriteLine($"[HANDLE_CLIENT] Exiting for {parts[1]} - match threads will handle connection");
                        break;

                    case "[CHALLENGE_REQUEST]":
                    case "[CHALLENGE_ACCEPT]":
                    case "[CHALLENGE_DECLINE]":
                        HandleChallenge(client, parts);
                        Console.WriteLine($"[HANDLE_CLIENT] Exiting for challenge - match threads will handle connection");
                        break;

                    case "[MATCH_END]":
                        if (parts.Length >= 2)
                        {
                            inMatch.TryRemove(parts[1], out _);
                        }
                        removeChallengesOf(client);
                        client.Close();
                        Console.WriteLine($"[HANDLE_CLIENT] Match ended, connection closed");
                        break;

                    default:
                        Console.WriteLine($"[HANDLE_CLIENT] Invalid command: {command}");
                        ServerUtils.SendMessage(socket, "[INVALID_REQUEST]");
                        client.Close();
                        break;
                }

                Console.WriteLine($"[HANDLE_CLIENT] Thread exiting");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] HandleClient: {ex.Message}");
                try { client?.Close(); } catch { }
            }
        }



        private void StartMatch(TcpClient player1, TcpClient player2, string name1, string name2)
        {
            removeChallengesOf(player1);
            removeChallengesOf(player2);
            inMatch.TryAdd(name1, true);
            inMatch.TryAdd(name2, true);
            Console.WriteLine($"[MATCH] Starting: {name1} vs {name2}");

            int clock1 = 300;
            int clock2 = 300;

            MatchHandle matchHandle = new MatchHandle(player1, player2, clock1, clock2, name1, name2);

            try
            {
                ServerUtils.SendMessage(player1.Client, $"[INIT];{clock1};{clock2};X;{name2}");
                ServerUtils.SendMessage(player2.Client, $"[INIT];{clock1};{clock2};O;{name1}");

                Console.WriteLine($"[MATCH] Sent [INIT] to both players");

                Console.WriteLine($"[MATCH] Waiting for HandleClient threads to exit...");
                Thread.Sleep(500);

                Console.WriteLine($"[MATCH] Starting match threads...");

                Thread clockThread = new Thread(matchHandle.StartClock);
                Thread p1Thread = new Thread(matchHandle.Handle_Player1);
                Thread p2Thread = new Thread(matchHandle.Handle_Player2);

                clockThread.IsBackground = true;
                p1Thread.IsBackground = true;
                p2Thread.IsBackground = true;

                clockThread.Start();
                p1Thread.Start();
                p2Thread.Start();

                Console.WriteLine($"[MATCH] All threads started for {name1} vs {name2}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MATCH ERROR]: {ex}");
            }
        }


        public void HandleRandomMatchRequest(TcpClient client, string[] parts)
        {
            try
            {
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

        public void HandleChallenge(TcpClient client, string[] parts)
        {
            string? room = null;

            try
            {
                if (parts.Length < 3)
                {
                    ServerUtils.SendMessage(client.Client, "[INVALID_REQUEST]");
                    return;
                }

                string command = parts[0];
                room = parts[1] + ";" + parts[2];
                TcpClient challenger = null;
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
                        if (!challenges.TryGetValue(room, out challenger) || challenger == null)
                        {
                            ServerUtils.SendMessage(client.Client, $"[CHALLENGE_CANCELED];{room}");
                            break;
                        }

                        bool isChallengerInMatch = inMatch.ContainsKey(parts[1]);
                        bool isChallengerConnected = ServerUtils.StillConnected(challenger.Client);

                        if (isChallengerInMatch)
                        {
                            Console.WriteLine($"[LOG]: {parts[2]} accepted {parts[1]}'s challenge, but {parts[1]} is in another match");
                            ServerUtils.SendMessage(client.Client, $"[CHALLENGE_CANCELED];{room}");
                        }
                        else if (!isChallengerConnected)
                        {
                            Console.WriteLine($"[LOG]: {parts[2]} accepted {parts[1]}'s challenge but {parts[1]} disconnected");
                            ServerUtils.SendMessage(client.Client, $"[CHALLENGE_CANCELED];{room}");
                        }
                        else
                        {
                            Console.WriteLine($"[LOG]: {parts[2]} accepted {parts[1]}'s challenge");
                            ServerUtils.SendMessage(challenger.Client, $"[CHALLENGE_ACCEPT];{room}");
                            challenges.TryRemove(room, out _);
                            names.TryAdd(client, parts[2]);
                            names.TryAdd(challenger, parts[1]);
                            new Thread(() =>
                            {
                                StartMatch(challenger, client, parts[1], parts[2]);
                            }).Start();
                        }
                        break;


                    case "[CHALLENGE_DECLINE]":
                        if (challenges.TryGetValue(room, out challenger) && challenger != null)
                        {
                            ServerUtils.SendMessage(challenger.Client, $"[CHALLENGE_DECLINE];{room}");
                            Console.WriteLine($"[LOG]: {parts[2]} declined {parts[1]}'s challenge");
                        }
                        else
                        {
                            Console.WriteLine($"[LOG]: Declined but challenger not found for room {room}");
                        }
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
            if (client == null) return;

            string? username = null;
            names.TryGetValue(client, out username);

            var keysToRemove = challenges
                .Where(kvp => kvp.Value == client)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in keysToRemove)
            {
                challenges.TryRemove(key, out _);
                Console.WriteLine($"[LOG]: Deleted challenge request for room: {key}");
            }

            if (!string.IsNullOrEmpty(username))
            {
                inMatch.TryRemove(username, out _);
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

            Console.WriteLine($"[LOG]: Challenge request timeout for room: {room}");
            ServerUtils.SendMessage(client.Client, $"[CHALLENGE_TIMEOUT];{room}");
            challenges.TryRemove(room, out _);
            client.Close();
        }

        public static void Log(string message)
        {
            string time = DateTime.Now.ToString("HH:mm:ss");
            Console.WriteLine($"[{time}] [LOG] {message}");
        }

    }
}
