/********************************************************************
	created:	2020/02/12
	author:		OneJun
	
	purpose:	统一的日志打印 后续对接bugly
*********************************************************************/
using System.Diagnostics;
using UnityEngine;
using Engine.Base;

public static class GLog
{
    //日志等级 默认错误才输出 Release
    public static GGLog.Type LogLevel = GGLog.Type.Error;

    public static bool Initialize()
    {
        if(GConst.IsEditor){
            LogLevel = GGLog.Type.Debug;
        }else if(GConst.IsDebug){
            LogLevel = GGLog.Type.Debug;
        }else{
            LogLevel = GGLog.Type.None;
        }
        GGLog.GetInstance().SetConfig(LogLevel, Engine.Res.FileUtil.GetCachePath());
        //挂接
        Application.logMessageReceived += Application_LogMessageReceived;
        return true;
    }

    //挂接异常输出
    private static void Application_LogMessageReceived(string condition, string stackTrace, LogType type)
    {
        if (type == LogType.Exception)
        {
            GGLog.LogE("Crash:" + condition + "\n" + stackTrace);
        }
    }

    //注意 条件是 或的关系
    [Conditional("UNITY_EDITOR"), Conditional("APP_DEBUG")]
    public static void LogD(string content)
    {
        GGLog.LogD(content);
    }

    //注意 条件是 或的关系
    [Conditional("UNITY_EDITOR"), Conditional("APP_DEBUG")]
    public static void LogD(string format, params object[] args)
    {
        GGLog.LogD(format, args);
    }

    //注意 条件是 或的关系
    [Conditional("UNITY_EDITOR"), Conditional("APP_DEBUG")]
    public static void LogW(string content)
    {
        GGLog.LogW(content);
    }

    //注意 条件是 或的关系
    [Conditional("UNITY_EDITOR"), Conditional("APP_DEBUG")]
    public static void LogW(string format, params object[] args)
    {
        GGLog.LogW(format, args);
    }

    //注意 条件是 或的关系
    [Conditional("UNITY_EDITOR"), Conditional("APP_DEBUG"), Conditional("APP_RELEASE")]
    public static void LogE(string content)
    {
        GGLog.LogE(content);
    }

    //注意 条件是 或的关系
    [Conditional("UNITY_EDITOR"), Conditional("APP_DEBUG"), Conditional("APP_RELEASE")]
    public static void LogE(string format, params object[] args)
    {
        GGLog.LogE(format, args);
    }

}


