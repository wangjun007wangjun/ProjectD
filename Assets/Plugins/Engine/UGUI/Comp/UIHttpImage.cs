using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using UnityEngine.Networking;
using Engine.Asset;
using Engine.Res;
using Engine.NetAsset;
using Engine.Base;

namespace Engine.UGUI
{
    public enum UIHttpImageState
    {
        Unload,
        Loading,
        Loaded
    };

    [RequireComponent(typeof(Image))]
    public class UIHttpImage : UIComponent, IAssetLoadCallback
    {
        [Header("图片地址(Asset://)(http://)")]
        public string ImageUrl;

        [HideInInspector]
        public bool IsAsyncAssetLoad = false;

        [Header("缓存网络图片")]
        public bool IsCacheTexture = true;
        [Header("缓存时间")]
        public float CachedTextureValidDays = 2f;
        [Header("加载封面显示对象")]
        public GameObject LoadingCover;

        [Header("缓存图片宽度")]
        public int CachedNetAssetWidth = 0;
        [Header("缓存图片高度")]
        public int CachedNetAssetHeight = 0;
        [Header("显示图片实际大小")]
        public bool IsSetNativeSize;

        //UGUI Image
        private Image _image;
        private Sprite _imageDefaultSprite;
        private UIHttpImageState _httpImageState = UIHttpImageState.Unload;

        //本地资源图片
        private SpriteAsset _resAsset;
        private string _resAsyncPath;

        public override void Initialize(UIForm formScript)
        {
            if (_isInited)
            {
                return;
            }
            base.Initialize(formScript);
            _image = this.gameObject.GetComponent<Image>();
            _imageDefaultSprite = _image.sprite;
            _httpImageState = UIHttpImageState.Unload;
            if (LoadingCover != null)
            {
                LoadingCover.SetActive(true);
            }
        }


        private void Awake()
        {
            if (this._image == null)
            {
                _image = this.gameObject.GetComponent<Image>();
                _imageDefaultSprite = _image.sprite;
                if (LoadingCover != null)
                {
                    LoadingCover.SetActive(true);
                }
            }
        }

        //隐藏
        public override void OnHide()
        {
            //JWLog.LogD("---------->UIHttpImage----->OnHide");
        }

        //显示
        public override void OnAppear()
        {
            //JWLog.LogD("---------->UIHttpImage----->OnAppear");
            if (this._image == null)
            {
                _image = this.gameObject.GetComponent<Image>();
                _imageDefaultSprite = _image.sprite;
            }
        }

        //窗口关闭
        public override void OnClose()
        {
            if (_image)
            {
                _image.sprite = null;
                if (_imageDefaultSprite)
                {
                    _image.sprite = _imageDefaultSprite;
                }
            }
            //
            ReleaseImage();
        }

        //
        private void OnDestroy()
        {
            OnClose();
            if (LoadingCover != null)
            {
                LoadingCover.SetActive(false);
            }
        }

        //设置Url
        public void SetImageUrl(string url)
        {
            //JWLog.LogD("---------->UIHttpImage----->SetImageUrl");
            if (string.IsNullOrEmpty(url))
            {
                ReleaseImage();
                ImageUrl = null;
                return;
            }
            if (_isInited == false)
            {
                _image = this.gameObject.GetComponent<Image>();
                _imageDefaultSprite = _image.sprite;
                _httpImageState = UIHttpImageState.Unload;
                if (LoadingCover != null)
                {
                    LoadingCover.SetActive(true);
                }
            }

            ImageUrl = url;
            if (_image != null)
            {
                _image.sprite = _imageDefaultSprite;
            }
            //停止之前
            if (this.gameObject.activeInHierarchy)
            {
                ReleaseImage();
            }
            //本地异步
            if (_resAsyncPath != null)
            {
                AssetService.GetInstance().CancelAsyn(this, _resAsyncPath);
                _resAsyncPath = null;
            }
            //加载
            _httpImageState = UIHttpImageState.Loading;
            if (LoadingCover != null)
            {
                LoadingCover.SetActive(true);
            }
            LoadTexture(ImageUrl);
        }

        //释放掉
        public void ReleaseImage()
        {
            if (_image == null)
            {
                return;
            }
            if (_imageDefaultSprite)
            {
                _image.sprite = _imageDefaultSprite;
            }
            else
            {
                _image.sprite = null;
            }
            if (_resAsyncPath != null)
            {
                AssetService.GetInstance().CancelAsyn(this, _resAsyncPath);
                _resAsyncPath = null;
            }
            if (_resAsset != null)
            {
                AssetService.GetInstance().Unload(_resAsset);
                _resAsset = null;
            }
            _httpImageState = UIHttpImageState.Loaded;
            StopAllCoroutines();
        }

        private void LoadTexture(string url)
        {
            //支持ab 
            if (!(url.StartsWith("Http://", StringComparison.OrdinalIgnoreCase) || url.StartsWith("Https://", StringComparison.OrdinalIgnoreCase)))
            {
                //释放旧的
                if (_resAsset != null)
                {
                    AssetService.GetInstance().Unload(_resAsset);
                    _resAsset = null;
                }
                //
                if (IsAsyncAssetLoad == false)
                {
                    //resources
                    string assetPath = url;
                    _resAsset = AssetService.GetInstance().LoadSpriteAsset(assetPath);
                    if (_resAsset != null)
                    {
                        if (_image != null)
                        {
                            _image.sprite = _resAsset.SpriteObj;
                            if (IsSetNativeSize)
                            {
                                if (_image != null)
                                {
                                    _image.SetNativeSize();
                                }
                            }
                            _httpImageState = UIHttpImageState.Loaded;
                            if (LoadingCover != null)
                            {
                                LoadingCover.SetActive(false);
                            }
                        }
                    }
                    else
                    {
                        GGLog.LogE("UIHttpImage Load Local Image Error:" + url);
                    }
                }
                else
                {
                    _resAsyncPath = url;
                    //异步获取
                    AssetService.GetInstance().LoadAsyn(AssetType.Sprite, url, this, LifeType.Manual);
                }
                return;
            }

            //网络图片
            if (IsCacheTexture)
            {
                Texture2D texture2D;
                if (CachedNetAssetWidth != 0 && CachedNetAssetHeight != 0)
                {
                    texture2D = NetAssetService.GetInstance().GetCachedNetImage(url, CachedTextureValidDays, CachedNetAssetWidth, CachedNetAssetHeight);
                }
                else
                {
                    texture2D = NetAssetService.GetInstance().GetCachedNetImage(url, CachedTextureValidDays);
                }
                //
                if (texture2D != null)
                {
                    if (_image != null)
                    {
                        _image.sprite = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));

                        if (IsSetNativeSize)
                        {
                            if (_image != null)
                            {
                                _image.SetNativeSize();
                            }
                        }

                        _httpImageState = UIHttpImageState.Loaded;

                        if (LoadingCover != null)
                        {
                            LoadingCover.SetActive(false);
                        }
                    }
                }
                else
                {
                    if (gameObject.activeSelf)
                    {
                        StartCoroutine(DownloadImage(url));
                    }
                }
            }
            else
            {
                if (gameObject.activeSelf)
                {
                    StartCoroutine(DownloadImage(url));
                }
            }
        }

        public void OnLoadAssetCompleted(string assetName, int result, ResObj resource)
        {
            if (assetName.Equals(_resAsyncPath))
            {
                _resAsset = AssetService.GetInstance().LoadSpriteAsset(_resAsyncPath, LifeType.Manual);
                if (_resAsset != null)
                {
                    if (_image != null)
                    {
                        _image.sprite = _resAsset.SpriteObj;
                        if (IsSetNativeSize)
                        {
                            if (_image != null)
                            {
                                _image.SetNativeSize();
                            }
                        }
                        _httpImageState = UIHttpImageState.Loaded;
                        if (LoadingCover != null)
                        {
                            LoadingCover.SetActive(false);
                        }
                    }
                }
                else
                {
                    GGLog.LogE("UIHttpImage Load Local Image Error:" + _resAsyncPath);
                }
                _resAsyncPath = null;
            }
        }


        //网络下载
        private IEnumerator DownloadImage(string url)
        {
            _httpImageState = UIHttpImageState.Loading;

            float startTime = Time.realtimeSinceStartup;

            UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
            yield return www.SendWebRequest();

            _httpImageState = UIHttpImageState.Loaded;

            if (LoadingCover != null)
            {
                LoadingCover.SetActive(false);
            }
            //无错误
            if (!(www.isNetworkError || www.isHttpError))
            {
                {
                    Texture2D texture2D = null;
                    texture2D = ((DownloadHandlerTexture)www.downloadHandler).texture;
                    if (texture2D != null)
                    {
                        if (_image != null)
                        {
                            _image.sprite = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));

                            if (IsSetNativeSize)
                            {
                                if (_image != null)
                                {
                                    _image.SetNativeSize();
                                }
                            }
                        }

                        if (IsCacheTexture)
                        {
                            NetAssetService.GetInstance().AddCachedNetImage(url, texture2D.width, texture2D.height, www.downloadHandler.data);
                        }

                        texture2D = null;
                    }
                }
            }
            else
            {
                GGLog.LogE("UIHttpImage Download Image Error:" + ImageUrl + www.error);
            }
            //
            www.Dispose();
            www = null;
        }
    };
};