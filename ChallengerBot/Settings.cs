using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ChallengerBot
{
    public static class ChallengerConfig
    {
        private const string SettingFile = "MySQLSettings.ini";

        public static string MHost;
        public static string MUser;
        public static string MPass;
        public static string MData;

        public static void Initialize()
        {
            if (!File.Exists(SettingFile))
            {
                var MySQLConfig = new JObject(
                    new JProperty("MySQL_host", "localhost"),
                    new JProperty("MySQL_user", "root"),
                    new JProperty("MySQL_password", "password"),
                    new JProperty("MySQL_database", "challenger"));

                using (StreamWriter file = File.CreateText(SettingFile))
                using (JsonTextWriter writer = new JsonTextWriter(file))
                {
                    writer.Formatting = Formatting.Indented;
                    MySQLConfig.WriteTo(writer);
                }

                Initialize();
            }
            else
            {
                using (StreamReader reader = File.OpenText(SettingFile))
                {
                    JObject settings = (JObject)JToken.ReadFrom(new JsonTextReader(reader));
                    MHost = (string)settings["MySQL_host"];
                    MUser = (string)settings["MySQL_user"];
                    MPass = (string)settings["MySQL_password"];
                    MData = (string)settings["MySQL_database"];
                    WebService.Initialize();
                }
            }
        }
    }
}