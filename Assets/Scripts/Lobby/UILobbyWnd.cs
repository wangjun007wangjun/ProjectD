/********************************************************************
  created:  2020-06-05         
  author:    OneJun           
  purpose:  大厅窗口                
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
using Engine.State;
using TMPro;

namespace Lobby
{
    public class UILobbyForm : UIFormClass, IScheduleHandler
    {
        private const int _uiAudioBtnButtonIndex = 0;
        private const int _uiScrollViewLoopListView2Index = 1;
        private const int _uiBgMusicDataCfgListIndex = 2;
        private const int _uiMusicSliderSliderIndex = 3;
        private const int _uiSoundSliderSliderIndex = 4;
        private const int _uiAudioSettingBtnButtonIndex = 5;
        private const int _uiBackBtnButtonIndex = 6;
        private const int _uiBgRectTransformIndex = 7;
        private const int _uiRankBtnButtonIndex = 8;

        private Button _uiAudioBtnButton;
        private LoopListView2 _uiScrollViewLoopListView2;
        private MusicDataCfgList _uiBgMusicDataCfgList;
        private Slider _uiMusicSliderSlider;
        private Slider _uiSoundSliderSlider;
        private Button _uiAudioSettingBtnButton;
        private Button _uiBackBtnButton;
        private RectTransform _uiBgRectTransform;
        private Button _uiRankBtnButton;
        private const int _uiPlayerNameTMPTextIndex = 9;
        private TextMeshProUGUI _uiPlayerNameTMPText;



        private uint _timerFrame2;


        public override string GetPath()
        {
            return "Lobby/UILobbyWnd";
        }
        protected override void OnResourceLoaded()
        {
            _uiAudioBtnButton = GetComponent(_uiAudioBtnButtonIndex) as Button;
            _uiScrollViewLoopListView2 = GetComponent(_uiScrollViewLoopListView2Index) as LoopListView2;
            _uiBgMusicDataCfgList = DataService.GetInstance().MusicDataCfgList;
            _uiMusicSliderSlider = GetComponent(_uiMusicSliderSliderIndex) as Slider;
            _uiSoundSliderSlider = GetComponent(_uiSoundSliderSliderIndex) as Slider;
            _uiAudioSettingBtnButton = GetComponent(_uiAudioSettingBtnButtonIndex) as Button;
            _uiBackBtnButton = GetComponent(_uiBackBtnButtonIndex) as Button;
            _uiBgRectTransform = GetComponent(_uiBgRectTransformIndex) as RectTransform;
            _uiRankBtnButton = GetComponent(_uiRankBtnButtonIndex) as Button;
            _uiPlayerNameTMPText = GetComponent(_uiPlayerNameTMPTextIndex) as TextMeshProUGUI;

            _uiAudioBtnButton.onClick.AddListener(() =>
            {
                // Debug.Log("点击音量");
                _uiAudioSettingBtnButton.gameObject.SetActive(true);
                _uiMusicSliderSlider.value = AudioService.GetInstance().GetVolumeByChannel(1) / 1.0f;
                _uiSoundSliderSlider.value = AudioService.GetInstance().GetVolumeByChannel(2) / 1.0f;
            });
            _uiBackBtnButton.onClick.AddListener(() =>
            {
                StateService.Instance.ChangeState(GConst.StateKey.Menu);
            });
            _uiRankBtnButton.onClick.AddListener(() =>
            {
                SendAction("OpenRank");
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
        }
        protected override void OnResourceUnLoaded()
        {
            _uiAudioBtnButton = null;
            _uiScrollViewLoopListView2 = null;
            _uiBgMusicDataCfgList = null;
            _uiMusicSliderSlider = null;
            _uiSoundSliderSlider = null;
            _uiAudioSettingBtnButton = null;
            _uiBackBtnButton = null;
            _uiBgRectTransform = null;
            _uiRankBtnButton = null;
            _uiPlayerNameTMPText = null;

        }
        protected override void OnInitialize(object param)
        {
            int model = (int)param;
            _uiBgRectTransform.gameObject.SetActive(model == 1);
            SendAction("SendMusicCfgDataList", _uiBgMusicDataCfgList);

            _timerFrame2 = this.AddFrame(1, true);

            _uiPlayerNameTMPText.text = DataService.GetInstance().Me.PlayerName;
            _uiScrollViewLoopListView2.InitListView(-1, OnRefreshListItem);
            _uiScrollViewLoopListView2.MovePanelToItemIndex(0, 0);
        }

        protected override void OnUninitialize()
        {
        }

        private void OnBagDataChangeEvent(EventArg arg)
        {
            RefreshMeInfo();
        }

        private void UpdateTaskHintRed(EventArg arg)
        {
        }
        //刷新信息
        private void RefreshMeInfo()
        {
        }

        protected override void OnUpdateUI(string id, object param)
        {
            if (id.Equals("RefreshAll"))
            {
                _uiBgRectTransform.gameObject.SetActive(DataService.GetInstance().Model == 1);

                _uiScrollViewLoopListView2.RefreshAllShownItem();
            }
        }
        protected override void OnAction(string id, object param)
        {

        }
        LoopListViewItem2 OnRefreshListItem(LoopListView2 listView, int index)
        {
            if (index < 0)
            {
                index = _uiBgMusicDataCfgList.list.Count + ((index + 1) % _uiBgMusicDataCfgList.list.Count) - 1;
            }
            int indexTemp = index % _uiBgMusicDataCfgList.list.Count;
            MusicData itemData = _uiBgMusicDataCfgList.list[indexTemp];
            if (itemData == null)
            {
                return null;
            }
            LoopListViewItem2 item = listView.NewListViewItem("SelectItem");
            PrefabLink prefabLink = item.GetComponent<PrefabLink>();

            (prefabLink.GetCacheComponent(1) as Image).sprite = itemData.musicTexture;
            (prefabLink.GetCacheComponent(2) as Text).text = "Best Score:" + DataService.GetInstance().Score.GetScoreInfoById(itemData.id).ToString();
            (prefabLink.GetCacheComponent(7) as Text).text = itemData.name;
            Button button = (prefabLink.GetCacheComponent(3) as Button);
            GameObject star1 = prefabLink.GetCacheGameObject(4);
            GameObject star2 = prefabLink.GetCacheGameObject(5);
            GameObject star3 = prefabLink.GetCacheGameObject(6);

            star1.SetActive(false);
            star2.SetActive(false);
            star3.SetActive(false);
            if (itemData.difficulty == GameDifficulty.Easy)
            {
                star1.SetActive(true);
            }
            else if (itemData.difficulty == GameDifficulty.Normal)
            {
                star1.SetActive(true);

                star2.SetActive(true);
            }
            else if (itemData.difficulty == GameDifficulty.Hard)
            {
                star1.SetActive(true);
                star2.SetActive(true);
                star3.SetActive(true);
            }
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                SendAction("EnterGaming", indexTemp);
            });
            return item;
        }

        public void OnScheduleHandle(ScheduleType type, uint id)
        {
            if (id == _timerFrame2)
            {
                _uiScrollViewLoopListView2.UpdateAllShownItemSnapData();
                int count = _uiScrollViewLoopListView2.ShownItemCount;
                for (int i = 0; i < count; ++i)
                {
                    LoopListViewItem2 item = _uiScrollViewLoopListView2.GetShownItemByIndex(i);
                    PrefabLink itemScript = item.GetComponent<PrefabLink>();
                    float scale = 1 - Mathf.Abs(item.DistanceWithViewPortSnapCenter) / 1000f;
                    scale = Mathf.Clamp(scale, 0.4f, 1);
                    (itemScript.GetCacheComponent(0) as CanvasGroup).alpha = scale;
                    itemScript.GetCacheTransform(0).localScale = new Vector3(scale, scale, 1);
                }
            }
        }
    }
}