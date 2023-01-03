using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace VRCDiscordBotNotifier
{
    internal class ConsoleManager
    {
        public static void Write(object obj,ConsoleColor color = ConsoleColor.DarkGray)
        {
            var outLines = new StringBuilder("");
            for (int i = -14; i < obj.ToString().Length; i++)
                outLines.Append("─");
            Console.ForegroundColor = color;
            Console.WriteLine(outLines.ToString());
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("[Debugger] ~> ");
            Console.ForegroundColor = color;
            Console.WriteLine(obj);
            Console.WriteLine(outLines.ToString());
        }

    }
}
