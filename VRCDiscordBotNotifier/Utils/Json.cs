using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRCDiscordBotNotifier.Utils
{
    internal class Json
    {
        [Serializable]
        public class Config
        {
            public string BotToken { get; set; }
            public string DiscordServerId { get; set; }
            public string AuthCookie { get; set; }
            public string UserId { get; set; }
            public string[] DmUsersId { get; set; }
            public bool DmFriendEvent { get; set; }
            public bool DmNewNotifications { get; set; }
            public bool FriendsActivity { get; set; }
            public bool FirstTime { get; set; }

        }

        public class User
        {
            public string Name { get; set; }
            public string Id { get; set; }
            public string Status { get; set; }
            public string State { get; set; }

        }

        public class WebSocket
        {
            public string type { get; set; }

            public string content { get; set; }
        }

        public class FriendGroup
        {
            public string id { get; set; }
            public string type { get; set; }
            public string favoriteId { get; set; }

        }
    }
}
