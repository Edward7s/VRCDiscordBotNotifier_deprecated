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
        public static async void AllFriends()
        {
            var Users = JsonConvert.DeserializeObject<List<Json.FriendGroup>>(VRCWebRequest.Instance.SendVRCWebReq(VRCWebRequest.RequestType.Get, VRCInfo.VRCApiLink + VRCInfo.EndPoints.FavoritesFriends));
            FriendList.Clear();
            for (int i = 0; i < Users.Count; i++)
            {
                Thread.Sleep(10);
                FriendList.Add(Users[i].favoriteId);
            }
            Users = null;
             var friends = JObject.FromObject(JObject.Parse(VRCWebRequest.Instance.SendVRCWebReq(VRCWebRequest.RequestType.Get, VRCInfo.VRCApiLink + VRCInfo.EndPoints.LocalUser)))["friends"];
            if (Config.Instance.JsonConfig.DmFriendEvent)
            {
                string friendsText = File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Config.Instance.FriendsFile);
                DiscordMember member = null;
                DiscordDmChannel dmChannel = null;
                string UserName = null;
                string[] ids = File.ReadAllLines(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Config.Instance.FriendsFile);
                for (int i = 0; i < ids.Length; i++)
                {
                    Thread.Sleep(100);
                    if (friends.FirstOrDefault(x => x.ToString() == ids[i]) != null) continue;
                    UserName = JObject.FromObject(JObject.Parse(VRCWebRequest.Instance.SendVRCWebReq(VRCWebRequest.RequestType.Get, VRCInfo.VRCApiLink + VRCInfo.EndPoints.UserEndPoint + ids[i])))["displayName"].ToString();
                    for (int j = 0; j < Config.Instance.JsonConfig.DmUsersId.Length; j++)
                    {
                        member = await Program.DiscordGuild.GetMemberAsync(ulong.Parse(Config.Instance.JsonConfig.DmUsersId[j]));
                        dmChannel = await member.CreateDmChannelAsync();
                        await dmChannel.SendMessageAsync(new DiscordEmbedBuilder() { Title = "{ " + UserName + " }  Unfriended you.".ToString(), Color = DiscordColor.Red, Description = "UserId: " + ids[i] } );
                    }
                }

                for (int i = 0; i < friends.ToArray().Length; i++)
                {
                    Thread.Sleep(100);
                    if (friendsText.Contains(friends[i].ToString())) continue;
                    UserName = JObject.FromObject(JObject.Parse(VRCWebRequest.Instance.SendVRCWebReq(VRCWebRequest.RequestType.Get, VRCInfo.VRCApiLink + VRCInfo.EndPoints.UserEndPoint + friends[i])))["displayName"].ToString();
                    for (int j = 0; j < Config.Instance.JsonConfig.DmUsersId.Length; j++)
                    {
                        member = await Program.DiscordGuild.GetMemberAsync(ulong.Parse(Config.Instance.JsonConfig.DmUsersId[j]));
                        dmChannel = await member.CreateDmChannelAsync();
                        await dmChannel.SendMessageAsync(new DiscordEmbedBuilder() { Title = "{ " + UserName + " }  Added you.".ToString(), Color = DiscordColor.Green, Description = "UserId: " + friends[i] });
                    }
                }
                member = null;
                dmChannel = null;
                ids = null;
            }

            string? FriendsList = string.Empty;
            for (int i = 0; i < friends.ToArray().Length; i++)
                FriendsList += friends[i].ToString() + "\n";
            File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Config.Instance.FriendsFile, FriendsList);
            FriendsList = null;
            friends = null;
            Thread.Sleep(15000);
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
