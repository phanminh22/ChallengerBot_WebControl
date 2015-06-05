using System.Text.RegularExpressions;
using System;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

#region Systemusing System;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Net.Http;
using Timer = System.Timers.Timer;
#endregion

using PVPNetConnect;
using PVPNetConnect.RiotObjects.Platform.Catalog.Champion;
using PVPNetConnect.RiotObjects.Platform.Clientfacade.Domain;
using PVPNetConnect.RiotObjects.Platform.Game;
using PVPNetConnect.RiotObjects.Platform.Matchmaking;
using PVPNetConnect.RiotObjects.Platform.Statistics;
using PVPNetConnect.RiotObjects.Platform.Gameinvite.Contract;
using PVPNetConnect.RiotObjects.Team.Dto;
using PVPNetConnect.RiotObjects.Platform.Systemstate;

namespace ChallengerBot
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
                WebService.Status("Error received: " + error.Message, AccountName);
                return;
            };

            Connections.OnLogin += new PVPNetConnection.OnLoginHandler(OnLogin);
            Connections.OnMessageReceived += new PVPNetConnection.OnMessageReceivedHandler(OnMessageReceived);
            #endregion

            Connections.Connect(AccountName, playerBot.Password, curRegion, Core.ClientVersion);
         
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
                    WebService.Status("Maximum level reached!", AccountName);
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

                WebService.Status("Successfully connected!", AccountName);
                Core.Accounts.Add(Packets);

                var playerCount = Core.Accounts.Count();
                var lastConnectedPlayer = Core.Accounts.LastOrDefault();
                if (Account.Autoboost) BuyBoost();

                if (lastConnectedPlayer == null)
                {
                    Console.WriteLine("Critical error!");
                    Controller.Restart();
                    return;
                }

                if (playerCount == _setting.MaxBots && lastConnectedPlayer.AllSummonerData.Summoner.SumId.Equals(SummonerId))
                {
                    WebService.Status("Players connected! Creating lobby...", AccountName);
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
                    WebService.Status("QueueType is invalid or it is not supported!", AccountName);
                    return;
                }

                GameQueueConfig Game = new GameQueueConfig();
                Game.Id = SummonerQueue;

                if (_setting.Difficulty == "EASY" || _setting.Difficulty == "MEDIUM")
                {
                    Core.Lobby = await Connections.createArrangedBotTeamLobby(Game.Id, _setting.Difficulty);
                }
                else Core.Lobby = await Connections.createArrangedTeamLobby(Game.Id);

                PlayerAcceptedInvite = true;
                WebService.Status("Lobby created. Inviting players...", AccountName);

                foreach (var bot in Core.Accounts)
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

                if (invitation.InvitationId == Core.Lobby.InvitationID && PlayerAcceptedInvite == false)
                {
                    Core.Lobby = await Connections.AcceptLobby(invitation.InvitationId);
                    PlayerAcceptedInvite = true;
                    WebService.Status("Invitation accepted.", AccountName);
                    return;
                }
            }
            #endregion

            #region Lobby status
            if (message is LobbyStatus)
            {
                #region Ignore pls
                List<string> errors = new List<string>();
                if (Core.Lobby == null)
                    errors.Add("NO!");
                if (SummonerName != Core.Lobby.Owner.SummonerName)
                    errors.Add("Trying to access LobbyStatus not as owner.");
                if (Core.LobbyStatusWaiting)
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

                if (Core.Lobby.Members.Count < _setting.MaxBots && !Core.LobbyStatusWaiting)
                {
                    Core.LobbyStatusWaiting = true;
                    while (Core.Lobby.Members.Count < _setting.MaxBots)
                        Thread.Sleep(100);
                }
  
                var lobbyInfo = Core.Lobby;
                WebService.Status("Players are ready to start the game!", AccountName);
                
                #region Queue
                Core.LobbyGame.QueueIds = new Int32[1] { (int)SummonerQueue };
                Core.LobbyGame.InvitationId = lobbyInfo.InvitationID;
                Core.LobbyGame.Team = lobbyInfo.Members.Select(stats => Convert.ToInt32(stats.SummonerId)).ToList();
                Core.LobbyGame.BotDifficulty = _setting.Difficulty;
                #endregion

                OnMessageReceived(sender, await Connections.AttachTeamToQueue(Core.LobbyGame));
                WebService.Status("Game search initialized!", AccountName);
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
                        WebService.Status("Champion select in.", AccountName);
                        await Connections.SetClientReceivedGameMessage(game.Id, "CHAMP_SELECT_CLIENT");

                        if (Convert.ToInt32(game.Id) != 65)
                        {
                            var hArray = HeroesArray.Shuffle();
                            await Connections.SelectChampion(hArray.First(hr => hr.FreeToPlay || hr.Owned || !hr.OwnedByYourTeam).ChampionId);
                            await Connections.ChampionSelectCompleted();
                        }

                        break;
                    case "POST_CHAMP_SELECT":
                        FirstQueue = true;
                        //WebService.Status("Post champion select.", AccountName);
                        break;
                    case "PRE_CHAMP_SELECT":
                        break;
                    case "GAME_START_CLIENT":
                        WebService.Status("Lauching League of Legends.", AccountName);
                        break;
                    case "GameClientConnectedToServer":
                        break;
                    case "IN_QUEUE":
                        WebService.Status("Waiting for game.", AccountName);
                        break;
                    case "TERMINATED":
                        FirstQueue = true;
                        PlayerAcceptedInvite = false;
                        WebService.Status("Re-entering queue.", AccountName);
                        break;
                    case "JOINING_CHAMP_SELECT":
                        if (FirstQueue)
                        {
                            WebService.Status("Game accepted!", AccountName);
                            FirstQueue = false;
                            FirstSelection = false;
                            await Connections.AcceptPoppedGame(true);
                            break;
                        }
                        break;
                    case "LEAVER_BUSTED":
                        WebService.Status("Leave Busted!", AccountName);
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
                WebService.Status("Launching League of Legends", AccountName);
                


                new Thread((ThreadStart)(() =>
                {
                    while (Core.ClientDelay)
                        Thread.Sleep(100);

                    Core.ClientDelay = true;
                    LeagueProcess = Process.Start(startInfo);
                    LeagueProcess.Exited += LeagueProcess_Exited;
                    while (LeagueProcess.MainWindowHandle == IntPtr.Zero) ;
                    LeagueProcess.PriorityClass = ProcessPriorityClass.Idle;
                    LeagueProcess.EnableRaisingEvents = true;
                    Timer clientDelay = new Timer { AutoReset = false, Interval = Core.Delay };
                    clientDelay.Elapsed += (o, args) =>
                    {
                        Core.ClientDelay = false;
                    };
                    clientDelay.Start();
                    
                })).Start();
            }

            if (message is EndOfGameStats)
            {
                Core.Accounts.Clear();
                Core.LobbyStatusWaiting = false;

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
                    WebService.Status("Level up! " + Packets.AllSummonerData.SummonerLevel.Level, AccountName);

                // Player level limit
                if (MaxLevelReached((int)Packets.AllSummonerData.SummonerLevel.Level))
                {
                    // This player will not be added to lobby!
                    WebService.Status("Maximum level reached!", AccountName);
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
                            WebService.Status("Reason: " + x.ReasonFailed, AccountName);
                            
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
                        WebService.Status("Waiting " + time.Minutes + " to be able to join queue", AccountName);
                        Thread.Sleep(timeWait + 2999);

                        if (SummonerName == Core.Lobby.Owner.SummonerName)
                        {
                            OnMessageReceived(sender, await Connections.AttachToQueue(Core.LobbyGame, accessToken));
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
                WebService.Status("Bought XP boost!", AccountName);
                httpClient.Dispose();
            }
        }

        private async void LeagueProcess_Exited(object sender, EventArgs e)
        {
            WebService.Status("Restart League of Legends.", AccountName);
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
            WebService.Status("Created summoner: " + summonerName, AccountName);
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
