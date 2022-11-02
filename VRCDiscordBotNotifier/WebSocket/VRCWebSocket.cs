﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRCDiscordBotNotifier.Utils;
using WebSocketSharp;

namespace VRCDiscordBotNotifier.WebSocket
{
    internal class VRCWebSocket
    {

        public VRCWebSocket()
        {

            Console.WriteLine("Connecting To VRC ws....");
            using (var ws = new WebSocketSharp.WebSocket("wss://vrchat.com/?authToken=" + Config.Instance.JsonConfig.AuthCookie))
            {
                ws.SslConfiguration.EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12;
                ws.Connect();
                ws.OnClose += (s, e) =>
                {
                    try
                    {
                        Console.WriteLine("Re Connecting to VRC ws.");
                        ws.Connect();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                };
                ws.OnOpen += (s, e) => Console.WriteLine("Connected to the VRChat WebSocket.");
                ws.Log.Output = (s, e) => { };
                ws.OnMessage += Ws_OnMessage;
            }

        }

        private Json.WebSocket _wsJson { get; set; }
        public void Ws_OnMessage(object? sender, MessageEventArgs e)
        {
            _wsJson = JsonConvert.DeserializeObject<Json.WebSocket>(e.Data);
            switch (_wsJson.type)
            {
                case "friend-offline":
                    WebSocketMessageManager.Offline(JObject.FromObject(JObject.Parse(_wsJson.content))["userId"].ToString());
                    break;
                case "friend-online":
                    WebSocketMessageManager.Online(JObject.FromObject(JObject.Parse(JObject.FromObject(JObject.Parse(_wsJson.content))["user"].ToString())));
                    break;
                case "friend-location":
                    WebSocketMessageManager.Location(JObject.FromObject(JObject.Parse(_wsJson.content)));
                    break;
            }

        }
    }
}
