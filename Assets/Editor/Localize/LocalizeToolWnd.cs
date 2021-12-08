using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Engine.Localize;

public class LocalizeToolWnd : EditorWindow
{
    public string sheetNames = "menu";
    public SystemLanguage language = SystemLanguage.Chinese;
    public string curTestUIR = "";
    public string curQueryResult = "Click Show Result";
    public bool isLoadSheet = false;
    public string[] allKeys;
    private int _keyIndex;
    private string _chooseKey;

    void OnGUI()
    {
        //
        GUILayout.Label("选择语言", EditorStyles.boldLabel);
        language = (SystemLanguage)EditorGUILayout.EnumPopup("Select Language", language);

        if (GUILayout.Button("加载"))
        {
            LocalizeService.GetInstance().SwitchLanguage(language);
            allKeys = LocalizeService.GetInstance().GetAllTextId();
            isLoadSheet = true;
        }
        if (GUILayout.Button("重载"))
        {
            LocalizeService.GetInstance().ReloadLanguage();
            allKeys = LocalizeService.GetInstance().GetAllTextId();
            isLoadSheet = true;
        }
        if (isLoadSheet)
        {
            GUILayout.Label("------->" + "Load Success " + "<---------", EditorStyles.boldLabel);
            if (allKeys != null)
            {
                _keyIndex = EditorGUILayout.Popup("All Keys", _keyIndex, allKeys);
                _chooseKey = allKeys[_keyIndex];
                if (!string.IsNullOrEmpty(_chooseKey))
                {
                    string chooseResult = "";
                    chooseResult = LocalizeService.GetInstance().GetTextById(_chooseKey);
                    if (chooseResult != null)
                    {
                        GUIStyle style = new GUIStyle();
                        style.fontSize = 40;
                        style.normal.textColor = Color.blue;
                        GUILayout.Label("->" + chooseResult + "<--", style);
                    }
                }
            }
        }
        else
        {
            GUILayout.Label("------->" + "Wait Load Sheet " + "<---------", EditorStyles.boldLabel);
        }

        GUILayout.Label("输入Id Like LC_MENU_OK", EditorStyles.boldLabel);
        curTestUIR = EditorGUILayout.TextField("Id", curTestUIR);
        if (GUILayout.Button("查找"))
        {
            curQueryResult = LocalizeService.GetInstance().GetTextById(curTestUIR);
        }
        //
        if (curQueryResult != null)
        {
            GUIStyle style = new GUIStyle();
            style.fontSize = 40;
            style.normal.textColor = Color.yellow;
            GUILayout.Label("->" + curQueryResult + "<--", style);
        }
        else
        {
            GUIStyle style = new GUIStyle();
            style.fontSize = 40;
            style.normal.textColor = Color.red;
            GUILayout.Label("-->" + "错误 Id" + "<--", style);
        }
    }

    //Show window
    public static void ShowWindow()
    {
        LocalizeToolWnd thisWindow = (LocalizeToolWnd)EditorWindow.GetWindow(typeof(LocalizeToolWnd));
        thisWindow.titleContent = new GUIContent("国际化");
    }
}

