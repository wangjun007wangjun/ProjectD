/********************************************************************
	created:	2020/02/12
	author:		OneJun
	purpose:	游戏启动状态
*********************************************************************/
using Engine.State;
using Engine.UGUI;
using Engine.Schedule;
using UnityEngine;
using Net;

public class LaunchState : IState, IScheduleHandler
{
    private UILaunchForm _launchForm;

    private uint _launchTimer;

    public void InitializeState()
    {

    }

    public string Name()
    {
        return GConst.StateKey.Launch;
    }

    public void OnStateEnter(object usrData = null)
    {
        GLog.LogD("Enter Launch Game State");
        _launchForm = UIFormHelper.CreateFormClass<UILaunchForm>(null, null, false);
        _launchTimer = this.AddTimer(2000, false);
    }


    public void OnStateLeave()
    {
        if (_launchForm != null)
        {
            UIFormHelper.DisposeFormClass(_launchForm);
            _launchForm = null;
        }
       
    }

    public void OnStateChanged(string srcSt, string curSt, object usrData)
    {

    }


    public void UninitializeState()
    {
        if (_launchForm != null)
        {
            UIFormHelper.DisposeFormClass(_launchForm);
            _launchForm = null;
        }
        
    }

    public void OnScheduleHandle(ScheduleType type, uint id)
    {
        if (id == _launchTimer)
        {
            StateService.Instance.ChangeState(GConst.StateKey.Lobby);
            return;
        }
    }
    private void OnAgreePopUpAction(string key, object param)
    {
        

    }
    private bool IsFirstLogin()
    {
        return PlayerPrefs.GetInt("FirstLogin", 0) == 0;
    }
}
