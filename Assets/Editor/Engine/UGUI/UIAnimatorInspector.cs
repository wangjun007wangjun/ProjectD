using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Engine.UGUI;
using UnityEditor;
using JW.Editor.Utils;

[CustomEditor(typeof(UIAnimator))]
public class UIAnimatorInspector : UnityEditor.Editor
{
    protected UIAnimator _aniCom;
    protected virtual void OnEnable()
    {
        _aniCom = target as UIAnimator;
        Animator animation = _aniCom.gameObject.GetComponent<Animator>();
        if (animation == null)
        {
            _aniCom.gameObject.AddComponent<Animator>();
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
