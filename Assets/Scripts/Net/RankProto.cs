namespace Net
{
    [System.Serializable]
    public struct RankReq : NetReq
    {
        public int player_id;
    }
    [System.Serializable]
    public class RankPlayerInfo
    {
        public string show_id;//id
        public string name;//玩家姓名
        public int score;//积分
        public int daily_rank;//排名

    }
    [System.Serializable]
    public class RankData
    {
        public int player_rank;
        public int player_score;
        public string date_time;
        public RankPlayerInfo[] rank_arr;
    }

    [System.Serializable]
    public class RankRsp : NetRsp
    {
        public RankData data;
    }
}