/********************************************************************
	created:	2020/02/14
	author:		OneJun
	
	purpose:	统一的时间同步服务
*********************************************************************/
using Engine.Base;
using System;
using UnityEngine;
using Engine.Schedule;
using Engine.Http;
using Engine.Event;

namespace Engine.TimeSync
{

    [Serializable]
    public class TimeSycRsp
    {
        [SerializeField]
        public long CurrentTime;
    }

    public struct SvrTimeInfo
    {
        //游戏启动经过时间 Time.realtimeSinceStartup
        public float StartupOffset;
        //服务器日期
        public DateTime SvrDateTime;
        //服务器时间戳
        public long SvrTimeStamp;
    }

    public delegate void TimeSyncHeartbeatCallBack(long curSvrTSec);

    public class TimeSyncService : Singleton<TimeSyncService>,IScheduleHandler
    {
        //Utc时间起
        private static readonly DateTime S_UtcOri = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        //时间同步地址
        private const string C_TimeURL = "https://mnru0dzqtj.execute-api.us-west-2.amazonaws.com/v1/now";
        //定时同步间隔 5分钟
        private const int C_SyncIntervalSec = 5*60;
        
        //当前时间信息
        private SvrTimeInfo _curTimeInfo;
        
        //心跳回调
        private event TimeSyncHeartbeatCallBack _heartbeatEvent;

        public override bool Initialize()
        {
            _curTimeInfo = new SvrTimeInfo();
            //心跳
            this.AddTimer(C_SyncIntervalSec * 1000, true);
            //first
            //SyncSvrTime();
            return true;
        }

        public override void Uninitialize()
        {
            this.RemoveTimer();
            _heartbeatEvent=null;
            return ;
        }

        /// <summary>
        /// 添加心跳回调
        /// </summary>
        /// <param name="handler"></param>
        public void AddHearbeatCallBack(TimeSyncHeartbeatCallBack handler){
            _heartbeatEvent+=handler;
        }

        /// <summary>
        /// 移除心跳回调
        /// </summary>
        /// <param name="hander"></param>
        public void RemoveHearbeatCallBack(TimeSyncHeartbeatCallBack hander){
            _heartbeatEvent-=hander;
        }

        /// <summary>
        /// 当前服务器日期
        /// </summary>
        /// <returns></returns>
        public DateTime GetCurSvrDateTime()
        {
            return _curTimeInfo.SvrDateTime.AddSeconds(UnityEngine.Time.realtimeSinceStartup - _curTimeInfo.StartupOffset);
        }

        /// <summary>
        /// 当前服务器时间戳 秒
        /// </summary>
        /// <returns></returns>
        public long GetCurSvrTimeStamp()
        {
            return (long)_curTimeInfo.SvrTimeStamp + Mathf.RoundToInt(UnityEngine.Time.realtimeSinceStartup - _curTimeInfo.StartupOffset);
        }

        /// <summary>
        /// 提供给外部初始化调用
        /// </summary>
        /// <param name="callback"></param>
        public void DoSyncSvrTime(Action<bool> callback,int timeout)
        {
            HttpService.GetInstance().AsyncGetText(C_TimeURL, (ret) =>
            {
                if (string.IsNullOrEmpty(ret) || ret.Equals("error"))
                {
                    callback?.Invoke(false);
                }
                else
                {
                    TimeSycRsp rsp = new TimeSycRsp();
                    JsonUtility.FromJsonOverwrite(ret, rsp);
                    if (rsp.CurrentTime != 0)
                    {
                        DateTime time = TimeHelper.UnixTimeStampToDateTime(rsp.CurrentTime);
                        _curTimeInfo.SvrDateTime = time;
                        _curTimeInfo.SvrTimeStamp = rsp.CurrentTime;
                        _curTimeInfo.StartupOffset = UnityEngine.Time.realtimeSinceStartup;
                        callback?.Invoke(true);
                    }
                    else
                    {
                        callback?.Invoke(false);
                    }
                }
            });
        }

        //同步服务器时间
        private void SyncSvrTime()
        {
            HttpService.GetInstance().AsyncGetText(C_TimeURL, (ret) =>
            {
                if (string.IsNullOrEmpty(ret) || ret.Equals("error"))
                {
                    GGLog.LogE("TimeService Sync Failed!");
                }
                else
                {
                    TimeSycRsp rsp = new TimeSycRsp();
                    JsonUtility.FromJsonOverwrite(ret, rsp);
                    if (rsp.CurrentTime != 0)
                    {
                        DateTime time = TimeHelper.UnixTimeStampToDateTime(rsp.CurrentTime);
                        _curTimeInfo.SvrDateTime = time;
                        _curTimeInfo.SvrTimeStamp = rsp.CurrentTime;
                        _curTimeInfo.StartupOffset = UnityEngine.Time.realtimeSinceStartup;
                        GGLog.LogD("<color=yellow>Sync Svr Time Back:</color>" + rsp.CurrentTime + "<color=red>@@@</color>" + time);
                        //JWLog.LogD("Device Local UTC && Device Local Now:" + System.DateTime.UtcNow + "<color=red>%%%</color>" + System.DateTime.Now);
                        //DateTime svrT2Local = TimeZoneInfo.ConvertTime(time, TimeZoneInfo.Local);
                        //JWLog.LogD("SvrUTC 2 LocaZone:" + svrT2Local);
                        if(_heartbeatEvent!=null){
                            _heartbeatEvent(_curTimeInfo.SvrTimeStamp);
                        }
                    }
                }
            });
        }

        public void OnScheduleHandle(ScheduleType type, uint id)
        {
            SyncSvrTime();
        }
    }
}