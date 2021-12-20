/********************************************************************
	created:	2020/02/12
	author:		OneJun
	purpose:	游戏菜单状态
*********************************************************************/
using Engine.State;
using Engine.UGUI;
using Engine.Asset;
using Data;

namespace Lobby
{
    public class MenuState : IState
    {
        private UIMenuWnd _menuForm;
        private BaseAsset _musicCfgAsset;
        public void InitializeState()
        {
        }

        public string Name()
        {
            return GConst.StateKey.Menu;
        }

        public void OnStateEnter(object usrData = null)
        {
            if (_musicCfgAsset == null)
            {
                DataService.GetInstance().Score.Load();

                _musicCfgAsset = AssetService.GetInstance().LoadInstantiateAsset("Menu/MusicCfg", LifeType.Manual);
                DataService.GetInstance().MusicDataCfgList = _musicCfgAsset.RootGo.GetComponent<MusicDataCfgList>();
            }
            if (_menuForm == null)
            {
                _menuForm = UIFormHelper.CreateFormClass<UIMenuWnd>(OnMenuFormAction, null, true);
            }
            else
            {
                _menuForm.ActiveForm(true);
            }
        }

        private void OnMenuFormAction(string key, object param)
        {
            if (key.Equals("OnClickMoShi1"))
            {
                StateService.Instance.ChangeState(GConst.StateKey.Lobby);
            }
            if (key.Equals("OnClickMoShi2"))
            {
                StateService.Instance.ChangeState(GConst.StateKey.Lobby);
            }
        }

        public void OnStateLeave()
        {
            if (_musicCfgAsset != null)
            {
                AssetService.GetInstance().Unload(_musicCfgAsset);
                _musicCfgAsset = null;
            }
            _menuForm.ActiveForm(false);
        }

        public void OnStateChanged(string srcSt, string curSt, object usrData)
        {

        }


        public void UninitializeState()
        {
        }
    }
}
