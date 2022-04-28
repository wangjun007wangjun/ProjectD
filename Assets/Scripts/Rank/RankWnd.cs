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
using System.Collections.Generic;
using Net;
using System;
using System.Globalization;
using TMPro;

public class RankWnd : UIFormClass
{
    private const int _uiTabBarTabBarIndex = 0;
    private const int _uiScrollViewLoopListView2Index = 1;
    private const int _uiCloseBtnButtonIndex = 2;
    private const int _uiName1TMPTextIndex = 3;
    private const int _uiName2TMPTextIndex = 4;
    private const int _uiName3TMPTextIndex = 5;
    private const int _uiName4TMPTextIndex = 6;
    private const int _uiName5TMPTextIndex = 7;
    private const int _uiName6TMPTextIndex = 8;

    private UITabBar _uiTabBarTabBar;
    private LoopListView2 _uiScrollViewLoopListView2;
    private Button _uiCloseBtnButton;
    private TextMeshProUGUI _uiName1TMPText;
    private TextMeshProUGUI _uiName2TMPText;
    private TextMeshProUGUI _uiName3TMPText;
    private TextMeshProUGUI _uiName4TMPText;
    private TextMeshProUGUI _uiName5TMPText;
    private TextMeshProUGUI _uiName6TMPText;



    private Dictionary<int, List<RankPlayerInfo>> rankData = new Dictionary<int, List<RankPlayerInfo>>();
    private List<RankPlayerInfo> curRankData = new List<RankPlayerInfo>();

    bool isInitLoop = false;
    public override string GetPath()
    {
        return "Rank/UIRankWnd";
    }
    protected override void OnResourceLoaded()
    {
    }
    protected override void OnResourceUnLoaded()
    {


    }
    protected override void OnInitialize(object param)
    {
        _uiTabBarTabBar = GetComponent(_uiTabBarTabBarIndex) as UITabBar;
        _uiScrollViewLoopListView2 = GetComponent(_uiScrollViewLoopListView2Index) as LoopListView2;
        _uiCloseBtnButton = GetComponent(_uiCloseBtnButtonIndex) as Button;
        _uiName1TMPText = GetComponent(_uiName1TMPTextIndex) as TextMeshProUGUI;
        _uiName2TMPText = GetComponent(_uiName2TMPTextIndex) as TextMeshProUGUI;
        _uiName3TMPText = GetComponent(_uiName3TMPTextIndex) as TextMeshProUGUI;
        _uiName4TMPText = GetComponent(_uiName4TMPTextIndex) as TextMeshProUGUI;
        _uiName5TMPText = GetComponent(_uiName5TMPTextIndex) as TextMeshProUGUI;
        _uiName6TMPText = GetComponent(_uiName6TMPTextIndex) as TextMeshProUGUI;

        rankData = new Dictionary<int, List<RankPlayerInfo>>();
        curRankData = new List<RankPlayerInfo>();

        _uiCloseBtnButton.onClick.AddListener(() =>
        {
            SendAction("CloseRank");
        });
        _uiTabBarTabBar.OnSelectChanged = OnTabChange;

        _uiTabBarTabBar.CurSellectedIndex = 0;

        //歌曲名字
        _uiName1TMPText.text = DataService.GetInstance().MusicDataCfgList.list[0].name;
        _uiName2TMPText.text = DataService.GetInstance().MusicDataCfgList.list[1].name;
        _uiName3TMPText.text = DataService.GetInstance().MusicDataCfgList.list[2].name;
        _uiName4TMPText.text = DataService.GetInstance().MusicDataCfgList.list[3].name;
        _uiName5TMPText.text = DataService.GetInstance().MusicDataCfgList.list[4].name;
        _uiName6TMPText.text = DataService.GetInstance().MusicDataCfgList.list[5].name;
    }

    protected override void OnUninitialize()
    {
        _uiTabBarTabBar = null;
        _uiScrollViewLoopListView2 = null;
        _uiCloseBtnButton = null;
        _uiName1TMPText = null;
        _uiName2TMPText = null;
        _uiName3TMPText = null;
        _uiName4TMPText = null;
        _uiName5TMPText = null;
        _uiName6TMPText = null;

    }

    protected override void OnUpdateUI(string id, object param)
    {
    }
    protected override void OnAction(string id, object param)
    {

    }

    private void OnTabChange(int index, int tag)
    {
        int tempIndex = index + DataService.GetInstance().Model * 100;
        if (rankData.ContainsKey(tempIndex))
        {
            if (rankData.TryGetValue(tempIndex, out curRankData))
            {
                if (isInitLoop)
                {
                    _uiScrollViewLoopListView2.SetListItemCount(curRankData.Count);
                    _uiScrollViewLoopListView2.RefreshAllShownItem();

                }
                else
                {
                    _uiScrollViewLoopListView2.InitListView(curRankData.Count, RefreshItem);
                    isInitLoop = true;
                }
            }
        }
        else
        {
            // 请求数据
            RankReq req = new RankReq();
            req.musicId = tempIndex;
            var url = NetDeclare.RankTotalAPI + tempIndex.ToString();

            UICommon.GetInstance().ShowWaiting("...", true);
            //请求
            NetService.GetInstance().SendNetPostReq(url, req, (isError, rspStr) =>
            {
                UICommon.GetInstance().CleanWaiting();
                if (isError)
                {
                    UICommon.GetInstance().ShowBubble(rspStr);
                    return;
                }
                RankRsp rankRsp = JsonUtility.FromJson<RankRsp>(rspStr);
                if (rankRsp.code != 2000)
                {
                    UICommon.GetInstance().ShowBubble(rankRsp.message);
                    return;
                }

                rankData[tempIndex] = new List<RankPlayerInfo>(rankRsp.data);
                curRankData = rankData[tempIndex];
                if (isInitLoop)
                {
                    _uiScrollViewLoopListView2.SetListItemCount(curRankData.Count);
                    _uiScrollViewLoopListView2.RefreshAllShownItem();
                }
                else
                {
                    _uiScrollViewLoopListView2.InitListView(curRankData.Count, RefreshItem);
                    isInitLoop = true;
                }
            }, false);
        }
    }
    private LoopListViewItem2 RefreshItem(LoopListView2 listView, int index)
    {
        if (index < 0)
        {
            return null;
        }
        RankPlayerInfo data = curRankData[index];
        LoopListViewItem2 item = listView.NewListViewItem("Rank_Item");
        PrefabLink prefabLink = item.GetComponent<PrefabLink>();
        (prefabLink.GetCacheTransform(0).gameObject).SetActive(data.name != DataService.GetInstance().Me.PlayerName);
        (prefabLink.GetCacheTransform(5).gameObject).SetActive(data.name == DataService.GetInstance().Me.PlayerName);
        (prefabLink.GetCacheComponent(1) as Text).text = (index + 1).ToString();
        // (prefabLink.GetCacheComponent(1) as Text).text = data.daily_rank.ToString();
        (prefabLink.GetCacheComponent(2) as Text).text = data.name;
        (prefabLink.GetCacheComponent(3) as Text).text = data.score.ToString();

        DateTime v = Convert.ToDateTime(data.update_time);
        (prefabLink.GetCacheComponent(4) as Text).text = v.ToString("yyyy-MM-dd HH:mm:ss");
        return item;
    }
}
