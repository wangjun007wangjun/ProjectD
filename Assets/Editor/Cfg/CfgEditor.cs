/********************************************************************
    created:	2020-04-28 				
    author:		OneJun						
    purpose:	管理游戏配置的 导入 导出								
*********************************************************************/
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using UnityEditor.Callbacks;
namespace Cfg
{
    public static class CfgEditor
    {

        private static string CfgsAssetDir = "Assets/Resources/Cfgs/";
        private static string CfgsResDir = "Resources/Cfgs/";
        private static string JsonExportDir = Application.dataPath + "/../../JsonCfgs/";

        //策划配置目录
        private static string GDCfgDir = Application.dataPath + "/../../GD/Cfg";
        //c#代码定义生成目录
        private static string CfgCSharpDir = Application.dataPath + "/Scripts/Cfg/Gen";
        //c# 模型inspect代码目录
        private static string CfgCSharpInspectorDir = Application.dataPath + "/Editor/Cfg/Gen";
        //excel 转换成scriptobj资源目录
        private static string CfgAssetDir = Application.dataPath + "/Resources/Cfgs";

        [MenuItem(@"Cfg/导入&&生成代码&&资源", false, 100)]
        public static void ImportExcel()
        {
            //创建目录
            if (!Directory.Exists(CfgCSharpDir))
            {
                Engine.Res.FileUtil.CreateDirectory(CfgCSharpDir);
            }
            if (!Directory.Exists(CfgCSharpInspectorDir))
            {
                Engine.Res.FileUtil.CreateDirectory(CfgCSharpInspectorDir);
            }
            if (!Directory.Exists(CfgsResDir))
            {
                Engine.Res.FileUtil.CreateDirectory(CfgsResDir);
            }
            //
            var excelPath = EditorUtility.OpenFolderPanel("选择配置文档目录", GDCfgDir, "");
            if (string.IsNullOrEmpty(excelPath))
                return;
            //生成
            EEConverter.GenerateCSharpFiles(excelPath, CfgCSharpDir, CfgCSharpInspectorDir, CfgAssetDir);
        }

        [MenuItem(@"Cfg/刷新配置资源", false, 100)]
        public static void RefreshExcelAsset()
        {
            //刷新数据
            EEConverter.GenerateScriptableObjects(GDCfgDir, CfgAssetDir);
        }


        [MenuItem(@"Cfg/删除代码&&Asset", false, 101)]
        public static void Clean()
        {
            EditorPrefs.SetBool(EEConverter.csChangedKey, false);
            DeleteGenedCfgCSFile();
            DeleteCfgScriptableObjectFolder();
            AssetDatabase.Refresh();
        }

        //脚本编译完成回调
        [DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            if (!EditorPrefs.GetBool(EEConverter.csChangedKey, false)) return;
            EditorPrefs.SetBool(EEConverter.csChangedKey, false);
            Debug.Log("Scripts are reloaded, start generating assets...");
            EEConverter.GenerateScriptableObjects(GDCfgDir, CfgAssetDir);
        }

        //清理配置定义Cs 文件夹
        private static void DeleteGenedCfgCSFile()
        {
            //cs 
            if (Directory.Exists(CfgCSharpDir))
            {
                Engine.Res.FileUtil.ClearDirectory(CfgCSharpDir);
            }
            //inspector
            if (Directory.Exists(CfgCSharpInspectorDir))
            {
                Engine.Res.FileUtil.ClearDirectory(CfgCSharpInspectorDir);
            }
        }

        //清理配置资源文件夹
        private static void DeleteCfgScriptableObjectFolder()
        {
            if (Directory.Exists(CfgAssetDir))
            {
                Engine.Res.FileUtil.ClearDirectory(CfgAssetDir);
            }
        }


        /// <summary>
        /// 创建配置资产
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void CreateAsset<T>() where T : ScriptableObject
        {
            string name = typeof(T).Name;
            string path = CfgsAssetDir + name + ".asset";
            string fullPath = Application.dataPath + "/" + CfgsResDir + name + ".asset";
            if (System.IO.File.Exists(fullPath))
            {
                bool ret = EditorUtility.DisplayDialog("警告", "已经存在该配置确认覆盖！", "重置", "取消");
                if (ret)
                {
                    T asset = ScriptableObject.CreateInstance<T>();
                    AssetDatabase.CreateAsset(asset, path);
                    AssetDatabase.SaveAssets();
                    EditorUtility.FocusProjectWindow();
                    Selection.activeObject = asset;
                }
            }
            else
            {
                T asset = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(asset, path);
                AssetDatabase.SaveAssets();
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = asset;
            }

        }

        /// <summary>
        /// 导出资产为json 文件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void ExportCfgAssetToJsonFile<T>() where T : ScriptableObject
        {
            string name = typeof(T).Name;
            string resPath = "Cfgs/" + name;
            string outPath = JsonExportDir + name + ".json";
            T so = Resources.Load(resPath) as T;
            if (so != null)
            {
                string json = JsonUtility.ToJson(so);
                byte[] data = System.Text.Encoding.UTF8.GetBytes(json);
                Engine.Res.FileUtil.WriteFile(outPath, data, true);
                Debug.Log("Export Success " + outPath);
            }
            else
            {
                EditorUtility.DisplayDialog("错误", "不存在对应配置的Asset文件!", "OK");
            }
        }

        /// <summary>
        /// 根据json 文件创建配置资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void LoadCfgJsonToAsset<T>() where T : ScriptableObject
        {
            string name = typeof(T).Name;
            string outFilePath = JsonExportDir + name + ".json";
            string assetPath = CfgsAssetDir + name + ".asset";
            if (System.IO.File.Exists(outFilePath))
            {
                if (System.IO.File.Exists(assetPath))
                {
                    bool ret = EditorUtility.DisplayDialog("警告", "已经存在该配置Asset确认覆盖！", "重置", "取消");
                    if (ret)
                    {
                        string datas = System.IO.File.ReadAllText(outFilePath);
                        T so = ScriptableObject.CreateInstance<T>();
                        JsonUtility.FromJsonOverwrite(datas, so);
                        AssetDatabase.CreateAsset(so, assetPath);
                        AssetDatabase.SaveAssets();
                    }
                }
                else
                {
                    string datas = System.IO.File.ReadAllText(outFilePath);
                    T so = ScriptableObject.CreateInstance<T>();
                    JsonUtility.FromJsonOverwrite(datas, so);
                    AssetDatabase.CreateAsset(so, assetPath);
                    AssetDatabase.SaveAssets();
                }
            }
            else
            {
                Debug.LogError("Not Exist Json Cfg File:" + name);
            }
        }

    }
}
