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

        public static List<string> ActiveMatchList = new List<string>();
        public static object MatchListLock = new object();

        TcpListener TcpListener;

        public Server()
        {

        }

        public async Task Start(int port)
        {
            try { TcpListener?.Stop(); } catch { }

            TcpListener = new TcpListener(IPAddress.Parse("127.0.0.1"), port);
            TcpListener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            Thread listenThread = new Thread(ListenForClients);
            listenThread.IsBackground = false;
            listenThread.Start();

            Logger.UpdateDashboard(0, new List<string>());

            await CheckPlayersInQueue();
        }

        public void Stop()
        {
            try
            {
                TcpListener?.Stop();
                Logger.Log("[LOG]: Server stopped");
            }
            catch (Exception ex)
            {
                Logger.Log($"[ERROR] Stop: {ex.Message}");
            }
        }

        private void ListenForClients()
        {
            try
            {
                TcpListener.Start();
                Logger.Log($"[LOG]: Server started successfully on port");

                while (true)
                {
                    TcpClient client = TcpListener.AcceptTcpClient();
                    Thread t = new Thread(() => HandleClient(client));
                    t.IsBackground = true;
                    t.Start();
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"[ERROR] ListenForClients: {ex.Message}");
            }
        }

        public async Task CheckPlayersInQueue()
        {
            int lastCount = -1;
            string lastActiveSnapshot = "";

            while (true)
            {
                int currentCount;
                List<string> activeSnapshot;

                // read queue count and active match list in a thread-safe manner
                lock (waiting_queue)
                {
                    currentCount = waiting_queue.Count;
                }

                lock (MatchListLock)
                {
                    // create a copy to avoid sharing the internal list reference
                    activeSnapshot = new List<string>(ActiveMatchList);
                }

                // create a simple snapshot string to detect changes
                string activeJoined = string.Join("|", activeSnapshot);

                if (currentCount != lastCount || activeJoined != lastActiveSnapshot)
                {
                    // update logger with latest values
                    Logger.UpdateDashboard(currentCount, activeSnapshot);
                    lastCount = currentCount;
                    lastActiveSnapshot = activeJoined;
                }

                await Task.Delay(5000);
            }
        }

        public void HandleClient(TcpClient client)
        {
            try
            {
                Socket socket = client.Client;
                byte[] buffer = new byte[1024];

                Logger.Log($"[HANDLE_CLIENT] New connection received");

                int byteRead = socket.Receive(buffer);
                if (byteRead == 0)
                {
                    client.Close();
                    return;
                }

                string message = Encoding.UTF8.GetString(buffer, 0, byteRead).Trim();
                string[] parts = message.Split(';');

                Logger.Log($"[HANDLE_CLIENT] Received: {message}");

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
                        break;

                    case "[CHALLENGE_REQUEST]":
                    case "[CHALLENGE_ACCEPT]":
                    case "[CHALLENGE_DECLINE]":
                        HandleChallenge(client, parts);
                        break;

                    case "[MATCH_END]":
                        if (parts.Length >= 2) inMatch.TryRemove(parts[1], out _);
                        removeChallengesOf(client);
                        client.Close();
                        break;

                    default:
                        ServerUtils.SendMessage(socket, "[INVALID_REQUEST]");
                        client.Close();
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"[ERROR] HandleClient: {ex.Message}");
                try { client?.Close(); } catch { }
            }
        }

        public static void AddActiveMatchDisplay(string name1, string name2)
        {
            lock (MatchListLock)
            {
                ActiveMatchList.Add($"{name1} VS {name2}");
                Logger.UpdateDashboard(waiting_queue.Count, ActiveMatchList);
            }
        }

        public static void RemoveActiveMatchDisplay(string name1, string name2)
        {
            lock (MatchListLock)
            {
                ActiveMatchList.Remove($"{name1} VS {name2}");
                // Handle trường hợp đảo ngược tên nếu cần
                ActiveMatchList.Remove($"{name2} VS {name1}");
                Logger.UpdateDashboard(waiting_queue.Count, ActiveMatchList);
            }
        }

        private void StartMatch(TcpClient player1, TcpClient player2, string name1, string name2)
        {
            removeChallengesOf(player1);
            removeChallengesOf(player2);
            inMatch.TryAdd(name1, true);
            inMatch.TryAdd(name2, true);

            Logger.Log($"[MATCH] Starting: {name1} vs {name2}");

            AddActiveMatchDisplay(name1, name2);

            int clock1 = 300;
            int clock2 = 300;

            MatchHandle matchHandle = new MatchHandle(player1, player2, clock1, clock2, name1, name2);

            try
            {
                ServerUtils.SendMessage(player1.Client, $"[INIT];{clock1};{clock2};X;{name2}");
                ServerUtils.SendMessage(player2.Client, $"[INIT];{clock1};{clock2};O;{name1}");

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
                Logger.Log($"[MATCH ERROR]: {ex}");
                RemoveActiveMatchDisplay(name1, name2);
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
                    Logger.Log($"[LOG]: Random match request from {parts[1]}");

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
                Logger.Log($"[ERROR] HandleRandomMatchRequest: {ex.Message}");
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

                switch (command)
                {
                    case "[CHALLENGE_REQUEST]":
                        Logger.Log($"[LOG]: {parts[1]} is challenging {parts[2]}");
                        challenges.TryAdd(room, client);
                        names.TryAdd(client, parts[1]);
                        new Thread(() =>
                        {
                            KeepAliveConnection(client, room, TimeSpan.FromSeconds(20));
                        }).Start();
                        break;


                    case "[CHALLENGE_ACCEPT]":
                        TcpClient? challenger;
                        bool hasChallenger = challenges.TryGetValue(room, out challenger);

                        if (!hasChallenger || challenger == null)
                        {
                            ServerUtils.SendMessage(client.Client, $"[CHALLENGE_CANCELED];{room}");
                            break;
                        }

                        bool isChallengerInMatch = inMatch.ContainsKey(parts[1]);
                        bool isChallengerConnected = ServerUtils.StillConnected(challenger.Client);

                        if (isChallengerInMatch)
                        {
                            Logger.Log($"[LOG]: {parts[2]} accepted {parts[1]}'s challenge, but {parts[1]} is in another match");
                            ServerUtils.SendMessage(client.Client, $"[CHALLENGE_CANCELED];{room}");
                        }
                        else if (!isChallengerConnected)
                        {
                            Logger.Log($"[LOG]: {parts[2]} accepted {parts[1]}'s challenge but {parts[1]} disconnected");
                            ServerUtils.SendMessage(client.Client, $"[CHALLENGE_CANCELED];{room}");
                        }
                        else
                        {
                            Logger.Log($"[LOG]: {parts[2]} accepted {parts[1]}'s challenge");
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
                        Logger.Log($"[LOG]: {parts[2]} declined {parts[1]}'s challenge");

                        TcpClient? waitingChallenger;
                        if (challenges.TryGetValue(room, out waitingChallenger))
                        {
                            try
                            {
                                ServerUtils.SendMessage(waitingChallenger.Client, $"[CHALLENGE_DECLINE];{parts[1]};{parts[2]}");
                            }
                            catch (Exception ex)
                            {
                                Logger.Log($"[ERROR] Failed to send decline to challenger: {ex.Message}");
                            }
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
                Logger.Log("[ERROR]: " + ex.ToString());
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
                Logger.Log($"[LOG]: Deleted challenge request for room: {key}");
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
                    Logger.Log($"[LOG]: Challenger disconnected for room: {room}");
                    challenges.TryRemove(room, out _);
                    client.Close();
                    return;
                }

                if (!challenges.TryGetValue(room, out _))
                {
                    Logger.Log($"[LOG]: Challenge request for room: {room} was deleted");
                    return;
                }

                Thread.Sleep(1000);
            }

            Logger.Log($"[LOG]: Challenge request timeout for room: {room}");
            ServerUtils.SendMessage(client.Client, $"[CHALLENGE_TIMEOUT];{room}");
            challenges.TryRemove(room, out _);
            client.Close();
        }

        public static void Log(string message)
        {
            string time = DateTime.Now.ToString("HH:mm:ss");
            Logger.Log($"[{time}] [LOG] {message}");
        }

    }
}