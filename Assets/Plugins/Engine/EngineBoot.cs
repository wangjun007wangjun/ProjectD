/********************************************************************
	created:	2020/02/12
	author:		OneJun
	
	purpose:	
*********************************************************************/
using System;
using Engine.Base;
using Engine.Res;
using Engine.Http;
using Engine.Schedule;
using Engine.State;
using Engine.Event;
using Engine.Audio;
using Engine.Asset;
using Engine.UGUI;
using Engine.Native;
using Engine.TimeSync;
using Engine.Sys;
using Engine.Cor;
using Engine.Localize;


public static class EngineBoot
{
    /// <summary>
    /// 初始化公共层
    /// </summary>
    /// <param name="initialize">初始化//反初始化</param>
    public static void InitCommon(bool initialize)
    {
        if (initialize)
        {
            UnityEngine.Random.InitState((int)DateTime.Now.Ticks);
            // GGLog.GetInstance();
            //Engine.Res.FileUtil.Proxy = new JWEngineFileUtilProxy();
            ResService.GetInstance();
            LocalizeService.GetInstance();
        }
        else
        {
            LocalizeService.DestroyInstance();
            ResService.DestroyInstance();
            // GGLog.DestroyInstance();
        }
    }

    /// <summary>
    /// 初始化框架层
    /// </summary>
    /// <param name="initialize">初始化/反初始化</param>
    public static void InitFramework(bool initialize)
    {
        if (initialize)
        {
            ScheduleService.GetInstance();
            CorService.GetInstance();
            NativeService.GetInstance();
            AssetService.GetInstance();
            EventService.GetInstance();
            StateService.GetInstance();
            HttpService.GetInstance();
            UGUIRoot.GetInstance();
            UICommon.GetInstance();
            AudioService.GetInstance();
            TimeSyncService.GetInstance();
            SysService.GetInstance();
        }
        else
        {
            SysService.DestroyInstance();
            StateService.DestroyInstance();
            UICommon.DestroyInstance();
            UGUIRoot.DestroyInstance();
            TimeSyncService.DestroyInstance();
            EventService.DestroyInstance();
            HttpService.DestroyInstance();
            AudioService.DestroyInstance();
            NativeService.DestroyInstance();
            AssetService.GetInstance().Destroy();
            AssetService.DestroyInstance();
            CorService.DestroyInstance();
            ScheduleService.DestroyInstance();
        }
    }

    public static void Update()
    {
        ScheduleService.GetInstance().Update();
        UGUIRoot.GetInstance().CustomUpdate();
    }

    public static void LateUpdate()
    {
        
    }

    public static void Over()
    {
        InitFramework(false);
        InitCommon(false);
        SingletonManager.Clear();
    }
}

