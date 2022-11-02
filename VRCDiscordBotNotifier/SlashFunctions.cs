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
