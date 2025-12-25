using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Gomoku_Server
{
    public static class Logger
    {
        private static string LogFilePath = "server_log.txt";
        private static object _lock = new object();

        // Lưu trạng thái để hiển thị
        private static int _queueCount = 0;
        private static List<string> _activeMatches = new List<string>();

        // Mặc định là "Loading..." để người dùng biết đang lấy IP
        private static string _publicIp = "Loading...";
        private static readonly HttpClient _httpClient = new HttpClient() { Timeout = TimeSpan.FromSeconds(5) };

        // Render interval
        private static readonly TimeSpan _renderInterval = TimeSpan.FromSeconds(5);

        static Logger()
        {
            try
            {
                File.WriteAllText(LogFilePath, $"[STARTUP] Server started at {DateTime.Now}\n");
            }
            catch { }

            Task.Run(async () => await GetPublicIp());

            Task.Run(async () => await RenderLoop());
        }

        private static async Task GetPublicIp()
        {
            try
            {
                string ip = await _httpClient.GetStringAsync("https://api.ipify.org");
                _publicIp = ip.Trim();

                Log($"[NETWORK] Public IP detected: {_publicIp}");
            }
            catch (Exception ex)
            {
                _publicIp = "Error/Localhost";
                Log($"[NETWORK] Could not fetch public IP: {ex.Message}");
            }

            Render();
        }

        private static async Task RenderLoop()
        {
            try
            {
                Render();

                while (true)
                {
                    await Task.Delay(_renderInterval);
                    Render();
                }
            }
            catch (Exception ex)
            {
                Log($"[ERROR] RenderLoop: {ex.Message}");
            }
        }

        public static void Log(string message)
        {
            try
            {
                string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string logLine = $"[{time}] {message}\n";

                lock (_lock)
                {
                    File.AppendAllText(LogFilePath, logLine);
                }
            }
            catch { }
        }

        public static void UpdateDashboard(int queueCount, List<string> matches)
        {
            _queueCount = queueCount;
            _activeMatches = matches;
            Render();
        }

        public static void UpdateQueue(int count)
        {
            _queueCount = count;
            Render();
        }

        private static void Render()
        {
            lock (_lock)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("========== GOMOKU SERVER MONITOR ==========");
                Console.ResetColor();


                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Public IP: {_publicIp}:9999");
                Console.ResetColor();

                Console.WriteLine($"Status: Running");
                Console.WriteLine($"Log file: {Path.GetFullPath(LogFilePath)}");
                Console.WriteLine("-------------------------------------------");

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Players in Queue: {_queueCount}");
                Console.ResetColor();

                Console.WriteLine("-------------------------------------------");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Active Matches ({_activeMatches.Count}):");
                Console.ResetColor();

                if (_activeMatches.Count == 0)
                {
                    Console.WriteLine("  (No active matches)");
                }
                else
                {
                    for (int i = 0; i < _activeMatches.Count; i++)
                    {
                        Console.WriteLine($"  {i + 1}: {_activeMatches[i]}");
                    }
                }
                Console.WriteLine("===========================================");
            }
        }
    }
}