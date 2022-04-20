namespace Net
{
    [System.Serializable]
    public struct AddNewRankReq : NetReq
    {
        public RankPlayerInfo score;
    }
    [System.Serializable]
    public class AddNewRankRsp : NetRsp
    {
        public string data;
    }

    [System.Serializable]
    public struct RankReq : NetReq
    {
        // public int player_id;
        public int musicId;
    }
    [System.Serializable]
    public class RankPlayerInfo
    {
        public string name;//玩家姓名
        public int score;//积分
        public int music;//音乐id
        public string update_time;//时间

    }

    [System.Serializable]
    public class RankRsp : NetRsp
    {
        public RankPlayerInfo[] data;
    }
}