/********************************************************************
  created:  2020-06-05         
  author:    OneJun           

  purpose:  逻辑层资产服务 todo 提供逻辑上内存管理                
*********************************************************************/
using System;
using Engine.Res;
using Engine.Base;
using Engine.State;
using UnityEngine;

namespace Engine.Asset
{
    public class AssetService : Singleton<AssetService>
    {
        //根
        private Transform _root;
        //资产管理器
        private AssetHolder _assetHolder;
        //资产同步异步加载器
        private AssetLoader _assetLoader;

        public override bool Initialize()
        {
            _root = new GameObject("AssetService").transform;
            _root.gameObject.ExtDontDestroyOnLoad();

            _assetHolder = new AssetHolder();
            _assetHolder.Create(_root);

            _assetLoader = _root.gameObject.AddComponent<AssetLoader>();
            _assetLoader.Initialize(_assetHolder);

            return true;
        }

        public override void Uninitialize()
        {
            Destroy();
            if (_assetHolder != null)
            {
                _assetHolder.Destroy();
                _assetHolder = null;
            }
            _root.gameObject.ExtDestroy();
            _root = null;
        }

        public void Destroy()
        {
            if (_assetLoader != null)
            {
                _assetLoader.Uninitialize();
                _assetLoader = null;
            }
        }

        //清理所有 释放内存
        public void UnloadAllUnusedAssets()
        {
            if (_assetHolder != null)
            {
                _assetHolder.ClearCache(null);
            }
            ResService.GetInstance().UnloadUnusedAssets();
        }

        //异步加载 ToDo 支持UniTask
        public void LoadAsyn(int type, string filename, IAssetLoadCallback callback, int life = LifeType.Trust, int cnt = 1)
        {
            AddLoad(callback, type, filename, life, cnt);
        }

        /// <summary>
        /// 取消异步加载
        /// </summary>
        /// <param name="callback">回调接口</param>
        /// <param name="assetName">资源名或者类名（=null 标识取消所有跟回调接口相关的异步加载）</param>
        public void CancelAsyn(IAssetLoadCallback callback, string assetName = null)
        {
            if (_assetLoader == null)
            {
                return;
            }
            _assetLoader.CancelTask(callback, assetName);
        }


        /// <summary>
        /// 装载具体窗口对象 同步
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="life">生命周期</param>
        /// <returns>对象</returns>
        public UIFormAsset LoadFormAsset<T>(int life) where T : UIFormAsset, new()
        {
            //创建
            T instance = new T();
            Type t = typeof(T);

            AssetData data;
            data.Callback = null;

            data.Type = AssetType.UIForm;
            data.Life = life;
            data.Name = t.Name;
            data.Filename = instance.GetPath();

            data.Factory = AssetClassFactory.GetUIFormAssetClassFactory(instance);
            data.Count = 1;
            data.Priority = (int)LoadPriority.Wait;
            //
            BaseAsset ba = _assetHolder.Load(ref data, false);
            if (ba == null)
            {
                GGLog.LogE("AssetService.LoadAsset : failed to load asset - {0}", t.Name);
                return null;
            }
            return (UIFormAsset)ba;
        }

        /// 加载UIView资产
        public UIViewAsset LoadViewAsset<T>() where T : UIViewAsset, new()
        {
            //创建
            T instance = new T();
            Type t = typeof(T);

            AssetData data;
            data.Callback = null;

            data.Type = AssetType.UIView;
            data.Life = LifeType.Trust;
            data.Name = t.Name;
            data.Filename = instance.GetPath();

            data.Factory = AssetClassFactory.GetUIViewAssetClassFactory(instance);
            data.Count = 1;
            data.Priority = (int)LoadPriority.Wait;
            //
            BaseAsset ba = _assetHolder.Load(ref data, false);
            if (ba == null)
            {
                GGLog.LogE("AssetService.LoadAsset : failed to load asset - {0}", t.Name);
                return null;
            }
            return (UIViewAsset)ba;
        }


        /// <summary>
        /// 装载模型资源
        /// </summary>
        /// <param name="filename">资源名</param>
        /// <returns>资源</returns>
        public ModelAsset LoadModelAsset(string filename,int lifeType = LifeType.Trust)
        {
            return (ModelAsset)LoadAsset(AssetType.Model, filename,lifeType);
        }


        /// <summary>
        /// 装载实例化资源
        /// </summary>
        /// <param name="filename">资源名</param>
        /// <returns>资源</returns>
        public BaseAsset LoadInstantiateAsset(string filename, int lifeType = LifeType.Trust)
        {
            return LoadAsset(AssetType.Instantiate, filename, lifeType);
        }


        /// <summary>
        /// 加载声音资源
        /// </summary>
        /// <param name="filename">资源名</param>
        /// <returns>资源</returns>
        public AudioAsset LoadAudioAsset(string filename, int life = LifeType.Trust, string fileExt = ".mp3")
        {
            return (AudioAsset)LoadAsset(AssetType.Audio, filename, life);
        }

        /// <summary>
        /// 加载精灵资源
        /// </summary>
        /// <param name="filename">资源名</param>
        /// <param name="lifeType">强制以什么生命周期加载</param>
        /// <returns>资源</returns>
        public SpriteAsset LoadSpriteAsset(string filename, int life = LifeType.Trust)
        {
            return (SpriteAsset)LoadAsset(AssetType.Sprite, filename, life);
        }


        /// <summary>
        /// 加载精灵图集资源
        /// </summary>
        /// <param name="filename">资源名</param>
        /// <param name="lifeType">强制以什么生命周期加载</param>
        /// <returns>资源</returns>
        public SpriteAtlasAsset LoadSpriteAtlasAsset(string filename)
        {
            return (SpriteAtlasAsset)LoadAsset(AssetType.SAtlas, filename);
        }


        /// <summary>
        /// 加载Spine资源
        /// </summary>
        /// <param name="filename">资源名</param>
        /// <param name="lifeType">强制以什么生命周期加载</param>
        /// <returns>资源</returns>
        public SpineAsset LoadSpineAsset(string filename,int life = LifeType.Trust)
        {
            return (SpineAsset)LoadAsset(AssetType.Spine, filename,life);
        }

        /// <summary>
        /// 装载原始资源 纹理 材质球 ScriptObject等
        /// </summary>
        /// <param name="filename">资源名</param>
        /// <returns>资源</returns>
        public BaseAsset LoadPrimitiveAsset(string filename,int life = LifeType.Trust)
        {
            return LoadAsset(AssetType.Primitive, filename,life);
        }


        /// <summary>
        /// 卸载
        /// </summary>
        /// <param name="ba">资源</param>
        public void Unload(BaseAsset ba)
        {
            if (ba == null)
            {
                return;
            }

            if (_assetHolder != null)
            {
                _assetHolder.Unload(ba, SingletonState == SingletonStateEnum.Destroying);
            }
            else
            {
                if (!AssetProcessor.ProcessDestroy(ba))
                {
                    return;
                }

                if (ba.Resource != null)
                {
                    ResService.UnloadResource(ba.Resource);
                    ba.Resource = null;
                }

                ba.BaseData.Factory.DestroyObject(ba);
            }
        }

        //-------------------------------------------------------------------------
        /// 同步加载
        private BaseAsset LoadAsset(int type, string filename, int life = LifeType.Trust, bool clone = false)
        {
            AssetData data;
            data.Callback = null;

            data.Type = type;
            data.Life = life;
            data.Name = filename;
            data.Filename = filename;
            data.Factory = AssetClassFactory.GetFactory(type);
            data.Priority = (int)LoadPriority.ImmediateShow;
            data.Count = 1;

            BaseAsset ba = _assetHolder.Load(ref data, clone);

            if (ba == null)
            {
                GGLog.LogE("AssetService.LoadAsset : failed to load asset - {0}", data.Filename);
                return null;
            }

            if (ba.BaseData.Type != type)
            {
                GGLog.LogE("AssetService.LoadAsset : type mismatch - {0}, type - {1};{2}", data.Filename, ba.BaseData.Type, type);
                Unload(ba);
                return null;
            }
            return ba;
        }

        //添加异步加载
        private void AddLoad(IAssetLoadCallback callback, int type, string filename, int life, int count)
        {
            if (string.IsNullOrEmpty(filename) || count < 1)
            {
                GGLog.LogE("AssetService.AddAsyncLoad : invalid parameter");
                return;
            }
            if (_assetLoader == null)
            {
                return;
            }

            AssetData data = new AssetData();
            data.Callback = callback;
            data.Name = filename;
            data.Type = type;
            data.Filename = filename;
            data.Factory = AssetClassFactory.GetFactory(type);
            data.Count = count;
            data.Life = life;
            data.Priority = LoadPriority.ImmediateShow;
            _assetLoader.AddTask(ref data);
        }

        public AssetHolder GetHolder()
        {
            return _assetHolder;
        }

    }
}
