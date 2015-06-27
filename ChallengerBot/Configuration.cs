using System;
using System.IO;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PVPNetBot
{
    internal class Configuration : Client
    {
        private static int MySQL;
        private static string hostname;
        private static string username;
        private static string password;
        private static string database;

        public Configuration()
        {
            if (!File.Exists("MySQLSettings.ini"))
            {
                var MySQLConfig = new JObject(
                    new JProperty("MySQLUsage", "0"),
                    new JProperty("MySQL_host", "localhost"),
                    new JProperty("MySQL_user", "root"),
                    new JProperty("MySQL_password", "password"),
                    new JProperty("MySQL_database", "challenger"));

                using (StreamWriter file = File.CreateText("MySQLSettings.ini"))
                using (JsonTextWriter writer = new JsonTextWriter(file))
                {
                    writer.Formatting = Formatting.Indented;
                    MySQLConfig.WriteTo(writer);
                }

                new Configuration();
            }
            else
            {
                using (StreamReader reader = File.OpenText("MySQLSettings.ini"))
                {
                    try
                    {
                        JObject settings = (JObject)JToken.ReadFrom(new JsonTextReader(reader));
                        MySQL = (int)settings["MySQLUsage"];
                        hostname = (string)settings["MySQL_host"];
                        username = (string)settings["MySQL_user"];
                        password = (string)settings["MySQL_password"];
                        database = (string)settings["MySQL_database"];
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Error occured in MySQLSettings.ini; Delete this file and run console application.");
                        Thread.Sleep(3000);
                        Environment.Exit(0);
                    }
                }
            }
        }

        internal static void LoadSettings()
        {
            if (IsMySQLEnabled)
                return;

            using (StreamReader reader = File.OpenText("Settings.ini"))
            {
                try
                {
                    JObject settings = (JObject)JToken.ReadFrom(new JsonTextReader(reader));
                    WebService.Setting = new Settings
                    {
                        MaxBots = (int)settings["MaxBots"],
                        Difficulty = (string)settings["Difficulty"],
                        GamePath = (string)settings["GamePath"],
                        QueueType = (int)settings["QueueType"],
                        Region = (string)settings["Region"]
                    };
                }
                catch (Exception)
                {
                    Console.WriteLine("Error occured in Settings.ini; Delete this file and run console application.");
                    Thread.Sleep(3000);
                    Environment.Exit(0);
                }
            }
        }

        internal static string ConnectionString
        {
            get
            {
                return "SERVER=" + hostname + ";PASSWORD=" + password + ";DATABASE=" + database + ";UID=" + username + ";";
            }
        }

        internal static bool IsMySQLEnabled
        {
            get { return MySQL == 1; }
        }
    }
}