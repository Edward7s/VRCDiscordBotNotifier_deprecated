﻿using DSharpPlus.Entities;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace VRCDiscordBotNotifier.Utils
{
    internal class Extentions
    {
        public static async Task<DiscordChannel> GetOrCreateChannel(string Channel)
        {
            var channels = await BotSetup.Instance.DiscordGuild.GetChannelsAsync();
            if (channels.FirstOrDefault(x => x.Name == Channel) == null)
                return await BotSetup.Instance.DiscordGuild.CreateChannelAsync(Channel, DSharpPlus.ChannelType.Text);
            else
                return channels.First(x => x.Name == Channel);
        }

        public static void CheckFileSanity(string path, string content)
        {
            if (!File.Exists(path))
            {
                Filemanager.WriteFile(path, content);
                return;
            }
            if (Filemanager.ReadFile(path) == string.Empty)
                Filemanager.WriteFile(path,content);
        }

        public static string GetDetailsInfo(string info)
        {
            JObject jobj = JObject.Parse(info);
            if (jobj["worldId"] != null)
                return string.Format("World Name: {0}\nId: {1}", jobj["worldName"], jobj["worldId"]);
            

            return string.Empty;
        }


        public static string InstanceType(string type)
        {
            if ("hidden" == type)
                return "Friends+";
            if ("friends" == type)
                return "Friends";
            if ("private" == type)
                return "Private";

            return "Public";
        }

        public static string PlatformType(string platform)
        {
            if ("web" == platform)
                return "Browser";
            if ("standalonewindows" == platform)
                return "PC";

            return "Quest";
        }
        public static string GetColorFromUserStatus(string State)
        {
            if (State == "active")
                return "#4dff5e";

            if (State == "join me")
                return  "#94bfff";

            if (State == "join me")
                return "#4dff5e";

            if (State == "ask me")
                return "#ffdd00";

            return "#ff0011";
        }
    }
}
