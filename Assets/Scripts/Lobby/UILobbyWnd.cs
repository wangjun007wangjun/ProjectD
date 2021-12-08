/********************************************************************
  created:  2020-06-05         
  author:    OneJun           
  purpose:  大厅窗口                
*********************************************************************/
using Engine.UGUI;
using Engine.Event;
using UnityEngine.UI;
using Data;
using UnityEngine;

namespace Lobby
{
    public class UILobbyForm : UIFormClass
    {
        private const int _uiStartBtnButtonIndex = 0;

        private Button _uiStartBtnButton;

        public override string GetPath()
        {
            return "Lobby/UILobbyWnd";
        }
        protected override void OnResourceLoaded()
        {
            _uiStartBtnButton = GetComponent(_uiStartBtnButtonIndex) as Button;
            _uiStartBtnButton.onClick.AddListener(()=>{
                Debug.Log("点击开始");
                SendAction("EnterGaming");
            });
        }
        protected override void OnResourceUnLoaded()
        {
            _uiStartBtnButton = null;

        }
        protected override void OnInitialize(object param)
        {

        }

        protected override void OnUninitialize()
        {
        }

        private void OnBagDataChangeEvent(EventArg arg)
        {
            RefreshMeInfo();
        }

        private void UpdateTaskHintRed(EventArg arg)
        {
        }
        //刷新信息
        private void RefreshMeInfo()
        {
        }

        protected override void OnUpdateUI(string id, object param)
        {
        }
        protected override void OnAction(string id, object param)
        {

        }
    }
}