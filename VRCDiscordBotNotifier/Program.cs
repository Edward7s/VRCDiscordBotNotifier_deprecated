using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System.Text;
using VRCDiscordBotNotifier.Utils;

namespace VRCDiscordBotNotifier
{
    internal class Program
    {
        private static void ClearMem()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Thread.Sleep(2000);
            ClearMem();
        }
        static void Main(string[] args)
        {
            Console.Title = "VRCDiscordBot";
            Task.Run(() => ClearMem());
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Initializing Config, This might take a bit depending on your enthernet speed and how much vrc friends you have.");
            new Config();
            Console.WriteLine("Initializing Bot");
            new BotSetup();
            Console.ReadLine();
        }
      
    }
}