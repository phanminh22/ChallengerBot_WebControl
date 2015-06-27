using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using Newtonsoft.Json;
using PVPNetConnect;
using PVPNetConnect.RiotObjects.Platform.Catalog.Champion;
using PVPNetConnect.RiotObjects.Platform.Clientfacade.Domain;
using PVPNetConnect.RiotObjects.Platform.Game;
using PVPNetConnect.RiotObjects.Platform.Gameinvite.Contract;
using PVPNetConnect.RiotObjects.Platform.Matchmaking;
using PVPNetConnect.RiotObjects.Platform.Statistics;
using PVPNetConnect.RiotObjects.Platform.Systemstate;
using PVPNetConnect.RiotObjects.Team.Dto;

#region Systemusing System;

using Timer = System.Timers.Timer;
#endregion

namespace PVPNetBot
{
    internal class Engine
    {
        public PlayerDTO Hero = new PlayerDTO();
        public LoginDataPacket Packets = new LoginDataPacket();
        public PVPNetConnection Connections = new PVPNetConnection();
        public ChampionDTO[] HeroesArray;
        public Process LeagueProcess;

        private readonly Settings _setting = WebService.Setting;
        private Accounts Account;

        public string   AccountName;
        public string   SummonerName;
        public int      SummonerQueue;
        public double   SummonerLevel;
        public double   SummonerId;

        public bool PlayerAcceptedInvite = false;
        public bool LetPlayerCreateLobby = true;
        public bool FirstSelection = false;
        public bool FirstQueue = true;

        public Engine(Accounts playerBot)
        {
            Account = playerBot;
            Region curRegion = (Region)Enum.Parse(typeof(Region), _setting.Region);
            SummonerQueue = _setting.QueueType;
            AccountName = playerBot.Account;

            #region Callbacks
            Connections.OnError += (object sender, Error error) =>
            {
                Console.WriteLine("Error received: " + error.Message);
                return;
            };

            Connections.OnLogin += OnLogin;
            Connections.OnMessageReceived += OnMessageReceived;
            #endregion

            Connections.Connect(AccountName, playerBot.Password, curRegion, Client.ClientVersion);
        }

        private void OnLogin(object sender, string username, string ipAddress)
        {
            new Thread(async () =>
            {
                try
                {
                    Packets = await Connections.GetLoginDataPacketForUser();
                }
                catch(NotSupportedException)
                {
                    // Restarting BotClient;
                    Controller.Restart();
                }

                if (Packets.AllSummonerData == null)
                {
                    NewPlayerAccout();
                    OnLogin(sender, username, ipAddress);
                    return;
                }

                await Connections.Subscribe("bc", Packets.AllSummonerData.Summoner.AcctId);
                await Connections.Subscribe("cn", Packets.AllSummonerData.Summoner.AcctId);
                await Connections.Subscribe("gn", Packets.AllSummonerData.Summoner.AcctId);

                if (Packets.AllSummonerData.Summoner.ProfileIconId == -1)
                    SetSummonerIcon();

                SummonerLevel   = Packets.AllSummonerData.SummonerLevel.Level;
                SummonerName    = Packets.AllSummonerData.Summoner.Name;
                SummonerId      = Packets.AllSummonerData.Summoner.SumId;
                HeroesArray     = await Connections.GetAvailableChampions();
                Debug.WriteLine(JsonConvert.SerializeObject(HeroesArray.Where(ho => ho.Owned || ho.FreeToPlay)));

                WebService.SetLevel(Account.Id, (int)SummonerLevel);
                WebService.SetMoney(Account.Id, (int)Packets.IpBalance);

                if (SummonerLevel > Account.Maxlevel || Convert.ToInt32(SummonerLevel) == Account.Maxlevel)
                {
                    Client.Status("Maximum level reached!", AccountName);
                    return;
                }
                    
                Hero        = await Connections.CreatePlayer();
                OnMessageReceived(sender, new ClientBeforeStart());
                Thread.Sleep(2000);
            }).Start();
        }

        public async void OnMessageReceived(object sender, object message)
        {
            Debug.WriteLine("Calling message: " + message.GetType());

            #region Before Start
            if (message is ClientBeforeStart)
            {
                if (Packets.ReconnectInfo != null && Packets.ReconnectInfo.Game != null)
                {
                    OnMessageReceived(sender, (object)Packets.ReconnectInfo.PlayerCredentials);
                    return;
                }

                Client.Status("Successfully connected!", AccountName);
                Client.Accounts.Add(Packets);

                var playerCount = Client.Accounts.Count();
                var lastConnectedPlayer = Client.Accounts.LastOrDefault();
                if (Account.Autoboost) BuyBoost();

                if (lastConnectedPlayer == null)
                {
                    Console.WriteLine("Critical error!");
                    Controller.Restart();
                    return;
                }

                if (playerCount == _setting.MaxBots && lastConnectedPlayer.AllSummonerData.Summoner.SumId.Equals(SummonerId))
                {
                    Client.Status("Players connected! Creating lobby...", AccountName);
                    Timer createPremade = new Timer { Interval = 3000, AutoReset = false };
                    createPremade.Elapsed += (ek, eo) =>
                    {
                        createPremade.Stop();
                        OnMessageReceived(sender, (object)new CreateLobby());
                    };
                    createPremade.Start();
                    return;
                }
            }
            #endregion

            #region Creating lobby...
            if (message is CreateLobby)
            {
                if (!Controller.IsAvailable(SummonerQueue))
                {
                    Client.Status("QueueType is invalid or it is not supported!", AccountName);
                    return;
                }

                GameQueueConfig Game = new GameQueueConfig();
                Game.Id = SummonerQueue;

                if (_setting.Difficulty == "EASY" || _setting.Difficulty == "MEDIUM")
                {
                    Client.Lobby = await Connections.createArrangedBotTeamLobby(Game.Id, _setting.Difficulty);
                }
                else Client.Lobby = await Connections.createArrangedTeamLobby(Game.Id);

                PlayerAcceptedInvite = true;
                Client.Status("Lobby created. Inviting players...", AccountName);

                foreach (var bot in Client.Accounts)
                {
                    if ((int)bot.AllSummonerData.Summoner.SumId != (int)SummonerId)
                        await Connections.Invite(bot.AllSummonerData.Summoner.SumId);
                }
            }
            #endregion

            #region Invite requested
            if (message is InvitationRequest)
            {
                var invitation = message as InvitationRequest;

                if (invitation.InvitationId == Client.Lobby.InvitationID && PlayerAcceptedInvite == false)
                {
                    Client.Lobby = await Connections.AcceptLobby(invitation.InvitationId);
                    PlayerAcceptedInvite = true;
                    Client.Status("Invitation accepted.", AccountName);
                    return;
                }
            }
            #endregion

            #region Lobby status
            if (message is LobbyStatus)
            {
                #region Ignore pls
                List<string> errors = new List<string>();
                if (Client.Lobby == null)
                    errors.Add("NO!");
                if (SummonerName != Client.Lobby.Owner.SummonerName)
                    errors.Add("Trying to access LobbyStatus not as owner.");
                if (Client.LobbyStatusWaiting)
                    errors.Add("Currently waiting for all players.");
                
                if (errors.Count > 0)
                {
                    Debug.WriteLine("-----------------------------");
                    Debug.WriteLine("LobbyStatus was terminated due following errors:");
                    foreach (var msg in errors)
                    {
                        Debug.WriteLine("        " + msg);
                    }
                    Debug.WriteLine("-----------------------------");
                    return;
                } 
                #endregion

                if (Client.Lobby.Members.Count < _setting.MaxBots && !Client.LobbyStatusWaiting)
                {
                    Client.LobbyStatusWaiting = true;
                    while (Client.Lobby.Members.Count < _setting.MaxBots)
                        Thread.Sleep(100);
                }
  
                var lobbyInfo = Client.Lobby;
                Client.Status("Players are ready to start the game!", AccountName);
                
                #region Queue
                Client.LobbyGame.QueueIds = new Int32[1] { (int)SummonerQueue };
                Client.LobbyGame.InvitationId = lobbyInfo.InvitationID;
                Client.LobbyGame.Team = lobbyInfo.Members.Select(stats => Convert.ToInt32(stats.SummonerId)).ToList();
                Client.LobbyGame.BotDifficulty = _setting.Difficulty;
                #endregion

                OnMessageReceived(sender, await Connections.AttachTeamToQueue(Client.LobbyGame));
                Client.Status("Game search initialized!", AccountName);
                return;
            }
            #endregion

            #region Game state
            if (message is GameDTO)
            {
                
                GameDTO game = message as GameDTO;
                switch (game.GameState)
                {
                    case "CHAMP_SELECT":
                        if (FirstSelection)
                            break;

                        FirstSelection = true;
                        Client.Status("Champion select in.", AccountName);
                        await Connections.SetClientReceivedGameMessage(game.Id, "CHAMP_SELECT_CLIENT");

                        if (SummonerQueue != 65)
                        {
                            var hArray = HeroesArray.Shuffle();
                            await Connections.SelectChampion(hArray.First(hr => hr.FreeToPlay || hr.Owned || !hr.OwnedByYourTeam).ChampionId);
                            await Connections.ChampionSelectCompleted();
                        }

                        break;
                    case "POST_CHAMP_SELECT":
                        FirstQueue = true;
                        //Client.Status("Post champion select.", AccountName);
                        break;
                    case "PRE_CHAMP_SELECT":
                        break;
                    case "GAME_START_CLIENT":
                        Client.Status("Lauching League of Legends.", AccountName);
                        break;
                    case "GameClientConnectedToServer":
                        break;
                    case "IN_QUEUE":
                        Client.Status("Waiting for game.", AccountName);
                        break;
                    case "TERMINATED":
                        FirstQueue = true;
                        PlayerAcceptedInvite = false;
                        Client.Status("Re-entering queue.", AccountName);
                        break;
                    case "JOINING_CHAMP_SELECT":
                        if (FirstQueue)
                        {
                            Client.Status("Game accepted!", AccountName);
                            FirstQueue = false;
                            FirstSelection = false;
                            await Connections.AcceptPoppedGame(true);
                            break;
                        }
                        break;
                    case "LEAVER_BUSTED":
                        Client.Status("Leave Busted!", AccountName);
                        break;
                }
            }
            #endregion

            #region Starting game...
            if (message is PlayerCredentialsDto)
            {
                string gameLocation = Controller.GameClientLocation(_setting.GamePath);
                PlayerCredentialsDto credentials = message as PlayerCredentialsDto;
                ProcessStartInfo startInfo = new ProcessStartInfo();

                startInfo.CreateNoWindow = false;
                startInfo.WorkingDirectory = gameLocation;
                startInfo.FileName = "League of Legends.exe";
                startInfo.Arguments = "\"8394\" \"LoLLauncher.exe\" \"\" \"" + credentials.ServerIp + " " +
                                      credentials.ServerPort + " " + credentials.EncryptionKey + " " + credentials.SummonerId + "\"";
                Client.Status("Launching League of Legends", AccountName);
                


                new Thread((ThreadStart)(() =>
                {
                    while (Client.ClientDelay)
                        Thread.Sleep(100);

                    Client.ClientDelay = true;
                    LeagueProcess = Process.Start(startInfo);
                    LeagueProcess.Exited += LeagueProcess_Exited;
                    while (LeagueProcess.MainWindowHandle == IntPtr.Zero) ;
                    LeagueProcess.PriorityClass = ProcessPriorityClass.Idle;
                    LeagueProcess.EnableRaisingEvents = true;
                    Timer clientDelay = new Timer { AutoReset = false, Interval = Client.Delay };
                    clientDelay.Elapsed += (o, args) =>
                    {
                        Client.ClientDelay = false;
                    };
                    clientDelay.Start();
                    
                })).Start();
            }

            if (message is EndOfGameStats)
            {
                Client.Accounts.Clear();
                Client.LobbyStatusWaiting = false;

                // Process kill
                LeagueProcess.Exited -= LeagueProcess_Exited;

                while (LeagueProcess.Responding)
                {
                    LeagueProcess.Kill();
                    Thread.Sleep(500);
                }
                

                var msg = message as EndOfGameStats;
                Packets = await Connections.GetLoginDataPacketForUser();
                WebService.SetLevel(Account.Id, (int)Packets.AllSummonerData.SummonerLevel.Level);
                WebService.SetMoney(Account.Id, (int)Packets.IpBalance);


                if (SummonerLevel < Packets.AllSummonerData.SummonerLevel.Level)
                    Client.Status("Level up! " + Packets.AllSummonerData.SummonerLevel.Level, AccountName);

                // Player level limit
                if (MaxLevelReached((int)Packets.AllSummonerData.SummonerLevel.Level))
                {
                    // This player will not be added to lobby!
                    Client.Status("Maximum level reached!", AccountName);
                    return;
                }
                else if (Account.Autoboost) BuyBoost();
                
                OnMessageReceived(sender, new ClientBeforeStart());
                return;
            }
            #endregion

            #region Searching for match
            if (message is SearchingForMatchNotification)
            {
                var result = message as SearchingForMatchNotification;

                if (result.PlayerJoinFailures != null)
                {
                    List<Tuple<string, int>> summoners = new List<Tuple<string, int>>();
                    string accessToken = null;
                    bool penalty = false;
                    

                    foreach (var item in result.PlayerJoinFailures)
                    {
                        var x = new QueueDodger(item as TypedObject);
                        if (x.ReasonFailed == "LEAVER_BUSTED")
                        {
                            accessToken = x.AccessToken;
                            summoners.Add(new Tuple<string, int>(x.Summoner.Name, x.LeaverPenaltyMillisRemaining));
                            penalty = true;

                        }
                        else
                        {
                            Client.Status("Reason: " + x.ReasonFailed, AccountName);
                            
                            return;
                        }
                    }

                    if (penalty)
                    {
                        Debug.WriteLine("Penalty timer.");
                        var timeWait = summoners.OrderByDescending(s => s.Item2).FirstOrDefault().Item2;
                        var time = TimeSpan.FromMilliseconds(timeWait);
                        var players = string.Join(",", summoners.Select(s => s.Item1).ToArray());
                        Debug.WriteLine("Time wait" + timeWait + "ms." + "Counted summoners: " + summoners.Count + "; Summoners: " + players);
                        Client.Status("Waiting " + time.Minutes + " mins to be able to join queue", AccountName);
                        Thread.Sleep(timeWait + 2999);

                        if (SummonerName == Client.Lobby.Owner.SummonerName)
                        {
                            OnMessageReceived(sender, await Connections.AttachToQueue(Client.LobbyGame, accessToken));
                        }
                    }
                }
            }
            #endregion

        }

        private bool MaxLevelReached(int nextLevel)
        {
            return nextLevel >= Account.Maxlevel;
        }

        private async void BuyBoost()
        {
            if (Packets.RpBalance > 260)
            {
                var url = await Connections.GetStoreUrl();
                var regEx = new Regex(@"(https://).*?(?=com)\w+");
                var Second = regEx.Matches(url);
                string uri = null;
                foreach (Match URRL in Second)
                {
                    uri = URRL.Value;
                }

                var storeUrl = uri + "/store/tabs/view/boosts/1";
                var purchaseUrl = uri + "/store/purchase/item";

                HttpClient httpClient = new HttpClient();
                await httpClient.GetStringAsync(url);
                await httpClient.GetStringAsync(storeUrl);
                
                List<KeyValuePair<string, string>> storeItemList = new List<KeyValuePair<string, string>>();
                storeItemList.Add(new KeyValuePair<string, string>("item_id", "boosts_2"));
                storeItemList.Add(new KeyValuePair<string, string>("currency_type", "rp"));
                storeItemList.Add(new KeyValuePair<string, string>("quantity", "1"));
                storeItemList.Add(new KeyValuePair<string, string>("rp", "260"));
                storeItemList.Add(new KeyValuePair<string, string>("ip", "null"));
                storeItemList.Add(new KeyValuePair<string, string>("duration_type", "PURCHASED"));
                storeItemList.Add(new KeyValuePair<string, string>("duration", "3"));
                HttpContent httpContent = new FormUrlEncodedContent(storeItemList);
                await httpClient.PostAsync(purchaseUrl, httpContent);
                Client.Status("Bought XP boost!", AccountName);
                httpClient.Dispose();
            }
        }

        private async void LeagueProcess_Exited(object sender, EventArgs e)
        {
            Client.Status("Restart League of Legends.", AccountName);
            Packets = await Connections.GetLoginDataPacketForUser();
            if (Packets.ReconnectInfo != null && Packets.ReconnectInfo.Game != null)
            {
                OnMessageReceived(sender, (object)Packets.ReconnectInfo.PlayerCredentials);
            }                
        }

        private async void NewPlayerAccout()
        {
            
            var summonerName = "SN" + Guid.NewGuid().ToString("N").Substring(0, 12).ToUpper();
            await Connections.CreateDefaultSummoner(summonerName);
            Client.Status("Created summoner: " + summonerName, AccountName);
        }

        private async void SetSummonerIcon()
        {
            await Connections.UpdateProfileIconId(12);
            return;
        }
    }

    public static class EnumerableExtensions
    {
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            return source.Shuffle(new Random());
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, Random rng)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (rng == null) throw new ArgumentNullException("rng");

            return source.ShuffleIterator(rng);
        }

        private static IEnumerable<T> ShuffleIterator<T>(
            this IEnumerable<T> source, Random rng)
        {
            List<T> buffer = source.ToList();
            for (int i = 0; i < buffer.Count; i++)
            {
                int j = rng.Next(i, buffer.Count);
                yield return buffer[j];

                buffer[j] = buffer[i];
            }
        }
    }
}