using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using ChallengerBot.SWF;
using ChallengerBot.SWF.SWFTypes;

namespace ChallengerBot
{
    public static class Controller
    {
        public static string GetCurrentVersion(string location)
        {
            location += "RADS\\projects\\lol_air_client\\releases\\";
            ASCIIEncoding encoding = new ASCIIEncoding();
            DirectoryInfo dInfo = new DirectoryInfo(location);
            DirectoryInfo[] subdirs = null;
            try
            {
                subdirs = dInfo.GetDirectories();
            }
            catch { return "0.0.0.0"; }
            string latestVersion = "0.0.1";
            foreach (DirectoryInfo info in subdirs)
            {
                latestVersion = info.Name;
            }

            // https://github.com/eddy5641/LegendaryClient/blob/b6cbb58d3d2f5153f8cb1b693275c15064d6beac/LegendaryClient/Windows/LoginPage.xaml.cs#L232-L250
            string CommonLib = Path.Combine(location, latestVersion, "deploy\\lib\\ClientLibCommon.dat");
             var reader = new SWFReader(CommonLib);

            foreach (var secondSplit in from abcTag in reader.Tags.OfType<DoABC>()
                                        where abcTag.Name.Contains("riotgames/platform/gameclient/application/Version")
                                        select Encoding.Default.GetString(abcTag.ABCData)
                                            into str
                                            select str.Split((char)6)
                                                into firstSplit

                                                select firstSplit[0].Split((char)18))

            try
            {
                return secondSplit[1];
            }
            catch
            {
                var thirdSplit = secondSplit[0].Split((char)19);
                return thirdSplit[1];
            }

            return "0";
        }

        public static string GameClientLocation(string gamePath)
        {
            gamePath += "RADS\\solutions\\lol_game_client_sln\\releases\\";
            ASCIIEncoding encoding = new ASCIIEncoding();
            DirectoryInfo dInfo = new DirectoryInfo(gamePath);
            DirectoryInfo[] subdirs = null;
            try
            {
                subdirs = dInfo.GetDirectories();
            }
            catch { return "0.0.0.0"; }
            string latestVersion = "0.0.1";
            foreach (DirectoryInfo info in subdirs)
            {
                latestVersion = info.Name;
            }

            return Path.Combine(gamePath, latestVersion, "deploy\\");
        }

        public static void Restart()
        {
            Process.Start("ChallengerBot.exe");
            Thread.Sleep(1000);
            Environment.Exit(0);
            return;
        }

        public static bool IsAvailable(int queueId)
        {
            var viableQueues = new List<int>(){32, 33, 65, 25, 52};
            return viableQueues.Contains(queueId);
        }
    }
}
