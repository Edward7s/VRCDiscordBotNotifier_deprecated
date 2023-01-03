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
            ConsoleManager.Write("Starting Friends Checker...");
            while (true)
            {
                _users = JsonConvert.DeserializeObject<List<Json.FriendGroup>>(VRCWebRequest.Instance.SendVRCWebReq(VRCWebRequest.RequestType.Get, VRCInfo.VRCApiLink + VRCInfo.EndPoints.FavoritesFriends));
                Thread.Sleep(2500);
                FriendList.Clear();
                for (int i = 0; i < _users.Count; i++)
                    FriendList.Add(_users[i].favoriteId);

                _users = null;//onlineFriends
                _localUser = JObject.Parse(VRCWebRequest.Instance.SendVRCWebReq(VRCWebRequest.RequestType.Get, VRCInfo.VRCApiLink + VRCInfo.EndPoints.LocalUser));
                _friends = _localUser["friends"];
                CurrentInstanceId = String.Format("{0}:{1}", _localUser["presence"]["world"], _localUser["presence"]["instance"]).ToString();
                _localUser = null;
                Thread.Sleep(2500);
                if (Config.Instance.JsonConfig.DmFriendEvent)
                {
                    string friendsText = Filemanager.ReadFile(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Config.Instance.FriendsFile);
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
                            await dmChannel.SendMessageAsync(new DiscordEmbedBuilder() { Title = String.Format("{{ {0} }} Removed You From Friends.", User["displayName"]).ToString(), Color = DiscordColor.Red, Description = String.Format("UserId: {0}", User["id"]).ToString() });

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
                            await dmChannel.SendMessageAsync(new DiscordEmbedBuilder() { Title = String.Format("{{ {0} }} Added You.", User["displayName"]).ToString(), Color = DiscordColor.Green, Description = String.Format("UserId: {0}", User["id"]).ToString() });

                        }
                    }
                    member = null;
                    dmChannel = null;
                    ids = null;
                }
                s_friendList = string.Empty;
                for (int i = 0; i < _friends.ToArray().Length; i++)
                    s_friendList += _friends[i].ToString() + "\n";
                Filemanager.WriteFile(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Config.Instance.FriendsFile, s_friendList);
                s_friendList = null;
                _friends = null;
                Thread.Sleep(50000);
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }   
    }
}
