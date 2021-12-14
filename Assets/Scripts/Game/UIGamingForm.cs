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
        _uiScoreText = GetComponent(_uiScoreTextIndex) as Text;
        _uiResultPanelRectTransform = GetComponent(_uiResultPanelRectTransformIndex) as RectTransform;
        _uiMusicPicImage = GetComponent(_uiMusicPicImageIndex) as Image;
        _uiMusicNameText = GetComponent(_uiMusicNameTextIndex) as Text;
        _uiBestScoreText = GetComponent(_uiBestScoreTextIndex) as Text;
        _uiCurScoreText = GetComponent(_uiCurScoreTextIndex) as Text;
        _uiExitButton = GetComponent(_uiExitButtonIndex) as Button;
        _uiReContinueButton = GetComponent(_uiReContinueButtonIndex) as Button;

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
