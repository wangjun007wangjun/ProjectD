using System;
using UnityEngine;
using Engine.Res;
public static class VersionInfo
{
    private static string _appVersion = null;
    private static string _buildNumber = null;

    /// <summary>
    /// 返回APP完整版本号
    /// </summary>
    /// <returns></returns>
    public static string GetAppVersion()
    {
        if (_appVersion == null)
        {
            Initialize();
        }
        return _appVersion;
    }

    /// <summary>
    /// 返回build 号
    /// </summary>
    /// <returns></returns>
    public static string GetBuildNumber()
    {
        if(_buildNumber==null){
            Initialize();
        }
        return _buildNumber;
    }

    //读取Version 文件
    private static void Initialize()
    {
        string fileName="Version_Android";
        if(Application.platform==RuntimePlatform.IPhonePlayer){
            fileName="Version_Ios";
        }
        _appVersion = string.Empty;
        TextAsset textAsset = Resources.Load(fileName, typeof(TextAsset)) as TextAsset;
        if (textAsset != null)
        {
            string content = textAsset.text;
            string[] infos=content.Split(new string[]{"\r\n"},StringSplitOptions.None);
            _appVersion = infos[0].Trim();
            _buildNumber=infos[1].Trim();
        }
    }
};
