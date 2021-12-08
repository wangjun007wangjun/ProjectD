/********************************************************************
  created:  2020-06-05         
  author:    OneJun           
  purpose:  全局常量定义                
*********************************************************************/
public static class GConst
{
    public static bool IsDebug
    {
        get
        {
#if APP_DEBUG
            return true;
#else
            return false;
#endif
        }
    }

    public static bool IsEditor
    {
        get
        {
#if UNITY_EDITOR
            return true;
#else
        return false;
#endif

        }
    }


    public static bool IsWin
    {
        get
        {
#if UNITY_STANDALONE_WIN
            return true;
#else
            return false;
#endif
        }
    }

    public static bool IsIphone
    {
        get
        {
#if UNITY_IPHONE
            return true;
#else
            return false;
#endif
        }
    }

    public static bool IsAndroid
    {
        get
        {
#if UNITY_ANDROID
            return true;
#else
            return false;
#endif
        }
    }

    public static string PlatName
    {
        get
        {
#if UNITY_STANDALONE_WIN
        return "Win";
#elif UNITY_STANDALONE_OSX
            return "Mac";
#elif UNITY_IOS
            return "IOS";
#elif UNITY_ANDROID
            return "Android";
#else
        return "NotSupport";
#endif
        }
    }

    //IOS白包版本
    public static bool IsIosExp
    {
        get
        {
#if APP_EXP && UNITY_IPHONE
        return true;
#else
            return false;
#endif
        }
    }

    //Android 白包版本
    public static bool IsAndroidExp
    {
        get
        {
#if UNITY_ANDROID && APP_EXP
        return true;
#else
            return false;
#endif
        }
    }


    public static bool IsUseAres
    {
        get
        {
#if USE_ARES
        return true;
#else
            return false;
#endif
        }
    }

    public class StateKey
    {
        public static readonly string Launch = "StateLaunch";
        public static readonly string Login = "StateLogin";
        public static readonly string Lobby = "StateLobby";
        public static readonly string Game = "StateGame";
    }
}

