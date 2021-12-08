using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.XCodeEditor;
#endif
using System.IO;
using System;

public static class XCodePostProcess
{
#if UNITY_EDITOR

    enum WriteOpt
    {
        Below = 1,
        Above = 2,
        Replace = 3,
    }

    [PostProcessBuild(999)]
    public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
    {
        //IOS
        if (target != BuildTarget.iOS)
        {
            Debug.LogWarning("Target is not iPhone. XCodePostProcess will not run");
            return;
        }

        Debug.Log("pathToBuiltProject:" + pathToBuiltProject);
        string path = Path.GetFullPath(pathToBuiltProject);
        Debug.Log("pathToBuiltProject full path:" + path);
        // Create a new project object from build target
        XCProject project = new XCProject(path);
        //添加类库搜索路径
        project.AddFrameworkSearchPaths("$(SRCROOT)/Frameworks/Plugins/iOS");

        //根据projmods配置文件扩展Xcode工程
        string[] files = null;
        files = Directory.GetFiles(Application.dataPath, "*.projmods", SearchOption.AllDirectories);
        //
        foreach (string file in files)
        {
            UnityEngine.Debug.Log("ProjMod File: " + file);
            project.ApplyMod(file);
        }
        //
        project.overwriteBuildSetting("GCC_ENABLE_OBJC_EXCEPTIONS", "YES", "Release");
        project.overwriteBuildSetting("GCC_ENABLE_OBJC_EXCEPTIONS", "YES", "Debug");
        project.overwriteBuildSetting("GCC_ENABLE_OBJC_EXCEPTIONS", "YES", "ReleaseForProfiling");
        project.overwriteBuildSetting("GCC_ENABLE_OBJC_EXCEPTIONS", "YES", "ReleaseForRunning");
        //编辑InfoPlist
        //EditorPlist(path);

#if !APP_EXP
        //copy pod File
        Debug.Log("Copy Pod");
        Debug.Log("--------------Copy Ios Pod File------------------");
        string projPath = project.projectRootPath;
        string oldPodFilePath = Engine.Res.FileUtil.CombinePath(Application.dataPath, "Plugins/iOS/Podfile");
        string newPodFilePath = Engine.Res.FileUtil.CombinePath(projPath, "Podfile");
        Engine.Res.FileUtil.CopyFile(oldPodFilePath, newPodFilePath);
        //
        Debug.Log("--------------Copy Google Service Info--------------");
        string oldPath = Engine.Res.FileUtil.CombinePath(Application.dataPath, "Plugins/iOS/GoogleService-Info.plist");
        string newPath = Engine.Res.FileUtil.CombinePath(projPath, "GoogleService-Info.plist");
        Engine.Res.FileUtil.CopyFile(oldPath, newPath);
        //添加到工程引用
        project.AddFile(newPath, null, "GROUP");
#endif
        project.Save();
    }


    private static void EditorPlist(string filePath)
    {
        // XCPlist list = new XCPlist(filePath);
        // string PlistAdd = @"
		// 	<key>XUPORTER</key>
		// 	<string>XUPorter XCodePostProcess Add</string>
		// 	<key>NSAppTransportSecurity</key>
		// 	<dict>
		// 		<key>NSAllowsArbitraryLoads</key>
		// 		<true/>
		// 	</dict>	
		// 	";

        // list.AddKey(PlistAdd);
        // list.Save();
    }

#endif

}
