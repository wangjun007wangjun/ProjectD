/********************************************************************
	created:	2020/02/14
	author:		OneJun
	
	purpose:	系统剪贴板功能
*********************************************************************/
using System.Runtime.InteropServices;
using UnityEngine;

namespace Engine.Native
{
    public class ClipboardBridge
    {
        //初始化
        public void Initialize()
        {

        }

        //反初始化
        public void Uninitialize()
        {

            return;
        }
/*
#if UNITY_EDITOR || UNITY_STANDALONE
        public void CopyTextToClipboard(string str)
        {
            GUIUtility.systemCopyBuffer = str;
        }
#elif UNITY_IOS
    [DllImport("__Internal")]
    public static extern void UnityCopyTextToClipboard(string text);//对ios剪切板的调用
    public void CopyTextToClipboard(string str)
    {
        UnityCopyTextToClipboard(str);
    }
#elif UNITY_ANDROID
     public void CopyTextToClipboard(string str)
    {
        AndroidJavaClass javaBridge = new AndroidJavaClass("com.jw.plugin.ClipboardBridge");
        if (javaBridge!=null)
        {
            javaBridge.CallStatic("UnityCopyToClipboard", str);
        }
    }
#endif*/
    }
}
