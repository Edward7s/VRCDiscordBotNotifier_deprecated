using DiscordRPC;
using DiscordRPC.Logging;
using DSharpPlus.Entities;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRCDiscordBotNotifier.Utils
{
    internal class RichPresence : IDisposable
    {
        private DiscordRpcClient _client { get; set; }
        private JObject _localUser { get; set; }
        private JObject _worldInfo { get; set; }
        private string _worldStringInfo { get; set; }

        private Assets _assets { get; set; } = new Assets();

        private DiscordRPC.RichPresence _richPresence { get; } = new DiscordRPC.RichPresence();

        public RichPresence()
        {
            _client = new DiscordRpcClient(Config.Instance.JsonConfig.AplicationId, -1, null, false);
            _client.Logger = new ConsoleLogger() { Level = LogLevel.Warning };
            _client.Initialize();
            _richPresence.Assets = _assets;
            _richPresence.Buttons = new Button[] { new Button() {Label = "User Profile.", Url = $"https://vrchat.com/home/user/{Config.Instance.JsonConfig.UserId}"} };
            _client.SetPresence(_richPresence);
            _richPresence.Timestamps = new Timestamps() { StartUnixMilliseconds = (ulong)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds };
            Loop();
        }

        private void Loop()
        {
            if (!Config.Instance.JsonConfig.RichPresence)
            {
                Dispose();
                return;
            }
            _localUser = JObject.Parse(VRCWebRequest.Instance.SendVRCWebReq(VRCWebRequest.RequestType.Get, VRCInfo.VRCApiLink + VRCInfo.EndPoints.LocalUser));
            Thread.Sleep(300);
            _richPresence.State = new StringBuilder().AppendFormat("User: {0}, On: {1}", _localUser["displayName"] , Extentions.PlatformType((string)_localUser["presence"]["platform"])).ToString();
            _assets.LargeImageText = "Offline";
            _assets.LargeImageKey = "https://i.imgur.com/TcYu7kO.png";
            if (_localUser["presence"]["world"].ToString() != "offline" && _localUser["presence"]["world"].ToString() != "traveling")
            {
                Thread.Sleep(300);
                _worldInfo = JObject.Parse(VRCWebRequest.Instance.SendVRCWebReq(VRCWebRequest.RequestType.Get, VRCInfo.VRCApiLink + VRCInfo.EndPoints.Worlds + _localUser["presence"]["world"]));
                _worldStringInfo = new StringBuilder().AppendFormat("In: {0}, ", Extentions.InstanceType((string)_localUser["presence"]["instanceType"])).ToString();
                _assets.LargeImageText = new StringBuilder().AppendFormat("| {0} | Cap: {1} | Occupants: {2} | Fav: {3} | Visits: {4} | Heat: {5} | By: {6} |", _worldInfo["name"], _worldInfo["capacity"], _worldInfo["occupants"], _worldInfo["favorites"], _worldInfo["visits"], _worldInfo["heat"], _worldInfo["authorName"]).ToString();
                _assets.LargeImageKey = _worldInfo["imageUrl"].ToString();
            }
            _richPresence.Details = new StringBuilder().AppendFormat("{0}Status: {1}, Type: {2}", _worldStringInfo, _localUser["statusDescription"], _localUser["status"]).ToString();
            _assets.SmallImageKey = _localUser["currentAvatarImageUrl"].ToString();
            _assets.SmallImageText = _localUser["allowAvatarCopying"].ToString()  == "True" ? "Cloning On." : "Cloning Off";
            _client.SetPresence(_richPresence);
            _client.Invoke();
            _worldStringInfo = string.Empty;
            Thread.Sleep(5000);
            Loop();
        }

        public void Dispose()
        {
            _client.Dispose();
            _client = null;
            _localUser = null;
            _worldInfo = null;
            _worldStringInfo = null;
            _assets = null;
        }
    }
}
