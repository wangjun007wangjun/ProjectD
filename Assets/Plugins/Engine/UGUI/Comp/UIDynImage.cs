/********************************************************************
  created:  2020-06-05         
  author:    OneJun        

  purpose:  动态图片 图片纹理精灵  自动读取并显示                
*********************************************************************/
using System.Collections;
using System.Collections.Generic;
using Engine.Asset;
using Engine.Base;
using Engine.Res;
using UnityEngine;
using UnityEngine.UI;

namespace Engine.UGUI
{
    [AddComponentMenu("Engine/UGUI/UIDynImage")]
    [RequireComponent(typeof(Image))]
    public class UIDynImage : UIComponent,IAssetLoadCallback
    {
        [Header("图片地址")]
        public string ImageAddr;
        [Header("是否异步加载")]
        public bool IsAsyncLoad = true;

        [Header("是否NativeSize")]
        public bool IsNativeSize = false;

        [Header("是否采用平台后缀")]
        public bool IsUsePlatSuffix = false;

        //UGUI Image
        [HideInInspector]
        public Image _image;
        //asset
        private SpriteAsset _resAsset;
        //最终地址
        public string _realAddr = null;

        public override void Initialize(UIForm formScript)
        {
            if (_isInited)
            {
                return;
            }
            base.Initialize(formScript);
            if (this._image == null)
            {
                _image = this.gameObject.GetComponent<Image>();
            }
        }

        private void Awake()
        {
            if (this._image == null)
            {
                _image = this.gameObject.GetComponent<Image>();
            }
        }

        //窗口隐藏
        public override void OnHide()
        {

        }

        //窗口显示
        public override void OnAppear()
        {

        }

        //窗口关闭
        public override void OnClose()
        {
            ReleaseImage();
        }

        private void OnDestroy()
        {
            ReleaseImage();
            if(_image!=null){
                _image.sprite=null;
            }
        }

        void OnEnable()
        {
            if (!string.IsNullOrEmpty(ImageAddr))
            {
                SetImageAddr(ImageAddr);
            }
        }

        /// <summary>
        /// 设置图像gray 
        /// </summary>
        /// <param name="v">0---1</param>
        public void SetImageGray(float v)
        {
            if (_image == null)
            {
                return;
            }
            Material oldMat = _image.material;
            Material newMat = new Material(oldMat);
            _image.material = newMat;
            newMat.SetFloat("_GrayAmount", Mathf.Clamp01(v));
            oldMat = null;
        }

        /// <summary>
        /// 设置图片资源地址
        /// </summary>
        /// <param name="url"></param>
        public void SetImageAddr(string url)
        {
            //未激活 先暂存
            if (!gameObject.activeInHierarchy)
            {
                ImageAddr = url;
                return;
            }
            if (_image == null)
            {
                return;
            }
            //置空清除
            if (string.IsNullOrEmpty(url))
            {
                ReleaseImage();
                ImageAddr = null;
                _realAddr = null;
                return;
            }
            //停止之前
            if (this.gameObject.activeInHierarchy)
            {
                ReleaseImage();
            }
            ImageAddr = url;
            if (IsUsePlatSuffix)
            {
#if UNITY_IOS
                _realAddr = ImageAddr + "_Ios";
#else
                _realAddr=ImageAddr;
#endif
            }
            else
            {
                _realAddr = ImageAddr;
            }
            //加载
            LoadAsset();
        }

        public void ReleaseImage()
        {
            if(_realAddr != null && IsAsyncLoad)
            {
                AssetService.GetInstance().CancelAsyn(this,_realAddr);
            }
            if(_resAsset != null)
            {
                AssetService.GetInstance().Unload(_resAsset);
                _resAsset = null;
            }
        }

        private void LoadAsset()
        {
            //释放旧的
            if (_resAsset != null)
            {
                AssetService.GetInstance().Unload(_resAsset);
                _resAsset = null;
            }
            if (IsAsyncLoad)
            {
                AssetService.GetInstance().LoadAsyn(AssetType.Sprite, _realAddr, this, LifeType.Manual);
            }
            else
            {
                //同步
                _resAsset = AssetService.GetInstance().LoadSpriteAsset(_realAddr, LifeType.Manual);
                if (_resAsset != null)
                {
                    if (_image != null)
                    {
                        _image.sprite = _resAsset.SpriteObj;
                        if (IsNativeSize)
                        {
                            _image.SetNativeSize();
                        }
                    }
                }
                else
                {
                    GGLog.LogD("UIDynImage Load Error:" + _realAddr);
                    //回滚
                    if(_realAddr.EndsWith("Ios")){
                        _realAddr=_realAddr.Substring(0,_realAddr.Length-4);
                        //
                        LoadAsset();
                    }
                }
            }
        }

        public void OnLoadAssetCompleted(string assetName, int result, ResObj resource)
        {
            if (assetName.Equals(_realAddr))
            {
                _resAsset = AssetService.GetInstance().LoadSpriteAsset(_realAddr, LifeType.Manual);
                if (_resAsset != null)
                {
                    if (_image != null)
                    {
                        _image.sprite = _resAsset.SpriteObj;
                        if (IsNativeSize)
                        {
                            _image.SetNativeSize();
                        }
                    }
                }
                else
                {
                    GGLog.LogD("UIDynImage Load Error:" + _realAddr);
                    //回滚
                    if(_realAddr.EndsWith("Ios")){
                        _realAddr=_realAddr.Substring(0,_realAddr.Length-4);
                        //
                        LoadAsset();
                    }
                }
            }
        }
    };
};