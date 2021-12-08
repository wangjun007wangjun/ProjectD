/********************************************************************
  created:  2020-04-01         
  author:    OneJun  

  purpose:  视图类 原则上尽量以窗口为单位 应用类开发 局部视图可以用一个VIew 预制体                
*********************************************************************/
using Engine.Base;
using UnityEngine;

namespace Engine.UGUI
{
    public class UIView : UIViewLink
    {
        /// <summary>
        /// 所属窗口
        /// </summary>
        [HideInInspector]
        public UIForm BelongedForm;

        /// <summary>
        /// 控制器
        /// </summary>
        [HideInInspector]
        public UIViewClass Controller = null;

        /// <summary>
        /// UIComponent列表
        /// </summary>
        private ObjList<UIComponent> _uiComponents;

        /// <summary>
        /// 是否初始化完成
        /// </summary>
        private bool _isInitialIzed = false;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="form"></param>
        public void Initialize(UIForm form)
        {
            if(_isInitialIzed)
            {
                return;
            }
            _isInitialIzed = true;

            BelongedForm = form;
            //初始化UI组件
            UIComponent[] ccs = new UIComponent[50];
            int ccCnt = 0;
            UIUtility.GetComponentsInChildren<UIComponent>(gameObject, ccs, ref ccCnt);
            if(ccs != null && ccCnt > 0)
            {
                if(_uiComponents == null)
                {
                    _uiComponents = new ObjList<UIComponent>();
                }
                for (int i = 0; i < ccCnt; i++)
                {   
                    ccs[i].Initialize(BelongedForm);
                    ccs[i].BelongedView = this;
                    //Cache
                    _uiComponents.Add(ccs[i]);
                }
            }
        }

        public void UnInitialize()
        {
            _isInitialIzed = false;
            if(_uiComponents != null)
            {
                _uiComponents.Clear();
                _uiComponents = null;
            }
        }
    }
}