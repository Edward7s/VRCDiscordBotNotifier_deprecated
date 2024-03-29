﻿using System;
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
            public bool DmOnFriendJoin { get; set; }
            public bool RichPresence { get; set; }
            public string AplicationId { get; set; }
            public bool DmFavoriteFriendOnlline { get; set; }

        }

        public class User
        {
            public string Name { get; set; }
            public string Id { get; set; }
            public string Status { get; set; }
            public string State { get; set; }

        }

        public class Friend
        {
            public string displayName { get; set; }
            public string id { get; set; }
            public string location { get; set; }
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
        public class Notification
        {
            public string id { get; set; }
            public string senderUsername { get; set; }
            public string type { get; set; }
            public string details { get; set; }
            public string created_at { get; set; }

        }
    }
}
