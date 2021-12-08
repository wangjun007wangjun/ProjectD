using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;
using Engine.UGUI;
using JW.Editor.Utils;

[CustomEditor(typeof(UIForm))]
public class UIFormInspector : UIViewLinkInspector
{
    public new void OnEnable()
    {
        base.OnEnable();
    }

    //private bool _audioExpand = false;
    private bool _genExpand = false;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.BeginVertical("box");
        SerializedProperty sp = serializedObject.FindProperty("ReferenceResolution");
        EditorGUILayout.PropertyField(sp, new GUIContent("设计分辨率"));

        sp = serializedObject.FindProperty("IsSingleton");
        EditorGUILayout.PropertyField(sp, new GUIContent("是否单例"));

        sp = serializedObject.FindProperty("IsModal");
        EditorGUILayout.PropertyField(sp, new GUIContent("是否模态"));

        sp = serializedObject.FindProperty("ShowPriority");
        EditorGUILayout.PropertyField(sp, new GUIContent("显示优先级"));

       // sp = serializedObject.FindProperty("GroupId");
        //EditorGUILayout.PropertyField(sp, new GUIContent("显示组"));

        //sp = serializedObject.FindProperty("IsFullScreenBG");
       // EditorGUILayout.PropertyField(sp, new GUIContent("全屏背景"));

        sp = serializedObject.FindProperty("IsDisableInput");
        EditorGUILayout.PropertyField(sp, new GUIContent("禁止输入"));

        //sp = serializedObject.FindProperty("IsHideUnderForms");
        //EditorGUILayout.PropertyField(sp, new GUIContent("隐藏下面Form"));

        //sp = serializedObject.FindProperty("IsAlwaysKeepVisible");
        //EditorGUILayout.PropertyField(sp, new GUIContent("始终保持可见"));

        //sp = serializedObject.FindProperty("EnableAutoAdaptScreen");
        //EditorGUILayout.PropertyField(sp, new GUIContent("启用自动适配"));

        EditorGUILayout.EndVertical();
        /*
        _audioExpand=EditorGUIHelper.DrawHead("音效", _audioExpand);
        if (_audioExpand)
        {
            //todo 拖动填充
            GUILayout.BeginVertical("box");
            {
                sp = serializedObject.FindProperty("OpenedWwiseEvents");
                EditorGUIHelper.ArrayField(sp, "打开");
                sp = serializedObject.FindProperty("ClosedWwiseEvents");
                EditorGUIHelper.ArrayField(sp, "关闭");
            }
            EditorGUILayout.EndVertical();
        }
        */

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