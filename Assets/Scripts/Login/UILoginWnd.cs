/********************************************************************
	created:	2020/02/12
	author:		OneJun
	purpose:	游戏更新与加载窗口
*********************************************************************/
using Engine.UGUI;
using UnityEngine.UI;
using System.Collections;
using UnityEngine;
using Engine.Base;
using DG.Tweening;

public class UILoginForm : UIFormClass
{
    private const int _uiProgressSliderIndex = 0;
    private const int _uiInfoTextIndex = 1;
    private const int _uiProgressTextTextIndex = 2;

    private Slider _uiProgressSlider;
    private Text _uiInfoText;
    private Text _uiProgressTextText;


    private Tweener _progressTW;
    private float _curProgress = 0f;
    public override string GetPath()
    {
        return "Fixed/UIUpdateForm";
    }

    /// <summary>
    /// 资源加载后回调
    /// </summary>
    protected override void OnResourceLoaded()
    {
        _uiProgressSlider = GetComponent(_uiProgressSliderIndex) as Slider;
        _uiInfoText = GetComponent(_uiInfoTextIndex) as Text;
        _uiProgressTextText = GetComponent(_uiProgressTextTextIndex) as Text;
    }

    /// <summary>
    /// 资源卸载后回调
    /// </summary>
    protected override void OnResourceUnLoaded()
    {
        _uiProgressSlider = null;
        _uiInfoText = null;
        _uiProgressTextText = null;
    }

    /// <summary>
    /// 逻辑初始化
    /// </summary>
    /// <param name="parameter">初始化参数</param>
    protected override void OnInitialize(object parameter)
    {
        _curProgress = 0f;
    }

    /// <summary>
    /// 逻辑反初始化
    /// </summary>
    protected override void OnUninitialize()
    {

    }

    /// <summary>
    /// 更新UI
    /// </summary>
    /// <param name="id">更新ID标识</param>
    /// <param name="param">更新参数</param>
    protected override void OnUpdateUI(string id, object param)
    {
        if (id.Equals("ShowProgress"))
        {
            if (param != null)
            {
                Hashtable ht = param as Hashtable;
                string txt = ht["tip"] as string;
                float progress = (float)ht["progress"];
                _uiInfoText.text = txt;
                ProgrssAni(progress);
            }
            return;
        }

        if (id.Equals("WaitProgress"))
        {
            if (param != null)
            {
                Hashtable ht = param as Hashtable;
                string txt = ht["tip"] as string;
                _uiInfoText.text = txt;
            }
            return;
        }
    }

    private void ProgrssAni(float progress)
    {
        _progressTW?.Kill();
        _progressTW = DOTween.To(() => _curProgress, (val) =>
                     {
                         _curProgress = val;
                         _uiProgressSlider.value = _curProgress;
                         int showValue = (int)(_curProgress * 100);
                         _uiProgressTextText.text = showValue + "%";
                     }, progress, 1f);
    }
    protected override void OnAction(string id, object param)
    {


    }
}
