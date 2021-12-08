using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Engine.UGUI;
using UnityEditor;
using JW.Editor.Utils;

[CustomEditor(typeof(UIAnimation))]
public class UIAnimationInspector : UnityEditor.Editor
{
    protected UIAnimation _aniCom;
    protected virtual void OnEnable()
    {
        _aniCom = target as UIAnimation;
        Animation animation = _aniCom.gameObject.GetComponent<Animation>();
        if (animation == null)
        {
            _aniCom.gameObject.AddComponent<Animation>();
        }
    }

    void OnDisable()
    {

    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
    }
}
