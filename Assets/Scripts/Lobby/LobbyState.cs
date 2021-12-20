/********************************************************************
	created:	2020/02/12
	author:		OneJun
	purpose:	游戏启动状态
*********************************************************************/
using Engine.State;
using Engine.UGUI;
using Engine.Asset;
using Data;

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
                int dataIndex = (int)param;
                // Debug.Log("回调EnterGaming");
                MusicData data = DataService.GetInstance().MusicDataCfgList.list[dataIndex];
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
