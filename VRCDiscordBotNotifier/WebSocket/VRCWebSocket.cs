using Newtonsoft.Json;
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
                try
                {
                    ws.Connect();
                }
                catch {

                    while (!ws.IsAlive)
                    {
                        Thread.Sleep(300);
                        try
                        {
                            ws.Connect();
                        }
                        catch { }
                        Console.WriteLine("!");

                    }
                }
                ws.OnClose += (s, e) =>
                {
                    try
                    {
                        Console.WriteLine("Re Connecting to VRC ws.");
                        try
                        {
                            ws.Connect();
                        }
                        catch
                        {

                            while (!ws.IsAlive)
                            {
                                Thread.Sleep(300);
                                try
                                {
                                    ws.Connect();
                                }
                                catch { }
                                Console.WriteLine("!");

                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                };
                var user = JObject.Parse(VRCWebRequest.Instance.SendVRCWebReq(VRCWebRequest.RequestType.Get, VRCInfo.VRCApiLink + VRCInfo.EndPoints.LocalUser));             
                FriendsMethods.CurrentInstanceId = new StringBuilder().AppendFormat("{0}:{1}", user["presence"]["world"], user["presence"]["instance"]).ToString();
                ws.OnOpen += (s, e) => Console.WriteLine("Connected to the VRChat WebSocket.");
                ws.Log.Output = (s, e) => { };
                ws.OnMessage += Ws_OnMessage;
            }
          
        }
        private WebSocketMessageManager _webSocketManager { get; } = new WebSocketMessageManager();
        private Json.WebSocket _wsJson { get; set; }
        public void Ws_OnMessage(object? sender, MessageEventArgs e)
        {
            _wsJson = JsonConvert.DeserializeObject<Json.WebSocket>(e.Data);
            switch (_wsJson.type)
            {
                case "friend-offline":
                    _webSocketManager.Offline(JObject.Parse(_wsJson.content)["userId"].ToString());
                    break;
                case "friend-online":
                    _webSocketManager.Online(JObject.Parse(JObject.Parse(_wsJson.content)["user"].ToString()));
                    break;
                case "friend-location":
                    _webSocketManager.Location(JObject.Parse(_wsJson.content));
                    break;
            }

        }
    }
}
