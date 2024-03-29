﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using VRCDiscordBotNotifier.Utils;
using System.Diagnostics;
using System.Reflection;
using Newtonsoft.Json.Linq;
using System.ComponentModel;
using Microsoft.Win32;
using VRCDiscordBotNotifier.Utils;
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
            Filemanager.WriteFile(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + _fileName, JsonConvert.SerializeObject(JsonConfig));

        public List<Json.User> UsersArr { get; } = new List<Json.User>();
        private PropertyInfo[] _props { get; set; }
        public bool Initialize()
        {
            Instance = this;
            ConsoleManager.Write("Cheecking Main Config...");
            Extentions.CheckFileSanity(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + _fileName,
                JsonConvert.SerializeObject(new Json.Config()
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
            JsonConfig = JsonConvert.DeserializeObject<Json.Config>(Filemanager.ReadFile(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + _fileName));
            ConsoleManager.Write("Fixing Config...");
            _props = typeof(Json.Config).GetProperties();
            for (int i = 0; i < _props.Length; i++)
            {
                if (_props[i].GetValue(JsonConfig) != String.Empty) continue;
                Register();
                break;
            }
            Thread.Sleep(100);


            ConsoleManager.Write("Creating Cookies For Requests...");
            new VRCWebRequest();

            ConsoleManager.Write("Checking User Info...");
            CheckVRCUser();

            ConsoleManager.Write("Checking File Integrity...");
            for (int i = 0; i < _props.Length; i++)
            {
                if (_props[i].GetValue(JsonConfig) != null) continue;

                if (_props[i].GetType() == typeof(bool))
                {
                    _props[i].SetValue(JsonConfig, false);
                    continue;
                }
                _props[i].SetValue(JsonConfig, string.Empty);
            }

            ConsoleManager.Write("Checking Friends File...");
            if (!File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + FriendsFile) || Filemanager.ReadFile(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + FriendsFile) == string.Empty)
            {
                string FriendsList = string.Empty;
                var jobject = JObject.Parse(VRCWebRequest.Instance.SendVRCWebReq(VRCWebRequest.RequestType.Get, VRCInfo.VRCApiLink + VRCInfo.EndPoints.LocalUser));
                for (int i = 0; i < jobject["friends"].ToArray().Length; i++)
                    FriendsList += jobject["friends"][i].ToString() + "\n";
                Filemanager.WriteFile(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + FriendsFile, FriendsList);
                jobject = null;
                FriendsList = null;
            }
            ConsoleManager.Write("Checking Registry Key...");

            RegistryKey startup = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            if (startup.GetValue("VRCDiscordBotNotifier") == null)
            {
                startup.SetValue("VRCDiscordBotNotifier", Directory.GetCurrentDirectory() + "\\VRCDiscordBotNotifier.exe");

            }

            ConsoleManager.Write("Checking Notifications File...");
            Extentions.CheckFileSanity(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Notifications, VRCWebRequest.Instance.SendVRCWebReq(VRCWebRequest.RequestType.Get, VRCInfo.VRCApiLink + VRCInfo.EndPoints.Notifications));
            ConsoleManager.Write("Config Initialized.");
            return true;
        }
        private void Register()
        {
            Console.Clear();
            JsonConfig = JsonConvert.DeserializeObject<Json.Config>(Filemanager.ReadFile(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + _fileName));
            ConsoleManager.Write("Please Enter Your Discord Bot Token.", ConsoleColor.Yellow);
            JsonConfig.BotToken = Console.ReadLine().Trim();
            ConsoleManager.Write("Please Enter Your Discord Server Id.", ConsoleColor.Yellow);
            JsonConfig.DiscordServerId = Console.ReadLine().Trim();
            ConsoleManager.Write("Please Enter Your VRChat AuthCookie.", ConsoleColor.Yellow);
            JsonConfig.AuthCookie = Console.ReadLine().Trim();
            Filemanager.WriteFile(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + _fileName, JsonConvert.SerializeObject(JsonConfig));
            CheckVRCUser();
        }

        private void CheckVRCUser()
        {
            string code = VRCWebRequest.TestReqest(JsonConfig.AuthCookie);
            if (code == "401" || code == "404")
            {
                ConsoleManager.Write("+ Your VRC Information its not right make sure you enter the right infomation. +", ConsoleColor.DarkRed);
                ConsoleManager.Write("Please Enter Your VRChat AuthCookie.", ConsoleColor.Red);
                JsonConfig.AuthCookie = Console.ReadLine().Trim();
                Filemanager.WriteFile(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + _fileName, JsonConvert.SerializeObject(JsonConfig));
                new VRCWebRequest();
                CheckVRCUser();
                return;
            }
            JsonConfig.UserId = JObject.Parse(VRCWebRequest.Instance.SendVRCWebReq(VRCWebRequest.RequestType.Get, VRCInfo.VRCApiLink + VRCInfo.EndPoints.LocalUser))["id"].ToString();
            Filemanager.WriteFile(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + _fileName, JsonConvert.SerializeObject(JsonConfig));
        }
    }
}
