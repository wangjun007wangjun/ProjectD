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
    private uint _switchTimer;


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
        // CheckIsFirstInstall();
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
       string localInfo = "";
        //上次登陆过
        localInfo = Engine.Res.FileUtil.ReadFileText(S_LastLoginInfoFilePath);
        //有上次登陆信息
        if (!string.IsNullOrEmpty(localInfo))
        {
            bool isOk = false;
            try
            {
                _lastLoginInfo = JsonUtility.FromJson<LastLoginInfo>(localInfo);
                if (string.IsNullOrEmpty(_lastLoginInfo.AccountId))
                {
                    _lastLoginInfo.AccountId = GetUDID();
                }
                isOk = true;
            }
            catch (Exception)
            {
                isOk = false;
            }
            if (isOk && (_lastLoginInfo != null))
            {
                //自动登陆
                DoLoginSession();
            }
            else
            {
                //游客登录
                _lastLoginInfo = new LastLoginInfo();
                _lastLoginInfo.AccountId = GetUDID();
                _lastLoginInfo.PlayerName = GetRandomGuestName();
                DoLoginSession();
            }
        }
        else
        {
            //游客登录
            _lastLoginInfo = new LastLoginInfo();
            _lastLoginInfo.AccountId = GetUDID();
            _lastLoginInfo.PlayerName = GetRandomGuestName();
            DoLoginSession();
        }
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
       LoginReq req = new LoginReq();
        req.account_id = _lastLoginInfo.AccountId;
        req.name = _lastLoginInfo.PlayerName;
        req.device_id = GetUDID();
        //请求
        NetService.GetInstance().SendNetPostReq(NetDeclare.LoginAPI, req, this.OnLoginSvrRsp);
    }

    
    //登录服务器 回包
    private void OnLoginSvrRsp(bool isError, string rspStr)
    {
        if (isError)
        {
            UICommon.GetInstance().ShowBubble("出错：" + rspStr);
            //UICommon.GetInstance().CleanWaiting();
            _loginRetryTimer = this.AddTimer(1000, false);
            return;
        }
        LoginRsp loginRsp = JsonUtility.FromJson<LoginRsp>(rspStr);
        if (loginRsp.code != 0)
        {
            UICommon.GetInstance().ShowBubble("出错 code：" + loginRsp.code.ToString());

            GLog.LogE("code:" + loginRsp.code + "   message:" + loginRsp.message);
            UICommon.GetInstance().CleanWaiting();
            _loginRetryTimer = this.AddTimer(1000, false);
            return;
        }
        //token
        NetService.GetInstance().AccessToken = loginRsp.login_token;
        //自己数据
        UserInfo Me = DataService.GetInstance().Me;
        Me.AccountId = _lastLoginInfo.AccountId;

        Me.InitByRsp(loginRsp);
        //记录上次登录用户信息
        _lastLoginInfo.PlayerId = Me.PlayerId;
        string meJson = JsonUtility.ToJson(_lastLoginInfo);
        byte[] data = System.Text.Encoding.UTF8.GetBytes(meJson);
        Engine.Res.FileUtil.WriteFile(S_LastLoginInfoFilePath, data, true);
        //数据处理
        LoginPostSession(loginRsp);
    }

    /// <summary>
    /// 游戏登录成功 后处理 加载各种本地游戏 游戏数据
    /// </summary>
    private void LoginPostSession(LoginRsp data)
    {
        //大厅
        UICommon.GetInstance().CleanWaiting();

        var ranDom = 1f;
        _uiParam["progress"] = ranDom;
        _loginForm?.UpdateUI("ShowProgress", _uiParam);
        //
        _switchTimer = this.AddTimer(1000, false);
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
        if (id == _switchTimer)
        {
            StateService.Instance.ChangeState(GConst.StateKey.Menu);
        }
    }
    public string GetUDID()
    {
        return UnityEngine.SystemInfo.deviceUniqueIdentifier;
    }
    
    private string GetRandomGuestName()
    {
        var nameId = UnityEngine.Random.Range(0, 10000000);
        return "Player_" + nameId;
    }
}
