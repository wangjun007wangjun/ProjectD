/********************************************************************
	created:	2020/02/12
	author:		OneJun
	purpose:	游戏菜单状态
*********************************************************************/
using Engine.State;
using Engine.UGUI;
using Engine.Asset;
using Data;
using UnityEngine;

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
                if (DataService.GetInstance().MusicDataCfgList == null)
                {
                    DataService.GetInstance().Score.Load();
                    _musicCfgAsset = AssetService.GetInstance().LoadInstantiateAsset("Menu/MusicCfg", LifeType.Manual);
                    _musicCfgAsset.RootGo.SetActive(true);
                    DataService.GetInstance().MusicDataCfgList = _musicCfgAsset.RootGo.GetComponent<MusicDataCfgList>();
                }
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
                DataService.GetInstance().Model = 1;
                StateService.Instance.ChangeState(GConst.StateKey.Lobby, 1);
            }
            if (key.Equals("OnClickMoShi2"))
            {
                DataService.GetInstance().Model = 2;
                StateService.Instance.ChangeState(GConst.StateKey.Lobby, 2);
            }
        }

        public void OnStateLeave()
        {
            if (_musicCfgAsset != null)
            {
                if (DataService.GetInstance().Model == 2)
                {
                    AssetService.GetInstance().Unload(_musicCfgAsset);
                    _musicCfgAsset = null;
                }
                
            }
            _menuForm.ActiveForm(false);
            // _menuForm.OnFormRemoved();
            // _menuForm = null;
        }

        public void OnStateChanged(string srcSt, string curSt, object usrData)
        {

        }


        public void UninitializeState()
        {
        }
    }
}
