using DSharpPlus.Entities;
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
            var channels = await Program.DiscordGuild.GetChannelsAsync();
            if (channels.FirstOrDefault(x => x.Name == Channel) == null)
                return await Program.DiscordGuild.CreateChannelAsync(Channel, DSharpPlus.ChannelType.Text);
            else
                return channels.First(x => x.Name == Channel);
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
