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
        public const string SERVER_URL = "http://47.107.108.176:8080";
#endif
        //隐私政策url
        public const string PRIVACY_URL = "https://blackmoongame.github.io/privacy/";
        //用户协议url
        public const string TERMS_URL = "https://blackmoongame.github.io/terms/";

        //登录接口
        public const string LoginAPI = "/User/Login";
        public const string UpdateRankAPI = "/Score/AddNewScore";
        //总排行榜接口
        public const string RankTotalAPI = "/Score/Top20/";
    }
}
