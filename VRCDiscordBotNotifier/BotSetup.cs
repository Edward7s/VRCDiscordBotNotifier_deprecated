using DSharpPlus.Entities;
using DSharpPlus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRCDiscordBotNotifier.Utils;
using DSharpPlus.SlashCommands;

namespace VRCDiscordBotNotifier
{
    internal class BotSetup
    {
        public static BotSetup Instance { get; private set; }
        public BotSetup()
        {
            Instance = this;
            Task.Run(() => Setup());
        }
        public  DiscordClient DiscordClientManager { get; set; }
        public  DiscordGuild DiscordGuild { get; private set; }
        public async Task Setup()
        {

            try
            {
                DiscordClientManager = new DiscordClient(new DiscordConfiguration()
                {
                    Token = Config.Instance.JsonConfig.BotToken,
                    TokenType = TokenType.Bot,
                    AutoReconnect = true,
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Your bot Token Might Be Not Valid.");
                Console.WriteLine(ex);
                Console.WriteLine("Please go to =>");
                Console.WriteLine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\VRCDiscordBotManager.json");
                Console.WriteLine("And replace the BotToken");
                return;
            }
            var slash = DiscordClientManager.UseSlashCommands();
            slash.RegisterCommands<SlashFunctions>(ulong.Parse(Config.Instance.JsonConfig.DiscordServerId));
            DiscordClientManager.Ready += OnClientReady;
            DiscordClientManager.MessageCreated += OnMessage;
            DiscordClientManager.MessageReactionAdded += OnReaction;
            await DiscordClientManager.ConnectAsync();
            DiscordGuild = await DiscordClientManager.GetGuildAsync(ulong.Parse(Config.Instance.JsonConfig.DiscordServerId));
            new Initialization();
            Task.Run(() => new NotificationsLoop().Loop());
            Task.Run(() => new WebSocket.VRCWebSocket());
            if (Config.Instance.JsonConfig.RichPresence)
                Task.Run(() => new RichPresence());
            await Task.Delay(-1);
        }

        private async Task OnReaction(DiscordClient sender, DSharpPlus.EventArgs.MessageReactionAddEventArgs e)
        {
            try
            {
                if (e.User.Id == DiscordClientManager.CurrentUser.Id) return;

                if (e.Emoji != DiscordEmoji.FromUnicode("⬆️")) return;

                var reactions = await e.Message.GetReactionsAsync(DiscordEmoji.FromUnicode("⬆️"));
              
                if (reactions.FirstOrDefault(x => x.Id == DiscordClientManager.CurrentUser.Id) == null)
                {
                    reactions = null;
                    return;
                }
                reactions = null;
                DiscordMessage message = await e.Channel.GetMessageAsync(e.Message.Id);
                string[] lines = message.Embeds[0].Description.Split("\n");
                if (lines.ElementAtOrDefault(4).Trim().Length > 40)
                {
                    VRCWebRequest.Instance.SendVRCWebReq(VRCWebRequest.RequestType.Post, VRCInfo.VRCApiLink + VRCInfo.EndPoints.SelfInvite + lines.ElementAtOrDefault(4).Replace("Traveling to: ", "").Trim());
                    lines = null;
                    return;
                }
                if (lines.ElementAtOrDefault(3).Trim().Length > 40)
                {
                    VRCWebRequest.Instance.SendVRCWebReq(VRCWebRequest.RequestType.Post, VRCInfo.VRCApiLink + VRCInfo.EndPoints.SelfInvite + lines.ElementAtOrDefault(3).Replace("Location: ", "").Trim());
                    lines = null;
                    return;
                }
            }
            finally
            {

            }

        }

        private async Task OnMessage(DiscordClient sender, DSharpPlus.EventArgs.MessageCreateEventArgs e)
        {
            try
            {
                if (e.Author.Id != DiscordClientManager.CurrentUser.Id) return;
                if (e.Message.Embeds.Count == 0) return;
                for (int i = 0; i < FriendsMethods.FriendList.Count; i++)
                {
                    try
                    {
                        if (!e.Message.Embeds[0].Description.Contains(FriendsMethods.FriendList[i])) continue;
                        await e.Message.CreateReactionAsync(DiscordEmoji.FromUnicode("🖤"));
                        Thread.Sleep(50);
                        break;
                    }
                    catch(Exception ex) { Console.WriteLine(ex); }
                    
                }
            }
            finally
            {
            }
        }
        private async Task OnClientReady(DiscordClient sender, DSharpPlus.EventArgs.ReadyEventArgs e)
        {
            DiscordActivity presence = new DiscordActivity()
            {
                StreamUrl = "https://music.youtube.com/playlist?list=PLR3eEWxDgAkOgKu1q95p7gGriozwOtziA",
                ActivityType = DSharpPlus.Entities.ActivityType.Streaming,
                Name = "9𝓽𝓪𝓲𝓵𝓼 - 𝓬𝓸𝓶𝓪𝓽𝓸𝓼𝓮 (𝓵𝔂𝓻𝓲𝓬𝓼).",
            };
            await DiscordClientManager.UpdateStatusAsync(presence);
        }
    }
}
