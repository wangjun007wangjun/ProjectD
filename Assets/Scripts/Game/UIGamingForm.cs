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
using Engine.State;
using Engine.Audio;

public class UIGamingForm : UIFormClass, IScheduleHandler
{
private const int _uiExitBtnButtonIndex = 0;
private const int _uiCountDownTxtTMPTextIndex = 1;
private const int _uiSafeAreaRectTransformIndex = 2;
private const int _uiScoreTextIndex = 3;
private const int _uiResultPanelRectTransformIndex = 4;
private const int _uiMusicPicImageIndex = 5;
private const int _uiMusicNameTextIndex = 6;
private const int _uiBestScoreTextIndex = 7;
private const int _uiCurScoreTextIndex = 8;
private const int _uiExitButtonIndex = 9;
private const int _uiReContinueButtonIndex = 10;
private const int _uiAudioSettingBtnButtonIndex = 11;
private const int _uiMusicSliderSliderIndex = 12;
private const int _uiSoundSliderSliderIndex = 13;
private const int _uiAudioBtnButtonIndex = 14;
private const int _uiPauseBtnButtonIndex = 15;
private const int _uiPlayBtnButtonIndex = 16;
private const int _uiPausePanelRectTransformIndex = 17;
private const int _uiContinueBigBtnButtonIndex = 18;

private Button _uiExitBtnButton;
private TextMeshProUGUI _uiCountDownTxtTMPText;
private RectTransform _uiSafeAreaRectTransform;
private Text _uiScoreText;
private RectTransform _uiResultPanelRectTransform;
private Image _uiMusicPicImage;
private Text _uiMusicNameText;
private Text _uiBestScoreText;
private Text _uiCurScoreText;
private Button _uiExitButton;
private Button _uiReContinueButton;
private Button _uiAudioSettingBtnButton;
private Slider _uiMusicSliderSlider;
private Slider _uiSoundSliderSlider;
private Button _uiAudioBtnButton;
private Button _uiPauseBtnButton;
private Button _uiPlayBtnButton;
private RectTransform _uiPausePanelRectTransform;
private Button _uiContinueBigBtnButton;




    private uint _timer;
    private int _countDown = 4;
    public override string GetPath()
    {
        return "Gaming/UIGamingWnd";
    }
    protected override void OnResourceLoaded()
    {
_uiExitBtnButton = GetComponent(_uiExitBtnButtonIndex)as Button;
_uiCountDownTxtTMPText = GetComponent(_uiCountDownTxtTMPTextIndex)as TextMeshProUGUI;
_uiSafeAreaRectTransform = GetComponent(_uiSafeAreaRectTransformIndex)as RectTransform;
_uiScoreText = GetComponent(_uiScoreTextIndex)as Text;
_uiResultPanelRectTransform = GetComponent(_uiResultPanelRectTransformIndex)as RectTransform;
_uiMusicPicImage = GetComponent(_uiMusicPicImageIndex)as Image;
_uiMusicNameText = GetComponent(_uiMusicNameTextIndex)as Text;
_uiBestScoreText = GetComponent(_uiBestScoreTextIndex)as Text;
_uiCurScoreText = GetComponent(_uiCurScoreTextIndex)as Text;
_uiExitButton = GetComponent(_uiExitButtonIndex)as Button;
_uiReContinueButton = GetComponent(_uiReContinueButtonIndex)as Button;
_uiAudioSettingBtnButton = GetComponent(_uiAudioSettingBtnButtonIndex)as Button;
_uiMusicSliderSlider = GetComponent(_uiMusicSliderSliderIndex)as Slider;
_uiSoundSliderSlider = GetComponent(_uiSoundSliderSliderIndex)as Slider;
_uiAudioBtnButton = GetComponent(_uiAudioBtnButtonIndex)as Button;
_uiPauseBtnButton = GetComponent(_uiPauseBtnButtonIndex)as Button;
_uiPlayBtnButton = GetComponent(_uiPlayBtnButtonIndex)as Button;
_uiPausePanelRectTransform = GetComponent(_uiPausePanelRectTransformIndex)as RectTransform;
_uiContinueBigBtnButton = GetComponent(_uiContinueBigBtnButtonIndex)as Button;

        _uiExitBtnButton.onClick.AddListener(() =>
        {
            StateService.Instance.ChangeState(GConst.StateKey.Lobby);
        });
         _uiExitButton.onClick.AddListener(() =>
        {
            StateService.Instance.ChangeState(GConst.StateKey.Lobby);
        });
         _uiReContinueButton.onClick.AddListener(() =>
        {
            _uiResultPanelRectTransform.gameObject.SetActive(false);
            SendAction("ReContinueGameTimer");
            _timer = this.AddTimer(1000, true);
            _uiScoreText.text = "0";
        });

        _uiAudioBtnButton.onClick.AddListener(() =>
            {
                Debug.Log("点击音量");
                _uiAudioSettingBtnButton.gameObject.SetActive(true);
                _uiMusicSliderSlider.value = AudioService.GetInstance().GetVolumeByChannel(1) / 1.0f;
                _uiSoundSliderSlider.value = AudioService.GetInstance().GetVolumeByChannel(2) / 1.0f;
            });
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

            _uiPauseBtnButton.onClick.AddListener(() =>{
                Time.timeScale = 0;
                _uiPlayBtnButton.gameObject.SetActive(true);
                _uiPauseBtnButton.gameObject.SetActive(false);
                _uiPausePanelRectTransform.gameObject.SetActive(true);

                AudioService.GetInstance().StopChannel(1);
                SendAction("GamePause", true);
            });
            _uiPlayBtnButton.onClick.AddListener(() =>{
                Time.timeScale = 1;
                _uiPlayBtnButton.gameObject.SetActive(false);
                _uiPauseBtnButton.gameObject.SetActive(true);
                AudioService.GetInstance().ContinueChannel(1);

                SendAction("GamePause", false);
            });
            _uiContinueBigBtnButton.onClick.AddListener(() =>{
                Time.timeScale = 1;
                _uiPlayBtnButton.gameObject.SetActive(false);
                _uiPausePanelRectTransform.gameObject.SetActive(false);
                _uiPauseBtnButton.gameObject.SetActive(true);
                AudioService.GetInstance().ContinueChannel(1);

                SendAction("GamePause", false);
            });
            
    }
    protected override void OnResourceUnLoaded()
    {
_uiExitBtnButton = null;
_uiCountDownTxtTMPText = null;
_uiSafeAreaRectTransform = null;
_uiScoreText = null;
_uiResultPanelRectTransform = null;
_uiMusicPicImage = null;
_uiMusicNameText = null;
_uiBestScoreText = null;
_uiCurScoreText = null;
_uiExitButton = null;
_uiReContinueButton = null;
_uiAudioSettingBtnButton = null;
_uiMusicSliderSlider = null;
_uiSoundSliderSlider = null;
_uiAudioBtnButton = null;
_uiPauseBtnButton = null;
_uiPlayBtnButton = null;
_uiPausePanelRectTransform = null;
_uiContinueBigBtnButton = null;

    }
    protected override void OnInitialize(object param)
    {
        _uiResultPanelRectTransform.gameObject.SetActive(false);

        _timer = this.AddTimer(1000, true);
        _uiScoreText.text = "0";
    }

    protected override void OnUninitialize()
    {
        this.RemoveTimer(_timer);
    }

    protected override void OnUpdateUI(string id, object param)
    {
        if (id.Equals("UpdateScore"))
        {
            _uiScoreText.text = ((int)param).ToString();
        }
        else if (id.Equals("OnShowResult"))
        {
            Hashtable table = (Hashtable)param;
            MusicData data = table["MusicData"] as MusicData;
            int curScore = (int)table["CurScore"];
            _uiResultPanelRectTransform.gameObject.SetActive(true);
            _uiMusicPicImage.sprite = data.musicTexture;
            _uiMusicNameText.text = data.name;
            int bestScore = DataService.GetInstance().Score.GetScoreInfoById(data.id);
            if (curScore > bestScore)
            {
                bestScore = curScore;
                DataService.GetInstance().Score.SaveScore(data.id, curScore);
            }
            _uiBestScoreText.text ="Best Score:" + bestScore.ToString();
            _uiCurScoreText.text = "Current Score:" + curScore.ToString();
        }
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
