using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using VRCDiscordBotNotifier.Utils;
using System.Diagnostics;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.ComponentModel;
using Microsoft.Win32;

namespace VRCDiscordBotNotifier
{
    internal class Config
    {
        public static Config Instance { get; set; }
        public Json.Config JsonConfig { get; set; }
        private string _fileName { get; } = "\\VRCDiscordBotManager.json";
        public string FriendsFile { get; } = "\\FriendsListBot.json";
        /*     public string FriendsInfo { get; } = "\\FriendsInfoBot.json";
             public string OnlineFriends { get; } = "\\OnlineFriendsBot.json";*/
        public string Notifications { get; } = "\\NotificationsBot.json";

        public void SaveConfig() =>
            File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + _fileName, JsonConvert.SerializeObject(JsonConfig));


        public List<Json.User> UsersArr { get; } = new List<Json.User>();
        private PropertyInfo[] _props { get; set; }
        public Config()
        {
            Instance = this;
            if (!File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + _fileName))
            {
                File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + _fileName, JsonConvert.SerializeObject(new Json.Config()
                {
                    AuthCookie = String.Empty,
                    UserId = "AutoChange",
                    BotToken = String.Empty,
                    DiscordServerId = String.Empty,
                    DmUsersId = new string[] { },
                    DmNewNotifications = false,
                    DmFriendEvent = false,
                    FriendsActivity = false,
                    FirstTime = true,
                }));
            }
            JsonConfig = JsonConvert.DeserializeObject<Json.Config>(File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + _fileName));

            _props = typeof(Json.Config).GetProperties();
            for (int i = 0; i < _props.Length; i++)
            {
                if (_props[i].GetValue(JsonConfig) != String.Empty) continue;
                Register();
                break;
            }
            Thread.Sleep(100);
            new VRCWebRequest();
            if (!File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + FriendsFile))
            {
                string FriendsList = string.Empty;
                var jobject = JObject.FromObject(JObject.Parse(VRCWebRequest.Instance.SendVRCWebReq(VRCWebRequest.RequestType.Get, VRCInfo.VRCApiLink + VRCInfo.EndPoints.LocalUser)));
                for (int i = 0; i < jobject["friends"].ToArray().Length; i++)
                    FriendsList += jobject["friends"][i].ToString() + "\n";
                File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + FriendsFile, FriendsList);
                jobject = null;
                FriendsList = null;
            }
            if (!File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Notifications))
                File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Notifications, VRCWebRequest.Instance.SendVRCWebReq(VRCWebRequest.RequestType.Get,VRCInfo.VRCApiLink + VRCInfo.EndPoints.Notifications));


            RegistryKey startup = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            if (startup.GetValue("VRCDiscordBotNotifier") == null)
                startup.SetValue("VRCDiscordBotNotifier", Directory.GetCurrentDirectory() + "\\VRCDiscordBotNotifier.exe");



            /*   JObject UserInfoObj = null;
               var jobject2 = JObject.FromObject(JObject.Parse(VRCWebRequest.Instance.SendVRCWebReq(VRCWebRequest.RequestType.Get, VRCInfo.VRCApiLink + VRCInfo.EndPoints.LocalUser)))["friends"];
               for (int i = 0; i < jobject2.ToArray().Length; i++)
               {
                   UserInfoObj = JObject.FromObject(JObject.Parse(VRCWebRequest.Instance.SendVRCWebReq(VRCWebRequest.RequestType.Get, VRCInfo.VRCApiLink + VRCInfo.EndPoints.UserEndPoint + jobject2[i].ToString())));
                   UsersArr.Add(new Json.User()
                   {
                       Id = jobject2[i].ToString(),
                       Name = UserInfoObj["displayName"].ToString(),
                       Status = UserInfoObj["statusDescription"].ToString(),
                       State = UserInfoObj["status"].ToString(),                      
                   }) ;
               }
               File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + FriendsInfo, JsonConvert.SerializeObject(UsersArr));
               UserInfoObj = null;
           jobject2 = null;*/


            /*  if (!File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + OnlineFriends))
              {
                  string FriendsList = string.Empty;
                  var jobject = JObject.FromObject(JObject.Parse(VRCWebRequest.Instance.SendVRCWebReq(VRCWebRequest.RequestType.Get, VRCInfo.VRCApiLink + VRCInfo.EndPoints.LocalUser)));
                  for (int i = 0; i < jobject["onlineFriends"].ToArray().Length; i++)
                      FriendsList += jobject["onlineFriends"][i].ToString() + "\n";
                  File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + OnlineFriends, FriendsList);
                  jobject = null;
                  FriendsList = null;
              }*/

        }
        private void Register()
        {
            Console.Clear();
            JsonConfig = JsonConvert.DeserializeObject<Json.Config>(File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + _fileName));
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Please Enter Your Discord Bot Token.");
            JsonConfig.BotToken = Console.ReadLine().Trim();
            Console.WriteLine("Please Enter Your Discord Server Id.");
            JsonConfig.DiscordServerId = Console.ReadLine().Trim();
            Console.WriteLine("Please Enter Your VRChat AuthCookie.");
            JsonConfig.AuthCookie = Console.ReadLine().Trim();
            File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + _fileName, JsonConvert.SerializeObject(JsonConfig));
            CheckVRCUser();
        }

        private void CheckVRCUser()
        {
            Console.Clear();
            string code = VRCWebRequest.TestReqest(JsonConfig.AuthCookie);
            Console.WriteLine(code);
            if (code == "401" || code == "404")
            {
                Console.WriteLine("+ - + - + - + - + - + - + - + - + - + - + - + - + - + - + - + - + - + - + - + - +");
                Console.WriteLine("+  Your VRC Information its not right make sure you enter the right infomation. +");
                Console.WriteLine("+ - + - + - + - + - + - + - + - + - + - + - + - + - + - + - + - + - + - + - + - +");
                Console.WriteLine();
                Console.WriteLine("Please Enter Your VRChat AuthCookie.");
                JsonConfig.AuthCookie = Console.ReadLine().Trim();
                File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + _fileName, JsonConvert.SerializeObject(JsonConfig));
                CheckVRCUser();
                return;
            }
            JsonConfig.UserId = JObject.FromObject(JObject.Parse(VRCWebRequest.Instance.SendVRCWebReq(VRCWebRequest.RequestType.Get, VRCInfo.VRCApiLink + VRCInfo.EndPoints.LocalUser)))["id"].ToString();
            File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + _fileName, JsonConvert.SerializeObject(JsonConfig));
        }
    }
}
