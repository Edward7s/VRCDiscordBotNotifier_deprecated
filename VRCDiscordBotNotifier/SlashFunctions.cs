using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using VRCDiscordBotNotifier.Utils;

namespace VRCDiscordBotNotifier
{
    internal class SlashFunctions : ApplicationCommandModule
    {
        private string Toggle(bool Toggle) => Toggle ? "ON" : "OFF";

        [SlashCommand("AddDmUser", "Add a user to dm every time an action happens.")]
        public async Task AddUser(InteractionContext context)
        {
            if (Config.Instance.JsonConfig.DmUsersId.Length != 0 && Config.Instance.JsonConfig.DmUsersId.FirstOrDefault(x => x != context.Member.Id.ToString()) == null)
            {
                await context.CreateResponseAsync($"User: {context.Member.DisplayName} is already in the list.");
                return;
            }
            List<string> list = Config.Instance.JsonConfig.DmUsersId.ToList();
            list.Add(context.Member.Id.ToString());
            Config.Instance.JsonConfig.DmUsersId = list.ToArray();
            Config.Instance.SaveConfig();
            list = null;
            await context.CreateResponseAsync($"Added: {context.Member.DisplayName} with the id: {context.Member.Id}");
        }

        [SlashCommand("RemoveDmUser", "Remove a user from reciving dms from the bot.")]
        public async Task RemoveUser(InteractionContext context)
        {
            if (Config.Instance.JsonConfig.DmUsersId.FirstOrDefault(x => x == context.Member.Id.ToString()) == null)
            {
                await context.CreateResponseAsync($"User: {context.Member.DisplayName} Is not on the list.");
                return;
            }
            List<string> list = Config.Instance.JsonConfig.DmUsersId.ToList();
            list.Remove(context.Member.Id.ToString());
            Config.Instance.JsonConfig.DmUsersId = list.ToArray();
            Config.Instance.SaveConfig();
            list = null;
            await context.CreateResponseAsync($"User: {context.Member.DisplayName} Removed with the id: {context.Member.Id}");
        }

        [SlashCommand("DmNotifications", "Dms you notifications like invites & stuff")]
        public async Task DmNotifications(InteractionContext context)
        {
            Config.Instance.JsonConfig.DmNewNotifications = !Config.Instance.JsonConfig.DmNewNotifications;
            Config.Instance.SaveConfig();

            await context.CreateResponseAsync($"You turned Notifications: {Toggle(Config.Instance.JsonConfig.DmNewNotifications)}");
        }

        [SlashCommand("DmFriendJoin", "Dms you if a friend joins you world")]
        public async Task Friendjoin(InteractionContext context)
        {
            Config.Instance.JsonConfig.DmOnFriendJoin = !Config.Instance.JsonConfig.DmOnFriendJoin;
            Config.Instance.SaveConfig();

            await context.CreateResponseAsync($"You turned Friend Join Dm: {Toggle(Config.Instance.JsonConfig.DmOnFriendJoin)}");
        }



        [SlashCommand("DmFriendAddOrRemove", "Dms you if someone added you or removed you")]
        public async Task FriendM(InteractionContext context)
        {
            Config.Instance.JsonConfig.DmFriendEvent = !Config.Instance.JsonConfig.DmFriendEvent;
            Config.Instance.SaveConfig();

            await context.CreateResponseAsync($"You turned Notifications: {Toggle(Config.Instance.JsonConfig.DmFriendEvent)}");
        }

        [SlashCommand("FriendsActivity", "Messages you if anything happens to one of your friends.")]
        public async Task MessageOn(InteractionContext context)
        {
           
            Config.Instance.JsonConfig.FriendsActivity = !Config.Instance.JsonConfig.FriendsActivity;
            if (Config.Instance.JsonConfig.FriendsActivity)
                Initialization.Instance.ChannelActivty = await Extentions.GetOrCreateChannel("n_activity");
            else
            {
                var channels = await context.Guild.GetChannelsAsync();
                if (channels.FirstOrDefault(x => x.Name != "n_activity") != null)
                  await channels.First(x => x.Name != "n_activity").DeleteAsync();
                channels = null;
            }
            Config.Instance.SaveConfig();

            await context.CreateResponseAsync($"You turned Online Users Messages: {Toggle(Config.Instance.JsonConfig.FriendsActivity)}");
        }

        [SlashCommand("ApplicationId", "Set's The Application Id for the Rich Presence")] 
        public async Task AppId(InteractionContext context, [Option("AplicationId","Your ApplicationId")] string str)
        {
            Config.Instance.JsonConfig.AplicationId = str;
            Config.Instance.SaveConfig();
            await context.CreateResponseAsync($"The AplicationId is {str}");
        }



        [SlashCommand("RichPresence", "Toggles the Rich Presnece")]
        public async Task RichPresence(InteractionContext context)
        {
            if (Config.Instance.JsonConfig.AplicationId == String.Empty)
            {
                await context.CreateResponseAsync("Please use before Toggeling The Presence /ApplicationId {ID}");
                return;
            }
            Config.Instance.JsonConfig.RichPresence = !Config.Instance.JsonConfig.RichPresence;
            Config.Instance.SaveConfig();
            if (Config.Instance.JsonConfig.RichPresence)
                Task.Run(() => new RichPresence());


            await context.CreateResponseAsync($"You turned The RichPresence: {Toggle(Config.Instance.JsonConfig.RichPresence)}");
        }


        /*  [SlashCommand("MessageStatus", "Messages you on a channel if someone changed they're status")]
          public async Task MessageStatus(InteractionContext context)
          {
              Config.Instance.JsonConfig.MessageStatusChange = !Config.Instance.JsonConfig.MessageStatusChange;
              if (Config.Instance.JsonConfig.MessageStatusChange)
                  Initialization.Instance.ChannelStatus = await Extentions.GetOrCreateChannel("n_status");
              else
              {
                  var channels = await context.Guild.GetChannelsAsync();
                  if (channels.FirstOrDefault(x => x.Name != "n_status") != null)
                      await channels.First(x => x.Name == "n_status").DeleteAsync();
                  channels = null;
              }
              Config.Instance.SaveConfig();

              await context.CreateResponseAsync($"You turned status Users Messages: {Toggle(Config.Instance.JsonConfig.MessageStatusChange)}");
          }
        */
    }
}
