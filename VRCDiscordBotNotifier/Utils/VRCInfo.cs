using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRCDiscordBotNotifier.Utils
{
    internal class VRCInfo
    {
        public static string ApiKey { get; } = "JlE5Jldo5Jibnk5O5hTx6XVqsJu4WJ26"; /* VRC ApiKey */
        public static string VRCApiLink { get; } = "https://vrchat.com/api/1/";
        public class EndPoints
        {
            public static string UserEndPoint { get; } = "users/"; /* Need UserId At The End */
            public static string LocalUser { get; } = "auth/user";
            public static string FriendsOnline { get; } = "auth/user/friends?offline=false&n=100";
            public static string Worlds { get; } = "worlds/"; /* WorldId */
            public static string Instance { get; } = "instances/"; /* InstanceId */
            public static string Avatar { get; } = "avatars/"; /* AvatarId */
            public static string Friends { get; } = "auth/user/friends";
            public static string FavoritesFriends { get; } = "favorites?type=friend&n=300";
            public static string Notifications { get; } = "auth/user/notifications?n=100";
            public static string SelfInvite { get; } = "invite/myself/to/"; /*  instance id  */

            public static string AcceptFriendReq(string ReqId) =>
                string.Format("auth/user/notifications/{0}/accept", ReqId);
           

            //notifications?n=100

        }
    }
}
