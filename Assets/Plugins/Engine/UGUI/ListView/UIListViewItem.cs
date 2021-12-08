/********************************************************************
  created:  2020-03-30         
  author:    OneJun           
  purpose:  列表Item                
*********************************************************************/
using Engine.Base;
using Engine.PLink;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Engine.UGUI
{
    /// <summary>
    /// 自定义Rect
    /// </summary>
    public struct stRect
    {
        //宽高
        public int m_width;
        public int m_height;

        //坐标
        public int m_top;
        public int m_bottom;
        public int m_left;
        public int m_right;
        public Vector2 m_center;
    }

    [RequireComponent(typeof(Engine.PLink.PrefabLink))]
    public class UIListViewItem : UIComponent
    {
        [Header("点击选中主体")]
        public GameObject m_bodyObj;

        [Header("使用点击选中动画")]
        public bool ApplyClickSelTween;


        [Header("自动添加UI事件脚本")]
        public bool m_autoAddUIEventScript = true;

        [Header("选中显示对象")]
        public GameObject m_selectFrontObj;

        [Header("选中显示精灵")]
        public Sprite m_selectedSprite;

        /// <summary>
        /// 原始背景Sprite
        /// </summary>
        [HideInInspector]
        public Sprite m_defaultSprite;

        /// <summary>
        /// 原始背景Color
        /// </summary>
        [HideInInspector]
        public Color m_defaultColor;

        /// <summary>
        /// 原始文本Color
        /// </summary>
        [HideInInspector]
        public Color m_defaultTextColor;

        [Header("选中显示文本")]
        public Text m_textObj;

        [Header("选中显示文本颜色")]
        public Color m_selectTextColor = new Color(1, 1, 1, 1);

        /// <summary>
        /// 默认尺寸
        /// </summary>
        [HideInInspector]
        public Vector2 m_defaultSize;

        /// <summary>
        /// 索引
        /// </summary>
        [HideInInspector]
        public int m_index;

        /// <summary>
        /// 用于显示的Image
        /// </summary>
        private Image m_image;

        /// <summary>
        /// 在Content上面的区域
        /// </summary>
        public stRect m_rect;

        [Header("采用Active做隐藏,使用SetActive()还是CanvasGroup来显示或隐藏list element")]
        public bool m_useSetActiveForDisplay = false;

        private CanvasGroup m_canvasGroup;

        [Header("所属List")]
        public UIListView m_belongedListScript;

        [Header("元素连接PLink")]
        public PrefabLink PLink;

        public override void Initialize(UIForm formScript)
        {
            if (_isInited)
            {
                return;
            }
            base.Initialize(formScript);

            PLink = gameObject.GetComponent<PrefabLink>();

            m_image = gameObject.GetComponent<Image>();
            if (m_image != null)
            {
                m_defaultSprite = m_image.sprite;
                m_defaultColor = m_image.color;
            }

            //如果Element不包含 增加一个以便于接收选中事件
            if (m_autoAddUIEventScript)
            {
                if (m_bodyObj)
                {
                    UIEventListener eventScript = m_bodyObj.GetComponent<UIEventListener>();
                    if (eventScript == null)
                    {
                        eventScript = m_bodyObj.AddComponent<UIEventListener>();
                        eventScript.Initialize(formScript);
                    }
                    if (ApplyClickSelTween)
                    {
                        eventScript.ApplyClickTween = true;
                    }
                }
                else
                {
                    UIEventListener eventScript = gameObject.GetComponent<UIEventListener>();
                    if (eventScript == null)
                    {
                        eventScript = gameObject.AddComponent<UIEventListener>();
                        eventScript.Initialize(formScript);
                    }
                    if (ApplyClickSelTween)
                    {
                        eventScript.ApplyClickTween = true;
                    }
                }
            }

            //如果Element不包含CanvasGroup，增加一个以便于隐藏/显示
            if (!m_useSetActiveForDisplay)
            {
                m_canvasGroup = gameObject.GetComponent<CanvasGroup>();
                if (m_canvasGroup == null)
                {
                    m_canvasGroup = gameObject.AddComponent<CanvasGroup>();
                }
            }

            m_defaultSize = GetDefaultSize();

            //初始化RectTransform
            InitRectTransform();
            //
            if (m_textObj != null)
            {
                m_defaultTextColor = m_textObj.color;
            }
        }

        /// 获取默认尺寸
        protected Vector2 GetDefaultSize()
        {
            return (new Vector2(((RectTransform)this.gameObject.transform).rect.width, ((RectTransform)this.gameObject.transform).rect.height));
        }

        public void Enable(UIListView belongedList, int index, string name, ref stRect rect, bool selected)
        {
            m_belongedListScript = belongedList;
            m_index = index;

            gameObject.name = name + "_" + index.ToString();

            if (m_useSetActiveForDisplay)
            {
                gameObject.ExtSetActive(true);
            }
            else
            {
                m_canvasGroup.alpha = 1f;
                m_canvasGroup.blocksRaycasts = true;
            }

            //递归设置从属List 子组件控制
            SetEnable(gameObject);
            //设置位置属性
            SetRect(ref rect);
            //设置选中/非选中外观
            ChangeSelectDisplay(selected);
        }

        /// <summary>
        /// 禁用元素
        /// </summary>
        public void Disable()
        {
            if (m_useSetActiveForDisplay)
            {
                gameObject.ExtSetActive(false);
            }
            else
            {
                m_canvasGroup.alpha = 0f;
                m_canvasGroup.blocksRaycasts = false;
            }
            //
            SetDisable(gameObject);
        }

        /// <summary>
        /// ListView关闭
        /// </summary>
        public void Close()
        {
            SetClose(gameObject);
        }

        /// <summary>
        /// 被选中时回调,此函数会被注册给element及其所有子元素
        /// </summary>
        /// <param name="baseEventData"></param>
        public void OnSelected(BaseEventData baseEventData)
        {
            m_belongedListScript.SelectElement(m_index);
        }

        /// <summary>
        /// 改变显示(选中/非选中)
        /// </summary>
        /// <param name="selected"></param>
        public void ChangeSelectDisplay(bool selected)
        {
            //处理背景选择图案
            if (m_image != null && m_selectedSprite != null)
            {
                if (selected)
                {
                    m_image.sprite = m_selectedSprite;
                    m_image.color = new Color(m_defaultColor.r, m_defaultColor.g, m_defaultColor.b, 255.0f);
                }
                else
                {
                    m_image.sprite = m_defaultSprite;
                    m_image.color = m_defaultColor;
                }
            }

            if (m_selectFrontObj != null)
            {
                m_selectFrontObj.ExtSetActive(selected);
            }

            if (m_textObj != null)
            {
                m_textObj.color = selected ? m_selectTextColor : m_defaultTextColor;
            }
        }

        private void SetClose(GameObject gameObject)
        {
            UIComponent[] componentScripts = gameObject.GetComponents<UIComponent>();
            if (componentScripts != null && componentScripts.Length > 0)
            {
                for (int i = 0; i < componentScripts.Length; i++)
                {
                    componentScripts[i].OnClose();
                }
            }
            //
            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                SetClose(gameObject.transform.GetChild(i).gameObject);
            }
        }

        private void SetEnable(GameObject gameObject)
        {
            UIComponent[] componentScripts = gameObject.GetComponents<UIComponent>();
            if (componentScripts != null && componentScripts.Length > 0)
            {
                for (int i = 0; i < componentScripts.Length; i++)
                {
                    //为UIComponent设置所属List
                    componentScripts[i].SetBelongedList(m_belongedListScript, m_index);
                    //Appear
                    componentScripts[i].OnAppear();
                }
            }

            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                SetEnable(gameObject.transform.GetChild(i).gameObject);
            }
        }

        ///子组件
        private void SetDisable(GameObject gameObject)
        {
            UIComponent[] componentScripts = gameObject.GetComponents<UIComponent>();
            if (componentScripts != null && componentScripts.Length > 0)
            {
                for (int i = 0; i < componentScripts.Length; i++)
                {
                    //Hide
                    componentScripts[i].OnHide();
                }
            }

            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                SetDisable(gameObject.transform.GetChild(i).gameObject);
            }
        }


        //设置SortingOrder
        public override void SetSortingOrder(int sortingOrder)
        {
            //无视
        }

        public void AdjustSortingOrder(int sortingOrder)
        {
            DoAdjustSortingOrder(this.gameObject, sortingOrder);
        }

        //子控件
        private void DoAdjustSortingOrder(GameObject gameObject, int sortingOrder)
        {
            UIComponent[] componentScripts = gameObject.GetComponents<UIComponent>();
            if (componentScripts != null && componentScripts.Length > 0)
            {
                for (int i = 0; i < componentScripts.Length; i++)
                {
                    componentScripts[i].SetSortingOrder(sortingOrder);
                }
            }

            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                DoAdjustSortingOrder(gameObject.transform.GetChild(i).gameObject, sortingOrder);
            }
        }

        /// <summary>
        /// 设置在Content上的位置
        /// </summary>
        /// <param name="rect"></param>
        private void SetRect(ref stRect rect)
        {
            m_rect = rect;
            RectTransform rectTransform = gameObject.transform as RectTransform;
            rectTransform.sizeDelta = new Vector2(m_rect.m_width, m_rect.m_height);
            rectTransform.anchoredPosition = new Vector2(rect.m_left, rect.m_top);
            rectTransform.anchoredPosition3D = new Vector3(rect.m_left, rect.m_top, 0);
        }

        /// <summary>
        /// 初始化Rect
        /// </summary>
        private void InitRectTransform()
        {
            //设置锚点和枢轴点均为Top-Left
            RectTransform rectTransform = gameObject.transform as RectTransform;
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.pivot = new Vector2(0, 1);
            rectTransform.sizeDelta = m_defaultSize;

            rectTransform.localRotation = Quaternion.identity;
            rectTransform.localScale = new Vector3(1, 1, 1);
        }
    };
};