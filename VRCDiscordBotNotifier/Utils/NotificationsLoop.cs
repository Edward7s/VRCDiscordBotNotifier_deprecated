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
        private  Json.Notification[]? s_notificationsArr { get; set; }
        private  string s_apiNotifications { get; set; } = string.Empty;
        private  Json.Notification[]? s_apiNotificationsArr { get; set; }
        private  DiscordMember? s_member { get; set; }
        private  DiscordDmChannel? s_dm { get; set; }

        //The reason why I am doing this and not using the websocket its be cause the websocket does not send notifications from the past.
        public async void Loop()
        {
            if (Config.Instance.JsonConfig.DmNewNotifications)
            {
           
                s_notificationsArr = JsonConvert.DeserializeObject<Json.Notification[]>(Filemanager.ReadFile(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Config.Instance.Notifications));
                s_apiNotifications = VRCWebRequest.Instance.SendVRCWebReq(VRCWebRequest.RequestType.Get, VRCInfo.VRCApiLink + VRCInfo.EndPoints.Notifications);
                s_apiNotificationsArr = JsonConvert.DeserializeObject<Json.Notification[]>(s_apiNotifications);
                for (int i = 0; i < s_apiNotificationsArr.Length; i++)
                {
                    Thread.Sleep(100);
                    if (s_notificationsArr.FirstOrDefault(x => x.id == s_apiNotificationsArr[i].id) != null) continue;
                    for (int j = 0; j < Config.Instance.JsonConfig.DmUsersId.Length; j++)
                    {
                        Thread.Sleep(200);
                        s_member = await BotSetup.Instance.DiscordGuild.GetMemberAsync(ulong.Parse(Config.Instance.JsonConfig.DmUsersId[j]));
                        s_dm = await s_member.CreateDmChannelAsync();
                        await s_dm.SendMessageAsync(new DiscordEmbedBuilder() { Title = String.Format("{{ {0} }} Sent you a {1}", s_apiNotificationsArr[i].senderUsername, s_apiNotificationsArr[i].type).ToString(),Description = String.Format("Created at: {0}\n{1}", DateTime.Parse(s_apiNotificationsArr[i].created_at).ToLocalTime(), s_apiNotificationsArr[i].details.Length < 6 ? string.Empty :Extentions.GetDetailsInfo(s_apiNotificationsArr[i].details)).ToString(), Color = DiscordColor.Purple });
                    }
                }
                Filemanager.WriteFile(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Config.Instance.Notifications, s_apiNotifications);
            }
            Thread.Sleep(17500);
            Loop();
        }
    }
}
