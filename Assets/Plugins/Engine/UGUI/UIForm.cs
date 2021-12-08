/********************************************************************
  created:  2020-03-17         
  author:    OneJun           
  purpose:   窗口挂接               
*********************************************************************/
using System;
using Engine.Base;
using UnityEngine;
using UnityEngine.UI;

namespace Engine.UGUI
{
    /// <summary>
    /// Form显示优先级,层级越高显示在越上层
    /// </summary>
    public enum UIFormPriority
    {
        Priority0,
        Priority1,
        Priority2,
        Priority3,
        Priority4,
        Priority5,
        Priority6,
        Priority7,
        Priority8,
        Priority9
    }

    /// <summary>
    /// Form事件类型
    /// </summary>
    public enum UIFormEventType
    {
        //打开
        Open,
        //关闭
        Close,
        //从窗口系统移除
        Remove,
        //可见性改变
        VisibleChanged,
        //优先级改变
        PriorityChanged,
        //恢复可见
        RevertToVisible,
        //隐藏
        RevertToHide,
    }

    //定义委托
    public delegate void UIFormEventDelegate(UIFormEventType eventType);

    /// <summary>
    ///Form淡入/淡出动画类型
    /// </summary>
    public enum UIFormFadeAnimationType
    {
        None,
        Animation,
        Animator
    };

    /// <summary>
    /// Form隐藏标志    
    /// </summary>
    public enum UIFormHideFlag
    {
        HideByCustom = 1 << 0,
        HideByOtherForm = 1 << 1,
    }
    /// <summary>
    /// 窗口定义
    /// </summary>
    [RequireComponent(typeof(CanvasScaler))]
    public class UIForm : UIViewLink, IComparable
    {
        /// <summary>
        /// 优先级Mask（0-9）
        /// </summary>
        private const int Const_OpenOrderMask = 10;
        private const int Const_PriorityOrderMask = 1000;
        private const int Const_OverlayOrderMask = 10000;

        /// <summary>
        /// 窗口基准分辨率
        /// </summary>
        [HideInInspector]
        public Vector2 ReferenceResolution = new Vector2(720f, 1280f);

        /// <summary>
        /// 是否时单例 仅存在一个
        /// </summary>
        public bool IsSingleton = true;
        /// <summary>
        /// 是否模态组织输入传递
        /// </summary>
        public bool IsModal;

        /// <summary>
        /// 显示优先级
        /// </summary>
        public UIFormPriority ShowPriority = UIFormPriority.Priority0;
        /// <summary>
        /// 原始优先级
        /// </summary>
        private UIFormPriority _defaultShowPriority = UIFormPriority.Priority0;

        /// <summary>
        /// Form分组 同组只能有一个Form存在，为0表示不启用
        /// </summary>
        public int GroupId = 0;
        /// <summary>
        /// 是否全屏背景, 适配时需要进行裁切
        /// </summary>
        public bool IsFullScreenBG = false;
        /// <summary>
        /// 是否禁止输入
        /// </summary>
        public bool IsDisableInput = false;
        /// <summary>
        /// 是否隐藏在本Form之下的其他Form
        /// </summary>
        public bool IsHideUnderForms = false;
        /// <summary>
        /// 是否在其他Form打开时始终保持可见
        /// </summary>
        public bool IsAlwayKeepVisible = false;
        /// <summary>
        /// 是否启用自动适配屏幕
        /// </summary>
        public bool EnableAutoAdaptScreen = true;
        /// <summary>
        /// 淡入动画
        /// </summary>
        public UIFormFadeAnimationType FadeInAnimationType = UIFormFadeAnimationType.None;
        public string FadeInAnimationName = string.Empty;
        private UIComponent _fadeInAnimationCtr = null;
        /// <summary>
        /// 淡出动画
        /// </summary>
        public UIFormFadeAnimationType FadeOutAnimationType = UIFormFadeAnimationType.None;
        public string FadeOutAnimationName = string.Empty;
        private UIComponent _fadeOutAnimationCtr = null;

        /// <summary>
        /// 预制体地址
        /// </summary>
        [NonSerialized]
        [HideInInspector]
        public string FormPath = string.Empty;

        /// <summary>
        /// 事件Handler
        /// </summary>
        [HideInInspector]
        public UIFormEventDelegate EventHandler;

        [HideInInspector]
        public UIFormClass Controller = null;

        //--------------------私有----------------------
        //是否需要关闭
        private bool _isNeedClose;
        //是否已经关闭
        private bool _isClosed;
        //是否正在FadeIn
        private bool _isInFadeIn;
        //是否正在FadeOut         
        private bool _isInFadeOut;

        //UGUI 
        private Canvas _canvas;
        private CanvasScaler _canvasScaler;
        private GraphicRaycaster _graphicRaycaster;
        //打开顺序
        private int _openOrder;
        //排序顺序
        private int _sortingOrder;
        //序列号(唯一标识)
        private int _sequence;

        //是否处于隐藏状态及是否因为其他Form打开而被隐藏
        private bool _isHided = false;
        private int _hideFlags = 0;
        //渲染桢时间戳
        private int _renderFrameStamp;
        //是否初始化完成
        private bool _isInitialized = false;

        //UIComponent列表
        private ObjList<UIComponent> _uiComponents;


        void Awake()
        {
            _canvas = gameObject.GetComponent<Canvas>();
            _canvasScaler = gameObject.GetComponent<CanvasScaler>();
            _graphicRaycaster = GetComponent<GraphicRaycaster>();
            if (_graphicRaycaster != null)
            {
                if (IsDisableInput)
                {
                    _graphicRaycaster.enabled = false;
                }
            }
            if (_canvas == null)
            {
                GGLog.LogE("UIForm Must Attach On A Canvas!");
                return;
            }
            //适配
            MatchScreen();
        }

        /// <summary>
        /// 适配屏幕
        /// </summary>
        public void MatchScreen()
        {
            if (!EnableAutoAdaptScreen)
            {
                return;
            }
            if (_canvasScaler == null)
            {
                return;
            }
            if (_canvasScaler.uiScaleMode == CanvasScaler.ScaleMode.ConstantPixelSize)
            {
                return;
            }

            _canvasScaler.referenceResolution = ReferenceResolution;
            if (Screen.width / _canvasScaler.referenceResolution.x > Screen.height / _canvasScaler.referenceResolution.y)
            {
                if (IsFullScreenBG)
                {
                    _canvasScaler.matchWidthOrHeight = 0f;
                }
                else
                {
                    _canvasScaler.matchWidthOrHeight = 1.0f;
                }
            }
            else
            {
                if (IsFullScreenBG)
                {
                    _canvasScaler.matchWidthOrHeight = 1.0f;
                }
                else
                {
                    _canvasScaler.matchWidthOrHeight = 0f;
                }
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized)
            {
                return;
            }
            //记录默认显示优先级
            _defaultShowPriority = ShowPriority;
            _isInitialized = true;
            //初始化UI组件
            UIComponent[] ccs = new UIComponent[100];
            int ccCnt = 0;
            UIUtility.GetComponentsInChildren<UIComponent>(gameObject, ccs, ref ccCnt);
            if (ccs != null && ccCnt > 0)
            {
                if (_uiComponents == null)
                {
                    _uiComponents = new ObjList<UIComponent>();
                }
                for (int i = 0; i < ccCnt; i++)
                {
                    ccs[i].Initialize(this);

                    //Cache
                    _uiComponents.Add(ccs[i]);
                }
            }
        }
#if UNITY_EDITOR
        void Start()
        {
            Initialize();
        }
        void Update()
        {
            MatchScreen();
        }
#endif
        void OnDestroy()
        {
            EventHandler = null;
            _isInitialized = false;
            if (_uiComponents != null)
            {
                _uiComponents.Clear();
                _uiComponents = null;
            }
            _fadeInAnimationCtr = null;
            _fadeOutAnimationCtr = null;
            Controller = null;
        }

        /// <summary>
        /// 自定义Update
        /// </summary>
        public void CustomUpdate()
        {
            UpdateFadeIn();
            UpdateFadeOut();
        }

        /// <summary>
        /// 自定义LateUpdate
        /// </summary>
        public void CustomLateUpdate()
        {
            /*调整子部件位置*/

            _renderFrameStamp++;
        }

        #region 基础访问
        /// <summary>
        /// 返回序列号
        /// </summary>
        /// <returns></returns>
        public int GetSequence()
        {
            return _sequence;
        }

        /// <summary>
        /// 返回GraphicRaycaster
        /// </summary>
        /// <returns></returns>
        public GraphicRaycaster GetGraphicRaycaster()
        {
            return _graphicRaycaster;
        }

        /// <summary>
        /// 返回Camera
        /// </summary>
        /// <returns></returns>
        public Camera GetCamera()
        {
            if (_canvas == null || _canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                return null;
            }
            return _canvas.worldCamera;
        }

        /// <summary>
        /// 返回基准尺寸
        /// </summary>
        /// <returns></returns>
        public Vector2 GetReferenceResolution()
        {
            return (_canvasScaler == null) ? Vector2.zero : _canvasScaler.referenceResolution;
        }

        /// <summary>
        /// 返回SortingOrder
        /// </summary>
        /// <returns></returns>
        public int GetSortingOrder()
        {
            return _sortingOrder;
        }

        /// <summary>
        /// 是否需要淡入 todo根据游戏设置
        /// </summary>
        /// <returns></returns>
        public bool IsNeedFadeIn()
        {
            return true;
        }

        /// <summary>
        /// 是否需要淡出 todo根据游戏设置
        /// </summary>
        /// <returns></returns>
        public bool IsNeedFadeOut()
        {
            return true;
        }

        /// <summary>
        /// /是否需要关闭
        /// </summary>
        /// <returns></returns>
        public bool IsNeedClose()
        {
            return _isNeedClose;
        }

        /// <summary>
        /// 是否处于关闭状态
        /// </summary>
        /// <returns></returns>
        public bool IsClosed()
        {
            return _isClosed;
        }

        /// <summary>
        /// Canvas是否启用
        /// </summary>
        /// <returns></returns>
        public bool IsCanvasEnabled()
        {
            if (_canvas == null)
            {
                return false;
            }
            return _canvas.enabled;
        }

        //是否隐藏
        public bool IsHided()
        {
            return _isHided;
        }

        //刷新CanvasScaler
        private void RefreshCanvasScaler()
        {
            if (_canvasScaler != null)
            {
                _canvasScaler.enabled = false;
                _canvasScaler.enabled = true;
            }
        }

        //是否是Overlay RenderMode
        private bool IsOverlay()
        {
            if (_canvas == null)
            {
                return false;
            }
            return (_canvas.renderMode == RenderMode.ScreenSpaceOverlay || _canvas.worldCamera == null);
        }

        /// 屏幕取值转换为Form取值
        public float ChangeScreenValueToForm(float value)
        {
            if (_canvasScaler.matchWidthOrHeight == 0f)
            {
                return (value * _canvasScaler.referenceResolution.x / Screen.width);
            }
            else if (_canvasScaler.matchWidthOrHeight == 1f)
            {
                return (value * _canvasScaler.referenceResolution.y / Screen.height);
            }
            return value;
        }

        /// Form取值转换为Screen取值
        public float ChangeFormValueToScreen(float value)
        {
            if (_canvasScaler.matchWidthOrHeight == 0f)
            {
                return (value * Screen.width / _canvasScaler.referenceResolution.x);
            }
            else if (_canvasScaler.matchWidthOrHeight == 1f)
            {
                return (value * Screen.height / _canvasScaler.referenceResolution.y);
            }
            return value;
        }

        /// 返回计算得到的sortingOrder
        private int CalculateSortingOrder(UIFormPriority priority, int openOrder)
        {
            if (openOrder * Const_OpenOrderMask >= Const_PriorityOrderMask)
            {
                openOrder %= (Const_PriorityOrderMask / Const_OpenOrderMask);
            }

            return ((IsOverlay() ? Const_OverlayOrderMask : 0) + ((int)priority * Const_PriorityOrderMask) + (openOrder * Const_OpenOrderMask));
        }

        /// 排序函数
        /// 按_sortingOrder升序排列
        public int CompareTo(object obj)
        {
            UIForm formScript = obj as UIForm;

            if (_sortingOrder > formScript.GetSortingOrder())
            {
                return 1;
            }
            else if (_sortingOrder == formScript.GetSortingOrder())
            {
                return 0;
            }
            else
            {
                return -1;
            }
        }

        //设置显示顺序
        //openOrder : 打开顺序
        public void SetDisplayOrder(int openOrder)
        {
            _openOrder = openOrder;
            if (_canvas != null)
            {
                _sortingOrder = CalculateSortingOrder(ShowPriority, _openOrder);
                _canvas.sortingOrder = _sortingOrder;
                //BUG
                if (_canvas.enabled)
                {
                    _canvas.enabled = false;
                    _canvas.enabled = true;
                }
            }
            //通知组件
            if (_uiComponents != null)
            {
                for (int i = 0; i < _uiComponents.Count; i++)
                {
                    _uiComponents[i].SetSortingOrder(_sortingOrder);
                }
            }
        }

        //派发Form事件
        private void DispatchFormEvent(UIFormEventType formEventType)
        {
            if (EventHandler != null)
            {
                EventHandler(formEventType);
            }
        }
        #endregion

        #region 打开关闭相关
        ///UGUIRoot 开启调用
        public void Open(Camera camera, int sequence, int openOrder, bool exist)
        {
            if (_canvas != null)
            {
                _canvas.worldCamera = camera;
                _canvas.renderMode = (camera == null) ? RenderMode.ScreenSpaceOverlay : RenderMode.ScreenSpaceCamera;
                //_canvas.pixelPerfect = true;
            }
            RefreshCanvasScaler();
            Open(sequence, openOrder, exist);
        }

        public void Open(int sequence, int openOrder, bool exist)
        {
            _isNeedClose = false;
            _isClosed = false;
            _isInFadeIn = false;
            _isInFadeOut = false;
            _sequence = sequence;
            _renderFrameStamp = 0;
            //
            if (!exist)
            {
                //初始化
                Initialize();
                //派发事件
                DispatchFormEvent(UIFormEventType.Open);
                //Todo 播放音效
                //淡入
                if (IsNeedFadeIn())
                {
                    StartFadeIn();
                }

                //通知组件
                if (_uiComponents != null)
                {
                    for (int i = 0; i < _uiComponents.Count; i++)
                    {
                        _uiComponents[i].OnAppear();
                    }
                }
            }
            //
            SetDisplayOrder(openOrder);
        }

        /// 转为关闭状态
        /// @ignoreFadeOut : 是否忽略FadeOut
        /// @返回true表示不需要FadeOut
        public bool TurnToClosed(bool ignoreFadeOut)
        {
            _isNeedClose = false;
            _isClosed = true;
            if (ignoreFadeOut || !IsNeedFadeOut())
            {
                return true;
            }
            else
            {
                StartFadeOut();
                return false;
            }

        }

        /// 设置关闭标志
        public void Close()
        {
            //防止多次调用close
            if (_isNeedClose)
            {
                return;
            }
            _isNeedClose = true;
            //派发事件
            DispatchFormEvent(UIFormEventType.Close);
            /*播放音效*/
            //通知组件
            if (_uiComponents != null)
            {
                for (int i = 0; i < _uiComponents.Count; i++)
                {
                    _uiComponents[i].OnClose();
                }
            }
        }

        /// <summary>
        /// 从UGUIRoot移除
        /// </summary>
        public void OnRemove()
        {
            //还原
            _hideFlags = 0;
            if (_canvas != null)
            {
                _canvas.enabled = true;
            }
            if (_graphicRaycaster != null)
            {
                _graphicRaycaster.enabled = true;
            }
            //
            if (Controller != null)
            {
                Controller.OnFormRemoved();
            }
        }

        #endregion

        #region 优先级相关

        //恢复原始优先级
        public void RestorePriority()
        {
            SetPriority(_defaultShowPriority);
        }

        //置优先级
        public void SetPriority(UIFormPriority priority)
        {
            if (ShowPriority == priority)
            {
                return;
            }
            ShowPriority = priority;
            //这里需要即时修改sortingOrder
            SetDisplayOrder(_openOrder);
            //派发事件
            DispatchFormEvent(UIFormEventType.PriorityChanged);
        }
        #endregion

        #region 可见相关
        /// 设置Active
        public void SetActive(bool active)
        {
            this.gameObject.ExtSetActive(active);
            if (active)
            {
                Appear();
            }
            else
            {
                Hide();
            }
        }

        /// 隐藏
        public void Hide(UIFormHideFlag hideFlag = UIFormHideFlag.HideByCustom, bool dispatchEvent = true)
        {
            //始终保持可见的Form，不能被隐藏
            if (IsAlwayKeepVisible)
            {
                return;
            }
            //Add flag
            _hideFlags |= (int)hideFlag;
            //隐藏标志为0或者已经被隐藏，不能再次进行隐藏操作
            if (_hideFlags == 0 || _isHided)
            {
                return;
            }
            _isHided = true;

            if (_canvas != null)
            {
                _canvas.enabled = false;
            }
            if (_graphicRaycaster != null)
            {
                _graphicRaycaster.enabled = false;
            }
            if (dispatchEvent)
            {
                DispatchFormEvent(UIFormEventType.VisibleChanged);
            }
            //通知组件
            if (_uiComponents != null)
            {
                for (int i = 0; i < _uiComponents.Count; i++)
                {
                    _uiComponents[i].OnHide();
                }
            }
        }

        /// 显示
        public void Appear(UIFormHideFlag hideFlag = UIFormHideFlag.HideByCustom, bool dispatchdEvent = true)
        {
            //始终保持可见的Form，直接return
            if (IsAlwayKeepVisible)
            {
                return;
            }
            //Remove Flag
            _hideFlags &= ~((int)hideFlag);
            //隐藏标志不为0或没有被隐藏，不能进行恢复操作
            if (_hideFlags != 0 || !_isHided)
            {
                return;
            }
            _isHided = false;
            if (_canvas != null)
            {
                _canvas.enabled = true;
                _canvas.sortingOrder = _sortingOrder;
            }
            if (_graphicRaycaster && !IsDisableInput)
            {
                _graphicRaycaster.enabled = true;
            }
            //派发RevertVisible事件
            DispatchFormEvent(UIFormEventType.RevertToVisible);
            //派发VisibleChanged事件
            if (dispatchdEvent)
            {
                DispatchFormEvent(UIFormEventType.VisibleChanged);
            }

            //通知组件
            if (_uiComponents != null)
            {
                for (int i = 0; i < _uiComponents.Count; i++)
                {
                    _uiComponents[i].OnAppear();
                }
            }
        }
        #endregion

        #region 淡入淡出
        /// 开始淡入
        private void StartFadeIn()
        {
            if (FadeInAnimationType == UIFormFadeAnimationType.None || string.IsNullOrEmpty(FadeInAnimationName))
            {
                return;
            }

            switch (FadeInAnimationType)
            {
                case UIFormFadeAnimationType.Animation:
                    {
                        _fadeInAnimationCtr = gameObject.GetComponent<UIAnimation>();
                        if (_fadeInAnimationCtr != null)
                        {
                            ((UIAnimation)_fadeInAnimationCtr).PlayAnimation(FadeInAnimationName, true);
                            _isInFadeIn = true;
                        }
                    }
                    break;

                case UIFormFadeAnimationType.Animator:
                    {
                        _fadeInAnimationCtr = gameObject.GetComponent<UIAnimator>();
                        if (_fadeInAnimationCtr != null)
                        {
                            ((UIAnimator)_fadeInAnimationCtr).PlayAnimator(FadeInAnimationName);
                            _isInFadeIn = true;
                        }
                    }
                    break;
            }
        }

        /// 开始淡出
        private void StartFadeOut()
        {
            if (FadeOutAnimationType == UIFormFadeAnimationType.None || string.IsNullOrEmpty(FadeOutAnimationName))
            {
                return;
            }

            switch (FadeOutAnimationType)
            {
                case UIFormFadeAnimationType.Animation:
                    {
                        _fadeOutAnimationCtr = gameObject.GetComponent<UIAnimation>();
                        if (_fadeOutAnimationCtr != null)
                        {
                            ((UIAnimation)_fadeOutAnimationCtr).PlayAnimation(FadeOutAnimationName, true);
                            _isInFadeOut = true;
                        }
                    }
                    break;

                case UIFormFadeAnimationType.Animator:
                    {
                        _fadeOutAnimationCtr = gameObject.GetComponent<UIAnimator>();
                        if (_fadeOutAnimationCtr != null)
                        {
                            ((UIAnimator)_fadeOutAnimationCtr).PlayAnimator(FadeOutAnimationName);
                            _isInFadeOut = true;
                        }
                    }
                    break;
            }
        }

        /// Update淡入
        private void UpdateFadeIn()
        {
            if (_isInFadeIn)
            {
                switch (FadeInAnimationType)
                {
                    case UIFormFadeAnimationType.Animation:
                        {
                            if (_fadeInAnimationCtr == null || ((UIAnimation)_fadeInAnimationCtr).IsAnimationStopped(FadeInAnimationName))
                            {
                                _isInFadeIn = false;
                            }
                        }
                        break;

                    case UIFormFadeAnimationType.Animator:
                        {
                            if (_fadeInAnimationCtr == null || ((UIAnimator)_fadeInAnimationCtr).IsAnimationStopped(FadeInAnimationName))
                            {
                                _isInFadeIn = false;
                            }
                        }
                        break;
                }
            }
        }

        /// Update淡出
        private void UpdateFadeOut()
        {
            if (_isInFadeOut)
            {
                switch (FadeOutAnimationType)
                {
                    case UIFormFadeAnimationType.Animation:
                        {
                            if (_fadeOutAnimationCtr == null || ((UIAnimation)_fadeOutAnimationCtr).IsAnimationStopped(FadeOutAnimationName))
                            {
                                _isInFadeOut = false;
                            }
                        }
                        break;

                    case UIFormFadeAnimationType.Animator:
                        {
                            if (_fadeOutAnimationCtr == null || ((UIAnimator)_fadeOutAnimationCtr).IsAnimationStopped(FadeOutAnimationName))
                            {
                                _isInFadeOut = false;
                            }
                        }
                        break;
                }
            }
        }

        /// 是否处于淡入状态
        public bool IsInFadeIn()
        {
            return _isInFadeIn;
        }

        /// 是否处于淡出状态
        public bool IsInFadeOut()
        {
            return _isInFadeOut;
        }
        #endregion
    }
}