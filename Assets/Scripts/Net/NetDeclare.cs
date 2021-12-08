/********************************************************************
	created:	2020/02/14
	author:		OneJun
	
	purpose:	游戏网络 声明 
*********************************************************************/
namespace Net
{
    /// <summary>
    /// 网络请求包
    /// </summary>
    public interface NetReq
    {

    }

    /// <summary>
    /// 网络回包
    /// </summary>
    [System.Serializable]
    public class NetRsp
    {
        public int code;
        public string message;
    }

    [System.Serializable]
    public struct PlayerSourceInfo
    {
        public int prop_id;
        public int prop_num;
    }

    public class NetDeclare
    {
        //密钥
        public const string SECRET = "ZWiMGl4EN2dGrHDroUgb7CEVuexHaYj6";
#if APP_DEBUG
        // 测试服
        public const string SERVER_URL = "https://0znln91z73.execute-api.us-west-2.amazonaws.com/dev/api";
#else
        // 正式服
        public const string SERVER_URL = "https://h3c227bszj.execute-api.cn-north-1.amazonaws.com.cn/v1/api";
#endif
        //隐私政策url
        public const string PRIVACY_URL = "https://blackmoongame.github.io/privacy/";
        //用户协议url
        public const string TERMS_URL = "https://blackmoongame.github.io/terms/";

        //登录接口
        public const string LoginAPI = "/monster/account/login";
        //签到数据接口
        public const string SignInRewardListAPI = "/monster/reward/sign_reward_list";
        //签到接口
        public const string SignInAPI = "/monster/reward/sign_reward";
        //日排行榜接口
        public const string RankDaylyAPI = "/monster/rank/daily_rank";
        //月排行榜接口
        public const string RankWeeklyAPI = "/monster/rank/weekly_rank";
        //总排行榜接口
        public const string RankTotalAPI = "/monster/rank/total_rank";
        //任务成就接口
        public const string TaskAPI = "/monster/reward/achievement_task_list";
        //任务领奖接口
        public const string TaskRewardAPI = "/monster/reward/task";
        //成就领奖接口
        public const string AchievementRewardAPI = "/monster/reward/achievement";
        //星级宝箱接口
        public const string StarBoxListAPI = "/monster/stage/box_list";
        //星级宝箱领奖接口
        public const string StarBoxClaimAPI = "/monster/stage/box";
        //升级颜色攻击力接口
        public const string UserAttUpAPI = "/monster/stage/upgrade_color";
        //游戏关卡胜利结算接口
        public const string LvWinAPI = "/monster/stage/complete";
        //游戏同步道具消耗接口
        public const string SyncConsumeAPI = "/monster/stage/stage_consume";
    }
}
