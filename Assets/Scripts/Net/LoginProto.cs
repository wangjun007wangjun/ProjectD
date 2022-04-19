using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Net
{
    [System.Serializable]
    public struct LoginReq : NetReq
    {
        public string account_id;
        public string device_id;
        public string name;
    }

    [System.Serializable]
    public class LoginRsp : NetRsp
    {
        public int player_id;
        public string name;
        public string login_token;

    }
}