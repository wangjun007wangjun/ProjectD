/********************************************************************
  created:  2020-04-01         
  author:    OneJun 

  purpose:  资产持有管理                
*********************************************************************/
using Engine.Base;
using Engine.Res;
using UnityEngine;

namespace Engine.Asset
{
    public class AssetHolder
    {
        private AssetCache _cache;

        /// <summary>
        /// 正在使用的资产
        /// </summary>
        private ArrayList<BaseAsset> _usingAsset;

        public void Create(Transform rootTf)
        {
            _cache = new AssetCache();
            _cache.Create(rootTf);

            _usingAsset = new ArrayList<BaseAsset>();
        }

        /// <summary>
        /// 销毁
        /// </summary>
        public void Destroy()
        {
            _cache.Destroy();
            _cache = null;
            if (_usingAsset.Count > 0)
            {
                GGLog.LogE("AssetHolder.Destroy : using asset is not empty");
                for (int i = 0; i < _usingAsset.Count; i++)
                {
                    GGLog.LogE(_usingAsset[i].BaseData.Name);
                }
                GGLog.LogE("---End---");
            }

            _usingAsset = null;
        }

        /// <summary>
        /// 获取缓存数量
        /// </summary>
        /// <param name="name">资产名</param>
        /// <returns>数量</returns>
        public int GetCacheCount(string name)
        {
            return _cache.GetCount(name);
        }
        
        public void CleanAllUsing()
        {
            if (_usingAsset == null)
            {
                return;
            }
            for (int i = _usingAsset.Count-1; i >=0; i--)
            {
                BaseAsset ba = _usingAsset[i];
                if (!AssetProcessor.ProcessDestroy(ba))
                {
                    continue;
                }

                if (ba.Resource != null)
                {
                    ResService.UnloadResource(ba.Resource);
                    ba.Resource = null;
                }

                ba.BaseData.Factory.DestroyObject(ba);
                //
                _usingAsset.RemoveAt(i);
            }
            
        }

        /// <summary>
        /// 添加缓存
        /// </summary>
        /// <param name="data">资产数据</param>
        /// <param name="resource">资源</param>
        /// <returns>添加到缓存的资产（null标识添加失败）</returns>
        public BaseAsset AddCache(AssetData data, ResObj resource)
        {
            BaseAsset ba = CreateAsset(data, resource);
            if (ba == null)
            {
                return null;
            }

            _cache.AddAsset(ba);

            return ba;
        }

        /// <summary>
        /// 清理缓存
        /// </summary>
        /// <param name="keepPathList">保留的资源名列表</param>
        public void ClearCache(ArrayList<string> keepPathList=null)
        {
            _cache.ClearAll();
        }

        /// <summary>
        /// 同步装载
        /// </summary>
        /// <param name="data">资产数据</param>
        /// <param name="clone">是否Clone一个副本</param>
        /// <returns>资产</returns>
        public BaseAsset Load(ref AssetData data, bool clone)
        {
            if (string.IsNullOrEmpty(data.Filename))
            {
                GGLog.LogE("AssetHolder.Load : invalid parameter");
                return null;
            }

            //缓存找
            BaseAsset ba = _cache.GetAsset(data.Name);
            if (ba != null)
            {
                //缓存复制
                if (clone)
                {
                    BaseAsset cloneBa = AssetProcessor.ProcessClone(ba);
                    _cache.AddAsset(ba);
                    ba = cloneBa;
                }

                if (ba != null)
                {
                    if (!AssetProcessor.ProcessCreate(ba))
                    {
                        GGLog.LogE("AssetHolder.CreateAsset : failed to process create - {0}", data.Name);
                        ba.BaseData.Callback = null;
                        ba.BaseData.Factory = null;
                        ba.Resource = null;
                        data.Factory.DestroyObject(ba);
                        return null;
                    }
                    _usingAsset.Add(ba);
                }
                return ba;
            }

            //没在缓存 同步创建
            ResObj resource = ResService.GetResource(data.Filename);
            if (resource == null)
            {
                GGLog.LogE("AssetHolder.Load : failed to load resource - {0}", data.Filename);
                return null;
            }

            if (resource.Content == null)
            {
                GGLog.LogE("AssetHolder.Load : failed to load resource - {0}", data.Filename);
                ResService.UnloadResource(resource);
                return null;
            }

            ba = CreateAsset(data, resource);
            if (ba == null)
            {
                ResService.UnloadResource(resource);
                return null;
            }

            if (clone)
            {
                BaseAsset cloneBa = AssetProcessor.ProcessClone(ba);
                _cache.AddAsset(ba);
                ba = cloneBa;
            }

            if (ba != null)
            {
                _usingAsset.Add(ba);
            }
            return ba;
        }

        /// <summary>
        /// 卸载
        /// </summary>
        /// <param name="ba">待卸载的资产</param>
        /// <param name="forceDestroy">是否强制销毁</param>
        public void Unload(BaseAsset ba, bool forceDestroy = false)
        {
            if (ba == null)
            {
                GGLog.LogE("AssetHolder.Unload : invalid parameter");
                return;
            }

            int found = _usingAsset.IndexOf(ba);
            if (found == -1)
            {
                GGLog.LogE("AssetHolder.Unload : can't find asset - {0}", ba.BaseData.Name);
                return;
            }
            _usingAsset.RemoveAt(found);
            //销毁手动和Imediate 
            if (forceDestroy || ba.BaseData.Life == LifeType.Manual)
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
            else
            {
                _cache.AddAsset(ba);
            }
        }

        private BaseAsset CreateAsset(AssetData data, ResObj resource)
        {
            if (resource == null || resource.Content == null)
            {
                GGLog.LogE("AssetHolder.CreateAsset : invalid parameter - {0}", data.Name);
                return null;
            }

            data.Callback = null;

            BaseAsset ba = data.Factory.CreateObject();
            if (ba == null)
            {
                GGLog.LogE("AssetHolder.CreateAsset : failed to create asset - {0}", data.Name);
                return null;
            }

            ba.BaseData = data;
            ba.Resource = resource;

            if (!AssetProcessor.ProcessCreate(ba))
            {
                GGLog.LogE("AssetHolder.CreateAsset : failed to process create - {0}", data.Name);
                ba.BaseData.Callback = null;
                ba.BaseData.Factory = null;
                ba.Resource = null;
                data.Factory.DestroyObject(ba);
                return null;
            }
            return ba;
        }

#if UNITY_EDITOR
        /// <summary>
        /// 正在使用的资产列表
        /// </summary>
        /// <returns></returns>
        public ArrayList<BaseAsset> GetUsingAssetList()
        {
            return _usingAsset;
        }
#endif
    }
}