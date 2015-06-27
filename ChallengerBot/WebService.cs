using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using MySql.Data.MySqlClient;

namespace PVPNetBot
{
    public class Accounts
    {
        public int Id;
        public string Account;
        public string Password;
        public string Champion;
        public int Maxlevel;
        public bool Autoboost;
    }

    public class Settings
    {
        public int MaxBots;
        public string Request;
        public string Region;
        public string GamePath;
        public string Difficulty;
        public int QueueType;
    }

    internal class WebService : Client
    {
        public static MySqlConnection SQL;
        public static List<Accounts> Players = new List<Accounts>();
        public static Settings Setting = new Settings();
        public static bool ConnectionEntablished = false;

        public WebService()
        {
            if (!Configuration.IsMySQLEnabled)
                return;

            try
            {
                SQL = new MySqlConnection(Configuration.ConnectionString);
                SQL.Open();
            }
            catch (MySqlException mer)
            {
                Console.WriteLine("Error: " + mer.Message);
                Debug.WriteLine("Error occured: " + mer);
                return;
            }

            ConnectionEntablished = true;
            SQL.Close();

            Preload();
            LoadAccounts();
            LoadSettings();
        }

        private static void Preload()
        {
            using (var con = new MySqlConnection(Configuration.ConnectionString))
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

        private static void LoadSettings()
        {
            using (var con = new MySqlConnection(Configuration.ConnectionString))
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

                        ClientVersion = Controller.GetCurrentVersion(Setting.GamePath);
                        if (ClientVersion.Equals("0"))
                        {
                            Status("Unable to get client version! Check your game path!", "Console");
                            Environment.Exit(0);
                        }
                    }

                    con.Close();
                }
            }
        }

        private static void LoadAccounts()
        {
            using (var con = new MySqlConnection(Configuration.ConnectionString))
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
            using (var con = new MySqlConnection(Configuration.ConnectionString))
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

        public static void ConsoleStatus(string msg, string player)
        {
            var cmd = " INSERT INTO console (content, timestamp, player) VALUES ('" + msg + "', UNIX_TIMESTAMP(), '" + player + "'); ";
            ExecuteNonQuery(cmd);
        }
    }
}
