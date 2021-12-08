using System;
using System.Collections;
using System.Collections.Generic;
using Engine.Base;
using Engine.Res;
using UnityEngine;

namespace Engine.Asset
{
    public class AssetCache
    {
        private Transform _parentTf;

        private Dictionary<string, ArrayList<BaseAsset>> _assets;

        /// <summary>
        /// 创建
        /// </summary>
        /// <param name="rootTf"></param>
        public void Create(Transform rootTf)
        {
            _parentTf = new GameObject("AssetCache").transform;
            _parentTf.parent = rootTf;
            _assets = new Dictionary<string, ArrayList<BaseAsset>>(StringComparer.OrdinalIgnoreCase);
        }

        public void Destroy()
        {
            foreach (KeyValuePair<string,ArrayList<BaseAsset>> ast in _assets)
            {
                for (int i = 0; i < ast.Value.Count; i++)
                {
                    BaseAsset ba = ast.Value[i];

                    UIFormAsset oa = ba as UIFormAsset;
                    if(oa != null)
                    {
                        oa.OnFormAssetDestroy();
                    }

                    if(ba.RootGo != null)
                    {
                        ba.RootGo.ExtDestroy();
                    }

                    if(ba.Resource != null)
                    {
                        ResService.UnloadResource(ba.Resource);
                    }
                }
            }

            _assets = null;
            _parentTf.gameObject.ExtDestroy();
            _parentTf = null;
        }

        /// <summary>
        /// 获取数量
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public int GetCount(string name)
        {
            if(string.IsNullOrEmpty(name))
            {
                GGLog.LogE("AssetCache.GetCount : invalid parameter");
                return 0;
            }
            ArrayList<BaseAsset> ast;
            return _assets.TryGetValue(name, out ast) ? ast.Count : 0;
        }


        /// <summary>
        /// 添加资产
        /// </summary>
        /// <param name="ba">资产</param>
        public void AddAsset(BaseAsset ba)
        {
            if (ba == null || string.IsNullOrEmpty(ba.BaseData.Name))
            {
                GGLog.LogE("AssetCache.AddAsset : invalid parameter");
                return;
            }

            ArrayList<BaseAsset> ast;
            //资产名称对应的 资产列表
            if (!_assets.TryGetValue(ba.BaseData.Name, out ast))
            {
                ast = new ArrayList<BaseAsset>();
                _assets.Add(ba.BaseData.Name, ast);
            }
            //回收
            AssetProcessor.ProcessRestore(ba);
            if (ba.RootGo != null)
            {
                ba.RootGo.ExtSetActive(false);
            }
            if (ba.RootTf != null)
            {
                ba.RootTf.SetParent(_parentTf, false);
            }
            ast.Add(ba);
        }

        /// <summary>
        /// 获取资产
        /// </summary>
        /// <param name="name">资产名</param>
        /// <returns>资产</returns>
        public BaseAsset GetAsset(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                GGLog.LogE("AssetCache.GetAsset : invalid parameter");
                return null;
            }

            ArrayList<BaseAsset> ast;
            if (!_assets.TryGetValue(name, out ast) || ast.Count <= 0)
            {
                return null;
            }
            //最后一个
            BaseAsset ba = ast[ast.Count - 1];
            ast.RemoveAt(ast.Count - 1);
            return ba;
        }

        /// <summary>
        /// 清理所有
        /// </summary>
        public void ClearAll(ArrayList<string> keepPathList=null)
        {
            Dictionary<string, ArrayList<BaseAsset>>.Enumerator enumerator = _assets.GetEnumerator();
            while (enumerator.MoveNext())
            {
                ArrayList<BaseAsset> val = enumerator.Current.Value;
                if (val == null)
                {
                    continue;
                }

                for (int i = val.Count - 1; i >= 0; i--)
                {
                    BaseAsset ba = val[i];
                    if (ba == null)
                    {
                        val.RemoveAt(i);
                        continue;
                    }
                    if (AssetProcessor.ProcessDestroy(ba))
                    {
                        if (ba.Resource != null)
                        {
                            ResService.UnloadResource(ba.Resource);
                            ba.Resource = null;
                        }

                        ba.BaseData.Factory.DestroyObject(ba);
                    }
                    else
                    {
                        GGLog.LogE("AssetCache.ClearCache : failed to process Destroy - {0}", ba.BaseData.Name);
                    }
                    val.RemoveAt(i);
                }
            }
        }
    }
}