/********************************************************************
  created:  2020-05-28         
  author:    OneJun           

  purpose:   Bundle 处理中间件               
*********************************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using Engine.Base;
using Engine.Res;
using UnityEngine;

namespace Engine.Asset
{
    public class BundleMediator : MonoBehaviour
    {
        private static BundleMediator _sInstance = null;

        public static BundleMediator GetInstance()
        {
            return _sInstance;
        }

        ObjList<string> _loadingResources = null;
        ObjList<string> _unbundleResources = null;
        ObjList<string> _relatedResources = null;

        public bool Initialize()
        {
            _loadingResources = new ObjList<string>();
            _unbundleResources = new ObjList<string>();
            _relatedResources = new ObjList<string>();
            _sInstance = this;
            return true;
        }

        public void Uninitialize()
        {
            StopAllCoroutines();

            _loadingResources.Clear();
            _loadingResources = null;
            _unbundleResources.Clear();
            _unbundleResources = null;
            _relatedResources.Clear();
            _relatedResources = null;
            _sInstance = null;
        }

        /// <summary>
        /// 卸载资源列表对应bundle
        /// </summary>
        /// <param name="resourcesPath"></param>
        public void UnloadBundle(ObjList<string> resourcesPath)
        {
            ObjList<BundlePackInfo> bundleList = GetBundlePackInfoListForResources(resourcesPath, false);
            if (bundleList != null && bundleList.Count > 0)
            {
                for (int i = 0; i < bundleList.Count; i++)
                {
                    BundlePackInfo packInfo = bundleList[i];

                    // unity场景
                    if (packInfo.HasFlag(EBundleFlag.UnityScene))
                    {
                        BundleService.GetInstance().OnAssetLoaded(packInfo);
                    }
                    else
                    {
                        BundleService.GetInstance().Unload(bundleList[i]);
                    }
                }
            }
        }
        /// <summary>
        /// unload bundle
        /// </summary>
        /// <param name="path">bundle路径</param>
        public void UnloadBundle(string path, bool immediately = false)
        {
            BundlePackInfo packInfo = GetPackInfo(path);
            if (packInfo != null)
            {
                BundleService.GetInstance().Unload(packInfo, immediately);
            }
        }

        public delegate void BundleLoadedOneDelegate(ObjList<string> resources, bool succeed);

        public int BundleCount(ObjList<string> resourceList)
        {
            ResPackConfig config = ResService.GetInstance().PackConfig;
            if (config == null)
            {
                return 0;
            }

            ObjList<BundlePackInfo> bundles = GetBundleList(resourceList);
            if (bundles == null)
            {
                return 0;
            }

            int count = 0;
            for (int i = 0; i < bundles.Count; i++)
            {
                // 非NoBundle资源，或者在磁盘上有这个bundle
                BundlePackInfo bpi = bundles[i];
                if (!bpi.IsNoBundle())
                {
                    count++;
                }
            }

            return count;
        }

        ObjList<BundlePackInfo> GetBundleList(ObjList<string> resources)
        {
            ResPackConfig config = ResService.GetInstance().PackConfig;
            if (config == null)
            {
                return null;
            }

            ObjList<BundlePackInfo> bundles = null;
            for (int i = 0; i < resources.Count; i++)
            {
                string path = resources[i];
                if (ResService.GetInstance().Exists(path))
                {
                    continue;
                }

                BundlePackInfo bpi = config.GetPackInfoForResource(path) as BundlePackInfo;
                if (bpi != null)
                {
                    if (bundles == null)
                    {
                        bundles = new ObjList<BundlePackInfo>();
                    }

                    if (!bundles.Contains(bpi))
                    {
                        bundles.Add(bpi);
                    }
                }
            }

            return bundles;
        }

        public void LoadBundleSync(ObjList<string> resourceList)
        {
            ObjList<BundlePackInfo> bundles = GetBundleList(resourceList);
            if (bundles == null || bundles.Count == 0)
            {
                return;
            }

            BundleService.GetInstance().LoadSync(bundles[0]);
        }

        /// <summary>
        /// 加载状态相关bundle
        /// </summary>
        /// <param name="resourceList">资源路径列表</param>
        /// <param name="complete">加载完成回调</param>
        /// <param name="progress">加载进度回调</param>
        /// <param name="loaded">单个bundle加载完成回调</param>
        /// <param name="singleSync">true:当只有一个bundle时，使用同步加载；否则使用并发异步加载</param>
        /// <returns></returns>
        public IEnumerator LoadBundle(ObjList<string> resourceList, Action complete = null, BundleBatchLoadingDelegate progress = null, BundleLoadedOneDelegate loaded = null, bool singleSync = true)
        {
            bool record = loaded != null;

            //删除已有资源
            for (int i = 0; i < resourceList.Count;)
            {
                string path = resourceList[i];
                if (ResService.GetInstance().Exists(path))
                {
                    if (record)
                    {
                        _unbundleResources.Add(path);
                    }

                    resourceList.RemoveAt(i);
                    continue;
                }
                i++;
            }

            //bundle list
            ObjList<BundlePackInfo> bundleList = GetBundlePackInfoListForResources(resourceList, record);

            //异步返回，在处理完源数据之后避免resourceList被reset
            yield return null;

            //未打包资源
            if (_unbundleResources.Count > 0)
            {
                if (loaded != null)
                {
                    loaded(_unbundleResources, true);
                }

                _unbundleResources.Clear();
            }

            //load
            if (bundleList != null && bundleList.Count > 0)
            {
                if (singleSync && bundleList.Count == 1)
                {
                    //只有一个bundle，使用同步加载方式
                    BundleService.GetInstance().LoadSync(bundleList[0]);

                    if (progress != null)
                    {
                        progress(1f);
                    }

                    if (loaded != null)
                    {
                        AssetBundle bundle = BundleService.GetInstance().GetBundle(bundleList[0].Path);
                        ObjList<string> relatedResources = GetLoadedBundleResources(bundleList[0].Path);
                        if (relatedResources != null && relatedResources.Count > 0)
                        {
                            loaded(relatedResources, bundle != null);
                            relatedResources.Clear();
                        }

                        BundleService.GetInstance().Unload(bundleList[0]);
                    }

                    if (complete != null)
                    {
                        complete();
                    }
                }
                else
                {
                    //多个bundle，使用并发加载
                    yield return StartCoroutine(BundleService.GetInstance().BatchLoadAsync(bundleList, delegate ()
                     {
                         if (complete != null)
                         {
                             complete();
                         }
                     }, progress, delegate (BundleRef bundle)
                      {
                         if (bundle != null)
                         {
                             if (loaded != null)
                             {
                                 ObjList<string> relatedResources = GetLoadedBundleResources(bundle.Path);
                                 if (relatedResources != null && relatedResources.Count > 0)
                                 {
                                     loaded(relatedResources, true);
                                     relatedResources.Clear();
                                 }

                                 BundleService.GetInstance().Unload(bundle.PackInfo);
                             }
                         }
                     }, delegate (BundlePackInfo packInfo, string error)
                     {
                         if (loaded != null)
                         {
                             ObjList<string> relatedResources = GetLoadedBundleResources(packInfo.Path);
                             if (relatedResources != null && relatedResources.Count > 0)
                             {
                                 bool succ = (packInfo.IsNoBundle());

                                 loaded(relatedResources, succ);
                                 relatedResources.Clear();
                             }
                         }
                     }));
                }
            }
            else
            {
                complete?.Invoke();
            }
        }

        /// <summary>
        /// 加载资源相关bundle
        /// </summary>
        /// <param name="resourcesPath">资源路径列表</param>
        /// <param name="loaded">单个资源加载完成</param>
        public void LoadBundle(ObjList<string> resourcesPath, BundleLoadedOneDelegate loaded = null)
        {
            StartCoroutine(LoadBundle(resourcesPath, null, null, loaded, false));
        }

        /// <summary>
        /// 加载bundle
        /// </summary>
        /// <param name="path">bundle路径</param>
        /// <param name="complete"></param>
        public void LoadBundle(string path, Action<string, bool> complete)
        {
            BundlePackInfo p = GetPackInfo(path);
            if (p == null)
            {
                if (complete != null)
                {
                    complete(path, false);
                }

                return;
            }

            StartCoroutine(BundleService.GetInstance().LoadAsync(p, delegate (BundleRef bundle)
             {
                 if (bundle != null && complete != null)
                 {
                     complete(bundle.Path, true);
                 }
             }, delegate (BundlePackInfo packInfo, string error)
             {
                 if (packInfo != null && complete != null)
                 {
                     complete(packInfo.Path, false);
                 }
             }, null));
        }

        /// <summary>
        /// 加载常驻bundle
        /// </summary>
        /// <param name="complete"></param>
        /// <param name="progress"></param>
        /// <returns></returns>
        public IEnumerator LoadResidentBundles(Action complete = null, BundleBatchLoadingDelegate progress = null)
        {
            ResPackConfig config = ResService.GetInstance().PackConfig;
            if (config == null)
            {
                GGLog.LogE("In LoadResidentBundles(), but ResPackConfig is null.");

                if (complete != null)
                {
                    complete();
                }

                yield break;
            }

            ObjList<BundlePackInfo> bundleList = new ObjList<BundlePackInfo>();
            for (int i = 0; i < config.PackInfo.Count; i++)
            {
                BundlePackInfo p = config.PackInfo[i] as BundlePackInfo;
                if (p != null && p.Life == EBundleLife.Resident)
                {
                    bundleList.Add(p);
                }
            }

            yield return StartCoroutine(BundleService.GetInstance().BatchLoadAsync(bundleList, complete, progress));
        }

        /// <summary>
        /// 获取预加载bundle列表
        /// </summary>
        /// <returns></returns>
        public ObjList<string> GetPreloadBundles()
        {
            ObjList<string> bundleList = new ObjList<string>();

            ResPackConfig config = ResService.GetInstance().PackConfig;
            for (int i = 0; i < config.PackInfo.Count; i++)
            {
                BundlePackInfo p = config.PackInfo[i] as BundlePackInfo;
                if (p == null)
                {
                    continue;
                }
                
                if (p.HasFlag(EBundleFlag.PreLoad))
                {
                    bundleList.Add(p.Path);
                }
            }

            return bundleList;
        }

        /// <summary>
        /// 获取已加载的bundle对应的资源
        /// </summary>
        /// <param name="bundlePath"></param>
        /// <returns></returns>
        public ObjList<string> GetLoadedBundleResources(string bundlePath)
        {
            if (string.IsNullOrEmpty(bundlePath) || _loadingResources.Count == 0)
            {
                return null;
            }

            ResPackConfig config = ResService.GetInstance().PackConfig;
            if (config == null)
            {
                return null;
            }

            BundlePackInfo pi = config.GetPackInfo(bundlePath) as BundlePackInfo;
            if (pi == null)
            {
                return null;
            }

            for (int i = _loadingResources.Count - 1; i >= 0; i--)
            {
                string path = _loadingResources[i];
                if (pi.Contains(path))
                {
                    if (!_relatedResources.Contains(path))
                    {
                        _relatedResources.Add(path);
                    }

                    _loadingResources.Remove(path);
                }
            }

            return _relatedResources;
        }

        /// <summary>
        /// 根据资源列表取bundle列表
        /// </summary>
        /// <param name="resourceList"></param>
        /// <param name="recordResources"></param>
        public ObjList<BundlePackInfo> GetBundlePackInfoListForResources(ObjList<string> resourceList, bool recordResources)
        {
            if (resourceList == null || resourceList.Count == 0)
            {
                return null;
            }

            ResPackConfig config = ResService.GetInstance().PackConfig;
            if (config == null)
            {
                if (recordResources)
                {
                    _unbundleResources.AddRange(resourceList);
                }

                return null;
            }

            ObjList<BundlePackInfo> bundleList = null;
            for (int i = 0; i < resourceList.Count; i++)
            {
                string path = resourceList[i];
                BundlePackInfo info = config.GetPackInfoForResource(Engine.Res.FileUtil.EraseExtension(path)) as BundlePackInfo;
                if (info != null)
                {
                    if (bundleList == null)
                    {
                        bundleList = new ObjList<BundlePackInfo>();
                    }

                    if (!bundleList.Contains(info))
                    {
                        bundleList.Add(info);
                    }

                    // bundle正在加载中的资源
                    if (recordResources && !_loadingResources.Contains(path))
                    {
                        _loadingResources.Add(path);
                    }
                }
                else
                {
                    if (recordResources && !_unbundleResources.Contains(path))
                    {
                        _unbundleResources.Add(path);
                    }
                }
            }

            return bundleList;
        }

        /// <summary>
        /// 获取bundle打包信息
        /// </summary>
        /// <param name="path">bundle路径</param>
        /// <returns></returns>
        BundlePackInfo GetPackInfo(string path)
        {
            ResPackConfig config = ResService.GetInstance().PackConfig;
            if (config == null)
            {
                return null;
            }
            return config.GetPackInfo(path) as BundlePackInfo;
        }
    }
}