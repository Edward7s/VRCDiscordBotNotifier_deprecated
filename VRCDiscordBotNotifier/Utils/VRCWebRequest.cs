﻿using DSharpPlus.Net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace VRCDiscordBotNotifier.Utils
{
    internal class VRCWebRequest
    {
        public static VRCWebRequest Instance { get; set; }
        public HttpWebRequest _httpWebRequest { get; set; }

        private CookieContainer s_cookies = new CookieContainer();
        public HttpWebResponse WebResponse { get; private set; }
        private string s_payload { get; set; } = string.Empty;
        private static HttpWebRequest? _testReq { get; set; }
        private static HttpWebResponse? _testResponse { get; set; }

        public static string TestReqest(string authCookie)
        {
            try
            {
                _testReq = (HttpWebRequest)WebRequest.Create(VRCInfo.VRCApiLink + VRCInfo.EndPoints.LocalUser);
                _testReq.CookieContainer = new CookieContainer();
                _testReq.CookieContainer.Add(new Cookie() { Name = "apiKey", Value = "JlE5Jldo5Jibnk5O5hTx6XVqsJu4WJ26", Domain = "vrchat.com" });
                _testReq.CookieContainer.Add(new Cookie() { Name = "auth", Value = authCookie, Domain = "vrchat.com" });
                _testReq.Method = "Get";
                _testReq.UserAgent = "Mozilla/5.0 (Linux; Android 6.0; Nexus 5 Build/MRA58N) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/104.0.5112.102 Mobile Safari/537.36";
                _testReq.ContentType = "application/json";
                _testReq.Accept = "application/json, text/plain, */*";
                _testReq.SendChunked = true;
                _testReq.ContentLength = 0;
                _testResponse = (HttpWebResponse)_testReq.GetResponse();
                return _testResponse.StatusCode.ToString();
            }
            catch
            {
                return "401";
            }
            
        }
        public enum RequestType
        {
            Put,
            Get,
            Post
        }
        public VRCWebRequest()
        {
            Instance = this;
            s_cookies.Add(new Cookie() { Name = "apiKey", Value = "JlE5Jldo5Jibnk5O5hTx6XVqsJu4WJ26", Domain = "vrchat.com" });
            s_cookies.Add(new Cookie() { Name = "auth", Value = Config.Instance.JsonConfig.AuthCookie, Domain = "vrchat.com" });
        }
        public string SendVRCWebReq(RequestType req, string url, object? payload = null)
        {
            bool toggle = true;
            while (toggle)
            {
                try
                {
                    s_payload = string.Empty;
                    if (_httpWebRequest != null)
                        _httpWebRequest = null;
                    if (payload != null)
                        s_payload = JsonConvert.SerializeObject(payload);
                    _httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                    _httpWebRequest.CookieContainer = s_cookies;
                    _httpWebRequest.Method = req.ToString();
                    _httpWebRequest.UserAgent = "Mozilla/5.0 (Linux; Android 6.0; Nexus 5 Build/MRA58N) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/104.0.5112.102 Mobile Safari/537.36";
                    _httpWebRequest.ContentType = "application/json";
                    _httpWebRequest.Accept = "application/json, text/plain, */*";
                    _httpWebRequest.SendChunked = true;
                    _httpWebRequest.ContentLength = s_payload == null ? 0 : s_payload.Length;
                    _httpWebRequest.AutomaticDecompression = DecompressionMethods.GZip;
                    _httpWebRequest.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate, br");
                    if (s_payload != string.Empty)
                    {
                        using (var writer = new StreamWriter(_httpWebRequest.GetRequestStream(), Encoding.UTF8))
                            writer.Write(s_payload);
                    }
                    WebResponse = (HttpWebResponse)_httpWebRequest.GetResponse();
                    toggle = false;
                    using (var reader = new StreamReader(WebResponse.GetResponseStream(), ASCIIEncoding.UTF8))
                        return reader.ReadToEnd();
                }
                catch(Exception ex)
                {
                    _httpWebRequest = null;
                    if (WebResponse != null)
                         WebResponse.Dispose();
                     ConsoleManager.Write(string.Format("ERROR ON REQ: {0}", url));
                }
                Thread.Sleep(600);
            }
            return null;
          
        }
    }
}
