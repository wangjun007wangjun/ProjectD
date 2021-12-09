/********************************************************************
  created:  2020-06-05         
  author:    OneJun           
  purpose:  游戏主窗口                
*********************************************************************/
using Engine.UGUI;
using Engine.Event;
using UnityEngine.UI;
using Data;
using TMPro;
using Engine.Schedule;
using UnityEngine;
using System.Collections;

public class UIGamingForm : UIFormClass, IScheduleHandler
{
    private const int _uiExitBtnButtonIndex = 0;
    private const int _uiCountDownTxtTMPTextIndex = 1;
    private const int _uiSafeAreaRectTransformIndex = 2;

    private Button _uiExitBtnButton;
    private TextMeshProUGUI _uiCountDownTxtTMPText;
    private RectTransform _uiSafeAreaRectTransform;

    private uint _timer;
    private int _countDown = 4;
    public override string GetPath()
    {
        return "Gaming/UIGamingWnd";
    }
    protected override void OnResourceLoaded()
    {
        _uiExitBtnButton = GetComponent(_uiExitBtnButtonIndex) as Button;
        _uiCountDownTxtTMPText = GetComponent(_uiCountDownTxtTMPTextIndex) as TextMeshProUGUI;
        _uiSafeAreaRectTransform = GetComponent(_uiSafeAreaRectTransformIndex) as RectTransform;

        _uiExitBtnButton.onClick.AddListener(() =>
        {
            SendAction("OnBtnExit");
        });
    }
    protected override void OnResourceUnLoaded()
    {
        _uiExitBtnButton = null;
        _uiCountDownTxtTMPText = null;
        _uiSafeAreaRectTransform = null;
    }
    protected override void OnInitialize(object param)
    {
        _timer = this.AddTimer(1000, true);
    }

    protected override void OnUninitialize()
    {
    }

    protected override void OnUpdateUI(string id, object param)
    {
    }
    protected override void OnAction(string id, object param)
    {

    }

    public void OnScheduleHandle(ScheduleType type, uint id)
    {
        if (id == _timer)
        {
            if (_countDown >= 4)
            {
                _uiCountDownTxtTMPText.text = "Ready!";
                _countDown -= 1;
            }
            else if (_countDown >= 0 && _countDown <= 3)
            {
                _uiCountDownTxtTMPText.text = _countDown.ToString();
                _countDown -= 1;
            }
            else
            {
                _uiCountDownTxtTMPText.text = "";
                this.RemoveTimer(_timer);
                SendAction("StartGaming");
            }
        }
    }

    public Hashtable GetSafeAreaInfo()
    {
        Vector3[] cor = new Vector3[4];
        _uiSafeAreaRectTransform.GetLocalCorners(cor);

        Hashtable table = new Hashtable();
        int width = (int)Mathf.Ceil(_uiSafeAreaRectTransform.rect.width);
        if (width % 2 == 1)
        {
            width += 1;
        }
        int height = (int)Mathf.Ceil(_uiSafeAreaRectTransform.rect.height);
        if (height % 2 == 1)
        {
            height += 1;
        }
        table["width"] = width;
        table["height"] = height;
        table["corners"] = cor;
        // SendAction("UpdateSafeArea", table);

        return table;
    }
}
