using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRCDiscordBotNotifier.Utils
{
    internal class Filemanager
    {
        private static StreamReader? s_streamReader { get; set; }
        private static StreamWriter? s_streamWriter { get; set; }

        public static string ReadFile(string path)
        {
            try
            {
                s_streamReader = new StreamReader(path);
                return s_streamReader.ReadToEnd();
            }
            finally
            {
                if (s_streamReader != null)
                {
                    s_streamReader.Close();
                    s_streamReader = null;
                }          
            }
        }

        public static void WriteFile(string path, string content)
        {
            try
            {
                s_streamWriter = new StreamWriter(path);
                s_streamWriter.Write(content);
            }
            finally
            {
                if (s_streamWriter != null)
                {
                    s_streamWriter.Close();
                    s_streamWriter = null;
                }
            }
        }
    }
}
