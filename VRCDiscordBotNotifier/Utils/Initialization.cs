using DSharpPlus.Entities;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace VRCDiscordBotNotifier.Utils
{
    internal class Initialization
    {
        public static Initialization Instance { get; private set; }

        public DiscordChannel ChannelActivty { get; set; }
  

        public Initialization()
        {
            Instance = this;
            Task.Run(() => ChannelManager() );
            Task.Run(() => new FriendsMethods().AllFriends());
            Task.Run(() => CreateSetupChannel());
        }

 
        private async Task ChannelManager()
        {
            if (Config.Instance.JsonConfig.FriendsActivity)
                ChannelActivty = await Extentions.GetOrCreateChannel("n_activity");
        }


        private async Task CreateSetupChannel()
        {
           if (Config.Instance.JsonConfig.FirstTime)
            {
                Config.Instance.JsonConfig.FirstTime = false;
                Config.Instance.SaveConfig();
                var channel = await BotSetup.Instance.DiscordGuild.CreateChannelAsync("Setup", DSharpPlus.ChannelType.Text);
                var message = await channel.SendMessageAsync(new DiscordEmbedBuilder() { Title = "Setup", Color = DiscordColor.Red, Description = "The bot only works with '/' Commands;\n/adddmuser is adding you discord account to a dm list from the dm commands.\n/removedmuser will remove your account from reciving dms.\n  > > Dm Commands < <\n/dmnotifications will dm you wen someone invites you or sends you a friend request\n/dmfriendaddorremove will dm you wen someone friends or unfriends you.\n/friendsactivity will create a channel and the bot will msg there every time a friend is online offline or changes the world if its not on yellow or red.\n/friendrequests Will Show you all the friend Requests you have\n /acceptfriend (The Number Of The User You Want To Accept From: /friendrequests)." });
                await message.CreateReactionAsync(DiscordEmoji.FromUnicode("😘"));
               await channel.SendMessageAsync("@everyone");
            }
        }

    }
}
