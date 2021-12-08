/********************************************************************
    created:	2020-04-30 				
    author:		OneJun						
    purpose:	本地化文本组件 inspector							
*********************************************************************/
using Engine.Localize;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[CustomEditor(typeof(UILcText), true)]
public class UILcTextInspector : Editor
{
    private int _keyIndex;
    private string _chooseKey;
    private string[] _allKeys;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        if (_allKeys == null)
        {
            _allKeys = LocalizeService.GetInstance().GetAllTextId();
        }
        if (_allKeys == null || _allKeys.Length <= 0)
        {
            return;
        }
        UILcText me = target as UILcText;
        if (me.UGUITextCom == null)
        {
            me.UGUITextCom = me.gameObject.GetComponent<UnityEngine.UI.Text>();
        }
        var currentKey = me.LcTextId;
        for (int i = 0; i < _allKeys.Length; i++)
        {
            if (_allKeys[i].Equals(currentKey))
            {
                _keyIndex = i;
                break;
            }
        }
        _keyIndex = EditorGUILayout.Popup("选择本地化文本Id", _keyIndex, _allKeys);
        _chooseKey = _allKeys[_keyIndex];
        if (_chooseKey != currentKey)
        {
            string chooseResult = "";
            chooseResult = LocalizeService.GetInstance().GetTextById(_chooseKey);
            if (chooseResult != null && (!me.LcTextId.Equals(_chooseKey)))
            {
                me.UGUITextCom.text = chooseResult;
                me.gameObject.name = "Lc_" + _chooseKey;
                me.LcTextId = _chooseKey;
                //serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(me);
            }
        }
        base.OnInspectorGUI();
    }
}
