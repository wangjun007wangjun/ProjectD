/********************************************************************
    created:	2020-04-30 				
    author:		OneJun						
    purpose:	本地化支持组件								
*********************************************************************/
using Engine.UGUI;
using UnityEngine;
using UnityEngine.UI;
using Engine.Localize;

[RequireComponent(typeof(UnityEngine.UI.Text))]
[ExecuteInEditMode]
public class UILcText : UIComponent
{
    //ugui text 组件
    [Header("绑定UGUI 组件")]
    public UnityEngine.UI.Text UGUITextCom;
    //对应的本地化Id
    [Header("本地化文本Id")]
    public string LcTextId=string.Empty;


    public override void Initialize(UIForm formScript)
    {
        base.Initialize(formScript);
        if (UGUITextCom == null)
        {
            UGUITextCom = this.gameObject.GetComponent<UnityEngine.UI.Text>();
        }
        if (LcTextId.Length>0)
        {
            string tt=LocalizeService.GetInstance().GetTextById(LcTextId);
            if(!string.IsNullOrEmpty(tt)){
                UGUITextCom.text = tt;
            }
            LocalizeService.GetInstance().AddLanguageChangedCallback(OnLanguageChanged);
        }
    }

    private void OnLanguageChanged()
    {
        if (LcTextId.Length>0)
        {
            string tt=LocalizeService.GetInstance().GetTextById(LcTextId);
            if(!string.IsNullOrEmpty(tt)){
                UGUITextCom.text = tt;
            }
        }
    }

    //窗口关闭
    public override void OnClose()
    {
        LocalizeService.GetInstance().RemoveLanguageChangedCallback(OnLanguageChanged);
    }

    //窗口隐藏
    public override void OnHide()
    {

    }

    //窗口显示
    public override void OnAppear()
    {

    }

#if UNITY_EDITOR
    void Awake()
    {
        LocalizeService.GetInstance().AddLanguageChangedCallback(OnLanguageChanged);
    }
#endif

}
