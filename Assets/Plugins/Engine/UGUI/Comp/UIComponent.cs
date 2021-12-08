/********************************************************************
  created:  2020-03-17         
  author:    OneJun           
  purpose:  UI 组件定义                     
*********************************************************************/
using UnityEngine;

namespace Engine.UGUI
{
    public class UIComponent : MonoBehaviour
    {
        /// <summary>
        /// 所属窗口
        /// </summary>
        [HideInInspector]
        public UIForm BelongedForm;
        /// <summary>
        /// 所属滚动视图
        /// </summary>
        [HideInInspector]
        public UIListView BelongedListView;

        [HideInInspector]
        public int IndexInListView;

        /// <summary>
        /// 所属View
        /// </summary>
        [HideInInspector]
        public UIView BelongedView;

        //标记
        protected bool _isInited = false;

        public virtual void Initialize(UIForm form)
        {
            if (_isInited)
            {
                return;
            }
            BelongedForm = form;
            _isInited = true;
        }

        void OnDestroy()
        {
            BelongedForm = null;
        }

        /// <summary>
        /// 关闭
        /// </summary>
        public virtual void OnClose()
        {

        }
        //隐藏
        public virtual void OnHide()
        {

        }

        //显示
        public virtual void OnAppear()
        {

        }

        //设置SortingOrder
        public virtual void SetSortingOrder(int sortingOrder)
        {

        }

        /// 设置所属ListView控件脚本及索引
        public void SetBelongedList(UIListView belongedListScript, int index)
        {
            BelongedListView = belongedListScript;
            IndexInListView = index;
            //
            //SetSortingOrder(BelongedListView.BelongedForm.GetSortingOrder());
        }


        /// 遍历初始化UI组件
        protected void InitializeComponent(GameObject root)
        {
            UIComponent[] uiComponents = root.GetComponents<UIComponent>();
            if (uiComponents != null && uiComponents.Length > 0)
            {
                for (int i = 0; i < uiComponents.Length; i++)
                {
                    uiComponents[i].Initialize(BelongedForm);
                }
            }
            for (int i = 0; i < root.transform.childCount; i++)
            {
                InitializeComponent(root.transform.GetChild(i).gameObject);
            }
        }
    }
}
