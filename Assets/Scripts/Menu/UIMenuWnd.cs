/********************************************************************
  created:  2020-06-05         
  author:    OneJun           
  purpose:  菜单窗口                
*********************************************************************/
using Engine.UGUI;
using Engine.Event;
using UnityEngine.UI;
using Data;
using UnityEngine;
using SuperScrollView;
using Engine.PLink;
using Engine.Schedule;
using Engine.Audio;
using TMPro;
using Net;

public class UIMenuWnd : UIFormClass
{
    private const int _uiAudioBtnButtonIndex = 0;
    private const int _uiMoShiBtn1ButtonIndex = 1;
    private const int _uiMoShiBtn2ButtonIndex = 2;
    private const int _uiMusicSliderSliderIndex = 3;
    private const int _uiSoundSliderSliderIndex = 4;
    private const int _uiAudioSettingBtnButtonIndex = 5;
    private const int _uiIntroduceBtnButtonIndex = 6;
    private const int _uiSettingBtnButtonIndex = 7;
    private const int _uiIntroductionPanelRectTransformIndex = 8;
    private const int _uiCloseIntroductionBtnButtonIndex = 9;
    private const int _uiQuitBtnButtonIndex = 10;
    private const int _uiChangeNameBtnButtonIndex = 11;
    private const int _uiNameInputTMP_InputFieldIndex = 12;

    private Button _uiAudioBtnButton;
    private Button _uiMoShiBtn1Button;
    private Button _uiMoShiBtn2Button;
    private Slider _uiMusicSliderSlider;
    private Slider _uiSoundSliderSlider;
    private Button _uiAudioSettingBtnButton;
    private Button _uiIntroduceBtnButton;
    private Button _uiSettingBtnButton;
    private RectTransform _uiIntroductionPanelRectTransform;
    private Button _uiCloseIntroductionBtnButton;
    private Button _uiQuitBtnButton;
    private Button _uiChangeNameBtnButton;
    private TMP_InputField _uiNameInputTMP_InputField;

    public override string GetPath()
    {
        return "Menu/UIMenuWnd";
    }
    protected override void OnResourceLoaded()
    {
        _uiAudioBtnButton = GetComponent(_uiAudioBtnButtonIndex) as Button;
        _uiMoShiBtn1Button = GetComponent(_uiMoShiBtn1ButtonIndex) as Button;
        _uiMoShiBtn2Button = GetComponent(_uiMoShiBtn2ButtonIndex) as Button;
        _uiMusicSliderSlider = GetComponent(_uiMusicSliderSliderIndex) as Slider;
        _uiSoundSliderSlider = GetComponent(_uiSoundSliderSliderIndex) as Slider;
        _uiAudioSettingBtnButton = GetComponent(_uiAudioSettingBtnButtonIndex) as Button;
        _uiIntroduceBtnButton = GetComponent(_uiIntroduceBtnButtonIndex) as Button;
        _uiSettingBtnButton = GetComponent(_uiSettingBtnButtonIndex) as Button;
        _uiIntroductionPanelRectTransform = GetComponent(_uiIntroductionPanelRectTransformIndex) as RectTransform;
        _uiCloseIntroductionBtnButton = GetComponent(_uiCloseIntroductionBtnButtonIndex) as Button;
        _uiQuitBtnButton = GetComponent(_uiQuitBtnButtonIndex) as Button;
        _uiChangeNameBtnButton = GetComponent(_uiChangeNameBtnButtonIndex) as Button;
        _uiNameInputTMP_InputField = GetComponent(_uiNameInputTMP_InputFieldIndex) as TMP_InputField;

        _uiAudioSettingBtnButton.onClick.AddListener(() =>
        {
            _uiAudioSettingBtnButton.gameObject.SetActive(false);
        });
        _uiMusicSliderSlider.onValueChanged.AddListener(delegate (float a)
        {
            AudioService.GetInstance().SetVolumeByChannel(1, a);
        });
        _uiSoundSliderSlider.onValueChanged.AddListener(delegate (float a)
        {
            AudioService.GetInstance().SetVolumeByChannel(2, a);
        });
        _uiMoShiBtn1Button.onClick.AddListener(() =>
        {
            SendAction("OnClickMoShi1");
        });
        _uiMoShiBtn2Button.onClick.AddListener(() =>
        {
            SendAction("OnClickMoShi2");
        });
        _uiIntroduceBtnButton.onClick.AddListener(() =>
       {
           _uiIntroductionPanelRectTransform.gameObject.SetActive(true);
       });
        _uiSettingBtnButton.onClick.AddListener(() =>
       {
           _uiAudioSettingBtnButton.gameObject.SetActive(true);
           _uiMusicSliderSlider.value = AudioService.GetInstance().GetVolumeByChannel(1) / 1.0f;
           _uiSoundSliderSlider.value = AudioService.GetInstance().GetVolumeByChannel(2) / 1.0f;
       });
        _uiCloseIntroductionBtnButton.onClick.AddListener(() =>
        {
            _uiIntroductionPanelRectTransform.gameObject.SetActive(false);
        });
        _uiQuitBtnButton.onClick.AddListener(() =>
        {
            Application.Quit();
        });
        _uiChangeNameBtnButton.onClick.AddListener(() =>
        {
            // 请求数据
            ChangeNameReq req = new ChangeNameReq();
            req.id = 0;
            req.name = _uiNameInputTMP_InputField.text;
            req.device_id = UnityEngine.SystemInfo.deviceUniqueIdentifier;

            UICommon.GetInstance().ShowWaiting("...", true);
            //请求
            NetService.GetInstance().SendNetPostReq(NetDeclare.ChangeName, req, (isError, rspStr) =>
            {
                UICommon.GetInstance().CleanWaiting();
                if (isError)
                {
                    UICommon.GetInstance().ShowBubble(rspStr);
                    return;
                }
                NetRsp rsp = JsonUtility.FromJson<NetRsp>(rspStr);
                if (rsp.code != 2000)
                {
                    UICommon.GetInstance().ShowBubble(rsp.message);
                    return;
                }
                DataService.GetInstance().Me.PlayerName = req.name;
                UICommon.GetInstance().ShowBubble(rsp.message);
            });
        });
    }
    protected override void OnResourceUnLoaded()
    {
        _uiAudioBtnButton = null;
        _uiMoShiBtn1Button = null;
        _uiMoShiBtn2Button = null;
        _uiMusicSliderSlider = null;
        _uiSoundSliderSlider = null;
        _uiAudioSettingBtnButton = null;
        _uiIntroduceBtnButton = null;
        _uiSettingBtnButton = null;
        _uiIntroductionPanelRectTransform = null;
        _uiCloseIntroductionBtnButton = null;
        _uiQuitBtnButton = null;
        _uiChangeNameBtnButton = null;
        _uiNameInputTMP_InputField = null;


    }
    protected override void OnInitialize(object param)
    {
        _uiNameInputTMP_InputField.text = DataService.GetInstance().Me.PlayerName;
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
}
