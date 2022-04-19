/********************************************************************
    created:	2020-05-12 				
    author:		OneJun						
    purpose:	用户信息定义								
*********************************************************************/
using Net;

namespace Data
{

    [System.Serializable]
    public class UserInfo
    {
        //服务器 玩家Id
        public int PlayerId = 1;
        //账号Id 游客device id facebook id
        public string AccountId = "";
        public string PlayerName = "Player";

        public void InitByRsp(LoginRsp rspData)
        {
            PlayerId = rspData.player_id;
            PlayerName = rspData.name;
        }
    }
}

