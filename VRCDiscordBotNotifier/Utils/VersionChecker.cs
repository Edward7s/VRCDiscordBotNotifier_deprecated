using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Diagnostics;
namespace VRCDiscordBotNotifier.Utils
{
    internal class VersionChecker
    {
        public static string Version { get; } = "V2.1";
        public VersionChecker()
        {
            Console.WriteLine("Checking For Any Updates...");
            if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\UpdatorNocturnal"))
                Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\UpdatorNocturnal");

            if (!File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\UpdatorNocturnal\\AutoUpdator.exe"))
                using (WebClient wc = new WebClient())
                    wc.DownloadFile("https://github.com/Edward7s/AutoUpdatorForDiscordBot/releases/download/InitialVersion/AutoUpdator.exe", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\UpdatorNocturnal\\AutoUpdator.exe");

            Process updator = new Process();
            updator.StartInfo.Arguments = Version;  
            updator.StartInfo.FileName = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\UpdatorNocturnal\\AutoUpdator.exe";
            updator.Start();
        }

    }
}
