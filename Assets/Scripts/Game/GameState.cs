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

public class GameState : IState
{
    private UIGamingForm _gamingForm;

    private DanceMgr _danceMgr;
    public void InitializeState()
    {

    }

    public string Name()
    {
        return GConst.StateKey.Game;
    }

    public void OnStateEnter(object usrData = null)
    {
        LoadDanceGame();

        GLog.LogD("Enter Gaming State");
        _gamingForm = UIFormHelper.CreateFormClass<UIGamingForm>(OnGamingAction, null, false);
    }


    public void OnStateLeave()
    {
        if (_gamingForm != null)
        {
            UIFormHelper.DisposeFormClass(_gamingForm);
            _gamingForm = null;
        }

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
        else if (key.Equals("UpdateSafeArea"))
        {
            _danceMgr.OnSetSafeInfo(param);
        }
    }

    private void LoadDanceGame()
    {
        _danceMgr = new DanceMgr();
        _danceMgr.Init(_gamingForm);
    }
}
