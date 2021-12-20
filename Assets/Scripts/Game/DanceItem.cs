using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System;
using Engine.Asset;
using Engine.Schedule;
using Engine.Audio;

public class DanceItem : MonoBehaviour, IScheduleHandler
{    //初始大小
    private Vector3 originScale = Vector3.one * 3f;
    //气泡根节点
    public GameObject itemRoot;
    //点击后特效
    public GameObject clickObj;
    //缩放节点
    public RectTransform scaleTran;
    //缩放动画
    public DOTweenAnimation dOTweenAnimation;
    //点击按钮
    public Button button;
    public Button center;
    //失败文字
    public GameObject failObj;
    //Good文字
    public GameObject goodObj;
    //Cool文字
    public GameObject coolObj;
    //完没文字
    public GameObject perfaceObj;
    //当前气泡状态
    private ItemDanceState curDanceState = ItemDanceState.None;
    //上次气泡状态
    private ItemDanceState lastDanceState = ItemDanceState.None;
    //是否开始
    private bool isStart = false;

    //点击上后的回调
    public Action OnClickAction;
    //加分的回调
    public Action<ItemDanceState> OnAddScoreAction;

    //卸载延时timer
    private uint _unLoadTimer;

    // Start is called before the first frame update
    void Start()
    {
        button.onClick.AddListener(OnBtnClick);
        center.onClick.AddListener(OnBtnClick);
    }

    // Update is called once per frame
    void Update()
    {
        if (!isStart)
        {
            return;
        }
        //根据缩放大小确定当前状态
        if (scaleTran.localScale.x >= 1.7f)
        {
            curDanceState = ItemDanceState.Good;
        }
        else if (scaleTran.localScale.x < 1.7f && scaleTran.localScale.x >= 1.3f)
        {
            curDanceState = ItemDanceState.Cool;
        }
        else if (scaleTran.localScale.x < 1.3f && scaleTran.localScale.x >= 0.85f)
        {
            curDanceState = ItemDanceState.Perfect;
        }
        else if (scaleTran.localScale.x < 0.85f && scaleTran.localScale.x >= 0.5f)
        {
            curDanceState = ItemDanceState.Cool;
        }
        else if (scaleTran.localScale.x < 0.5f && scaleTran.localScale.x >= 0.31f)
        {
            curDanceState = ItemDanceState.Good;
        }
        else
        {
            curDanceState = ItemDanceState.Fail;
            isStart = false;
            //根据状态显示
            SetStateShow(false);
        }
    }

    //根据状态对于显示
    private void SetStateShow(bool isClick)
    {
        if (curDanceState != lastDanceState)
        {
            if (curDanceState == ItemDanceState.Fail)
            {
                failObj.SetActive(true);
            }
            else if (curDanceState == ItemDanceState.Good)
            {
                goodObj.SetActive(true);
            }
            else if (curDanceState == ItemDanceState.Cool)
            {
                coolObj.SetActive(true);
            }
            else if (curDanceState == ItemDanceState.Perfect)
            {
                perfaceObj.SetActive(true);
            }
            lastDanceState = curDanceState;
            //等待后回收自己
            if (isClick)
            {
                _unLoadTimer = this.AddTimer(600, false);
            }
            else
            {
                _unLoadTimer = this.AddTimer(150, false);
            }
        }
    }
    /// <summary>
    /// This function is called when the object becomes enabled and active.
    /// </summary>
    void OnEnable()
    {
        //显示后重置显示
        scaleTran.localScale = originScale;
        curDanceState = ItemDanceState.None;
        lastDanceState = ItemDanceState.None;
        // dOTweenAnimation.DORestart();
        dOTweenAnimation.DORestart();
        dOTweenAnimation.DOPlay();
        isStart = true;
    }
    /// <summary>
    /// This function is called when the behaviour becomes disabled or inactive.
    /// </summary>
    void OnDisable()
    {
        //消失后重置显示
        failObj.SetActive(false);
        goodObj.SetActive(false);
        coolObj.SetActive(false);
        perfaceObj.SetActive(false);
        clickObj.SetActive(false);
        itemRoot.SetActive(true);
    }

    //点击到，播放音效，特效，加分，状态显示
    private void OnBtnClick()
    {
        // Handheld.Vibrate();
        AudioService.GetInstance().Play(AudioChannelType.UI, "Audio/itemWillClear", false);

        clickObj.SetActive(true);
        itemRoot.SetActive(false);
        isStart = false;
        OnAddScoreAction(curDanceState);
        SetStateShow(true);
    }

    public void OnScheduleHandle(ScheduleType type, uint id)
    {
        //卸载资源
        if (id == _unLoadTimer)
        {
            OnClickAction();
        }
    }
}
