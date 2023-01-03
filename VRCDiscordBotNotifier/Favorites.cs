using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRCDiscordBotNotifier.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json.Nodes;
using DSharpPlus.Entities;

namespace VRCDiscordBotNotifier
{
    internal class Favorites
    {

        public static Favorites Instance { get; private set; }
        public Favorites() =>
           Task.Run(() => {
               Instance = this;
               while (true)
               {
                   if (FriendsMethods.FriendList.Count == 0)
                   {
                       Thread.Sleep(500);
                       continue;
                   }
                   Initialize();
                   break;
               }
           });
        private DSharpPlus.Entities.DiscordChannel Category { get; set; }
        private async Task Initialize()
        {
            ConsoleManager.Write("Looking For Favorite Category...");
            var channels = await BotSetup.Instance.DiscordGuild.GetChannelsAsync();

            Category = channels.FirstOrDefault(x => x.Name == "online-favorites" && x.Type == DSharpPlus.ChannelType.Category);
            if (Category != null)
            {
                ConsoleManager.Write("Found Favorite Category.");
                ConsoleManager.Write("Clearing Old Channels...");
                for (int i = 0; i < channels.Count; i++)
                {

                    if (channels[i].Parent == null) continue;
                    if (channels[i].Parent.Id != Category.Id) continue;
                    await channels[i].DeleteAsync();
                }
                SetupChannels();
                return;
            }
            ConsoleManager.Write("Creating New Category...");
            Category = await BotSetup.Instance.DiscordGuild.CreateChannelCategoryAsync("online-favorites");
            SetupChannels();
        }

        private async void SetupChannels()
        {
            ConsoleManager.Write("Seting Up New Channels...");
            var onlineFavoriteFriends = JsonConvert.DeserializeObject<Utils.Json.Friend[]>(VRCWebRequest.Instance.SendVRCWebReq(VRCWebRequest.RequestType.Get, VRCInfo.VRCApiLink + VRCInfo.EndPoints.FriendsOnline)).Where(x => FriendsMethods.FriendList.Contains(x.id)).ToArray();
            for (int i = 0; i < onlineFavoriteFriends.Length; i++)
            {
                if (onlineFavoriteFriends[i].location == "offline") continue;
                if (onlineFavoriteFriends[i].location == "private") continue;
                if (onlineFavoriteFriends[i].location == "traveling") continue;
                await BotSetup.Instance.DiscordGuild.CreateChannelAsync(onlineFavoriteFriends[i].displayName, DSharpPlus.ChannelType.Text, Category, onlineFavoriteFriends[i].id);
                await UpdateOrSendMessage(onlineFavoriteFriends[i].id, onlineFavoriteFriends[i].location);
            }
            ConsoleManager.Write("Finished New Channels...");
        }
        private JObject _worldInfo { get; set; }
        private JObject _worldInstance { get; set; }
        private JObject _user { get; set; }
        public async Task UpdateOrSendMessage(string id, string instanceid)
        {
            if (instanceid == "offline") return;
            if (instanceid == "private") return;
            if (instanceid == "traveling") return;
            var channels = await BotSetup.Instance.DiscordGuild.GetChannelsAsync();
            var channel = channels.FirstOrDefault(x => x.Topic == id);
            _user = JObject.Parse(VRCWebRequest.Instance.SendVRCWebReq(VRCWebRequest.RequestType.Get, VRCInfo.VRCApiLink + VRCInfo.EndPoints.UserEndPoint + id));
            _worldInfo = JObject.Parse(VRCWebRequest.Instance.SendVRCWebReq(VRCWebRequest.RequestType.Get, VRCInfo.VRCApiLink + VRCInfo.EndPoints.Worlds + instanceid.Split(":")[0]));
            _worldInstance = JObject.Parse(VRCWebRequest.Instance.SendVRCWebReq(VRCWebRequest.RequestType.Get, VRCInfo.VRCApiLink + VRCInfo.EndPoints.Instance + instanceid));
            if (channel == null)
                channel = await BotSetup.Instance.DiscordGuild.CreateChannelAsync(_user["displayName"].ToString(), DSharpPlus.ChannelType.Text, Category, id);
            var pins = await channel.GetPinnedMessagesAsync();
            if (pins.Count != 0)
            {
                await pins[0].ModifyAsync(new DiscordEmbedBuilder()
                {
                    Title = _user["displayName"].ToString(),
                    Color = DiscordColor.IndianRed,
                    ImageUrl = _worldInfo["thumbnailImageUrl"].ToString(),
                    Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail() { Url = _user["currentAvatarImageUrl"].ToString().ToString(), Height = 600, Width = 600 },
                    Description =
            string.Format("<----------[User Info]---------->\n- Name: {0}\n- Id: {17}\n- Status: {1}\n- Platform: {2}\n" +
             "<----------[World Info]---------->\n- Name: {3}\n- Id: {4}\n- Capacity: {5}\n- Version: {6}\n- Fav: {7}\n- Visits: {8}\n- In Public: {9}\n- In Private: {10}\n- Instances: {11}" +
             "\n<----------[Instance Info]---------->\n- Type {12}\n- Id {16}\n- Users {13}/{14}\n- Region {15}\n<----------[w_w]---------->", _user["displayName"], _user["statusDescription"], Extentions.PlatformType(_user["last_platform"].ToString()), _worldInfo["name"]
             , _worldInfo["id"], _worldInfo["capacity"], _worldInfo["version"], _worldInfo["favorites"], _worldInfo["visits"], _worldInfo["publicOccupants"], _worldInfo["privateOccupants"], _worldInfo["instances"].ToArray().Length, Extentions.InstanceType(_worldInstance["type"].ToString())
             , _worldInstance["n_users"], _worldInstance["capacity"], _worldInstance["region"], instanceid, id
            )
                }.Build());
                return;
            }
            var message = await channel.SendMessageAsync(new DiscordEmbedBuilder()
            {
                Title = _user["displayName"].ToString(),
                Color = DiscordColor.IndianRed,
                ImageUrl = _worldInfo["thumbnailImageUrl"].ToString(),
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail() { Url = _user["currentAvatarImageUrl"].ToString().ToString(), Height = 600, Width = 600 },
                Description =
             string.Format("<----------[User Info]---------->\n- Name: {0}\n- Id: {17}\n- Status: {1}\n- Platform: {2}\n" +
             "<----------[World Info]---------->\n- Name: {3}\n- Id: {4}\n- Capacity: {5}\n- Version: {6}\n- Fav: {7}\n- Visits: {8}\n- In Public: {9}\n- In Private: {10}\n- Instances: {11}" +
             "\n<----------[Instance Info]---------->\n- Type {12}\n- Id {16}\n- Users {13}/{14}\n- Region {15}\n<----------[w_w]---------->", _user["displayName"], _user["statusDescription"], Extentions.PlatformType(_user["last_platform"].ToString()), _worldInfo["name"]
             , _worldInfo["id"], _worldInfo["capacity"], _worldInfo["version"], _worldInfo["favorites"], _worldInfo["visits"], _worldInfo["publicOccupants"], _worldInfo["privateOccupants"], _worldInfo["instances"].ToArray().Length, Extentions.InstanceType(_worldInstance["type"].ToString())
             , _worldInstance["n_users"], _worldInstance["capacity"], _worldInstance["region"], instanceid, id
             )
            });
            await message.PinAsync();
            await message.CreateReactionAsync(DiscordEmoji.FromUnicode("⬆️"));

        }

    }
}


