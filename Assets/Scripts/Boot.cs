/********************************************************************
  created:  2020-06-05         
  author:    OneJun           
  purpose:   程序启动根
*********************************************************************/
using UnityEngine;
using Engine.Base;
using Engine.State;
using Engine.Audio;
using Engine.Sys;
using Net;
using Data;
using Cfg;
using Lobby;

public class Boot : MonoBehaviour
{
    /// <summary>
    /// 入口
    /// </summary>
    protected void Start()
    {
        //不销毁
        ExtObject.ExtDontDestroyOnLoad(gameObject);
        GLog.LogD("--<color=yellow>Unity Start</color> --");
        //多点触摸
        Input.multiTouchEnabled = true;
        //启动框架
        GLog.LogD("--<color=yellow>Init JWEngine Common</color> --");
        EngineBoot.InitCommon(true);
        DG.Tweening.DOTween.Init(true, true, DG.Tweening.LogBehaviour.ErrorsOnly);
        //日志初始化 后续禁用Unity 的 GLog.LogD 
        // GLog.Initialize();
        GLog.LogD("--<color=yellow>Init JWEngine Framework</color> --");
        EngineBoot.InitFramework(true);
        //注册各个系统
        InitLogic(true);
        //Run
        GLog.LogD("---<color=yellow>Run Run Run</color> ---");
        Run();
    }

    private void InitLogic(bool isInit)
    {
        if (isInit)
        {
            CfgService.GetInstance();
            DataService.GetInstance();
            NetService.GetInstance();
            // SdkService.GetInstance();
            SysService.GetInstance();
            //添加各个系统
            // SysService.GetInstance().Create<SignInSys>();
            
        }
        else
        {

            SysService.DestroyInstance();
            StateService.DestroyInstance();
            NetService.DestroyInstance();
            DataService.DestroyInstance();
            CfgService.DestroyInstance();
            // SdkService.DestroyInstance();
        }
    }

    /// <summary>
    /// 开始游戏
    /// </summary>
    private void Run()
    {
        IState launch = new LaunchState();
        IState login = new LoginState();
        IState lobby = new LobbyState();
        IState gaming = new GameState();

        StateService.Instance.RegisteState(launch);
        StateService.Instance.RegisteState(login);
        StateService.Instance.RegisteState(lobby);
        StateService.Instance.RegisteState(gaming);
        //
        StateService.Instance.ChangeState(GConst.StateKey.Launch);
    }


    /// <summary>
    /// 统一驱动
    /// </summary>
    protected void Update()
    {
        //框架驱动
        EngineBoot.Update();
#if UNITY_EDITOR || APP_DEBUG
        CulculateFps();
#endif
    }

    /// <summary>
    /// 统一驱动
    /// </summary>
    protected void LateUpdate()
    {
        EngineBoot.LateUpdate();
        // if (Input.GetKeyDown(KeyCode.Escape))
        // {
        //     //TODO
        // }
    }

    /// <summary>
    /// 程序切前后台时调用
    /// </summary>
    /// <param name="pause"></param>
    protected void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            // if (StarData.IsValidate())
            // {
            //     StarData.GetInstance().TriggerPlayingLvDump();
            // }

            if (AudioService.IsValidate())
            {
                AudioService.GetInstance().CloseAll();
            }
        }
        else
        {
            if (AudioService.IsValidate())
            {
                AudioService.GetInstance().OpenAll();
            }
        }
    }

    /// <summary>
    /// 程序退出
    /// </summary>
    protected void OnApplicationQuit()
    {

        // if (StarData.IsValidate())
        // {
        //     StarData.GetInstance().TriggerPlayingLvDump();
        // }

        InitLogic(false);
        EngineBoot.Over();
    }


    //------------------------------------------
    float _fps = 0;
    int _frames = 0;
    float _lastInterval = 0;
    void CulculateFps()
    {
        ++_frames;
        float timeNow = Time.realtimeSinceStartup;
        if (timeNow > _lastInterval + 0.5)
        {
            _fps = (float)(_frames / (timeNow - _lastInterval));
            _frames = 0;
            _lastInterval = timeNow;
        }
    }
    GUIStyle _style;
#if APP_DEBUG
    void OnGUI()
    {
        if (_style == null)
        {
            _style = new GUIStyle();
            _style.normal.textColor = Color.red;
            _style.fontSize = 50;
        }
        string fps = "FPS:" + _fps;
        GUI.Label(new Rect(new Vector2(0, Screen.height - 60), new Vector2(200, 60)), new GUIContent(fps), _style);
        string desc = "APP_DEBUG";
        GUI.Label(new Rect(new Vector2(0, 0), new Vector2(600, 60)), new GUIContent(desc), _style);
    }
#endif

}

