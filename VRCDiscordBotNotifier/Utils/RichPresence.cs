﻿using DiscordRPC;
using DiscordRPC.Logging;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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

        private DateTime _time { get; set; }

        private string? _userData { get; set; }
        private string _lastWorld { get; set; } = string.Empty;
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
            Task.Run(()=> Loop());
        }

        private void Loop()
        {
            //To Fix
            while (true)
            {
                if (!Config.Instance.JsonConfig.RichPresence)
                {
                    Dispose();
                    return;
                }
                try
                {
                    _userData = VRCWebRequest.Instance.SendVRCWebReq(VRCWebRequest.RequestType.Get, VRCInfo.VRCApiLink + VRCInfo.EndPoints.LocalUser);
                    if (_userData == null || _userData.Length < 400)
                    {
                        Thread.Sleep(6000);
                        continue;
                    }
                    try
                    {
                        _localUser = JObject.Parse(_userData);
                    }
                    catch (JsonReaderException ex)
                    {
                        Console.WriteLine(ex);
                        Thread.Sleep(6000);
                        continue;
                    }
                    Thread.Sleep(300);
                    _richPresence.State = String.Format("👤User: {0}, On: {1}, Friends: {2}/{3}", _localUser["displayName"], Extentions.PlatformType((string)_localUser["presence"]["platform"]), _localUser["onlineFriends"].ToArray().Length, _localUser["friends"].ToArray().Length).ToString();
                    if (_lastWorld != _localUser["presence"]["world"].ToString())
                    {
                        _assets.LargeImageText = string.Format("Offline 🛏 , Last LogIn: {0}", DateTime.Parse(_localUser["last_login"].ToString()).ToLocalTime());
                        _assets.LargeImageKey = "https://raw.githubusercontent.com/Edward7s/AutoUpdatorForDiscordBot/master/dribbble.gif";
                        if (_localUser["presence"]["world"].ToString() == "traveling")
                        {
                            _assets.LargeImageKey = "https://raw.githubusercontent.com/Edward7s/AutoUpdatorForDiscordBot/master/Train.gif";
                            _assets.LargeImageText = "Joining A World 🚆";
                            _worldStringInfo = "Joining A World, ";
                        }
                        else if (_localUser["presence"]["world"].ToString() != "offline")
                        {
                            _time = DateTime.Now;
                            _lastWorld = _localUser["presence"]["world"].ToString();
                            Thread.Sleep(300);
                            _worldInfo = JObject.Parse(VRCWebRequest.Instance.SendVRCWebReq(VRCWebRequest.RequestType.Get, VRCInfo.VRCApiLink + VRCInfo.EndPoints.Worlds + _localUser["presence"]["world"]));
                            _worldStringInfo = String.Format("🏠In: {0} ", Extentions.InstanceType((string)_localUser["presence"]["instanceType"]));
                            _assets.LargeImageText = string.Format("{0} |Cap: {1} |👥: {2} |🖤: {3} |Visits: {4} |🔥: {5} |By: {6} |🕒: {7}", _worldInfo["name"], _worldInfo["capacity"], _worldInfo["occupants"], _worldInfo["favorites"], _worldInfo["visits"], _worldInfo["heat"], _worldInfo["authorName"], DateTime.Parse(_worldInfo["created_at"].ToString()).ToLocalTime()).ToString();
                            _assets.LargeImageKey = _worldInfo["imageUrl"].ToString();
                        }
                        else
                            _worldStringInfo = string.Empty;


                        if (_worldStringInfo != string.Empty && _worldStringInfo != "Joining A World, ")
                            _richPresence.Details = string.Format("{0}🕒For: {1}, State: {2}", _worldStringInfo, ((TimeSpan)(DateTime.Now - _time)).ToString(@"hh\:mm\:ss"), _localUser["status"]).ToString();
                        else
                            _richPresence.Details = String.Format("{0}State: {1}", _worldStringInfo, _localUser["status"]).ToString();

                    }
                    if (_richPresence.Details.Contains("For:"))
                        _richPresence.Details = string.Format("{0}🕒For: {1}, State: {2}", _worldStringInfo, ((TimeSpan)(DateTime.Now - _time)).ToString(@"hh\:mm\:ss"), _localUser["status"]).ToString();
                    else
                        _worldStringInfo = string.Empty;

                    _assets.SmallImageKey = _localUser["currentAvatarImageUrl"].ToString();
                    _assets.SmallImageText = string.Format("🗒: {0}, {1}", _localUser["statusDescription"], _localUser["allowAvatarCopying"].ToString() == "True" ? "Cloning On." : "Cloning Off");
                    _client.SetPresence(_richPresence);
                    _client.Invoke();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                Thread.Sleep(6000);
            }
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
