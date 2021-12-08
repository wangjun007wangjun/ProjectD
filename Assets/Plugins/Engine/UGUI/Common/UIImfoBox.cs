/********************************************************************
    created:	2020-05-26 				
    author:		OneJun						
    purpose:	信息弹窗								
*********************************************************************/
using Engine.UGUI;
using UnityEngine.UI;

namespace Engine.UGUI
{
    public class UIInfoBox : UIFormClass
    {
        private const int _uiTitleTextIndex = 0;
        private const int _uiInfoTextIndex = 1;

        private Text _uiTitleText;
        private Text _uiInfoText;


        public override string GetPath()
        {
            return "Common/UIInfoBox";
        }

        /// <summary>
        /// 资源加载后回调
        /// </summary>
        protected override void OnResourceLoaded()
        {
            _uiTitleText = GetComponent(_uiTitleTextIndex) as Text;
            _uiInfoText = GetComponent(_uiInfoTextIndex) as Text;


        }

        /// <summary>
        /// 资源卸载后回调
        /// </summary>
        protected override void OnResourceUnLoaded()
        {
            _uiTitleText = GetComponent(_uiTitleTextIndex) as Text;
            _uiInfoText = GetComponent(_uiInfoTextIndex) as Text;


        }

        /// <summary>
        /// 逻辑初始化
        /// </summary>
        /// <param name="parameter">初始化参数</param>
        protected override void OnInitialize(object parameter)
        {

        }

        /// <summary>
        /// 逻辑反初始化
        /// </summary>
        protected override void OnUninitialize()
        {

        }

        
        public void InitShow(string title, string content)
        {

            _uiTitleText.text = title;
            _uiInfoText.text = content;

        }

        /// <summary>
        /// 按钮动作处理
        /// </summary>
        /// <param name="id"></param>
        /// <param name="param"></param>
        protected override void OnAction(string id, object param)
        {

            if (id.Equals("Close"))
            {
                UIFormHelper.DisposeFormClass(this);
                return;
            }
         
        }
    }
}

