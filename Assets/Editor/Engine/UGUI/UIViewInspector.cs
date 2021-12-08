using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;
using Engine.UGUI;
using JW.Editor.Utils;

[CustomEditor(typeof(UIView))]
public class UIViewInspector : UIViewLinkInspector
{
    public new void OnEnable()
    {
        base.OnEnable();
    }
    private bool _genExpand = false;
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        //连接元素
        base.OnDrawElements();
        //
        serializedObject.ApplyModifiedProperties();

        //添加自动生成
        _genExpand = EditorGUIHelper.DrawHead("辅助", _genExpand);
        if (_genExpand)
        {
            GUILayout.BeginVertical("box");
            {
                EditorGUI.BeginDisabledGroup(true);
                _prefix = EditorGUILayout.TextField("引用前缀", _prefix);
                EditorGUI.EndDisabledGroup();
                if (GUILayout.Button("复制节点定义"))
                {
                    UIViewLink pl = target as UIViewLink;
                    CopyElementDeclare(pl);
                }
                if (GUILayout.Button("复制节点初始化"))
                {
                    UIViewLink pl = target as UIViewLink;
                    CopyElementInitialize(pl);
                }
                if (GUILayout.Button("复制节点反初始化"))
                {
                    UIViewLink pl = target as UIViewLink;
                    CopyElementUnIninitialize(pl);
                }

                /*
                if (GUILayout.Button("复制Lua节点定义"))
                {
                    UIViewLink pl = target as UIViewLink;
                    CopyElementDeclareLua(pl);
                }

                if (GUILayout.Button("复制Lua节点初始化"))
                {
                    UIViewLink pl = target as UIViewLink;
                    CopyElementInitializeLua(pl);
                }

                if (GUILayout.Button("复制Lua节点反初始化"))
                {
                    UIViewLink pl = target as UIViewLink;
                    CopyElementUnIninitializeLua(pl);
                }

                if (GUILayout.Button("复制Lua全部初始化代码"))
                {
                    UIViewLink pl = target as UIViewLink;
                    CopyElementWholeLua(pl);
                }
                */

            }
            EditorGUILayout.EndVertical();
        }
    }

}