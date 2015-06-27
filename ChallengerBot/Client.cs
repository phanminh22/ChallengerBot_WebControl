using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Timers;
using Newtonsoft.Json;
using PVPNetConnect;
using PVPNetConnect.RiotObjects.Platform.Clientfacade.Domain;
using PVPNetConnect.RiotObjects.Platform.Gameinvite.Contract;
using PVPNetConnect.RiotObjects.Platform.Matchmaking;

namespace PVPNetBot
{
    internal abstract class Client
    {
        private static void Main(string[] args)
        {
            InitializeConsole();
            InitializeConfiguration();

            while (true) Thread.Sleep(100);
        }


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

        /*static void Main(string[] args)
        {
            Console.Title = "ChallengerBot";
            Console.ForegroundColor = ConsoleColor.White;
            Console.SetWindowSize(Console.WindowWidth + 5, Console.WindowHeight);
            
            ChallengerConfig.Initialize();
            WebService.Status("ChallengerBot successfully initialized!", "Console");
            Connect();

            while (true) Thread.Sleep(100);
        }*/

        public static void Connect()
        {
            foreach (var playerBot in WebService.Players)
            {
                //new Engine(playerBot);
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
                    //new Engine(playerBot);
                    break;
                }
            }
        }

        /*public static IEnumerable<T> Randomize<T>(this IEnumerable<T> source)
        {
            Random rnd = new Random();
            return source.OrderBy<T, int>((item) => rnd.Next());
        }*/

        protected static void InitializeConsole()
        {
            Console.Title = "ChallengerBot";
            Console.ForegroundColor = ConsoleColor.White;
            Console.SetWindowSize(Console.WindowWidth + 5, Console.WindowHeight);
        }

        protected static void InitializeConfiguration()
        {
            new Configuration();
            Console.WriteLine("Configuration loaded!");
            Thread.Sleep(1000);
            var usingmysql = Configuration.IsMySQLEnabled ? "YES" : "NO";
            Console.WriteLine("Using MySQL: " + usingmysql);
            if (Configuration.IsMySQLEnabled)
            {
                Console.WriteLine("Connecting to MySQL");
                new WebService();
                while (!WebService.ConnectionEntablished)
                    Thread.Sleep(100);
                Console.WriteLine("Connected!");
            }
            else if (!File.Exists("Settings.ini"))
            {
                Console.WriteLine("Configuration file not found. Enter required information." + Environment.NewLine);

                Console.Write("League of Legends game path: ");
                var GamePath = Console.ReadLine();

                Console.Write(Environment.NewLine + "Select region [EUW, EUNE, NA, TR, ...]: ");
                var Region = Console.ReadLine();

                Console.Write(Environment.NewLine + "Max bots value: ");
                var MaxBots = Console.ReadLine();

                Console.Write(Environment.NewLine + "Queue type: [ARAM, MEDIUM_BOT, EASY_BOT, NORMAL5X5]: ");
                var Queue = Console.ReadLine();


                string SelectedDifficulty = string.Empty;
                int SelectedQueue = int.MaxValue;

                switch (Queue)
                {
                    case "MEDIUM_BOT":
                        SelectedDifficulty = "MEDIUM";
                        SelectedQueue = 33;
                        break;
                    case "EASY_BOT":
                        SelectedQueue = 32;
                        SelectedDifficulty = "EASY";
                        break;
                    case "ARAM":
                        SelectedQueue = 65;
                        break;
                    case "NORMAL5X5":
                        SelectedQueue = 2;
                        break;
                }

                if (MaxBots != null)
                {
                    var mbots = String.Join("", MaxBots.Where(char.IsDigit));
                    Settings SFile = new Settings
                    {
                        GamePath = GamePath,
                        MaxBots = Convert.ToInt32(mbots),
                        Difficulty = SelectedDifficulty,
                        QueueType = SelectedQueue,
                        Region = Region
                    };

                    WebService.Setting = SFile;
                    using (StreamWriter file = File.CreateText("Settings.ini"))
                    using (JsonTextWriter writer = new JsonTextWriter(file))
                    {
                        writer.WriteRaw(JsonConvert.SerializeObject(SFile, Formatting.Indented));
                    }
                }
                else return;
                
                Thread.Sleep(1000);
                Console.WriteLine("Settings.ini file saved.");
            }
            else if (File.Exists("Settings.ini")) Configuration.LoadSettings();
     
            if (!File.Exists("accounts.txt"))
            {
                Console.WriteLine("Missing accounts.txt file.");
                Console.WriteLine("One account per line. " + Environment.NewLine +
                                  " Format: accountname|accountpassword|maxlevel[1-31]|xpboostbuy[0 - NO and 1 YES]");
                while (true) Thread.Sleep(100);
            }
            else
            {
                int LoadedPlayers = 0;
                StreamReader file = new StreamReader("accounts.txt");
                string line;

                while ((line = file.ReadLine()) != null)
                {
                    string[] account = line.Split('|');

                    bool boost = !account[3].Equals(0);

                    Accounts bot = new Accounts
                    {
                        Account = account[0],
                        Password = account[1],
                        Maxlevel = Convert.ToInt32(account[2]),
                        Autoboost = boost
                    };
                    WebService.Players.Add(bot);
                    LoadedPlayers++;
                }

                Console.WriteLine("Loaded " + LoadedPlayers + " players.");
                file.Close();
            }

            ClientVersion = Controller.GetCurrentVersion(WebService.Setting.GamePath);
            Console.WriteLine("Bot will start in few seconds...");
            System.Timers.Timer eTimer = new System.Timers.Timer
            {
                AutoReset = false,
                Interval = 3000
            };
            eTimer.Elapsed += InitializeBotting;
            eTimer.Start();
        }

        private static void InitializeBotting(object sender, ElapsedEventArgs args)
        {
            if (WebService.Players == null)
            {
                Console.WriteLine("No players have been loaded. Please check accounts.txt");
                Thread.Sleep(7000);
                Environment.Exit(0);
            }

            if (WebService.Setting == null)
            {
                Console.WriteLine("Failed loading settings from file.");
                Thread.Sleep(7000);
                Environment.Exit(0);
            }

            Console.Clear();
            Status("Starting bots.." + Environment.NewLine, "Console");

            foreach (var account in WebService.Players)
            {
                new Engine(account);
                Thread.Sleep(1111);
            }
        }

        public static string Time
        {
            get
            {
                DateTime Date = DateTime.Now;
                var output = "[" + Date.ToString("HH:mm:ss") + "] ";
                return output;
            }
        }

        public static void Status(string text, string player)
        {
            if (Configuration.IsMySQLEnabled)
            {
                WebService.ConsoleStatus(text, player);
                return;
            }

            var Spacing = GetSpacing(player);
            Console.WriteLine(Time + Spacing + text);
            Thread.Sleep(250);
        }

        public static string GetSpacing(string player)
        {
            string result = "[" + player + "] ";
            int difference = player.Length;

            var max = WebService.Players.OrderByDescending(s => s.Account.Length).FirstOrDefault();
            if (max != null) difference = max.Account.Length - player.Length;

            for (int o = 1; o <= difference; o++)
                result += " ";
            return result;
        }

    }

}
