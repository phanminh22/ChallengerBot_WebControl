using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using MySql.Data;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using PVPNetConnect;
using PVPNetConnect.RiotObjects.Platform.Catalog.Champion;
using PVPNetConnect.RiotObjects.Platform.Clientfacade.Domain;

namespace ChallengerBot
{
    public class Accounts
    {
        public int Id { get; set; }
        public string Account { get; set; }
        public string Password { get; set; }
        public string Champion { get; set; }
        public int Maxlevel { get; set; }
        public bool Autoboost { get; set; }
    }

    public class Settings
    {
        public int MaxBots { get; set; }
        public string Request { get; set; }

        public string Region { get; set; }
        public string GamePath { get; set; }
        public string Difficulty { get; set; }
        public int QueueType { get; set; }
    }

    public static class WebService
    {
        public static MySqlConnection SQL = null;

        private static string Hostname = ChallengerConfig.MHost;
        private static string Username = ChallengerConfig.MUser;
        private static string Password = ChallengerConfig.MPass;
        private static string Database = ChallengerConfig.MData;

        public static List<Accounts> Players = new List<Accounts>();
        public static Settings Setting = new Settings();

        public static string GetConnectionString()
        {
            return "SERVER=" + Hostname + ";PASSWORD=" + Password + ";DATABASE=" + Database + ";UID=" + Username + ";";
        }

        public static MySqlConnection NewConnection()
        {
            var mySqlConnection = new MySqlConnection(GetConnectionString());
            mySqlConnection.Open();
            return mySqlConnection;
        }

        public static void Initialize()
        {
            try
            {
                SQL = NewConnection();
            }
            catch (MySqlException mer)
            {
                Console.WriteLine("Error: " + mer.Message);
                Debug.WriteLine("Error occured: " + mer.ToString());
                return;
            }
            finally
            {
                Console.WriteLine("MySQL connection successful!");
                SQL.Close();
            }
            
            Preload();
            LoadAccounts();
            LoadSettings();
        }

        public static void Preload()
        {
            using (var con = new MySqlConnection(GetConnectionString()))
            {
                using (var cmd = con.CreateCommand())
                {
                    con.Open();
                    var hashid = Guid.NewGuid().ToString("N");
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "UPDATE `settings` SET `hashid` = '" + hashid + "', `response` = UNIX_TIMESTAMP() WHERE `label` = 'CBot';";
                    cmd.ExecuteNonQuery();
                    con.Close();
                }
            }
        }

        public static void LoadSettings()
        {
            using (var con = new MySqlConnection(GetConnectionString()))
            {
                using (MySqlCommand cmd = con.CreateCommand())
                {
                    con.Open();
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "SELECT * FROM `settings` WHERE `label` = 'CBot' LIMIT 1;";

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        reader.Read();
                        Settings settings = new Settings
                        {
                            MaxBots = (int) reader["players"],
                            Region = (string) reader["platform"],
                            Difficulty = (string) reader["difficulty"],
                            QueueType = (int) reader["queue"], //Request
                            GamePath = (string) reader["gamepath"]
                        };

                        Setting = settings;
                        if (Setting == null)
                        {
                            Status("No settings found!", "Console");
                            Environment.Exit(0);
                        }

                        Core.ClientVersion = Controller.GetCurrentVersion(Setting.GamePath);
                        if (Core.ClientVersion.Equals("0"))
                        {
                            Status("Unable to get client version! Check your game path!", "Console");
                            Environment.Exit(0);
                        }
                    }

                    con.Close();
                }
            }
        }

        public static void LoadAccounts()
        {
            using (var con = new MySqlConnection(GetConnectionString()))
            {
                using (MySqlCommand cmd = con.CreateCommand())
                {
                    con.Open();
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "SELECT * FROM accounts WHERE level < maxlevel;";

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Accounts account = new Accounts
                            {
                                Id = (int) reader["id"],
                                Account = (string) reader["account"],
                                Password = (string) reader["password"],
                                Champion = (string) reader["champion"],
                                Maxlevel = (int) reader["maxlevel"],
                                Autoboost = Convert.ToBoolean(reader["autoboost"])
                            };

                            Players.Add(account);
                        }
                    }

                    con.Close();
                }
            }
        }

        public static void ExecuteNonQuery(string query)
        {
            using (var con = new MySqlConnection(GetConnectionString()))
            {
                using (MySqlCommand cmd = con.CreateCommand())
                {
                    con.Open();
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = query;
                    cmd.ExecuteNonQuery();
                    con.Close();
                }
            }
        }

        public static void SetLevel(int id, int level)
        {
            var cmd = "UPDATE accounts SET level = '" + level + "' WHERE id = '" + id + "'; ";
            ExecuteNonQuery(cmd);
        }

        public static void SetMoney(int id, int money)
        {
            var cmd = "UPDATE accounts SET money = '" + money + "' WHERE id = '" + id + "'; ";
            ExecuteNonQuery(cmd);
        }      

        public static void Status(string msg, string player)
        {
            var cmd = " INSERT INTO console (content, timestamp, player) VALUES ('" + msg + "', UNIX_TIMESTAMP(), '" + player + "'); ";
            ExecuteNonQuery(cmd);
        }
    }
}
