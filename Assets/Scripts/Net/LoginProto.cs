using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Net
{
    [System.Serializable]
    public struct LoginUser
    {
        public int id;
        public string device_id;
        public string name;
    }
    [System.Serializable]
    public struct LoginReq : NetReq
    {
        public LoginUser user;
        public string token;
    }

    [System.Serializable]
    public struct LoginRspInfo
    {
        // public int player_id;
        // public string name;
        // public string login_token;
        public LoginUser user;
    }
    [System.Serializable]
    public class LoginRsp : NetRsp
    {
        public LoginRspInfo data;
        public string token;
    }
}