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
        private  JObject s_user { get; set; }
        private  JObject s_world { get; set; }

        private  string s_lastInstance { get; set; }
        private  string s_lastId { get; set; }

        private  string s_worldInfo { get; set; }
        private  DiscordMessage s_message { get; set; }

        private  bool s_joinable { get; set; }
        public  async Task Offline(string id) =>
            await Initialization.Instance.ChannelActivty.SendMessageAsync(new DiscordEmbedBuilder() { Title = "{ " + JObject.FromObject(JObject.Parse(VRCWebRequest.Instance.SendVRCWebReq(VRCWebRequest.RequestType.Get, VRCInfo.VRCApiLink + VRCInfo.EndPoints.UserEndPoint + id)))["displayName"].ToString() + " } Is now offline.",Description = "UserId: " + id, Color = DiscordColor.Gray });


        public async Task Online(JObject User)
        {
            await Initialization.Instance.ChannelActivty.SendMessageAsync(new DiscordEmbedBuilder()
            {
                Title = new StringBuilder().AppendFormat("{{ {0} }} Is now online", User["displayName"]).ToString(),
                Color = new DiscordColor(Extentions.GetColorFromUserStatus(User["status"].ToString())),
                Description = new StringBuilder(string.Empty).AppendFormat("UserId: {0}\nState: {1}\nStatus: {2}",User["id"], User["status"], User["statusDescription"]).ToString(),
            });
        }

        public async Task Location(JObject jobj)
        {
            s_user = JObject.Parse(jobj["user"].ToString());
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
                s_world = JObject.Parse(jobj["world"].ToString());
                s_worldInfo = new StringBuilder().AppendFormat("Name: {0}\nId: {1}\nAuthor Name: {2}", s_world["name"], s_world["id"], s_world["authorName"]).ToString();
                s_world = null;
                s_joinable = true;
            }

            s_message = await Initialization.Instance.ChannelActivty.SendMessageAsync(new DiscordEmbedBuilder()
            {
                Title = new StringBuilder().AppendFormat("{{ {0} }} Changed His Location.", s_user["displayName"]).ToString(),
                Color = new DiscordColor(Extentions.GetColorFromUserStatus(s_user["status"].ToString())),
                Description = new StringBuilder().AppendFormat("UserId: {0}\nState: {1}\nStatus: {2}\nLocation: {3}\nTraveling to: {4}\n{5}", s_user["id"], s_user["status"], s_user["statusDescription"], jobj["location"], jobj["travelingToLocation"], s_worldInfo).ToString(),
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
    }
}
