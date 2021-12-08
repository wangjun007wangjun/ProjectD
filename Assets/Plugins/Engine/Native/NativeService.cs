using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Engine.Base;

namespace Engine.Native
{

    public class NativeService : Singleton<NativeService>
    {

        //粘贴板
        private ClipboardBridge _clipboard;


        public override bool Initialize()
        {

            return true;
        }

        public override void Uninitialize()
        {
            
            return;
        }

        //当前网络可用
        public bool GetIsNetEnable()
        {
            return Application.internetReachability != NetworkReachability.NotReachable;
        }
        
        //振动设备
        public void DoHandheldVibrate()
        {
#if UNITY_IOS
             Handheld.Vibrate();
#elif UNITY_ANDROID
             Handheld.Vibrate();
#elif UNITY_EDITOR || UNITY_STANDALONE

#endif
        }

        //复制到粘贴板
        public void CopyToClipboard(string context)
        {
            // if (_clipboard != null)
            // {
            //     _clipboard.CopyTextToClipboard(context);
            // }
        }

        //UUID
        public string GetDeviceUUID()
        {
            return UnityEngine.SystemInfo.deviceUniqueIdentifier;
        }

    }
}
