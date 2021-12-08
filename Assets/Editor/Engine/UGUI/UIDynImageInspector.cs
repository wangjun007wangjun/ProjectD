using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System;
using Engine.Base;
using Engine.UGUI;
using UnityEngine.UI;


[CustomEditor(typeof(Engine.UGUI.UIDynImage))]
public class UIDynImageInspector : Editor
{
    private SerializedProperty _dataProperty;
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        string addrPath = CheckImage();
        SerializedProperty sp = serializedObject.FindProperty("ImageAddr");
        if (!string.IsNullOrEmpty(addrPath))
        {
            sp.stringValue = addrPath;
            serializedObject.ApplyModifiedProperties();
        }
        base.OnInspectorGUI();
    }

    protected void OnEnable()
    {

    }

    protected void OnDisable()
    {

    }

    private string CheckImage()
    {

        UIDynImage me = target as UIDynImage;
        Image imgeCom = me.gameObject.GetComponent<Image>();
        Sprite source = imgeCom.sprite;
        if (source != null)
        {
            string path = AssetDatabase.GetAssetPath(source);
            if (string.IsNullOrEmpty(path))
            {
                return "";
            }
            if (path.StartsWith("Assets/Resources/"))
            {
                path = path.Replace("Assets/Resources/", "");
                path = path.Replace(".png", "");
                return path;
            }
            else
            {
                //Debug.LogError("DynImage Image Addr Must In Resources");
                return "";
            }
        }
        else
        {
            return "";
        }
    }

}
