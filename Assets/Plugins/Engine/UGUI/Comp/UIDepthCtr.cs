/********************************************************************
  created:  2020-06-05         
  author:    OneJun           

  purpose:  层级显示控制                
*********************************************************************/
using UnityEngine;
using UnityEngine.UI;

namespace Engine.UGUI
{
    public class UIDepthCtr : UIComponent
    {
        [Header("UI模式")]
        public bool IsUI = true;

        [Header("排序偏移")]
        public int SortOffset = 1;

        public override void Initialize(UIForm form)
        {
            if(_isInited)
            {
                //return;
            }
            base.Initialize(form);
            _isInited = true;
        }

        public override void OnClose()
        {

        }
        //隐藏
        public override void OnHide()
        {

        }

        //显示
        public override void OnAppear()
        {

        }

        public void OnEnable()
        {
            if (BelongedForm != null)
            {
                SetSortingOrder(BelongedForm.GetSortingOrder());
            }
        }

        /// <summary>
        /// 设置SortingOrder
        /// </summary>
        /// <param name="sortingOrder"></param>
        public override void SetSortingOrder(int sortingOrder)
        {
            if(IsUI)
            {
                Canvas canvas = GetComponent<Canvas>();
                if(canvas == null)
                {
                    canvas = gameObject.AddComponent<Canvas>();
                }
                if(canvas != null)
                {
                    canvas.overrideSorting = true;
                    canvas.sortingOrder = sortingOrder + SortOffset;
                }
                GraphicRaycaster gr = GetComponent<GraphicRaycaster>();
                if(gr == null)
                {
                    gr = gameObject.AddComponent<GraphicRaycaster>();
                }
            }
            else
            {
                Renderer[] renders = GetComponentsInChildren<Renderer>();
                foreach (Renderer render in renders)
                {
                    render.sortingOrder = sortingOrder + SortOffset;
                }
            }
        }
    }
}