/********************************************************************
*		作者： XH
*		时间： 2019-04-28
*		描述： 循环滚动容器组件,目前只支持水平滑动
*             (可实现banner循环滑动，捕鱼渔场选择那样的滑动效果)
*********************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using System;
using Engine.Asset;
using Engine.PLink;
using Engine.Base;

namespace Engine.UGUI { 
    public class UILoopScroll : UIComponent,IBeginDragHandler,IDragHandler,IEndDragHandler,IPointerExitHandler {
#region ###可序列化属性###
    #pragma warning disable 0649
        [SerializeField]
        private UILoopScrollCell _templateCell = null;
        [SerializeField]
        private int _showCount = 3;//展示的数量
        [SerializeField]
        private float _cellWidth = 100;//Cell宽度
        //缩放曲线
        public AnimationCurve _curve = null;
        //是否做缩放动画
        [SerializeField]
        private bool _scaleAnimation = false;        
#endregion

#region ###私有属性###

        //每个cell之间的间隔
        private float _gap;
        //自己的RectTransform
        private RectTransform _rectTransform = null;
        //列表
        private List<UILoopScrollCell> _cellList = new List<UILoopScrollCell>();

        //缓存列表
        private List<UILoopScrollCell> _cacheList = new List<UILoopScrollCell>();

        //选中的Item
        private int _centerIdx = 0;
        //最小x
        private float _xMin = 0;
        //最大x
        private float _xMax = 0;
        //间距
        private float _margin = 100;
        //当前在做动画的cell
        private Transform _curTweenTf = null;
        //select delegate
        public delegate void SelectCell(int index);
        //选中
        public SelectCell SelectHandler;
        //在缓动过程中
        private bool _inTween = false;
        //操作是否无效
        private bool _invalidOp = false;

#endregion

#region   ###外部调用###
        //当点击中间的Item的回调
        public Action<int> OnClick;
        //初始化回调
        public Action<int, PrefabLink> OnEnableCell;

        public Action<int> OnCenterIdxChanged;
        //当前最中间的index
        public int CenterIndex
        {
            get { return _centerIdx; }
            private set {
                // if(_centerIdx == value)
                // {
                //     return;
                // }
                _centerIdx = value;
                foreach(var cell in _cellList)
                {
                    cell.OnSelect(_centerIdx);
                }
                if(OnCenterIdxChanged!=null)
                {
                    OnCenterIdxChanged.Invoke(value);
                }
            }
        }

        //设置Container数量
        public void SetCount(int count)
        {
            if(_templateCell == null)
            {
                GGLog.LogE("Error!!! Please Select UILoopScrollCell Template.");
                return;
            }
            //隐藏Cell模板
            _templateCell.SetVisible(false);
            //清空Cells
            Clear();
            //计算每个Cell的间隔
            _gap = (GetWidth() - _showCount * _cellWidth) / (_showCount + 1.0f);
            //实例化Cell
            for (int i = 0; i < count; i++)
            {
                UILoopScrollCell cell = GetCell();
                cell.Init(i,this);
                cell.SetVisible(true);
                //Cell初始化回调
                if(OnEnableCell!=null)
                {
                    OnEnableCell.Invoke(i, cell.CellPLink);
                }
                //
                cell.transform.name = i.ToString();
                _cellList.Add(cell);
            }
            //
            OnAppear();
            CenterIndex = 0;
            UpdatePosition();
            if(_scaleAnimation)
            {
                UpdateScale();
            }
        }
        //选中某一项
        public void Select(int idx)
        {
            CenterIndex = idx;
            _cellList[_centerIdx].SetPosition(0);
            UpdatePosition();
            if(_scaleAnimation)
            {
                UpdateScale();
            }
        }
        //缓动到下一个
        public void TweenToNext()
        {
            if(_inTween)
            {
                return;
            }
            CenterIndex = GetRelativeCenterIndex(1);
            TweenToCenter();
        }
        //缓动到上一个
        public void TweenToPrevious()
        {
            CenterIndex = GetRelativeCenterIndex(-1);
            TweenToCenter();
        }

        //清除
        public void Clear(bool isCache = true)
        {
            if (_cellList != null && _cellList.Count > 0)
            {
                foreach (var cell in _cellList)
                {
                    if(isCache)
                    {
                        RecycleCell(cell);
                    }
                    else
                    {
                        Destroy(cell.gameObject);
                    }
                    cell.UnInit();
                }
                _cellList.Clear();
            }
            if(!isCache)
            {
                foreach(var cacheCell in _cacheList)
                {
                    Destroy(cacheCell.gameObject);
                }
                _cacheList.Clear();
            }
        }
#endregion

#region  ###内部调用###
        public void OnSelect(int index)
        {
            if(SelectHandler != null)
            {
                SelectHandler.Invoke(index);
            }
            if(CenterIndex == index)
            {
                if(OnClick != null)
                {
                    OnClick(index);
                }
            }
            else
            {
                CenterIndex = index;
                TweenToCenter();
            }
        }
        private float GetWidth()
        {
            return _rectTransform.rect.width;
        }

        public override void Initialize(UIForm formScript)
        {
            base.Initialize(formScript);
            _rectTransform = GetComponent<RectTransform>();
            float width = _rectTransform.rect.width;
            _xMin = -width * 0.5f;
            _xMax = width * 0.5f;
        }

        private float _down_x = 0;
        private int _down_center_idx = 0;
        private bool _is_down = false;
        [SerializeField]
        private float Min_Detect_Len = 10;
        public void OnBeginDrag(PointerEventData eventData)
        {
            _inTween = true;
            _down_x = eventData.position.x;
            _down_center_idx = CenterIndex;
            _cellList[CenterIndex].transform.DOKill();
            _invalidOp = false;
            _is_down = true;
        }

        // 
        public void OnDrag(PointerEventData eventData)
        {
            
            if(_cellList.Count == 0)
            {
                return;
            }
            if(_invalidOp)
            {
                return;
            }
            _cellList[CenterIndex].UpdatePosition(eventData.delta.x);

            FindCenter();
            UpdatePosition();
            if(_scaleAnimation)
            {
                UpdateScale();
            }
        }

        //
        public void OnEndDrag(PointerEventData eventData)
        {
            FindCenter();
            if(CenterIndex == _down_center_idx)
            {
                float x_offset =  eventData.position.x - _down_x;
                if(Math.Abs(x_offset) >= Min_Detect_Len)
                {
                    int direction = x_offset > 0 ? -1 : 1;
                    CenterIndex = GetRelativeCenterIndex(direction);
                }
            }
            TweenToCenter();
        }


        public void OnPointerExit(PointerEventData eventData)
        {
            FindCenter();
            TweenToCenter();
            _invalidOp = true;
        }

        //缓动到中心
        private void TweenToCenter()
        {
            if (_curTweenTf != null)
            {
                _curTweenTf.DOKill();
            }
            _curTweenTf = _cellList[CenterIndex].transform;
            _inTween = true;
            Tweener tweener = _curTweenTf.DOLocalMoveX(0, 0.5f);

            tweener.onUpdate = () =>
            {
                UpdatePosition();
                if(_scaleAnimation)
                {
                    UpdateScale();
                }
            };
            tweener.onComplete = () => { _inTween = false; };
        }
        //检测是否与scroll rect重叠
        private bool Overlaps(float centerX, float width)
        {
            return !(centerX - width * 0.5f > _xMax || centerX + width * 0.5f < _xMin);
        }
        //刷新CenterIdx
        private void FindCenter()
        {
            int newCenterIdx = CenterIndex;
            float minOffset  = Mathf.Abs(_cellList[newCenterIdx].GetPosX());

            foreach(var cell in _cellList)
            {
                float xOffset = Mathf.Abs(cell.GetPosX());
                if (xOffset < minOffset)
                {
                    minOffset = xOffset;
                    newCenterIdx = cell.Index;
                }
            }
            if(CenterIndex != newCenterIdx)
            {
                CenterIndex = newCenterIdx;
            }
        }
        //位置更新
        private void UpdatePosition()
        {
            int half = _cellList.Count / 2;
            for (int i = -half; i <= half; i++)
            {
                if (i != 0)
                {
                    int index = GetRelativeCenterIndex(i);
                    var cell = _cellList[index];
                    float x = _cellList[CenterIndex].GetPosX() + i * (_cellWidth + _gap);
                    if(_scaleAnimation)
                    {
                        x = Mathf.Clamp(x, _xMin - _margin, _xMax + _margin);
                    }
                    cell.SetPosition(x);
                    cell.SetVisible(Overlaps(cell.GetPosX(), _cellWidth));
                }
            }
        }
        //更新Scale
        private void UpdateScale()
        {
            foreach (UILoopScrollCell cell in _cellList)
            {
                float x = cell.GetPosX();
                cell.SetScale(GetScale(x));
            }
        }
        //获取相对于当前centerIdx的偏移的Index
        private int GetRelativeCenterIndex(int relate)
        {
            return (CenterIndex + relate+_cellList.Count)%_cellList.Count;
        }

        //实例化一个cell
        private UILoopScrollCell GetCell()
        {
            UILoopScrollCell cell = null;
            if(_cacheList.Count>0)
            {
                int idx = _cacheList.Count - 1;
                cell = _cacheList[idx];
                _cacheList.RemoveAt(idx);
            }
            else
            {
                GameObject obj = Instantiate<GameObject>(_templateCell.gameObject);
                RectTransform rectTrans = obj.GetComponent<RectTransform>();
                rectTrans.SetParent(_rectTransform);
                rectTrans.localScale = Vector3.one;
                rectTrans.localPosition = Vector3.zero;
                InitializeComponent(obj);
                cell = obj.GetComponent<UILoopScrollCell>();
            }
            return cell;
        }

        private void RecycleCell(UILoopScrollCell cell)
        {
            cell.SetVisible(false);
            cell.name = "Cache";
            _cacheList.Add(cell);
        }

        public override void OnHide()
        {
            base.OnHide();
            if(_cellList!=null && _cellList.Count > 0)
            {
                foreach(var cell in _cellList)
                {
                    Hide(cell.gameObject);
                }
            }
        }

        public override void OnAppear()
        {
            base.OnAppear();
            if(_cellList!=null && _cellList.Count > 0)
            {
                foreach(var cell in _cellList)
                {
                    Appear(cell.gameObject);
                }
            }
        }

        public override void OnClose()
        {
            if(_cellList != null && _cellList.Count > 0)
            {
                foreach (var cell in _cellList)
                {
                    Close(cell.gameObject);
                }
            }

            Clear();
            OnClick = null;
            OnEnableCell = null;
            base.OnClose();
        }

        private void Hide(GameObject root)
        {
            UIComponent[] uiComponents = root.GetComponents<UIComponent>();

            if (uiComponents != null && uiComponents.Length > 0)
            {
                for (int i = 0; i < uiComponents.Length; i++)
                {
                    //Debug.Log("Hide Component " + uiComponents[i].GetType().ToString() + ", name = " + uiComponents[i].name);
                    uiComponents[i].OnHide();
                }
            }

            for (int i = 0; i < root.transform.childCount; i++)
            {
                Hide(root.transform.GetChild(i).gameObject);
            }
        }
        
        private void Appear(GameObject root)
        {
            UIComponent[] uiComponents = root.GetComponents<UIComponent>();

            if (uiComponents != null && uiComponents.Length > 0)
            {
                for (int i = 0; i < uiComponents.Length; i++)
                {
                    uiComponents[i].OnAppear();
                }
            }

            for (int i = 0; i < root.transform.childCount; i++)
            {
                Appear(root.transform.GetChild(i).gameObject);
            }
        }

        private void OnDestroy()
        {
            Clear(false);
        }

        /// 遍历初始化UI组件
        private void Close(GameObject root)
        {
            UIComponent[] uiComponents = root.GetComponents<UIComponent>();

            if (uiComponents != null && uiComponents.Length > 0)
            {
                for (int i = 0; i < uiComponents.Length; i++)
                {
                    uiComponents[i].OnClose();
                }
            }
            for (int i = 0; i < root.transform.childCount; i++)
            {
                Close(root.transform.GetChild(i).gameObject);
            }
        }


        //如果有缩放动画，从曲线中获取缩放的值
        private float GetScale(float x)
        {
            return _curve.Evaluate(x);
        }

        #endregion
    }
}
