/********************************************************************
  created:  2020-06-05         
  author:    OneJun           

  purpose:  通用弹窗动画组件                
*********************************************************************/
using DG.Tweening;
using UnityEngine;

namespace Engine.UGUI
{
    public class UIPopUpAni : UIComponent
    {
        //动画类型
        public enum AniType
        {
            //居中缩放
            ScaleFromCenter,
            //落下
            VerticalToCenter,
            //水平进入
            HorizontalToCenter
        }

        [Header("动画方式")]
        public AniType PopAniType=AniType.ScaleFromCenter;
        [Header("显示缓动")]
        public Ease ShowEaseType = Ease.OutBack;
        // [Header("隐藏曲线")]
        // public Ease HideEaseType = Ease.Linear;

        [Header("时间间隔")]
        public float Duration = 0.5f;

        [Header("初始缩放")]
        public Vector2 InitScale = new Vector3(0.1f, 0.1f, 0.1f);

        [Header("水平进入偏移")]
        public int OffSetX = 0;

        [Header("水平进入偏移")]
        public int OffSetY = 0;

        [Header("延时播放")]
        public float Delay = 0f;

        private RectTransform _cachedTf;
        private Vector2 _cachedCenter;
        //初始化
        public override void Initialize(UIForm form)
        {
            if (_isInited)
            {
                return;
            }
            BelongedForm = form;

            _isInited = true;
        }

        void Awake()
        {
            _cachedTf = transform as RectTransform;
            _cachedCenter = _cachedTf.anchoredPosition;

        }

        void OnDestroy()
        {
            BelongedForm = null;
        }

        //窗口关闭
        public override void OnClose()
        {
            _cachedTf.localScale = Vector3.one;
            _cachedTf.anchoredPosition = _cachedCenter;
        }

        //窗口隐藏
        public override void OnHide()
        {
             _cachedTf.DOKill();
        }


        void OnEnable()
        {
            #if UNITY_EDITOR
            OnAppear();
            #endif
        }

        //窗口显示
        public override void OnAppear()
        {
            _cachedTf.DOKill();
            Tweener tweener = null;
            switch (PopAniType)
            {
                case AniType.ScaleFromCenter:
                    _cachedTf.localScale = InitScale;
                    // content.localPosition = Vector3.one;
                    tweener = _cachedTf.DOScale(1f, Duration).SetEase(ShowEaseType);
                    break;
                case AniType.VerticalToCenter:
                    _cachedTf.anchoredPosition = new Vector2(_cachedCenter.x, OffSetY);
                    tweener = _cachedTf.DOAnchorPos(_cachedCenter, Duration).SetEase(ShowEaseType);
                    break;

                case AniType.HorizontalToCenter:
                    _cachedTf.anchoredPosition = new Vector2(OffSetX, _cachedCenter.y);
                    tweener = _cachedTf.DOAnchorPos(_cachedCenter, Duration).SetEase(ShowEaseType);
                    break;
            }
            if (tweener != null)
            {
                tweener.SetDelay(Delay).SetUpdate(true);
            }
        }
    }
}