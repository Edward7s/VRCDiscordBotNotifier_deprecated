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
        public DiscordClient DiscordClientManager { get; set; }
        public DiscordGuild DiscordGuild { get; private set; }
        public async Task Setup()
        {

            try
            {
                DiscordClientManager = new DiscordClient(new DiscordConfiguration()
                {
                    Token = Config.Instance.JsonConfig.BotToken,
                    TokenType = TokenType.Bot,
                    AutoReconnect = true,
                    MinimumLogLevel = Microsoft.Extensions.Logging.LogLevel.Critical
                }) ;
            }
            catch (Exception ex)
            {
                 ConsoleManager.Write("Your bot Token Might Be Not Valid.");
                 ConsoleManager.Write(ex);
                 ConsoleManager.Write("Please go to =>");
                 ConsoleManager.Write(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\VRCDiscordBotManager.json");
                 ConsoleManager.Write("And replace the BotToken");
                return;
            }
            ConsoleManager.Write("Registering Bot Utils...");
            var slash = DiscordClientManager.UseSlashCommands();
            slash.RegisterCommands<SlashFunctions>(ulong.Parse(Config.Instance.JsonConfig.DiscordServerId));
            DiscordClientManager.Ready += OnClientReady;
            DiscordClientManager.MessageCreated += OnMessage;
            DiscordClientManager.MessageReactionAdded += OnReaction;
            await DiscordClientManager.ConnectAsync();
            ConsoleManager.Write("Getting Guild...");
            DiscordGuild = await DiscordClientManager.GetGuildAsync(ulong.Parse(Config.Instance.JsonConfig.DiscordServerId));
            new Initialization();
            Task.Run(() => new NotificationsLoop().Loop());
            Task.Run(() => new WebSocket.VRCWebSocket());
            if (Config.Instance.JsonConfig.RichPresence)
                Task.Run(() => new RichPresence());
            ConsoleManager.Write("Bot Initialized.");
            await Task.Delay(-1);
        }

        private async Task OnReaction(DiscordClient sender, DSharpPlus.EventArgs.MessageReactionAddEventArgs e)
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


            if (message.Pinned)
            {
                VRCWebRequest.Instance.SendVRCWebReq(VRCWebRequest.RequestType.Post, VRCInfo.VRCApiLink + VRCInfo.EndPoints.SelfInvite + lines[17].Replace("- Id ", "").Trim());
                lines = null;
                return;
            }

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

        private async Task OnMessage(DiscordClient sender, DSharpPlus.EventArgs.MessageCreateEventArgs e)
        {
            if (e.Author.Id != DiscordClientManager.CurrentUser.Id) return;
            if (e.Message.Embeds.Count == 0) return;
            if (e.Message.Embeds[0].Description == null) return;

            for (int i = 0; i < FriendsMethods.FriendList.Count; i++)
            {
                try
                {
                    if (!e.Message.Embeds[0].Description.Contains(FriendsMethods.FriendList[i])) continue;
                    await e.Message.CreateReactionAsync(DiscordEmoji.FromUnicode("🖤"));
                    Thread.Sleep(50);
                    break;
                }
                catch (Exception ex) {  ConsoleManager.Write(ex); }
            }
        }
        private async Task OnClientReady(DiscordClient sender, DSharpPlus.EventArgs.ReadyEventArgs e)
        {
            ConsoleManager.Write("Checking Client...");
            DiscordActivity presence = new DiscordActivity()
            {
                StreamUrl = "https://github.com/Edward7s/VRCDiscordBotNotifier",
                ActivityType = DSharpPlus.Entities.ActivityType.Streaming,
                Name = string.Format("On {0} By XOXO🖤", VersionChecker.Version),
            };
            await DiscordClientManager.UpdateStatusAsync(presence, UserStatus.Idle, DiscordClientManager.CurrentApplication.CreationTimestamp.DateTime);
            if (DiscordClientManager.CurrentUser.Username != "VRChat Notifier.")
              await DiscordClientManager.UpdateCurrentUserAsync("VRChat Notifier.", new MemoryStream(new System.Net.WebClient().DownloadData("https://raw.githubusercontent.com/Edward7s/AutoUpdatorForDiscordBot/master/033790b456c8206547b8fc5c297c4ce7.jpg")));
        }
    }
}
