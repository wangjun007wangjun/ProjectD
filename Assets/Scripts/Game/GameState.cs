/********************************************************************
	created:	2020/02/12
	author:		OneJun
	purpose:	游戏进行中的状态
*********************************************************************/
using Engine.State;
using Engine.UGUI;
using Engine.Schedule;
using DG.Tweening;
using Net;
using System.Collections;

public class GameState : IState
{
    //主界面
    private UIGamingForm _gamingForm;

    //游戏进行管理器
    private DanceMgr _danceMgr;

    //当前的音乐数据配置
    private MusicData musicData;

    public bool isShoudongPause = false;
    public void InitializeState()
    {

    }

    public string Name()
    {
        return GConst.StateKey.Game;
    }

    public void OnStateEnter(object usrData = null)
    {
        isShoudongPause = false;
        musicData = usrData as MusicData;

        _danceMgr = new DanceMgr();

        GLog.LogD("Enter Gaming State");
        _gamingForm = UIFormHelper.CreateFormClass<UIGamingForm>(OnGamingAction, null, false);

        _danceMgr.Init(this, _gamingForm, musicData.difficulty, _gamingForm.GetSafeAreaInfo(), musicData);
    }


    public void OnStateLeave()
    {
        if (_gamingForm != null)
        {
            UIFormHelper.DisposeFormClass(_gamingForm);
            _gamingForm = null;
        }
        _danceMgr.UnInit();
        DOTween.KillAll();
        isShoudongPause = false;
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
            // GLog.LogD("倒计时结束，可以开始游戏了");
            _danceMgr.BeginDanceGame();
        }
        else if (key.Equals("ReContinueGameTimer"))
        {
            _danceMgr.ReTry(musicData);
        }
        else if (key.Equals("GamePause"))
        {
            bool isPause = (bool)param;
            isShoudongPause = isPause == true;
            _danceMgr.OnSetMusicEnvPause(isPause);
        }
    }
    public void OnGamePause()
    {
        _danceMgr.OnSetMusicEnvPause(true);
    }
    public void OnGameContinue()
    {
        if(!isShoudongPause)
        {
            _danceMgr.OnSetMusicEnvPause(false);
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
