using DSharpPlus;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRCDiscordBotNotifier.Utils
{
    internal class FriendsMethods
    {
        public static List<string> FriendList { get; } = new List<string>();
        public static string CurrentInstanceId { get; set; } = string.Empty;
        private List<Json.FriendGroup> _users { get; set; }
        private JToken? _friends { get; set; }
        private JObject _localUser { get; set; }    

        private string? s_friendList { get; set; }

        public async void AllFriends()
        {
            _users = JsonConvert.DeserializeObject<List<Json.FriendGroup>>(VRCWebRequest.Instance.SendVRCWebReq(VRCWebRequest.RequestType.Get, VRCInfo.VRCApiLink + VRCInfo.EndPoints.FavoritesFriends));
            Thread.Sleep(1500);
            FriendList.Clear();
            for (int i = 0; i < _users.Count; i++)
            {
                Thread.Sleep(10);
                FriendList.Add(_users[i].favoriteId);
            }
            _users = null;
            _localUser = JObject.Parse(VRCWebRequest.Instance.SendVRCWebReq(VRCWebRequest.RequestType.Get, VRCInfo.VRCApiLink + VRCInfo.EndPoints.LocalUser));
            _friends = _localUser["friends"];
            CurrentInstanceId = new StringBuilder().AppendFormat("{0}:{1}", _localUser["presence"]["world"], _localUser["presence"]["instance"]).ToString();
            _localUser = null;
            Thread.Sleep(1500);
            if (Config.Instance.JsonConfig.DmFriendEvent)
            {
                string friendsText = File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Config.Instance.FriendsFile);
                DiscordMember member = null;
                DiscordDmChannel dmChannel = null;
                JObject User = null;
                string[] ids = File.ReadAllLines(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Config.Instance.FriendsFile);
                for (int i = 0; i < ids.Length; i++)
                {
                    Thread.Sleep(300);
                    if (_friends.FirstOrDefault(x => x.ToString() == ids[i]) != null) continue;
                    User = JObject.Parse(VRCWebRequest.Instance.SendVRCWebReq(VRCWebRequest.RequestType.Get, VRCInfo.VRCApiLink + VRCInfo.EndPoints.UserEndPoint + ids[i]));
                    for (int j = 0; j < Config.Instance.JsonConfig.DmUsersId.Length; j++)
                    {
                        Thread.Sleep(200);
                        member = await BotSetup.Instance.DiscordGuild.GetMemberAsync(ulong.Parse(Config.Instance.JsonConfig.DmUsersId[j]));
                        dmChannel = await member.CreateDmChannelAsync();
                        await dmChannel.SendMessageAsync(new DiscordEmbedBuilder() { Title = new StringBuilder().AppendFormat("{{ {0} }} Removed You From Friends, Id: {1}.", User["displayName"], User["id"]).ToString(), Color = DiscordColor.Red, Description = new StringBuilder().AppendFormat("UserId: {0}", _friends[i]).ToString() });

                    }
                }

                for (int i = 0; i < _friends.ToArray().Length; i++)
                {
                    Thread.Sleep(300);
                    if (friendsText.Contains(_friends[i].ToString())) continue;
                    User = JObject.Parse(VRCWebRequest.Instance.SendVRCWebReq(VRCWebRequest.RequestType.Get, VRCInfo.VRCApiLink + VRCInfo.EndPoints.UserEndPoint + _friends[i]));
                    Thread.Sleep(2000);

                    for (int j = 0; j < Config.Instance.JsonConfig.DmUsersId.Length; j++)
                    {
                        Thread.Sleep(200);
                        member = await BotSetup.Instance.DiscordGuild.GetMemberAsync(ulong.Parse(Config.Instance.JsonConfig.DmUsersId[j]));
                        dmChannel = await member.CreateDmChannelAsync();
                        await dmChannel.SendMessageAsync(new DiscordEmbedBuilder() { Title = new StringBuilder().AppendFormat("{{ {0} }} Added You, Id: {1}", User["displayName"], User["id"]).ToString(), Color = DiscordColor.Green, Description = new StringBuilder().AppendFormat("UserId: {0}", ids[i]).ToString() });

                    }
                }
                member = null;
                dmChannel = null;
                ids = null;
            }

            s_friendList = string.Empty;
            for (int i = 0; i < _friends.ToArray().Length; i++)
                s_friendList += _friends[i].ToString() + "\n";
            File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Config.Instance.FriendsFile, s_friendList);
            s_friendList = null;
            _friends = null;
            Thread.Sleep(45000);
            GC.Collect();
            GC.WaitForPendingFinalizers();
            AllFriends();
        }
        /* public static async Task OnlineOfflineFriends()
         {
             var onlineFriends = JObject.FromObject(JObject.Parse(VRCWebRequest.Instance.SendVRCWebReq(VRCWebRequest.RequestType.Get, VRCInfo.VRCApiLink + VRCInfo.EndPoints.LocalUser)))["onlineFriends"];
             JObject? userJobj = null;

             if (Config.Instance.JsonConfig.MessageOnline)
             {
                 string[] onlineFriendsArr = File.ReadAllLines(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Config.Instance.FriendsFile);
                 for (int i = 0; i < onlineFriendsArr.Length; i++)
                 {
                     Thread.Sleep(10);
                     if (onlineFriends.FirstOrDefault(x => x.ToString() == onlineFriendsArr[i]) != null) continue;
                     userJobj = JObject.FromObject(JObject.Parse(VRCWebRequest.Instance.SendVRCWebReq(VRCWebRequest.RequestType.Get, VRCInfo.VRCApiLink + VRCInfo.EndPoints.UserEndPoint + onlineFriendsArr[i])));
                     await Initialization.Instance.ChannelOnline.SendMessageAsync(new DiscordEmbedBuilder() { Title = "{ " + userJobj["displayName"].ToString() + " } Is now Online" , Description = "Status: " + userJobj["statusDescription"].ToString() , Color = new DiscordColor(Extentions.GetColorFromUserStatus(userJobj["status"].ToString())) });
                 }
                 userJobj = null;
             }

             if (Config.Instance.JsonConfig.MessageOffline)
             {
                 string onlineFriendsText = File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Config.Instance.FriendsFile);
                 for (int i = 0; i < onlineFriends.ToArray().Length; i++)
                 {
                     Thread.Sleep(10);
                     if (onlineFriendsText.Contains(onlineFriends[i].ToString())) continue;
                     userJobj = JObject.FromObject(JObject.Parse(VRCWebRequest.Instance.SendVRCWebReq(VRCWebRequest.RequestType.Get, VRCInfo.VRCApiLink + VRCInfo.EndPoints.UserEndPoint + onlineFriends[i].ToString())));
                     await Initialization.Instance.ChannelOnline.SendMessageAsync(new DiscordEmbedBuilder() { Title = "{ " + userJobj["displayName"].ToString() + " } Is now Offline", Description = "Status: " + userJobj["statusDescription"].ToString(), Color = new DiscordColor("#707070") });
                 }
                 userJobj = null;
             }

             string? FriendsList = string.Empty;
             for (int i = 0; i < onlineFriends.ToArray().Length; i++)
                 FriendsList += onlineFriends[i].ToString() + "\n";
             File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Config.Instance.OnlineFriends, FriendsList);
             FriendsList = null;
             GC.Collect();
         }*/


        /*public static async Task StateOrStatusChanged()
        {
            JObject User = null;

            List<Json.User>  usersInfo = JsonConvert.DeserializeObject<List<Json.User>>(File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Config.Instance.FriendsInfo));
          var friends = JObject.FromObject(JObject.Parse(VRCWebRequest.Instance.SendVRCWebReq(VRCWebRequest.RequestType.Get, VRCInfo.VRCApiLink + VRCInfo.EndPoints.LocalUser)))["friends"];
            for (int i = 0; i < usersInfo.Count; i++)
            {
               if (friends.FirstOrDefault(x => x.ToString() == usersInfo[i].Id) == null)
                {
                    usersInfo.Remove(usersInfo[i]);
                    continue;
                }
                Thread.Sleep(500);
                User = JObject.FromObject(JObject.Parse(VRCWebRequest.Instance.SendVRCWebReq(VRCWebRequest.RequestType.Get, VRCInfo.VRCApiLink + VRCInfo.EndPoints.UserEndPoint + usersInfo[i])));

                if (User["displayName"].ToString() != usersInfo[i].Name)
                {
                    await Initialization.Instance.ChannelStatus.SendMessageAsync(new DiscordEmbedBuilder() { Title = usersInfo[i].Name + " Changed His name to: " + User["displayName"].ToString() });
                }

            }


        }*/


    }
}
