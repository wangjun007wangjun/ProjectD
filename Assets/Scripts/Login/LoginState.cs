/********************************************************************
	created:	2020/02/12
	author:		OneJun
	purpose:	游戏登录状态
*********************************************************************/
using Engine.UGUI;
using Engine.State;
using Engine.Native;
using Net;
using System;
using Data;
using UnityEngine;
using Engine.Event;
using Engine.Schedule;
using System.Collections;
using Engine.TimeSync;

public class LoginState : IState, IScheduleHandler
{

    //登录窗口
    private UILoginForm _loginForm;
    private Hashtable _uiParam;

    private uint _netCheckTimer;
    private uint _syncTimeTimer;
    private uint _loginRetryTimer;

    //最近登录信息
    [System.Serializable]
    private class LastLoginInfo
    {
        public string AccountId;
        public int PlayerId;
        public string PlayerName;
        public int AccountType;
        public string AvatarUrl;
    }

    //上次信息
    private LastLoginInfo _lastLoginInfo;
    //上次登录玩家信息保存路径
    private static readonly string S_LastLoginInfoFilePath = Engine.Res.FileUtil.CombinePath(Engine.Res.FileUtil.GetCachePath(), "last_login.bytes");

    public void InitializeState()
    {
        this.RemoveTimer();
    }

    public string Name()
    {
        return GConst.StateKey.Login;
    }

    public void OnStateEnter(object usrData = null)
    {
        //首次安装
        CheckIsFirstInstall();
        //
        GLog.LogD("Enter Update Game State");
        _loginForm = UIFormHelper.CreateFormClass<UILoginForm>(null, null, false);
        if (_uiParam == null)
        {
            _uiParam = new Hashtable();
            _uiParam.Add("tip", "Check Network Reachable !");
            var ranDom = UnityEngine.Random.Range(0.3f, 0.4f);
            _uiParam.Add("progress", ranDom);
        }
        _loginForm.UpdateUI("ShowProgress", _uiParam);
        //开始网络检查
        DoNetCheck();
    }

    public void OnStateLeave()
    {
        if (_loginForm != null)
        {
            UIFormHelper.DisposeFormClass(_loginForm);
            _loginForm = null;
        }
    }

    public void OnStateChanged(string srcSt, string curSt, object usrData)
    {

    }

    public void UninitializeState()
    {
        if (_loginForm != null)
        {
            UIFormHelper.DisposeFormClass(_loginForm);
            _loginForm = null;
        }
        _lastLoginInfo = null;
    }

    //检查是否是首次安装
    private void CheckIsFirstInstall()
    {
        string appV = VersionInfo.GetAppVersion();
        string recordV = PlayerPrefs.GetString("APP_Main_Version");
        if (!string.Equals(appV, recordV))
        {
            GLog.LogD("-------------First Install App-----------");
            Engine.Res.FileUtil.ClearDirectory(Engine.Res.FileUtil.GetIFSExtractPath());
            PlayerPrefs.SetString("APP_Main_Version", appV);
        }
    }

    /// <summary>
    /// 网络检查
    /// </summary>
    private void DoNetCheck()
    {
        bool isOk = NativeService.GetInstance().GetIsNetEnable();
        if (isOk)
        {
            _uiParam["tip"] = "Sync Server Time....";
            var ranDom = UnityEngine.Random.Range(0.6f, 0.7f);
            _uiParam["progress"] = ranDom;
            _loginForm.UpdateUI("ShowProgress", _uiParam);
            DoTimeSyncCheck();
        }
        else
        {
            //延迟等待网络
            _uiParam["tip"] = "Waiting Net Connect....";
            _uiParam["progress"] = 0.0f;
            _loginForm.UpdateUI("WaitProgress", _uiParam);
            this.RemoveTimer(_netCheckTimer);
            _netCheckTimer = this.AddTimer(2000, false);
        }
    }

    /// <summary>
    /// 时间同步
    /// </summary>
    private void DoTimeSyncCheck()
    {
        TimeSyncService.GetInstance().DoSyncSvrTime((ok) =>
        {
            if (ok)
            {
                _uiParam["tip"] = "Update Game Cfg....";
                var ranDom = UnityEngine.Random.Range(0.9f, 1f);
                _uiParam["progress"] = ranDom;
                _loginForm.UpdateUI("ShowProgress", _uiParam);
                DoUpdateCfg();
            }
            else
            {
                _uiParam["tip"] = "Sync Server Time Retry....";
                _uiParam["progress"] = 0.7f;
                _loginForm.UpdateUI("WaitProgress", _uiParam);
                this.RemoveTimer(_syncTimeTimer);
                _syncTimeTimer = this.AddTimer(2000, false);
            }
        }, 5);
    }

    //更新游戏配置
    private void DoUpdateCfg()
    {
        //ToDO

        DoLogin();
    }

    private void DoLogin()
    {
       
    }

    // private void OnLoginFormAction(string key, object param)
    // {
    //     if (key.Equals("GuestLogin"))
    //     {
    //         //默认游客
    //         if (_lastLoginInfo == null)
    //         {
    //             _lastLoginInfo = new LastLoginInfo();
    //             _lastLoginInfo.AccountId = NativeService.GetInstance().GetDeviceUUID();
    //             _lastLoginInfo.AccountType = (int)AccountType.Guest;
    //             _lastLoginInfo.PlayerName = GetRandomGuestName();
    //             _lastLoginInfo.AvatarUrl = "http://d11o5zml8iewlo.cloudfront.net/DiceMaster/Avatars/" + UnityEngine.Random.Range(0, 41) + ".png";
    //         }
    //         DoLoginSession();
    //         return;
    //     };
    //     if (key.Equals("FBLogin"))
    //     {
    //         try
    //         {
    //             FaceBookLogin();
    //         }
    //         catch (System.Exception)
    //         {
    //             UICommon.GetInstance().ShowBubble("facebook login error,try again");
    //         }
    //         return;
    //     };
    // }

    // //facebook
    // private void FaceBookLogin()
    // {
    //     SdkService.GetInstance().FaceBookLogin((fbRsp) =>
    //     {
    //         try
    //         {
    //             if (fbRsp.Status != 0)
    //             {
    //                 throw new Exception("login sdk error!");
    //             }
    //             _lastLoginInfo = new LastLoginInfo();
    //             _lastLoginInfo.AccountId = fbRsp.FBId;
    //             _lastLoginInfo.AccountType = (int)AccountType.FaceBook;
    //             _lastLoginInfo.PlayerName = fbRsp.FBName;
    //             _lastLoginInfo.AvatarUrl = fbRsp.FBAvatarUrl;
    //             DoLoginSession();
    //         }
    //         catch (System.Exception e)
    //         {
    //             GLog.LogD("Fb login fail err:" + e);
    //             throw e;
    //         }
    //     });
    // }

    //登录游戏服务器
    private void DoLoginSession()
    {
        //UICommon.GetInstance().ShowWaiting("Login", true, "login to server!");
        //
       
    }

    public void OnScheduleHandle(ScheduleType type, uint id)
    {

        if (id == _netCheckTimer)
        {
            DoNetCheck();
            return;
        }
        if (id == _syncTimeTimer)
        {
            DoTimeSyncCheck();
            return;
        }

        if (id == _loginRetryTimer)
        {
            DoLogin();
        }
    }

}
