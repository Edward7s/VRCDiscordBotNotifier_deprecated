using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System.Diagnostics;
using System.Text;
using VRCDiscordBotNotifier.Utils;
using static System.Net.Mime.MediaTypeNames;

namespace VRCDiscordBotNotifier
{
    internal class Program
    {
  
        static void Main(string[] args)
        {
            Console.Title = "[VRCDiscordBot] By XOXO <3";
            ConsoleManager.Write("Initializng Program...");
            new VersionChecker();
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                 ConsoleManager.Write(e.ToString(), ConsoleColor.Red);

            ConsoleManager.Write("Running Force Memory Collection...");
            Task.Run(() =>
            {
                while (true)
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    Thread.Sleep(2000);
                }            
            });
             ConsoleManager.Write("Initializing Config...");
            bool config = new Config().Initialize();
            while (config != true)
            {
                return;
            }
             ConsoleManager.Write("Initializing Bot...");
            new BotSetup();
            new Favorites();
            Console.ReadLine();
        }
    }
}