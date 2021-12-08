/********************************************************************
	created:	2018-06-07   
	filename: 	CommandBuild
	author:		OneJun
	
	purpose:	对接项目自动构建
*********************************************************************/
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using FileUtil = Engine.Res.FileUtil;
using UnityEditor.Build.Reporting;

public static class CommandBuild
{
    /// 获取出档的场景列表
    static EditorBuildSettingsScene[] GetScenePaths(bool isFilter)
    {
        var names = new List<EditorBuildSettingsScene>();
        foreach (EditorBuildSettingsScene e in EditorBuildSettings.scenes)
        {
            if (e == null)
                continue;

            if (!e.enabled)
                continue;
            names.Add(e);
        }
        return names.ToArray();
    }

    /// <summary>
    /// 输出Android Studio 工程
    /// </summary>
    public static void BuildAndroidProj()
    {
        AssetDatabase.Refresh();
#if APP_RELEASE
        PlayerSettings.companyName = "SCM";
        PlayerSettings.productName = "SCM";
        PlayerSettings.applicationIdentifier = "saga.monster.crush.fun.happy.crazy.king.toy";
#else
        PlayerSettings.companyName = "SCM";
        PlayerSettings.productName = "SCM";
        PlayerSettings.applicationIdentifier = "saga.monster.crush.fun.happy.crazy.king.toy";
#endif

        PlayerSettings.Android.preferredInstallLocation = AndroidPreferredInstallLocation.PreferExternal;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel19;
        PlayerSettings.Android.forceSDCardPermission = true;
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64;
        EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        string vv = VersionTool.GetAppVersion();
        PlayerSettings.bundleVersion = vv;
        PlayerSettings.Android.bundleVersionCode = int.Parse(VersionTool.GetBuildNumber());

        string path = System.IO.Path.GetFullPath(Application.dataPath + "/../../Builds/BuildAndroid/Temp_Proj");
        if (FileUtil.IsDirectoryExist(path))
        {
            FileUtil.ClearDirectory(path);
        }
        //
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        PlayerSettings.enableInternalProfiler = false;
        BuildOptions option = BuildOptions.AcceptExternalModificationsToPlayer;
        BuildResult result = BuildPipeline.BuildPlayer(GetScenePaths(true), path, BuildTarget.Android, option).summary.result;
        //
        if (result != BuildResult.Succeeded)
        {
            Debug.Log("BuildAndroidProj Error");
#if !UNITY_EDITOR
            EditorApplication.Exit(2);
#endif
        }
        Debug.Log("BuildAndroidProj Done1!");
#if !UNITY_EDITOR
        Debug.Log("BuildAndroidProj Done2!");
	AssetDatabase.Refresh();
        EditorApplication.Exit(0);
#endif
    }


    //输出ios 工程
    public static void BuildIOSProject()
    {

        AssetDatabase.Refresh();
#if APP_RELEASE
        PlayerSettings.companyName = "SCM";
        PlayerSettings.productName = "SCM";
        PlayerSettings.applicationIdentifier = "com.scm.relase";
#else
        PlayerSettings.companyName = "SCM";
        PlayerSettings.productName = "SCM";
        PlayerSettings.applicationIdentifier = "com.scm.debug";
#endif

        string vv = VersionTool.GetAppVersion();
        PlayerSettings.bundleVersion = vv;
        PlayerSettings.iOS.buildNumber = VersionTool.GetBuildNumber();

        PlayerSettings.SetScriptingBackend(BuildTargetGroup.iOS, ScriptingImplementation.IL2CPP);
        string path = System.IO.Path.GetFullPath(Application.dataPath + "/../../Builds/BuildIOS/IOS_Proj");
        if (FileUtil.IsDirectoryExist(path))
        {
            FileUtil.ClearDirectory(path);
        }
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS);
        BuildOptions option = BuildOptions.None;
        BuildResult result = BuildPipeline.BuildPlayer(GetScenePaths(true), path, BuildTarget.iOS, option).summary.result;
        if (result != BuildResult.Succeeded)
        {
            Debug.Log("BuildIOSProject Error:");
#if !UNITY_EDITOR
              EditorApplication.Exit(2);
#endif
        }
        Debug.Log("BuildIOSProject Done1!");
#if !UNITY_EDITOR
        Debug.Log("BuildIOSProject Done2!");
		AssetDatabase.Refresh();
        EditorApplication.Exit(0);
#endif
    }

    public static void RefreshAssetDatabase()
    {
        AssetDatabase.Refresh();
    }

}

