/********************************************************************
  created:  2020-06-05         
  author:    OneJun           
  purpose:    各个系统之间事件定义              
*********************************************************************/
using Engine.Event;

/// <summary>
/// 事件ID 定义
/// </summary>
public enum EventId : uint
{
    ///内部保留事件，
    TestEventId = 0,
    //程序前后台切换
    ApplicationPause = 1,
    //用户登录成功
    UserLoginSucceed = 2,
    //玩家注销登出
    UserLogout = 6,
    //玩家信息改变
    UserInfoChanged = 7,
    //
    Count,
}

/// <summary>
/// 应用程序暂停
/// </summary>
public struct ApplicationPauseArg : EventArg
{
    public bool IsPause;
}


