/********************************************************************
	created:	2020/02/14
	author:		OneJun
	purpose:	负责游戏Http 协议收发
*********************************************************************/
using System;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Networking;
using Engine.Base;
using System.Text;
using Engine.TimeSync;
using Engine.Schedule;
using System.Collections.Generic;

namespace Net
{
    public class NetService : Engine.Base.Singleton<NetService>, IScheduleHandler
    {
        //登录成功后的 统一 token
        public string AccessToken { get; set; }
        //请求超时时间
        private int _timeOutSec = 10;
        //包ID
        private string _appBundleId = "";
        //空参数json
        private string _emptyParam = "";
        //
        private uint _currentId = 0;
        private readonly ArrayList<NetSession> _netSessions = new ArrayList<NetSession>(3);

        public override bool Initialize()
        {
            _appBundleId = Application.identifier;
            string ret = "{}";
            _emptyParam = ret.Insert(1, "\"package\":\"" + _appBundleId + "\"");
            this.AddUpdate();
            return true;
        }

        public override void Uninitialize()
        {
            this.RemoveUpdate();
            _currentId = 0;
            _netSessions.Clear();
        }

        /// <summary>
        /// 发送请求到服务器
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="api"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        public uint SendNetPostReq(string api, NetReq req, NetSessionDelegate hander)
        {
            string fullUrl = NetDeclare.SERVER_URL + api;
            string bodyStr = string.Empty;
            long timeStamp = TimeSyncService.GetInstance().GetCurSvrTimeStamp();
            if (req == null)
            {
                bodyStr = _emptyParam;
            }
            else
            {
                string reqStr = JsonUtility.ToJson(req, false);
                string last = "\",";
                if(reqStr.Equals("{}"))
                {
                    last = "\"";
                }
                bodyStr = reqStr.Insert(1, "\"timestamp\":" + (int)timeStamp + "," + "\"package\":\"" + _appBundleId + last);
            }
            //url
            fullUrl = System.Uri.EscapeUriString(fullUrl);
            //token
            var x_token = CreateToken(bodyStr, timeStamp);
            //事务
            NetSession session = new NetSession();
            session.Api = api;
            session.Url = fullUrl;
            session.Handler = hander;
            session.Id = _currentId++;
            //send
            GGLog.LogE("<color=yellow>[Net SendReq]</color>:API:<color=yellow>{0}</color> Body:{1}", api, bodyStr);
            session.Begin(x_token, bodyStr, AccessToken);
            _netSessions.Add(session);
            return session.Id;
        }

        /// <summary>
        /// 取消网络请求 
        /// </summary>
        /// <param name="sid">事务id</param>
        public void CancelNetPostReq(uint sid)
        {
            for (int i = 0; i < _netSessions.Count; i++)
            {
                NetSession session = _netSessions[i];
                if (session.Id == sid)
                {
                    session.Stop();
                    session.IsOver = true;
                    break;
                }
                _netSessions[i] = session;
            }
            Clean();
        }

        public void OnScheduleHandle(ScheduleType type, uint id)
        {
            if (_netSessions == null || _netSessions.Count <= 0)
            {
                return;
            }
            for (int i = 0; i < _netSessions.Count; i++)
            {
                NetSession ss = _netSessions[i];
                if (ss != null)
                {
                    ss.Update();
                }
                _netSessions[i] = ss;
            }
            Clean();
        }

        private void Clean()
        {
            for (int i = _netSessions.Count - 1; i >= 0; --i)
            {
                if (_netSessions[i].IsOver)
                {
                    _netSessions.RemoveAt(i);
                }
            }
        }

        private string CreateToken(string para, double timeStamp)
        {
            string source = para + NetDeclare.SECRET + timeStamp;
            using (MD5 md5Hash = MD5.Create())
            {
                string token = GetMd5Hash(md5Hash, source);
                return token;
            }
        }

        private string GetMd5Hash(MD5 md5Hash, string input)
        {
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }
    }
}

