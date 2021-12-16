/********************************************************************
	created:	2020/02/12
	author:		OneJun
	purpose:	游戏进行中的状态
*********************************************************************/
using Engine.State;
using Engine.UGUI;
using Engine.Schedule;
using UnityEngine;
using Net;
using System.Collections;

public class GameState : IState
{
    private UIGamingForm _gamingForm;

    private DanceMgr _danceMgr;
    private MusicData musicData;
    public void InitializeState()
    {

    }

    public string Name()
    {
        return GConst.StateKey.Game;
    }

    public void OnStateEnter(object usrData = null)
    {
        musicData = usrData as MusicData;

        _danceMgr = new DanceMgr();

        GLog.LogD("Enter Gaming State");
        _gamingForm = UIFormHelper.CreateFormClass<UIGamingForm>(OnGamingAction, null, false);

        _danceMgr.Init(this, _gamingForm, musicData.difficulty, _gamingForm.GetSafeAreaInfo(), musicData);
        // _danceMgr.OnInitMusicEnv(musicData.audio);
    }


    public void OnStateLeave()
    {
        if (_gamingForm != null)
        {
            UIFormHelper.DisposeFormClass(_gamingForm);
            _gamingForm = null;
        }
        _danceMgr.UnInit();
    }

    public void OnStateChanged(string srcSt, string curSt, object usrData)
    {

    }


    public void UninitializeState()
    {
        if (_gamingForm != null)
        {
            UIFormHelper.DisposeFormClass(_gamingForm);
            _gamingForm = null;
        }
    }

    public void OnScheduleHandle(ScheduleType type, uint id)
    {

    }
    private void OnGamingAction(string key, object param)
    {
        if (key.Equals("OnBtnExit"))
        {
            GLog.LogD("TODO退出游戏Gaming");
        }
        else if (key.Equals("StartGaming"))
        {
            GLog.LogD("倒计时结束，可以开始游戏了");
            _danceMgr.BeginDanceGame();
        }
        else if (key.Equals("ReContinueGameTimer"))
        {
            _danceMgr.ReTry(musicData);
        }
    }
    public void OnFinishDance(int totalScore)
    {
        Hashtable table = new Hashtable();
        table["MusicData"] = musicData;
        table["CurScore"] = totalScore;
        _gamingForm.UpdateUI("OnShowResult", table);
    }
}
