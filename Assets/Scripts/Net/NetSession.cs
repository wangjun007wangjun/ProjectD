/********************************************************************
    created:	2020-04-15 				
    author:		OneJun						
    purpose:	游戏网络session 							
*********************************************************************/
using System.Text;
using Engine.Base;
using UnityEngine;
using UnityEngine.Networking;

namespace Net
{
    public delegate void NetSessionDelegate(bool isError, string rspJsonStr);
    public class NetSession
    {
        //超时
        private const int C_TimeOut = 10;
        //session Id
        public uint Id;
        //url
        public string Url;
        //
        public string Api;
        //请求参数
        public string ReqArgJson;
        //回调
        public NetSessionDelegate Handler;
        //管理使用
        public bool IsOver = false;

        private bool _isOver = false;
        private bool _isError = false;

        private UnityWebRequest _worker;

        private float _startTime;
        public void Begin(string xtoken, string postJson, string ac)
        {
            if (string.IsNullOrEmpty(Url))
            {
                GGLog.LogE("NetSession Begin Url Error");
                _isOver = true;
                _isError = true;
            }
            _worker = new UnityWebRequest(Url, "POST");
            _worker.SetRequestHeader("Content-Type", "application/json;charset=utf-8");
            _worker.SetRequestHeader("x-token", xtoken);
            if (!string.IsNullOrEmpty(ac))
            {
                _worker.SetRequestHeader("token", ac);
            }
            byte[] body = Encoding.UTF8.GetBytes(postJson);
            _worker.uploadHandler = new UploadHandlerRaw(body);
            _worker.downloadHandler = new DownloadHandlerBuffer();
            _worker.timeout = C_TimeOut;

            _isOver = false;
            _isError = false;
            _worker.SendWebRequest();
            #if APP_DEBUG
            _startTime=Time.realtimeSinceStartup;
            #endif
        }

        public void Update()
        {
            if (IsOver)
            {
                return;
            }
            if (_isError)
            {
                Stop();
                _isOver = true;
                IsOver = true;
                return;
            }
            if (_isOver)
            {
                Stop();
                IsOver = true;
                return;
            }
            if (_worker == null)
            {
                IsOver = true;
                return;
            }

            if (_worker.isDone)
            {
                if (_worker.isNetworkError || _worker.isHttpError)
                {
                    GGLog.LogE("NetSession Net Error:" + _worker.error);
                    Handler?.Invoke(true, null);
                    Handler = null;
                    _isOver = true;
                    _isError = true;
                    return;
                }
                else
                {
                    string rspStr = _worker.downloadHandler.text;
                    GGLog.LogD("<color=yellow>[Net RecvRsp]</color>:API:<color=yellow>{0}</color> RspStr:{1}", Api, rspStr);
                    #if APP_DEBUG
                    float tt=Time.realtimeSinceStartup-_startTime;
                    GGLog.LogD("<color=yellow>[Net Session]</color>:API:<color=yellow>{0}</color> Use:<color=yellow>{1}</color>", Api,tt);
                    #endif
                    Handler?.Invoke(false, rspStr);
                    Handler = null;
                    _isOver = true;
                    return;
                }
            }
        }

        public void Stop()
        {
            _isOver = true;
            if (_worker != null)
            {
                _worker.Abort();
                _worker = null;
            }
           
        }
    }
}