/********************************************************************
	created:	2020/02/16
	author:		OneJun
	
	purpose:	统一的TabBar 组件
*********************************************************************/
using System.Collections.Generic;
using UnityEngine;
using System;
using Engine.Base;

namespace Engine.UGUI
{
    public class UITabBar : UIComponent
    {

        [Header("是否静态模式(静态模式TabItem直接预制件做好！)")]
        public bool IsStaticMode = true;
        [Header("总是选中回调(只要Item点击就触发)")]
        public bool IsAwaysDispatch = false;
        //提供给外部的
        public Action<int, int> OnSelectChanged;

        [SerializeField]
        [Header("动态创建模板Go")]
        private GameObject _templateObj;
        //支持静态拖
        [SerializeField]
        [Header("UITabBarItem列表")]
        private List<UITabItem> _tabItemList = new List<UITabItem>();

        //当前选择的索引
        private int _curSelectIdx;
        private int _curSelectTag;

        /// <summary>
        /// 当前选中索引
        /// </summary>
        public int CurSellectedIndex
        {
            get { return _curSelectIdx; }
            set
            {
                DealTabItemSelected(value, -1);
            }
        }

        public int CurSellectedTag
        {
            get { return _curSelectTag; }
            set
            {
                DealTabItemSelectedByTag(value);
            }
        }

        public override void Initialize(UIForm formScript)
        {
            base.Initialize(formScript);
            if (_templateObj)
            {
                _templateObj.SetActive(false);
            }
            //重置item
            if (_tabItemList != null && _tabItemList.Count > 0)
            {
                for (int i = 0; i < _tabItemList.Count; i++)
                {
                    _tabItemList[i].Init(this, i);
                }
            }
            _curSelectIdx = -1;
            _curSelectTag=-1;
        }

        //关闭
        public override void OnClose()
        {
            if (!IsStaticMode)
            {
                ClearTabItems();
            }
            OnSelectChanged = null;
        }

        //动态创建
        public void InitTabItems(List<string> items, int defaultSelIndex = 0)
        {
            if (items == null || items.Count == 0)
            {
                return;
            }
            if (!IsTemplateValid())
            {
                GGLog.LogE("UITabBar InitTabItems Template Error");
                return;
            }
            for (int i = 0; i < items.Count; i++)
            {
                InstantiateItem(items[i]);
            }
            if (_tabItemList != null && _tabItemList[defaultSelIndex] != null)
            {
                _curSelectIdx = defaultSelIndex;
                _tabItemList[defaultSelIndex].SelState = true;
            }
        }

        private void InstantiateItem(string name)
        {
            GameObject tabObj = GameObject.Instantiate<GameObject>(_templateObj);
            RectTransform rectTf = tabObj.GetComponent<RectTransform>();
            rectTf.SetParent(this.transform.GetComponent<RectTransform>());
            rectTf.localScale = Vector3.one;
            rectTf.localPosition = Vector3.zero;
            tabObj.SetActive(true);
            UITabItem tab = tabObj.GetComponent<UITabItem>();
            tab.DynInit(this, _tabItemList.Count, name);
            _tabItemList.Add(tab);
        }

        private void ClearTabItems()
        {
            for (int i = 0; i < _tabItemList.Count; i++)
            {
                _tabItemList[i].UnInit();
                GameObject.Destroy(_tabItemList[i].gameObject);
            }
            _tabItemList.Clear();
        }

        public void DealTabItemSelectedByTag(int tag, bool inner = true)
        {
            if (_tabItemList.Count == 0)
            {
                return;
            }
            int realIndex = -1;
            for (int i = 0; i < _tabItemList.Count; i++)
            {
                if (_tabItemList[i].ClickTag == tag)
                {
                    realIndex = i;
                    break;
                }
            }
            if(realIndex!=-1){
                DealTabItemSelected(realIndex, tag, inner);
            }else{
                GGLog.LogE("UITabBar No Tag Set ");
            }
            
        }


        public void DealTabItemSelected(int index, int tag, bool inner = true)
        {
            if (_tabItemList.Count == 0 || index >= _tabItemList.Count)
            {
                return;
            }
            if (_curSelectIdx == index)
            {
                if (IsAwaysDispatch && inner)
                {
                    if (OnSelectChanged != null)
                    {
                        OnSelectChanged.Invoke(index, tag);
                    }
                }
                return;
            }
            if (_curSelectIdx != -1)
            {
                _tabItemList[_curSelectIdx].SelState = false;
            }
            _curSelectIdx = index;
            _curSelectTag = tag;
            if (_curSelectIdx != -1)
            {
                _tabItemList[_curSelectIdx].SelState = true;
            }
            if (OnSelectChanged != null && inner)
            {
                OnSelectChanged.Invoke(index, tag);
            }
        }

        private bool IsTemplateValid()
        {
            if (_templateObj == null)
            {
                return false;
            }
            if (_templateObj.GetComponent<UITabItem>() == null)
            {
                return false;
            }
            return true;
        }


    }
}
