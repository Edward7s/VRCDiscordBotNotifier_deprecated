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
        private JObject _world { get; set; }

        private string _lastInstance { get; set; }
        private string _lastId { get; set; }
        private DiscordMember _member { get; set; }
        private DiscordDmChannel _dm { get; set; }

        private string _worldInfo { get; set; }

        public async Task Offline(string id) =>
            await Initialization.Instance.ChannelActivty.SendMessageAsync(new DiscordEmbedBuilder() { Title = "{ " + JObject.FromObject(JObject.Parse(VRCWebRequest.Instance.SendVRCWebReq(VRCWebRequest.RequestType.Get, VRCInfo.VRCApiLink + VRCInfo.EndPoints.UserEndPoint + id)))["displayName"].ToString() + " } Is now offline.", Description = "UserId: " + id, Color = DiscordColor.Gray });


        public async Task Online(JObject User)
        {
            await Initialization.Instance.ChannelActivty.SendMessageAsync(new DiscordEmbedBuilder()
            {
                Title = new StringBuilder().AppendFormat("{{ {0} }} Is now online", User["displayName"]).ToString(),
                Color = new DiscordColor(Extentions.GetColorFromUserStatus(User["status"].ToString())),
                Description = new StringBuilder(string.Empty).AppendFormat("UserId: {0}\nState: {1}\nStatus: {2}", User["id"], User["status"], User["statusDescription"]).ToString(),
            });
        }

        public async Task Location(JObject jobj)
        {
            bool joinable = false;
            JObject user = JObject.Parse(jobj["user"].ToString());
            if (_lastId == user["id"].ToString() && _lastInstance == jobj["location"].ToString())
            {
                user = null;
                return;
            }
            _lastId = user["id"].ToString();
            _lastInstance = jobj["location"].ToString();

            if (Config.Instance.JsonConfig.DmOnFriendJoin && FriendsMethods.CurrentInstanceId != "offline:offline" && jobj["travelingToLocation"].ToString() == FriendsMethods.CurrentInstanceId)
            {
                for (int j = 0; j < Config.Instance.JsonConfig.DmUsersId.Length; j++)
                {
                    Thread.Sleep(300);
                    _member = await BotSetup.Instance.DiscordGuild.GetMemberAsync(ulong.Parse(Config.Instance.JsonConfig.DmUsersId[j]));
                    _dm = await _member.CreateDmChannelAsync();
                    await _dm.SendMessageAsync(new DiscordEmbedBuilder() { Title = new StringBuilder().AppendFormat("{{ {0} }} Is Joining You.", user["displayName"]).ToString(), Color = DiscordColor.Cyan });
                }
            }

            if (user["status"].ToString() == "ask me" || user["status"].ToString() == "busy")
            {
                user = null;
                return;
            }
            if (jobj["world"].ToString().Length > 30 && _lastInstance != "private" && jobj["travelingToLocation"].ToString() != "private")
            {
                _world = JObject.Parse(jobj["world"].ToString());
                _worldInfo = new StringBuilder().AppendFormat("Name: {0}\nId: {1}\nAuthor Name: {2}", _world["name"], _world["id"], _world["authorName"]).ToString();
                joinable = true;
            }

            DiscordMessage message = await Initialization.Instance.ChannelActivty.SendMessageAsync(new DiscordEmbedBuilder()
            {
                Title = new StringBuilder().AppendFormat("{{ {0} }} Changed His Location.", user["displayName"]).ToString(),
                Color = new DiscordColor(Extentions.GetColorFromUserStatus(user["status"].ToString())),
                Description = new StringBuilder().AppendFormat("UserId: {0}\nState: {1}\nStatus: {2}\nLocation: {3}\nTraveling to: {4}\n{5}", _lastId, user["status"], user["statusDescription"], _lastInstance, jobj["travelingToLocation"], _worldInfo).ToString(),
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail() { Url = joinable ? _world["imageUrl"].ToString() : "https://raw.githubusercontent.com/Edward7s/AutoUpdatorForDiscordBot/master/PrivateWorld.png", Height = 800, Width = 800 }
            });
            user = null;
            Thread.Sleep(700);
            if (joinable)
            {
                await message.CreateReactionAsync(DiscordEmoji.FromUnicode("⬆️"));
                message = null;
            }


        }
    }
}
