using DSharpPlus.Entities;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using VRCDiscordBotNotifier.Utils;
using static VRCDiscordBotNotifier.Utils.Json;

namespace VRCDiscordBotNotifier.WebSocket
{
    internal class WebSocketMessageManager
    {
        private static JObject s_user { get; set; }
        private static JObject s_world { get; set; }

        private static string s_lastInstance { get; set; }
        private static string s_lastId { get; set; }

        private static string s_worldInfo { get; set; }
        private static DiscordMessage s_message { get; set; }

        private static bool s_joinable { get; set; }
        public static async Task Offline(string id) =>
            await Initialization.Instance.ChannelActivty.SendMessageAsync(new DiscordEmbedBuilder() { Title = "{ " + JObject.FromObject(JObject.Parse(VRCWebRequest.Instance.SendVRCWebReq(VRCWebRequest.RequestType.Get, VRCInfo.VRCApiLink + VRCInfo.EndPoints.UserEndPoint + id)))["displayName"].ToString() + " } Is now offline.",Description = "UserId: " + id, Color = DiscordColor.Gray });


        public static async Task Online(JObject User)
        {
            await Initialization.Instance.ChannelActivty.SendMessageAsync(new DiscordEmbedBuilder()
            {
                Title = "{ " + User["displayName"].ToString() + " } Is now Online.",
                Color = new DiscordColor(Extentions.GetColorFromUserStatus(User["status"].ToString())),
                Description = "UserId: " + User["id"].ToString() + "\nState: " + User["status"].ToString() + " \nStatus: " + User["statusDescription"].ToString(),
            });
        }

        public static async Task Location(JObject jobj)
        {
            s_user = JObject.FromObject(JObject.Parse(jobj["user"].ToString()));
            if (s_lastId == s_user["id"].ToString() && s_lastInstance == jobj["location"].ToString())
            {
                s_user = null;
                return;
            }
            s_lastId = s_user["id"].ToString();
            s_lastInstance = jobj["location"].ToString();

            if (s_user["status"].ToString() == "ask me" || s_user["status"].ToString() == "busy")
            {
                s_user = null;
                return;
            }
            if (jobj["world"].ToString().Length > 30)
            {
                s_world = JObject.FromObject(JObject.Parse(jobj["world"].ToString()));
                s_worldInfo = $"Name: {s_world["name"].ToString()}\nId: {s_world["id"].ToString()}\nAuthorId: {s_world["authorName"].ToString()}";
                s_world = null;
                s_joinable = true;
            }

            s_message = await Initialization.Instance.ChannelActivty.SendMessageAsync(new DiscordEmbedBuilder()
            {
                Title = "{ " + s_user["displayName"].ToString() + " } Changed His Location.",
                Color = new DiscordColor(Extentions.GetColorFromUserStatus(s_user["status"].ToString())),
                Description = "UserId: " + s_user["id"].ToString() + "\nState: " + s_user["status"].ToString() + " \nStatus: " + s_user["statusDescription"].ToString() + "\nLocation: " + jobj["location"].ToString() + "\nTraveling to: " + jobj["travelingToLocation"].ToString() + "\n" + s_worldInfo,
            });
            s_user = null;
            s_worldInfo = string.Empty;
            Thread.Sleep(700);
            if (s_joinable)
            {
                await s_message.CreateReactionAsync(DiscordEmoji.FromUnicode("⬆️"));
                s_message = null;
            }
         
        }

        /*  public static async Task Location(JObject User)
          {
              await Initialization.Instance.ChannelActivty.SendMessageAsync(new DiscordEmbedBuilder()
              {
                  Title = "{ " + User["displayName"].ToString() + " } Changed His Location.",
                  Color = new DiscordColor("#4d2000"),
                  Description = "State: " + User["status"].ToString() + " \nStatus: " + User["statusDescription"].ToString() + "\nLocation: " + User["location"].ToString(),
              });
          }
        */
    }
}
