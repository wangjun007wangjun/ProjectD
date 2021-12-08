using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class DanceItem : MonoBehaviour
{
    private Vector3 originScale = Vector3.one * 1.8f;
    public RectTransform scaleTran;
    public DOTweenAnimation dOTweenAnimation;
    public Button button;
    public GameObject failObj;
    public GameObject goodObj;
    public GameObject coolObj;
    public GameObject perfaceObj;
    private ItemDanceState curDanceState = ItemDanceState.None;
    private ItemDanceState lastDanceState = ItemDanceState.None;
    private bool isStart = false;
    // Start is called before the first frame update
    void Start()
    {
        button.onClick.AddListener(OnBtnClick);
    }

    // Update is called once per frame
    void Update()
    {
        if (!isStart)
        {
            return;
        }
        if (scaleTran.localScale.x >= 1.5f)
        {
            curDanceState = ItemDanceState.Good;
        }
        else if (scaleTran.localScale.x < 1.5f && scaleTran.localScale.x >= 1.2f)
        {
            curDanceState = ItemDanceState.Cool;
        }
        else if (scaleTran.localScale.x < 1.2f && scaleTran.localScale.x >= 0.9f)
        {
            curDanceState = ItemDanceState.Perfect;
        }
        else
        {
            curDanceState = ItemDanceState.Fail;
            SetStateShow();
        }
    }
    private void SetStateShow()
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
            //等待后回收自己TODO
        }
    }
    /// <summary>
    /// This function is called when the object becomes enabled and active.
    /// </summary>
    void OnEnable()
    {
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
        failObj.SetActive(false);
        goodObj.SetActive(false);
        coolObj.SetActive(false);
        perfaceObj.SetActive(false);
    }
    private void OnBtnClick()
    {
        SetStateShow();
    }
}