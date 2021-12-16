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
using Engine.Asset;
using Engine.Audio;

namespace Lobby
{
    public class LobbyState : IState
    {
        private UILobbyForm _lobbyForm;
        private BaseAsset _lobbyAsset;
        public void InitializeState()
        {
        }

        public string Name()
        {
            return GConst.StateKey.Lobby;
        }

        public void OnStateEnter(object usrData = null)
        {
            if (_lobbyAsset == null)
            {
                _lobbyAsset = AssetService.GetInstance().LoadInstantiateAsset("Lobby/LobbyEnv", LifeType.Manual);
                _lobbyAsset.RootGo.SetActive(true);
            }
            if (_lobbyForm == null)
            {
                _lobbyForm = UIFormHelper.CreateFormClass<UILobbyForm>(OnLobbyFormAction, null, true);
            }
            else
            {
                _lobbyForm.ActiveForm(true);
                _lobbyForm.UpdateUI("RefreshAll", null);
            }
        }

        private void OnLobbyFormAction(string key, object param)
        {
            if (key.Equals("EnterGaming"))
            {
                // AudioService.GetInstance().UnloadAudioClipCache();
                MusicData data = param as MusicData;
                // Debug.Log("回调EnterGaming");
                StateService.Instance.ChangeState(GConst.StateKey.Game, data);
            }
        }

        public void OnStateLeave()
        {
            // Debug.Log("离开主菜单");
            if (_lobbyAsset != null)
            {
                AssetService.GetInstance().Unload(_lobbyAsset);
                _lobbyAsset = null;
            }
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
