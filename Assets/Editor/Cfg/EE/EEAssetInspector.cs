using Cfg;
using UnityEngine;
using UnityEditor;

namespace Cfg
{
    public abstract class EEAssetInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            var dataCollection = target as EERowDataCollection;
            if (dataCollection != null)
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.HelpBox(@"配置Excel文件名： " + dataCollection.ExcelFileName, MessageType.Info);
                GUILayout.EndHorizontal();

                var prevGUIState = GUI.enabled;
                GUI.enabled = true;
                EditorGUILayout.LabelField("配置条目个数:  " + dataCollection.GetEntryCount());
                base.OnInspectorGUI();
                GUI.enabled = prevGUIState;
            }
            else
            {
                base.OnInspectorGUI();
            }
        }
    }
}

