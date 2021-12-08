/********************************************************************
  created:  2020-06-05         
  author:    OneJun           

  purpose:  UI粒子控件                
*********************************************************************/
using UnityEngine;
using System;
using Engine.Asset;
using System.Collections.Generic;
using Engine.Base;
using System.Collections;


namespace Engine.UGUI
{
    public class UIParticle : UIComponent
    {
        class ScaleData
        {
            public Transform Transform;
            public Vector3 BeginScale = Vector3.one;
        }

        [Header("资源路径")]
        public string ResPath = "";
        [Header("是否固定缩放")]
        public bool IsFixScaleToForm = false;
        [Header("排序偏移")]
        public int SortOffset = 1;

        [Header("延时播放时间")]
        public float DelayPlay = 0.2f;

        private Renderer[] _renderers;
        private int _rendererCount;
        private BaseAsset _cachedAsset;

        //缩放数据
        private List<ScaleData> _scaleDatas = null;

        //加载粒子资源
        private void LoadRes()
        {
            if(_cachedAsset!=null){
                return;
            }
            string realPath = ResPath;
            if (!String.IsNullOrEmpty(realPath))
            {
                _cachedAsset = AssetService.GetInstance().LoadInstantiateAsset(realPath, LifeType.Trust);
                if (_cachedAsset != null)
                {
                    if (_cachedAsset.RootTf != null)
                    {
                        _cachedAsset.RootTf.SetParent(gameObject.transform);
                        _cachedAsset.RootTf.localPosition = Vector3.zero;
                        _cachedAsset.RootTf.localRotation = Quaternion.identity;
                        _cachedAsset.RootTf.localScale = Vector3.one;
                        _cachedAsset.RootGo.ExtSetActive(true);
                    }
                }
            }
        }

        private IEnumerator DelayToPlay()
        {
            yield return new WaitForSeconds(DelayPlay);
            if (_cachedAsset != null)
            {
                _cachedAsset.RootGo.SetActive(true);
            }
        }


        /// 初始化
        public override void Initialize(UIForm formScript)
        {
            if (_isInited)
            {
                return;
            }
            LoadRes();
            InitializeRenderers();
            base.Initialize(formScript);
            if (IsFixScaleToForm)
            {
                ResetScale();
            }
            else
            {
                AdapteScale();
            }
        }

        //重置Scale，保证粒子发射器大小不受Form节点的影响
        private void ResetScale()
        {
            float newScale = 1 / BelongedForm.gameObject.transform.localScale.x;
            gameObject.transform.localScale = new Vector3(newScale, newScale, 0);
        }

        //关闭
        public override void OnClose()
        {
            if (_cachedAsset != null)
            {
                AssetService.GetInstance().Unload(_cachedAsset);
                _cachedAsset = null;
            }
            if (_renderers != null)
            {
                _renderers = null;
            }
            StopAllCoroutines();
        }

        private void OnDestroy()
        {

        }

        /// Hide
        public override void OnHide()
        {
            base.OnHide();
            StopAllCoroutines();
        }


        /// Appear
        public override void OnAppear()
        {
            if (_renderers == null || _cachedAsset == null)
            {
                LoadRes();
                InitializeRenderers();
            }
            if (gameObject.activeSelf)
            {
                StartCoroutine(DelayToPlay());
            }
        }

        public void OnEnable()
        {
            if (_renderers == null || _cachedAsset == null)
            {
                LoadRes();
                InitializeRenderers();
            }
            if (BelongedForm != null)
            {
                SetSortingOrder(BelongedForm.GetSortingOrder());
            }
            StartCoroutine(DelayToPlay());
        }

        /// Appear
        public override void SetSortingOrder(int sortingOrder)
        {
            if (_renderers != null)
            {
                for (int i = 0; i < _rendererCount; i++)
                {
                    _renderers[i].sortingOrder = sortingOrder + SortOffset;
                }
            }
        }

        /// 初始化渲染器
        private void InitializeRenderers()
        {
            if(_renderers!=null){
                return;
            }
            _renderers = new Renderer[40];
            _rendererCount = 0;
            UIUtility.GetComponentsInChildren<Renderer>(this.gameObject, _renderers, ref _rendererCount);
        }

        //调整缩放
        private void AdapteScale()
        {
            if (_scaleDatas == null)
            {
                _scaleDatas = new List<ScaleData>();
                foreach (ParticleSystem p in transform.GetComponentsInChildren<ParticleSystem>(true))
                {
                    _scaleDatas.Add(new ScaleData() { Transform = p.transform, BeginScale = p.transform.localScale });
                }
            }

            float designWidth = 1334;
            float designHeight = 750;
            if (BelongedForm)
            {
                designWidth = BelongedForm.ReferenceResolution.x;
                designHeight = BelongedForm.ReferenceResolution.y;
            }

            float designScale = designWidth / designHeight;
            float scaleRate = (float)Screen.width / (float)Screen.height;
            foreach (ScaleData scale in _scaleDatas)
            {
                if (scale.Transform != null)
                {
                    if (scaleRate < designScale)
                    {
                        float scaleFactor = scaleRate / designScale;
                        scale.Transform.localScale = scale.BeginScale * scaleFactor;
                    }
                    else
                    {
                        scale.Transform.localScale = scale.BeginScale;
                    }
                }
            }
        }

        //#if UNITY_EDITOR
        //        void Update()
        //        {

        //            AdapteScale(); 
        //        }
        //#endif
    }
}