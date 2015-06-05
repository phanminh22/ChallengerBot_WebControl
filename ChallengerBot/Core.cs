using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using ChallengerBot.SWF;
using ChallengerBot.SWF.SWFTypes;
using MySql.Data.MySqlClient.Memcached;
using PVPNetConnect.RiotObjects.Platform.Clientfacade.Domain;
using PVPNetConnect.RiotObjects.Platform.Gameinvite.Contract;
using PVPNetConnect.RiotObjects.Platform.Matchmaking;

namespace ChallengerBot
{
    public static class Core
    {
        // Client
        public static string ClientVersion;
        public static bool LobbyStatusWaiting = false;
        public static LobbyStatus Lobby;
        public static MatchMakerParams LobbyGame = new MatchMakerParams();

        // Players
        public static int Waiting = 0;
        public static List<LoginDataPacket> Accounts = new List<LoginDataPacket>();
        
        // Delay between client launch.
        public static int Delay = 15000;
        public static bool ClientDelay = false;

        static void Main(string[] args)
        {
            Console.Title = "ChallengerBot";
            Console.ForegroundColor = ConsoleColor.White;
            Console.SetWindowSize(Console.WindowWidth + 5, Console.WindowHeight);
            
            ChallengerConfig.Initialize();
            WebService.Status("ChallengerBot successfully initialized!", "Console");
            Connect();

            while (true) Thread.Sleep(100);
        }

        public static void Connect()
        {
            foreach (var playerBot in WebService.Players)
            {
                new Engine(playerBot);
                Waiting++;

                if (Waiting == WebService.Setting.MaxBots)
                break;
            }
        }

        public static void ConnectPlayer(string username)
        {
            foreach (var playerBot in WebService.Players)
            {
                if (playerBot.Account == username)
                {
                    new Engine(playerBot);
                    break;
                }
            }
        }

        public static IEnumerable<T> Randomize<T>(this IEnumerable<T> source)
        {
            Random rnd = new Random();
            return source.OrderBy<T, int>((item) => rnd.Next());
        }
    }
}
