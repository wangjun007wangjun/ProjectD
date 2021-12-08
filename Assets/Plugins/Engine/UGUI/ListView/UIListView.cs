/********************************************************************
  created:  2020-03-30         
  author:    OneJun           
  purpose:   列表视图               
*********************************************************************/
using DG.Tweening;
using System.Collections.Generic;
using Engine.Base;
using UnityEngine;
using UnityEngine.UI;

namespace Engine.UGUI
{
    public enum UIListViewType
    {
        Vertical,//垂直列表
        Horizontal,//水平列表
        VerticalGrid,//垂直表格
        HorizontalGrid,//水平表格
    };

    public class UIListView : UIComponent
    {
        [Header("滚动类型")]
        public UIListViewType m_listType = UIListViewType.Vertical;
        
        [Header("禁止滚动")]
        public bool m_forbidScroll = false;

        [Header("计算优化")]
        public bool m_useOptimized = false;

        [Header("当前Item数量")]
        public int m_elementAmount = 0;

        [Header("Item间隔")]
        public Vector2 m_elementSpacing = Vector2.zero;

        [Header("Item内部偏移,Vertical和Grid为y方向偏移，Horizontal为x方向偏移")]
        public float m_elementLayoutOffset = 0;

        [Header("自动居中显示")]
        public bool m_autoCenteredEnlements = false;

        [Header("空提示显示对象，List中没有元素时需要显示的提示Object")]
        public GameObject m_ZeroTipsObj = null;

        [Header("始终发送Item， 选中事件")]
        public bool m_alwaysDispatchSelectedChangeEvent;

        [Header("额外显示对象")]
        public GameObject m_extraContent = null;

        [Header("外部控制滚动")]
        public bool m_scrollExternal = false;

        //当前选中的元素索引
        protected int m_selectedElementIndex = -1;
        //上次选中的元素索引
        protected int m_lastSelectedElementIndex = -1;

        //选中项发生改变派发事件
        public System.Action<UIListViewItem,UIListViewItem> SelectedChangedHandler;

        //Scroll发生改变派发事件
        public System.Action ScrollChangedHandler;
        //激活item 处理
        public System.Action<UIListViewItem> OnEnableItemHandler;

        //元素
        protected ObjList<UIListViewItem> m_elementScripts;
        protected ObjList<UIListViewItem> m_unUsedElementScripts;
        protected GameObject m_elementTemplate;
        protected string m_elementName;
        protected Vector2 m_elementDefaultSize;
        protected List<Vector2> m_elementsSize;
        protected List<stRect> m_elementsRect;

        //Scroll区域相关
        [HideInInspector]
        public ScrollRect m_scrollRect;
        [HideInInspector]
        public Vector2 m_scrollAreaSize;
        protected GameObject m_content;
        protected RectTransform m_contentRectTransform;
        protected Vector2 m_contentSize;
        protected Scrollbar m_scrollBar;
        protected Vector2 m_scrollValue;
        protected Vector2 m_lastContentPosition;
        /// 是否动态调整ListSize
        [HideInInspector]
        public bool m_autoAdjustScrollAreaSize = false;
        [HideInInspector]
        public Vector2 m_scrollRectAreaMaxSize = new Vector2(100, 100);

        public override void Initialize(UIForm form)
        {
            if(_isInited)
            {
                return;
            }
            base.Initialize(form);

            m_selectedElementIndex = -1;
            m_lastSelectedElementIndex = -1;

            //初始化
            m_scrollRect = GetComponentInChildren<ScrollRect>(gameObject);
            if(m_scrollRect != null)
            {
                m_scrollRect.enabled = false;
                RectTransform scrollRectRectTransform = m_scrollRect.transform as RectTransform;
                m_scrollAreaSize = new Vector2(scrollRectRectTransform.rect.width,scrollRectRectTransform.rect.height);
                m_content = m_scrollRect.content.gameObject;
            }
            
            //初始化Scrollbar
            m_scrollBar = GetComponentInChildren<Scrollbar>(gameObject);
            if(m_listType == UIListViewType.Vertical || m_listType == UIListViewType.VerticalGrid)
            {
                if(m_scrollBar != null)
                {
                    m_scrollBar.direction = Scrollbar.Direction.BottomToTop;
                }
                if(m_scrollRect != null)
                {
                    m_scrollRect.horizontal = false;
                    m_scrollRect.vertical = true;
                    m_scrollRect.horizontalScrollbar = null;
                    m_scrollRect.verticalScrollbar = m_scrollBar;
                }
            }
            else if(m_listType == UIListViewType.Horizontal || m_listType == UIListViewType.HorizontalGrid)
            {
                if(m_scrollBar != null)
                {
                    m_scrollBar.direction = Scrollbar.Direction.LeftToRight;
                }

                if(m_scrollRect != null)
                {
                    m_scrollRect.horizontal = true;
                    m_scrollRect.vertical = false;
                    m_scrollRect.horizontalScrollbar = m_scrollBar;
                    m_scrollRect.verticalScrollbar = null;
                }
            }

            if(m_forbidScroll)
            {
                m_scrollRect.horizontal = false;
                m_scrollRect.vertical = false;
            }

            //初始化元素列表
            m_elementScripts = new ObjList<UIListViewItem>();
            m_unUsedElementScripts = new ObjList<UIListViewItem>();

            if(m_useOptimized)
            {
                m_elementsRect = new List<stRect>();
            }

            //模板
            UIListViewItem listElementScript = null;
            listElementScript = GetComponentInChildren<UIListViewItem>(gameObject);

            if(listElementScript != null)
            {
                listElementScript.Initialize(form);
                m_elementTemplate = listElementScript.gameObject;
                m_elementName = listElementScript.gameObject.name;
                m_elementDefaultSize = listElementScript.m_defaultSize;
                //
                if (m_elementTemplate != null)
                {
                    m_elementTemplate.name = m_elementName + "_Template";
                }
            }

            //初始化为不可见
            if (m_elementTemplate != null)
            {
                UIListViewItem elementScript = m_elementTemplate.GetComponent<UIListViewItem>();
                if (elementScript != null && elementScript.m_useSetActiveForDisplay)
                {
                    m_elementTemplate.ExtSetActive(false);
                }
                else
                {
                    //确保Element被激活
                    if (!m_elementTemplate.activeSelf)
                    {
                        m_elementTemplate.SetActive(true);
                    }
                    //为List元素模版添加Canvas Group
                    CanvasGroup canvasGroup = m_elementTemplate.GetComponent<CanvasGroup>();
                    if (canvasGroup == null)
                    {
                        canvasGroup = m_elementTemplate.AddComponent<CanvasGroup>();
                    }
                    canvasGroup.alpha = 0f;
                    canvasGroup.blocksRaycasts = false;
                }
            }

            //初始化content位置
            if (m_content != null)
            {
                m_contentRectTransform = m_content.transform as RectTransform;
                m_contentRectTransform.pivot = new Vector2(0, 1);
                m_contentRectTransform.anchorMin = new Vector2(0, 1);
                m_contentRectTransform.anchorMax = new Vector2(0, 1);
                m_contentRectTransform.anchoredPosition = Vector2.zero;
                m_contentRectTransform.localRotation = Quaternion.identity;
                m_contentRectTransform.localScale = new Vector3(1, 1, 1);
                m_lastContentPosition = m_contentRectTransform.anchoredPosition;
            }

            //初始化extraContent if exist
            if (m_extraContent != null)
            {
                RectTransform extraContentTransform = m_extraContent.transform as RectTransform;
                extraContentTransform.pivot = new Vector2(0, 1);
                extraContentTransform.anchorMin = new Vector2(0, 1);
                extraContentTransform.anchorMax = new Vector2(0, 1);
                extraContentTransform.anchoredPosition = Vector2.zero;
                extraContentTransform.localRotation = Quaternion.identity;
                extraContentTransform.localScale = Vector3.one;
                //宽度与listElement保持一致
                if (m_elementTemplate != null)
                {
                    extraContentTransform.sizeDelta = new Vector2((m_elementTemplate.transform as RectTransform).rect.width, extraContentTransform.sizeDelta.y);
                }
                if (extraContentTransform.parent != null && m_content != null)
                {
                    extraContentTransform.parent.SetParent(m_content.transform);
                }
                m_extraContent.SetActive(false);
            }
            SetElementAmount(m_elementAmount);
        }

        /// <summary>
        /// 窗口关闭
        /// </summary>
        public override void OnClose()
        {
            for (int i = 0; i < m_elementScripts.Count; i++)
            {
                m_elementScripts[i].Close();
            }

            for (int i = 0; i < m_unUsedElementScripts.Count; i++)
            {
                m_unUsedElementScripts[i].Close();
            }
        }

        //隐藏
        public override void OnHide()
        {
            //子条目显示 根据SetAcount 处理
        }

        //显示
        public override void OnAppear()
        {
            //子条目显示 根据SetAcount 处理
        }

        //销毁
        void OnDestroy()
        {
            //
            DOTween.Kill(this.gameObject);
            //
            if (m_elementScripts != null)
            {
                m_elementScripts.Clear();
                m_elementScripts = null;
            }
            if (m_unUsedElementScripts != null)
            {
                m_unUsedElementScripts.Clear();
                m_unUsedElementScripts = null;
            }
        }

        protected virtual void Update()
        {
            if (BelongedForm != null && BelongedForm.IsClosed())
            {
                return;
            }
            //优化模式
            if (m_useOptimized)
            {
                UpdateElementsScroll();
            }

            //屏蔽滚动
            if (m_scrollRect != null && !m_scrollExternal)
            {
                if (m_contentSize.x > m_scrollAreaSize.x || m_contentSize.y > m_scrollAreaSize.y)
                {
                    if (!m_scrollRect.enabled)
                    {
                        m_scrollRect.enabled = true;
                    }                    
                }
                else
                {
                    if (Mathf.Abs(m_contentRectTransform.anchoredPosition.x) < 0.001 && Mathf.Abs(m_contentRectTransform.anchoredPosition.y) < 0.001)
                    {
                        if (m_scrollRect.enabled)
                        {
                            m_scrollRect.enabled = false;
                        }                        
                    }
                }

                DetectScroll();
            } 
        }

        // 检测是否发生滚动
        protected void DetectScroll()
        {
            if (m_contentRectTransform.anchoredPosition != m_lastContentPosition)
            {
                if (m_listType == UIListViewType.Horizontal || m_listType == UIListViewType.HorizontalGrid)
                {
                    float scrollX = m_contentSize.x == m_scrollAreaSize.x ? 0 : m_contentRectTransform.anchoredPosition.x / (m_contentSize.x - m_scrollAreaSize.x);
                    OnScrollValueChanged(new Vector2(scrollX, 0));
                }
                else if (m_listType == UIListViewType.VerticalGrid || m_listType == UIListViewType.Vertical)
                {
                    float scrollY = m_contentSize.y == m_scrollAreaSize.y ? 0 : m_contentRectTransform.anchoredPosition.y / (m_contentSize.y - m_scrollAreaSize.y);
                    OnScrollValueChanged(new Vector2(0, scrollY));
                }
                m_lastContentPosition = m_contentRectTransform.anchoredPosition;
            }
        }

        /// Scroll回调
        protected void OnScrollValueChanged(Vector2 value)
        {
            m_scrollValue = value;
            DispatchScrollChangedEvent();
        }

        /// 设置元素数量
        /// !!该函数会触发Element OnCreate
        public void SetElementAmount(int amount)
        {
            SetElementAmount(amount, null);
        }

        /// 设置元素数量
        /// 会触发Element OnCreate
        private void SetElementAmount(int amount, List<Vector2> elementsSize)
        {
            if (amount < 0)
            {
                amount = 0;
            }

            if (elementsSize != null && amount != elementsSize.Count)
            {
                GGLog.LogE("List element amount is not equal to the count of elementsSize!!!");
                return;
            }

            //回收所有Element，但不设置为disable
            RecycleElement(false);

            //设定元素数量
            m_elementAmount = amount;
            m_elementsSize = elementsSize;

            //处理元素
            ProcessElements();
  
            //集中处理一次UnUsedElement
            ProcessUnUsedElement();

            //处理没有元素的时候提示物件的显示
            if (m_ZeroTipsObj != null)
            {
                if (amount <= 0)
                {
                    m_ZeroTipsObj.SetActive(true);
                }
                else
                {
                    m_ZeroTipsObj.SetActive(false);
                }
            }
        }

        /// <summary>
        /// 设置选中元素
        /// </summary>
        /// <param name="index"></param>
        /// <param name="isDispatchSelectedChangeEvent"></param>
        public virtual void SelectElement(int index, bool isDispatchSelectedChangeEvent = true)
        {
            m_lastSelectedElementIndex = m_selectedElementIndex;
            m_selectedElementIndex = index;

            if (m_lastSelectedElementIndex == m_selectedElementIndex)
            {
                if (m_alwaysDispatchSelectedChangeEvent)
                {
                    //派发事件
                    DispatchElementSelectChangedEvent();
                }
                return;
            }

            if (m_lastSelectedElementIndex >= 0)
            {
                UIListViewItem elementScript = GetElemenet(m_lastSelectedElementIndex);

                if (elementScript != null)
                {
                    elementScript.ChangeSelectDisplay(false);
                }                
            }

            if (m_selectedElementIndex >= 0)
            {
                UIListViewItem elementScript = GetElemenet(m_selectedElementIndex);
                if (elementScript != null)
                {
                    elementScript.ChangeSelectDisplay(true);
                }  
            }

            //派发事件
            if (isDispatchSelectedChangeEvent)
            {
                DispatchElementSelectChangedEvent();
            }
        }
        
        /// <summary>
        /// 返回元素数量
        /// </summary>
        /// <returns></returns>
        public int GetElementAmount()
        {
            return m_elementAmount;
        }

        /// <summary>
        /// 返回List元素
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public UIListViewItem GetElemenet(int index)
        {
            if (index < 0 || index >= m_elementAmount)
            {
                return null;
            }

            //优化模式下，element不能再以index作为下标直接从m_elementScripts获取
            if (m_useOptimized)
            {
                for (int i = 0; i < m_elementScripts.Count; i++)
                {
                    if (m_elementScripts[i].m_index == index)
                    {
                        return m_elementScripts[i];
                    }
                }
                return null;
            }
            else
            {
                return m_elementScripts[index];
            }            
        }

        /// <summary>
        /// 返回当前选中的List元素
        /// </summary>
        /// <returns></returns>
        public UIListViewItem GetSeletedElement()
        {
            return GetElemenet(m_selectedElementIndex);
        }

        /// <summary>
        /// 返回上次选中的List元素
        /// </summary>
        /// <returns></returns>
        public UIListViewItem GetLastSelectedElement()
        {
            return GetElemenet(m_lastSelectedElementIndex);
        }

        /// <summary>
        /// 返回当前选中的元素Index
        /// </summary>
        /// <returns></returns>
        public int GetSelectedIndeex()
        {
            return m_selectedElementIndex;
        }

        /// <summary>
        /// 返回上次选中的元素Index
        /// </summary>
        /// <returns></returns>
        public int getLastselectedIndex()
        {
            return m_lastSelectedElementIndex;
        }

        /// <summary>
        /// 某项元素是否在滚动区域内（是否可见）
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool IsElementInScrollArea(int index)
        {
            if (index < 0 || index >= m_elementAmount)
            {
                return false;
            }
            stRect rect = (m_useOptimized) ? m_elementsRect[index] : m_elementScripts[index].m_rect;
            return IsRectInScrollArea(ref rect);
        }

        /// <summary>
        /// 返回ScrollValue
        /// </summary>
        /// <returns></returns>
        public Vector2 GetScrollValue()
        {
            return m_scrollValue;
        }

        /// <summary>
        /// 重置content位置
        /// </summary>
        public void ResetContentPosition()
        {
            DOTween.Kill(this.gameObject);
            if (m_contentRectTransform != null)
            {
                m_contentRectTransform.anchoredPosition = Vector2.zero;
            }
        }

        /// <summary>
        /// 将某项元素移动到滚动区域内（使其可见）
        /// </summary>
        /// <param name="index"></param>
        /// <param name="moveImmediately"></param>
        public void MoveElementInScrollArea(int index, bool moveImmediately)
        {
            if (index < 0 || index >= m_elementAmount)
            {
                return;
            }

            Vector2 fixedOffset = Vector2.zero;
            Vector2 position = Vector2.zero;

            stRect rect = (m_useOptimized) ? m_elementsRect[index] : m_elementScripts[index].m_rect;            
            position.x = m_contentRectTransform.anchoredPosition.x + rect.m_left;
            position.y = m_contentRectTransform.anchoredPosition.y + rect.m_top;

            if (position.x < 0)
            {
                fixedOffset.x = -position.x;
            }
            else if (position.x + rect.m_width > m_scrollAreaSize.x)
            {
                fixedOffset.x = m_scrollAreaSize.x - (position.x + rect.m_width);
            }

            if (position.y > 0)
            {
                fixedOffset.y = -position.y;
            }
            else if (position.y - rect.m_height < -m_scrollAreaSize.y)
            {
                fixedOffset.y = -m_scrollAreaSize.y - (position.y - rect.m_height);
            }
           
            if (moveImmediately)
            {
                m_contentRectTransform.anchoredPosition += fixedOffset;
            }
            else
            {
                Vector2 contentTargetAnchoredPosition = m_contentRectTransform.anchoredPosition + fixedOffset;
                //动画
                DOTween.To( () =>{return m_contentRectTransform.anchoredPosition;}, 
                    pos => { m_contentRectTransform.anchoredPosition = pos; }, 
                    contentTargetAnchoredPosition, 
                    m_fSpeed);
            }
        }

        public float m_fSpeed = 0.2f;


        /// <summary>
        /// index项element是否被选中
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public virtual bool IsSelectedIndex(int index)
        {
            return (m_selectedElementIndex == index);
        }

        public void ShowExtraContent()
        {
            if(m_extraContent != null)
            {
                m_extraContent.ExtSetActive(true);
            }
        }

        public void HideExtraContent()
        {
            if (m_extraContent != null)
            {
                m_extraContent.ExtSetActive(false);
            }
        }

        /// <summary>
        /// 派发选项改变事件
        /// </summary>
        protected void DispatchElementSelectChangedEvent()
        {
            if (SelectedChangedHandler != null)
            {
                SelectedChangedHandler(GetElemenet(m_selectedElementIndex),GetElemenet(m_lastSelectedElementIndex));
            }
        }

        /// <summary>
        /// 派发Scroll改变事件
        /// </summary>
        protected void DispatchScrollChangedEvent()
        {
            if (ScrollChangedHandler != null)
            {
                ScrollChangedHandler();
            }
        }

        /// <summary>
        /// 处理元素
        /// </summary>
        protected virtual void ProcessElements()
        {
            m_contentSize = Vector2.zero;
            Vector2 offset = Vector2.zero;

            if (m_listType == UIListViewType.Vertical || m_listType == UIListViewType.VerticalGrid)
            {
                offset.y += m_elementLayoutOffset;
            }
            else
            {
                offset.x += m_elementLayoutOffset;
            }

            for (int i = 0; i < m_elementAmount; i++)
            {
                //Element布局
                stRect rect = LayoutElement(i, ref m_contentSize, ref offset);
                //记录Element Rect
                if (m_useOptimized)
                {
                    if (i < m_elementsRect.Count)
                    {
                        m_elementsRect[i] = rect;
                    }
                    else
                    {
                        m_elementsRect.Add(rect);
                    }
                }

                if (!m_useOptimized || IsRectInScrollArea(ref rect))
                {
                    CreateElement(i, ref rect);
                }                
            }
            //设置extraContent if exist
            if (m_extraContent != null)
            {
                if (m_elementAmount > 0)
                {
                    ProcessExtraContent(ref m_contentSize, offset);
                }
                else
                {
                    m_extraContent.SetActive(false);
                }
            }
            //设置内容区域大小
            ResizeContent(ref m_contentSize, false);
        }

        /// <summary>
        /// 处理特殊内容
        /// </summary>
        /// <param name="offset"></param>
        private void ProcessExtraContent(ref Vector2 contentSize, Vector2 offset)
        {
            RectTransform extraContentRectTransform = m_extraContent.transform as RectTransform;
            extraContentRectTransform.anchoredPosition = offset;
            m_extraContent.ExtSetActive(true);
            //计算内容区域大小时需加上extraContent的大小和element spacing的大小
            if (m_listType == UIListViewType.Horizontal || m_listType == UIListViewType.HorizontalGrid)
            {
                contentSize.x += extraContentRectTransform.rect.width + m_elementSpacing.x;
            }
            else
            {
                contentSize.y += extraContentRectTransform.rect.height + m_elementSpacing.y;
            }
        }

        protected void UpdateElementsScroll()
        {
            //回收显示区域以外的Element
            for (int i = 0; i < m_elementScripts.Count; )
            {
                if (!IsRectInScrollArea(ref m_elementScripts[i].m_rect))
                {
                    RecycleElement(m_elementScripts[i], true);
                    continue;
                }

                i++;
            }

            //展示显示区域以内的Element
            for (int i = 0; i < m_elementAmount; i++)
            {
                stRect rect = m_elementsRect[i];

                if (IsRectInScrollArea(ref rect))
                {
                    bool exist = false;

                    for (int j = 0; j < m_elementScripts.Count; j++)
                    {
                        if (m_elementScripts[j].m_index == i)
                        {
                            exist = true;
                            break;
                        }
                    }

                    if (!exist)
                    {
                        CreateElement(i, ref rect);
                    }
                }
            }
        }

        /// <summary>
        /// 计算元素布局
        /// </summary>
        /// <param name="index"></param>
        /// <param name="contentSize"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        protected stRect LayoutElement(int index, ref Vector2 contentSize, ref Vector2 offset)
        {
            stRect rect = new stRect();
            rect.m_width = (int)((m_elementsSize == null || m_listType == UIListViewType.Vertical || m_listType == UIListViewType.VerticalGrid || m_listType == UIListViewType.HorizontalGrid) ? m_elementDefaultSize.x : m_elementsSize[index].x);
            rect.m_height = (int)((m_elementsSize == null || m_listType == UIListViewType.Horizontal || m_listType == UIListViewType.VerticalGrid || m_listType == UIListViewType.HorizontalGrid) ? m_elementDefaultSize.y : m_elementsSize[index].y);
            rect.m_left = (int)offset.x;
            rect.m_top = (int)offset.y;
            rect.m_right = rect.m_left + rect.m_width;
            rect.m_bottom = rect.m_top - rect.m_height;
            rect.m_center = new Vector2(rect.m_left + rect.m_width * 0.5f, rect.m_top - rect.m_height * 0.5f);

            if (rect.m_right > contentSize.x)
            {
                contentSize.x = rect.m_right;
            }

            if (-rect.m_bottom > contentSize.y)
            {
                contentSize.y = -rect.m_bottom;
            }

            switch (m_listType)
            {
                case UIListViewType.Vertical:
                {                        
                    offset.y -= (rect.m_height + m_elementSpacing.y);
                }
                break;

                case UIListViewType.Horizontal:
                {
                    offset.x += (rect.m_width + m_elementSpacing.x);
                }
                break;

                case UIListViewType.VerticalGrid:
                {
                    offset.x += (rect.m_width + m_elementSpacing.x);

                    if (offset.x + rect.m_width > m_scrollAreaSize.x)
                    {
                        offset.x = 0;
                        offset.y -= (rect.m_height + m_elementSpacing.y);
                    }                        
                }
                break;

                case UIListViewType.HorizontalGrid:
                {
                    offset.y -= (rect.m_height + m_elementSpacing.y);

                    if (-offset.y + rect.m_height > m_scrollAreaSize.y)
                    {
                        offset.y = 0;
                        offset.x += (rect.m_width + m_elementSpacing.x);
                    }
                }
                break;
            }

            return rect;
        }

        /// <summary>
        /// Resize内容区域
        /// </summary>
        /// <param name="size"></param>
        /// <param name="resetPosition"></param>
        protected virtual void ResizeContent(ref Vector2 size, bool resetPosition)
        {            
            float contentOffsetX = 0;
            float contentOffsetY = 0;

            if (m_autoAdjustScrollAreaSize)
            {
                Vector2 lastScrollAreaSize = m_scrollAreaSize;

                m_scrollAreaSize = size;

                if (m_scrollAreaSize.x > m_scrollRectAreaMaxSize.x)
                {
                    m_scrollAreaSize.x = m_scrollRectAreaMaxSize.x;
                }

                if (m_scrollAreaSize.y > m_scrollRectAreaMaxSize.y)
                {
                    m_scrollAreaSize.y = m_scrollRectAreaMaxSize.y;
                }

                Vector2 deltaSize = m_scrollAreaSize - lastScrollAreaSize;

                //调整List大小从而达到调整scrollRect大小的目的
                if (deltaSize != Vector2.zero)
                {
                    RectTransform rectTransform = gameObject.transform as RectTransform;
                    if (rectTransform.anchorMin == rectTransform.anchorMax)
                    {
                        rectTransform.sizeDelta = rectTransform.sizeDelta + deltaSize;
                    }
                }
            }
            else
            {
                //如果需要用到自动居中，这里需要计算出content的offset
                if (m_autoCenteredEnlements)
                {
                    if (m_listType == UIListViewType.Vertical && size.y < m_scrollAreaSize.y)
                    {
                        contentOffsetY = ((size.y - m_elementSpacing.y) - m_scrollAreaSize.y) / 2;
                    }
                    else if (m_listType == UIListViewType.Horizontal && size.x < m_scrollAreaSize.x)
                    {
                        contentOffsetX = (m_scrollAreaSize.x - (size.x - m_elementSpacing.x)) / 2;
                    }
                    else if (m_listType == UIListViewType.VerticalGrid && size.x < m_scrollAreaSize.x)
                    {
                        //Grid需要以整个Grid所占的区域来居中
                        while (true)
                        {
                            float width = size.x + m_elementDefaultSize.x + m_elementSpacing.x;

                            if (width > m_scrollAreaSize.x)
                            {
                                break;
                            }
                            else
                            {
                                size.x = width;
                            }
                        }

                        contentOffsetX = (m_scrollAreaSize.x - (size.x - m_elementSpacing.x)) / 2;
                    }
                }
            }

            //修正size
            if (size.x < m_scrollAreaSize.x)
            {
                size.x = m_scrollAreaSize.x;
            }

            if (size.y < m_scrollAreaSize.y)
            {
                size.y = m_scrollAreaSize.y;
            }

            //设置内容区域
            if (m_contentRectTransform != null)
            {
                //设置大小
                m_contentRectTransform.sizeDelta = size;

                //设定位置
                if (resetPosition)
                {
                    m_contentRectTransform.anchoredPosition = Vector2.zero;
                }

                //自动调整位置使content居中
                if (m_autoCenteredEnlements)
                {
                    if (contentOffsetX != 0)
                    {
                        m_contentRectTransform.anchoredPosition = new Vector2(contentOffsetX, m_contentRectTransform.anchoredPosition.y);
                    }

                    if (contentOffsetY != 0)
                    {
                        m_contentRectTransform.anchoredPosition = new Vector2(m_contentRectTransform.anchoredPosition.x, contentOffsetY);
                    }
                }
            }
        }

        /// <summary>
        /// rect是否位于滚动可视区域
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        protected bool IsRectInScrollArea(ref stRect rect)
        {
            Vector2 position = Vector2.zero;
            position.x = m_contentRectTransform.anchoredPosition.x + rect.m_left;
            position.y = m_contentRectTransform.anchoredPosition.y + rect.m_top;

            if (position.x + rect.m_width < 0
            || position.x > m_scrollAreaSize.x
            || position.y - rect.m_height > 0
            || position.y < -m_scrollAreaSize.y
            )
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// 回收Element
        /// </summary>
        /// <param name="disableElement"></param>
        protected void RecycleElement(bool disableElement)
        {
            //将当前所有elements设置为unUsed
            while(m_elementScripts.Count > 0)
            {
                UIListViewItem elementScript = m_elementScripts[0];
                m_elementScripts.RemoveAt(0);

                if(disableElement)
                {
                    elementScript.Disable();
                }
                m_unUsedElementScripts.Add(elementScript);
            }
        }

        /// <summary>
        /// 回收Element
        /// </summary>
        /// <param name="elementScript"></param>
        /// <param name="disableElement"></param>
        protected void RecycleElement(UIListViewItem elementScript, bool disableElement)
        {
            if (disableElement)
            {
                elementScript.Disable();
            }

            m_elementScripts.Remove(elementScript);            
            m_unUsedElementScripts.Add(elementScript);
        }

        /// <summary>
        /// 创建Element
        /// </summary>
        /// <param name="index"></param>
        /// <param name="rect"></param>
        /// <returns></returns>
        protected UIListViewItem CreateElement(int index, ref stRect rect)
        {
            UIListViewItem elementScript = null;
            //找
            if (m_unUsedElementScripts.Count > 0)
            {
                elementScript = m_unUsedElementScripts[0];
                m_unUsedElementScripts.RemoveAt(0);
            }
            else if (m_elementTemplate != null)
            {
                //克隆
                GameObject elementObject = Instantiate(m_elementTemplate);
                elementObject.transform.SetParent(m_content.transform);
                elementObject.transform.localScale = Vector3.one;
                //初始化
                InitializeComponent(elementObject);
                //
                elementScript = elementObject.GetComponent<UIListViewItem>();
            }

            if (elementScript != null)
            {
                elementScript.Enable(this, index, m_elementName, ref rect, IsSelectedIndex(index));
                m_elementScripts.Add(elementScript);
                if (OnEnableItemHandler != null)
                {
                    OnEnableItemHandler(elementScript);
                }
            }
            return elementScript;
        }

        protected void ProcessUnUsedElement()
        {
            if (m_unUsedElementScripts != null && m_unUsedElementScripts.Count > 0)
            {
                for (int i = 0; i < m_unUsedElementScripts.Count; i++)
                {
                    m_unUsedElementScripts[i].Disable();
                }
            }            
        }

        //设置SortingOrder
        public override void SetSortingOrder(int sortingOrder)
        {
            for(int i = 0; i < m_elementScripts.Count; i++)
            {
                m_elementScripts[i].AdjustSortingOrder(sortingOrder);
            }
        }
    }
}