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

public class RankWnd : UIFormClass
{
    private const int _uiTabBarTabBarIndex = 0;
    private const int _uiScrollViewLoopListView2Index = 1;
    private const int _uiCloseBtnButtonIndex = 2;

    private UITabBar _uiTabBarTabBar;
    private LoopListView2 _uiScrollViewLoopListView2;
    private Button _uiCloseBtnButton;


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

        rankData = new Dictionary<int, List<RankPlayerInfo>>();
        curRankData = new List<RankPlayerInfo>();

        _uiCloseBtnButton.onClick.AddListener(() =>{
            SendAction("CloseRank");
        });
        _uiTabBarTabBar.OnSelectChanged = OnTabChange;

        _uiTabBarTabBar.CurSellectedIndex = 0;
    }

    protected override void OnUninitialize()
    {
        _uiTabBarTabBar = null;
        _uiScrollViewLoopListView2 = null;
        _uiCloseBtnButton = null;
    }

    protected override void OnUpdateUI(string id, object param)
    {
    }
    protected override void OnAction(string id, object param)
    {

    }

    private void OnTabChange(int index, int tag)
    {
        if(rankData.ContainsKey(index))
        {
            if(rankData.TryGetValue(index, out curRankData))
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
            req.musicId = index;
            var url = NetDeclare.RankTotalAPI +index.ToString();

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

                rankData[index] = new List<RankPlayerInfo>(rankRsp.data);
                curRankData = rankData[index];
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
