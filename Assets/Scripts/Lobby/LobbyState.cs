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
        private RankWnd _rankForm;
        private BaseAsset _lobbyAsset;
        private int model = 1;
        public void InitializeState()
        {
        }

        public string Name()
        {
            return GConst.StateKey.Lobby;
        }

        public void OnStateEnter(object usrData = null)
        {
            model = DataService.GetInstance().Model;
            if (model == 2)
            {
                if (_lobbyAsset == null)
                {
                    _lobbyAsset = AssetService.GetInstance().LoadInstantiateAsset("Lobby/LobbyEnv", LifeType.Manual);
                    _lobbyAsset.RootGo.SetActive(true);
                }
            }

            if (_lobbyForm == null)
            {
                _lobbyForm = UIFormHelper.CreateFormClass<UILobbyForm>(OnLobbyFormAction, model, true);
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
            if(key.Equals("OpenRank"))
            {
                if (_rankForm == null)
                {
                    _rankForm = UIFormHelper.CreateFormClass<RankWnd>(OnRankFormAction, null, true);
                }
                else
                {
                    _rankForm.ActiveForm(true);
                }
            }
        }

        private void OnRankFormAction(string key, object param)
        {
            if(key.Equals("CloseRank"))
            {
                UIFormHelper.DisposeFormClass(_rankForm);
                _rankForm = null;
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
