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
        private static void ClearMem()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Thread.Sleep(2000);
            ClearMem();
        }
        static void Main(string[] args)
        {
         /*   AppDomain.CurrentDomain.FirstChanceException += (s, e) =>
            {
                Console.WriteLine("---------------------Exception---------------------");
                Console.WriteLine(e.Exception.ToString());
                Console.WriteLine("---------------------------------------------------");
            };*/
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                Console.WriteLine("-----------------UnhandledException-----------------");
                Console.WriteLine(e.ToString());
                Console.WriteLine("----------------------------------------------------");
            }; 


            // Task.Run(() => new Utils.FilesWatcher());
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