/********************************************************************
  created:  2020-05-28         
  author:    OneJun           

  purpose:  资产类 cache工厂               
*********************************************************************/
using Engine.Base;
using UnityEngine;

namespace Engine.Asset
{
    /// <summary>
    /// 资源资产类型管理
    /// </summary>
    public class ResourceAssetClassCache
    {
        private readonly ArrayList<BaseAsset>[] _cache = new ArrayList<BaseAsset>[AssetType.BaseAssetTypeCount];

        public bool AddClass(BaseAsset ba)
        {
            if (ba == null)
            {
                GGLog.LogE("ResourceClassCache.ReleaseClass : invalid parameter");
                return false;
            }

            if (ba.BaseData.Type < 0 || ba.BaseData.Type >= _cache.Length)
            {
                return false;
            }

            if (_cache[ba.BaseData.Type] == null)
            {
                _cache[ba.BaseData.Type] = new ArrayList<BaseAsset>();
            }

            _cache[ba.BaseData.Type].Add(ba);

            return true;
        }

        public BaseAsset GetClass(int type)
        {
            if (type < 0 || type >= _cache.Length)
            {
                GGLog.LogE("ResourceClassCache.GetClass : invalid parameter - {0}", type);
                return null;
            }

            if (_cache[type] == null || _cache[type].Count <= 0)
            {
                return null;
            }

            BaseAsset ba = _cache[type][_cache[type].Count - 1];
            _cache[type].RemoveAt(_cache[type].Count - 1);
            return ba;
        }
    }

    public class ResourceAssetClassFactory : IObjectFactory<BaseAsset>
    {
        private readonly ResourceAssetClassCache _classCache;
        private readonly int _type;

        public ResourceAssetClassFactory(ResourceAssetClassCache classCache, int type)
        {
            _classCache = classCache;
            _type = type;
        }

        public BaseAsset CreateObject()
        {
            BaseAsset ba = _classCache.GetClass(_type);
            return ba ?? AssetProcessor.CreateAssetClass(_type);
        }

        public void DestroyObject(BaseAsset o)
        {
            _classCache.AddClass(o);
        }
    }

    public class UIFormAssetClassFactory<T> : IObjectFactory<BaseAsset> where T : UIFormAsset, new()
    {
        private T _instance;

        public UIFormAssetClassFactory(T instance)
        {
            _instance = instance;
        }

        public BaseAsset CreateObject()
        {
            T ret = _instance ?? new T();
            _instance = null;
            return ret;
        }

        public void DestroyObject(BaseAsset o) { }
    }

    public class UIViewAssetClassFactory<T> : IObjectFactory<BaseAsset> where T : UIViewAsset, new()
    {
        private T _instance;

        public UIViewAssetClassFactory(T instance)
        {
            _instance = instance;
        }

        public BaseAsset CreateObject()
        {
            T ret = _instance ?? new T();
            _instance = null;
            return ret;
        }

        public void DestroyObject(BaseAsset o) { }
    }

    public static class AssetClassFactory
    {
        private static readonly ResourceAssetClassFactory[] Factory;

        static AssetClassFactory()
        {
            ResourceAssetClassCache cache = new ResourceAssetClassCache();

            Factory = new ResourceAssetClassFactory[AssetType.BaseAssetTypeCount];
            for (int i = 0; i < AssetType.BaseAssetTypeCount; i++)
            {
                Factory[i] = new ResourceAssetClassFactory(cache, i);
            }
        }

        public static IObjectFactory<BaseAsset> GetFactory(int type)
        {
            if (type < 0 || type >= Factory.Length)
            {
                return null;
            }

            return Factory[type];
        }

        /// <summary>
        /// 根据具体UIFormClass获取
        /// </summary>
        /// <param name="instance"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IObjectFactory<BaseAsset> GetUIFormAssetClassFactory<T>(T instance) where T : UIFormAsset, new()
        {
            return new UIFormAssetClassFactory<T>(instance);
        }

        /// <summary>
        /// 根据具体UIVIewClass获取
        /// </summary>
        /// <param name="instance"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IObjectFactory<BaseAsset> GetUIViewAssetClassFactory<T>(T instance) where T : UIViewAsset, new()
        {
            return new UIViewAssetClassFactory<T>(instance);
        }
    }
}