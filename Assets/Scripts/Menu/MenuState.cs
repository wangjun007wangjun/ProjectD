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
        private BaseAsset _musicCfgAsset1;
        private BaseAsset _musicCfgAsset2;
        public void InitializeState()
        {
        }

        public string Name()
        {
            return GConst.StateKey.Menu;
        }

        public void OnStateEnter(object usrData = null)
        {
            if (_musicCfgAsset1 == null)
            {
                if (DataService.GetInstance().MusicDataCfgList == null)
                {
                    _musicCfgAsset1 = AssetService.GetInstance().LoadInstantiateAsset("Menu/MusicCfg1", LifeType.Manual);
                    _musicCfgAsset1.RootGo.SetActive(true);
                }
            }
            if (_musicCfgAsset2 == null)
            {
                if (DataService.GetInstance().MusicDataCfgList == null)
                {
                    _musicCfgAsset2 = AssetService.GetInstance().LoadInstantiateAsset("Menu/MusicCfg", LifeType.Manual);
                    _musicCfgAsset2.RootGo.SetActive(true);
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
                DataService.GetInstance().Score.Load();

                DataService.GetInstance().MusicDataCfgList = _musicCfgAsset1.RootGo.GetComponent<MusicDataCfgList>();
                StateService.Instance.ChangeState(GConst.StateKey.Lobby, 1);
            }
            if (key.Equals("OnClickMoShi2"))
            {
                DataService.GetInstance().Model = 2;
                DataService.GetInstance().Score.Load();

                DataService.GetInstance().MusicDataCfgList = _musicCfgAsset2.RootGo.GetComponent<MusicDataCfgList>();
                StateService.Instance.ChangeState(GConst.StateKey.Lobby, 2);
            }
        }

        public void OnStateLeave()
        {
            if (_musicCfgAsset1 != null)
            {
                if (DataService.GetInstance().Model == 2)
                {
                    AssetService.GetInstance().Unload(_musicCfgAsset1);
                    _musicCfgAsset1 = null;
                    AssetService.GetInstance().Unload(_musicCfgAsset2);
                    _musicCfgAsset2 = null;
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
