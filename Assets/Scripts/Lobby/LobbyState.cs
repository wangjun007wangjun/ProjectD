/********************************************************************
	created:	2020/02/12
	author:		OneJun
	purpose:	游戏启动状态
*********************************************************************/
using Engine.Event;
using Engine.State;
using Engine.Sys;
using Engine.UGUI;
using Cfg;
using Data;
using UnityEngine;

namespace Lobby
{
    public class LobbyState : IState
    {
        private UILobbyForm _lobbyForm;

        public void InitializeState()
        {
        }

        public string Name()
        {
            return GConst.StateKey.Lobby;
        }

        public void OnStateEnter(object usrData = null)
        {
            if (_lobbyForm == null)
            {
                _lobbyForm = UIFormHelper.CreateFormClass<UILobbyForm>(OnLobbyFormAction, null, true);
            }
            else
            {
                _lobbyForm.ActiveForm(true);
            }
        }

        private void OnLobbyFormAction(string key, object param)
        {
            if (key.Equals("EnterGaming"))
            {
                Debug.Log("回调EnterGaming");
                StateService.Instance.ChangeState(GConst.StateKey.Game);
            }
        }

        public void OnStateLeave()
        {
            Debug.Log("离开主菜单");
            _lobbyForm.ActiveForm(false);
        }

        public void OnStateChanged(string srcSt, string curSt, object usrData)
        {


        }


        public void UninitializeState()
        {
        }
    }
}
