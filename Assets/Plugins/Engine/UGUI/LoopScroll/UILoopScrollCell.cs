/********************************************************************
*		作者： XH
*		时间： 2019-04-28
*		描述： 循环滚动Cell组件，与UILoopScroll配合使用
*********************************************************************/
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using Engine.Asset;
using Engine.PLink;

namespace Engine.UGUI { 
    [RequireComponent(typeof(PrefabLink))]
    public class UILoopScrollCell : UIComponent,IPointerClickHandler, IBeginDragHandler, IDragHandler, 
                                    IEndDragHandler,IPointerDownHandler,IPointerUpHandler, IPointerExitHandler
    {
        [SerializeField]
        private bool _clickAnimation = false;//是否有点击缩放动画
        public PrefabLink CellPLink;//该cell的PrefabLink
        [SerializeField]
        private float _minScale = 0.85f;
        [SerializeField]
        private bool _ignorePointerExit = false;//忽略PointerExit事件
        [SerializeField]
        private bool _needGray = false;
        [SerializeField]
        private Graphic[] _graphics = null;

        public Spine.Unity.SkeletonGraphic Skeleton = null;

        private int _index;//当前Cell的索引

        private UILoopScroll _scroll;//所属UILoopScroll
        private Vector3 _originScale;//正常的Scale
        
        private bool _selected = false;//是否选中
        
        private float _pointerDownTime = 0;
        private bool _valideClickCheck = false;
        public int Index {
            get {
                    return _index;
                }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="index">索引</param>
        /// <param name="scroll">UILoopScroll</param>
        public void Init(int index, UILoopScroll scroll)
        {
            _index = index;
            _scroll = scroll;
            _scroll.SelectHandler += this.OnSelect;
            if(CellPLink==null)
            {
                CellPLink = GetComponent<PrefabLink>();
            }
            OnSelect(-1);
        }

        //选中通知
        public void OnSelect(int index)
        {
            _selected = index == _index;
            if(_needGray && _graphics != null)
            {
                foreach(var graph in _graphics)
                {
                    graph.color = _selected ? Color.white : Color.gray;
                }
                if(Skeleton!=null)
                {
                    Skeleton.color = _selected ? Color.white : Color.gray;
                    Skeleton.freeze = !_selected;
                }
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if(_clickAnimation)
            {
                _originScale = this.transform.localScale;
                this.transform.localScale = _originScale * _minScale;
            }
            _pointerDownTime = Time.time;
            _valideClickCheck = true;
        }
        public void OnPointerExit(PointerEventData eventData)
        {
            if (_scroll != null && !_ignorePointerExit)
            {
                _scroll.OnPointerExit(eventData);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if(_clickAnimation)
            {
                this.transform.localScale = _originScale;
            }
            if(_valideClickCheck && Time.time - _pointerDownTime <= 0.25f)
            {
                if (_scroll)
                {
                    _scroll.OnSelect(_index);
                }
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if(_scroll != null)
            {
                _scroll.OnBeginDrag(eventData);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_scroll != null)
            {
                _scroll.OnDrag(eventData);
            }
            _valideClickCheck = false;

        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_scroll != null)
            {
                _scroll.OnEndDrag(eventData);
            }

        }
        //点击回调
        public void OnPointerClick(PointerEventData eventData)
        {
 
        }
        //反初始化
        public void UnInit()
        {
            Skeleton = null;
            SetPosition(0);
            if(_scroll!=null)
            {
                _scroll.SelectHandler -= this.OnSelect;
            }
        }

       
        public float GetPosX()
        {
            return transform.localPosition.x;
        }
        //设置是否激活
        public void SetVisible(bool visible)
        {
            if(gameObject.activeSelf == visible)
            {
                return;
            }
            gameObject.SetActive(visible);
        }
        //增量更新坐标
        public void UpdatePosition(float deltaX)
        {
            transform.localPosition += new Vector3(deltaX, 0, 0);
        }
        //设置坐标
        public void SetPosition(float x)
        {
            transform.localPosition = new Vector3(x, 0, 0);
        }

        //设置缩放
        public void SetScale(float scale)
        {
            transform.localScale = new Vector3(scale, scale, 1);
        }
    }
}