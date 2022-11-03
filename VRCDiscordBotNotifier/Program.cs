using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using VRCDiscordBotNotifier.Utils;

namespace VRCDiscordBotNotifier
{
    internal class Program
    {
        private static void ClearMem()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Thread.Sleep(500);
            ClearMem();
        }
        static void Main(string[] args)
        {
            Console.Title = "VRCDiscordBot";
            Task.Run(() => ClearMem());
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Initializing Config, This might take a bit depending on your enthernet speed and how much vrc friends you have.");
            new Config();
            Console.WriteLine("Initializing Bot");
            Task.Run(() => Setup());
            Console.ReadLine();
        }
        public static DiscordClient DiscordClientManager { get; set; }
        public static DiscordGuild DiscordGuild { get; private set; }
        public static async Task Setup()
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
            DiscordGuild = await Program.DiscordClientManager.GetGuildAsync(ulong.Parse(Config.Instance.JsonConfig.DiscordServerId));
            new Initialization();
            Task.Run(() => NotificationsLoop.Loop());
            Task.Run(() => new WebSocket.VRCWebSocket());
            await Task.Delay(-1);
        }

        private async static Task OnReaction(DiscordClient sender, DSharpPlus.EventArgs.MessageReactionAddEventArgs e)
        {
            if (e.User.Id == DiscordClientManager.CurrentUser.Id) return;

            if (e.Emoji != DiscordEmoji.FromUnicode("⬆️")) return;

            var reactions =  await e.Message.GetReactionsAsync(DiscordEmoji.FromUnicode("⬆️"));

            if (reactions.FirstOrDefault(x => x.Id == DiscordClientManager.CurrentUser.Id) == null)
            {
                reactions = null;
                return;
            }
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

        private async static Task OnMessage(DiscordClient sender, DSharpPlus.EventArgs.MessageCreateEventArgs e)
        {
            if (e.Author.Id != DiscordClientManager.CurrentUser.Id) return;
            if (e.Message.Embeds.Count == 0) return;
            for (int i = 0; i < FriendsMethods.FriendList.Count; i++)
            {
                if (!e.Message.Embeds[0].Description.Contains(FriendsMethods.FriendList[i])) continue;
                await e.Message.CreateReactionAsync(DiscordEmoji.FromUnicode("🖤"));
                Thread.Sleep(50);
                break;
            }
        }
        private async static Task OnClientReady(DiscordClient sender, DSharpPlus.EventArgs.ReadyEventArgs e)
        {
            DiscordActivity presence = new DiscordActivity()
            {
                StreamUrl = "https://music.youtube.com/playlist?list=PLR3eEWxDgAkOgKu1q95p7gGriozwOtziA",
                ActivityType = DSharpPlus.Entities.ActivityType.ListeningTo,
                Name = "9𝓽𝓪𝓲𝓵𝓼 - 𝓬𝓸𝓶𝓪𝓽𝓸𝓼𝓮 (𝓵𝔂𝓻𝓲𝓬𝓼).",
            };
            await DiscordClientManager.UpdateStatusAsync(presence);
        }
    }
}