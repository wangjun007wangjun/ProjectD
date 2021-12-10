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
using System.Collections;

namespace Lobby
{
    public class UILobbyForm : UIFormClass, IScheduleHandler
    {
        private const int _uiStartBtnButtonIndex = 0;
        private const int _uiScrollViewLoopListView2Index = 1;
        private const int _uiBgMusicDataCfgListIndex = 2;

        private Button _uiStartBtnButton;
        private LoopListView2 _uiScrollViewLoopListView2;
        private MusicDataCfgList _uiBgMusicDataCfgList;



        private uint _timerFrame2;
        public override string GetPath()
        {
            return "Lobby/UILobbyWnd";
        }
        protected override void OnResourceLoaded()
        {
            _uiStartBtnButton = GetComponent(_uiStartBtnButtonIndex) as Button;
            _uiScrollViewLoopListView2 = GetComponent(_uiScrollViewLoopListView2Index) as LoopListView2;
            _uiBgMusicDataCfgList = GetComponent(_uiBgMusicDataCfgListIndex) as MusicDataCfgList;

            _uiStartBtnButton.onClick.AddListener(() =>
            {
                Debug.Log("点击开始");
                SendAction("EnterGaming");
            });
        }
        protected override void OnResourceUnLoaded()
        {
            _uiStartBtnButton = null;
            _uiScrollViewLoopListView2 = null;
            _uiBgMusicDataCfgList = null;

        }
        protected override void OnInitialize(object param)
        {
            SendAction("SendMusicCfgDataList", _uiBgMusicDataCfgList);

            _timerFrame2 = this.AddFrame(1, true);

            _uiScrollViewLoopListView2.InitListView(-1, OnRefreshListItem);
            // _uiScrollViewLoopListView2.InitListView(_uiBgMusicDataCfgList.list.Count, OnRefreshListItem);
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
        }
        protected override void OnAction(string id, object param)
        {

        }
        LoopListViewItem2 OnRefreshListItem(LoopListView2 listView, int index)
        {
            if (index < 0)
            {
                return null;
            }
            int indexTemp = index % _uiBgMusicDataCfgList.list.Count;
            MusicData itemData = _uiBgMusicDataCfgList.list[indexTemp];
            if (itemData == null)
            {
                return null;
            }
            LoopListViewItem2 item = listView.NewListViewItem("SelectItem");
            PrefabLink prefabLink = item.GetComponent<PrefabLink>();

            (prefabLink.GetCacheComponent(1) as UIDynImage).SetImageAddr("");
            (prefabLink.GetCacheComponent(2) as Text).text = "TODO" + index.ToString();
            (prefabLink.GetCacheComponent(7) as Text).text = itemData.name;
            Button button = (prefabLink.GetCacheComponent(3) as Button);
            GameObject star1 = prefabLink.GetCacheGameObject(4);
            GameObject star2 = prefabLink.GetCacheGameObject(5);
            GameObject star3 = prefabLink.GetCacheGameObject(6);

            for (int i = 0; i < (int)(itemData.difficulty + 1); i++)
            {

            }
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
                MusicData data = _uiBgMusicDataCfgList.list[indexTemp];

                SendAction("EnterGaming", data);
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