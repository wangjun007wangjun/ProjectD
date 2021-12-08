using Engine.Res;
using UnityEditor;
using System;
using UnityEngine;
public static class VersionTool
{
    public static string GetAppVersion()
    {
        string file = string.Empty;
#if UNITY_ANDROID || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
        file = "/Resources/Version_Android.bytes";
#else
        file="/Resources/Version_Ios.bytes";
#endif

        string content = System.Text.Encoding.UTF8.GetString(Engine.Res.FileUtil.ReadFile(Application.dataPath + file));
        if (!string.IsNullOrEmpty(content))
        {
            string[] infos = content.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            return infos[0].Trim();
        }
        return "1.0.0";
    }

    public static string GetBuildNumber()
    {

        string file = string.Empty;
#if UNITY_ANDROID || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
        file = "/Resources/Version_Android.bytes";
#else
        file="/Resources/Version_Ios.bytes";
#endif

        string content = System.Text.Encoding.UTF8.GetString(Engine.Res.FileUtil.ReadFile(Application.dataPath + file));
        if (!string.IsNullOrEmpty(content))
        {
            string[] infos = content.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            return infos[1].Trim();
        }
        return "1";
    }


}
