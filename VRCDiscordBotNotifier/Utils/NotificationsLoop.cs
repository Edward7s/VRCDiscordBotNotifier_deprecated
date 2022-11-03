using DSharpPlus.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using static VRCDiscordBotNotifier.Utils.Json;

namespace VRCDiscordBotNotifier.Utils
{
    internal class NotificationsLoop
    {
        private static Json.Notification[]? s_notificationsArr { get; set; }
        private static string s_apiNotifications { get; set; } = string.Empty;
        private static Json.Notification[]? s_apiNotificationsArr { get; set; }
        private static DiscordMember? s_member { get; set; }
        private static DiscordDmChannel? s_dm { get; set; }

        //The reason why I am doing this and not using the websocket its be cause the websocket does not send notifications from the past.
        public static async void Loop()
        {
            if (Config.Instance.JsonConfig.DmNewNotifications)
            {
                s_notificationsArr = JsonConvert.DeserializeObject<Json.Notification[]>(File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Config.Instance.Notifications));
                s_apiNotifications = VRCWebRequest.Instance.SendVRCWebReq(VRCWebRequest.RequestType.Get, VRCInfo.VRCApiLink + VRCInfo.EndPoints.Notifications);
                s_apiNotificationsArr = JsonConvert.DeserializeObject<Json.Notification[]>(s_apiNotifications);
                for (int i = 0; i < s_apiNotificationsArr.Length; i++)
                {
                    Thread.Sleep(100);
                    if (s_notificationsArr.FirstOrDefault(x => x.id == s_apiNotificationsArr[i].id) != null) continue;
                    for (int j = 0; j < Config.Instance.JsonConfig.DmUsersId.Length; j++)
                    {
                        Thread.Sleep(200);
                        s_member = await Program.DiscordGuild.GetMemberAsync(ulong.Parse(Config.Instance.JsonConfig.DmUsersId[j]));
                        s_dm = await s_member.CreateDmChannelAsync();
                        await s_dm.SendMessageAsync(new DiscordEmbedBuilder() { Title = $"{{ {s_apiNotificationsArr[i].senderUsername} }} Sent you a {s_apiNotificationsArr[i].type}", Description = "Created at: " + s_apiNotificationsArr[i].created_at , Color = DiscordColor.Purple });
                    }
                }
                File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Config.Instance.Notifications, s_apiNotifications);
            }
            Thread.Sleep(5000);
            Loop();
        }
    }
}
