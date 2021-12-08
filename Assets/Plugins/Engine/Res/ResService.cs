/********************************************************************
  created:  2020-05-20         
  author:    OneJun           

  purpose:  统一的资源服务           
*********************************************************************/
using System;
using Engine.Base;
using Engine.Res;
using UnityEngine;

namespace Engine.Res
{
    public enum ClearCacheResult
    {
        Suc = 1,
        FailClearUnityCache = 2,
        FailClearResCache = 3,
        FailClearAllCache = 4,
    }

    /// <summary>
    /// 全局游戏 资源管理器
    /// </summary>
    public class ResService : MonoSingleton<ResService>
    {
        //相关回调
        public delegate void OnResourceLoaded(ResObj ResObj);
        public delegate void OnResourcesLoaded(ObjList<ResObj> resourceList);

        //资源包的配置信息
        ResPackConfig _packConfig;
        public ResPackConfig PackConfig
        {
            get
            {
                return _packConfig;
            }
        }
        //资源缓存
        private ResCache _resCache;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns></returns>
        public override bool Initialize()
        {
            _resCache = new ResCache();
            _packConfig = null;
            return true;
        }

        private bool _useResPack = false;

        /// <summary>
        /// 配置是否打包
        /// </summary>
        /// <param name="resPack"></param>
        public void SetConfig(bool resPack)
        {
            _useResPack = resPack;
        }
        /// <summary>
        /// 配置是否打包
        /// </summary>
        public override void Uninitialize()
        {
            StopAllCoroutines();
            _resCache = null;
            _packConfig = null;
        }

        /// <summary>
        /// 加载资源包打包信息
        /// </summary>
        /// <param name="filebytes"></param>
        public void LoadResPackConfig(byte[] filebytes)
        {
            if(_packConfig == null)
            {
                _packConfig = new ResPackConfig();
            }
            else
            {
                _packConfig.ReloadDataBytes(filebytes);
            }
        }

        /// <summary>
        /// 合并配置
        /// </summary>
        /// <param name="fileName"></param>
        public void MergeResPackConfig(string fileName)
        {
            if (FileUtil.IsExistInIFSExtraFolder(fileName))
            {
                string resPackFile = FileUtil.CombinePath(FileUtil.GetIFSExtractPath(), fileName);
                byte[] bbs = FileUtil.ReadFile(resPackFile);
                ResPackConfig otherCfg = new ResPackConfig(bbs,true);
                if (_packConfig!=null)
                {
                    _packConfig.MergeWithOther(otherCfg);
                }
                bbs = null;
            }
            else
            {
                GGLog.LogE("ResService MergeResPackConfig No File Exist:" + fileName);
            }
        }

        /// <summary>
        /// GetResource的静态封装 同步获取
        /// </summary>
        /// <param name="fullPathInResources">资源在Resources目录下的相对路径</param>
        /// <returns></returns>
        public static ResObj GetResource(string fullPathInResources)
        {
            return GetInstance()._GetResource(fullPathInResources);
        }

        /// <summary>
        /// 同步方式获取资源，如果是打包资源自动加载对应bundle
        /// </summary>
        /// <param name="fullPathInResources">资源在Resources目录下的相对路径</param>
        /// <returns></returns>
        private ResObj _GetResource(string fullPathInResources)
        {
            if (string.IsNullOrEmpty(fullPathInResources)) 
            {
                return null;
            }
            ResObj resource = _resCache.Add(fullPathInResources);
            if (null == resource.Content)
            {
                LoadResource(resource);
                if (resource.Content == null)
                {
                    GGLog.LogE("Load resource failed, path:{0}", fullPathInResources);
                    _resCache.Remove(resource, true);
                    return null;
                }
            }
            return resource;
        }

        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns></returns>
        public ResObjRequest GetResourceAsync(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }
            // request
            ResObjRequest rr = new ResObjRequest();
            // complete loading
            Action<ResObj> complete = (ResObj r) =>
            {
                rr.isDone = true;
                rr.resource = r;
            };

            // loading progress
            Action<float> progress = (float prog) =>
            {
                rr.progress = prog;
            };

            // cache
            ResObj resource = _resCache.Add(path);
            if (resource.Content != null)
            {
                //cache包含直接返回
                complete(resource);
                return rr;
            }
            
            // real loading
            ResPackInfo packInfo = GetResPackInfo(resource);
            if (packInfo != null)
            {
                //打包的资源AssetBundle
                if (packInfo.GetPackType() == (byte)ResPackType.ResPackTypeBundle)
                {
                    //AssetBundle 信息
                    BundlePackInfo pi = packInfo as BundlePackInfo;
                    //补充扩展名
                    if (string.IsNullOrEmpty(resource.Ext))
                    {
                        resource.Ext = pi.GetResExt(resource.Path);
                    }

                    // AssetBundle 缓存
                    AssetBundle bundle = BundleService.GetInstance().GetBundle(packInfo.Path);
                    if (bundle == null)
                    {
                        //不在外且跟随Resource出档了
                        if (!pi.Outside && pi.IsNoBundle())
                        {
                            // in Resources
                            StartCoroutine(resource.LoadAsync(complete, progress));
                        }
                        else
                        {
                            // load bundle first
                            StartCoroutine(BundleService.GetInstance().LoadAsync(pi, delegate (BundleRef bundleRef)
                            {
                                if (bundleRef != null && bundleRef.Bundle != null)
                                {
                                    bundle = bundleRef.Bundle;
                                    //从asset bundle 加载
                                    StartCoroutine(resource.LoadAsync(bundle, complete, progress));
                                }
                                else
                                {
                                    GGLog.LogE("Async loading bundle failed, resource:{0}, bundle:{1}", path, packInfo.Path);
                                    complete(null);
                                    _resCache.Remove(resource, true);
                                }
                            }, delegate (BundlePackInfo failedPackInfo, string error)
                            {
                                GGLog.LogE("Load bundle failed, error:{0}", error);
                                complete(null);
                                _resCache.Remove(resource, true);
                            }));
                        }
                    }
                    else
                    {
                        // bundle is exists
                        // 增加引用计数
                        BundleService.GetInstance().LoadSync(pi);
                        // 加载
                        StartCoroutine(resource.LoadAsync(bundle, complete, progress));
                    }
                }
                else
                {
                    //二进制文件包异步获取
                    StartCoroutine(resource.LoadAsync(packInfo.Path, packInfo.IsOutside(resource.Path), complete, progress));
                }
            }
            else
            {
                // 非IFS 打包资源，从Resources目录异步读取
                //JWLog.LogD("Load From Resource Async:"+ path);
                StartCoroutine(resource.LoadAsync(complete, progress));
            }
            return rr;
        }

        /// <summary>
        /// 同步加载资源
        /// </summary>
        /// <param name="ResObj">资源实例</param>
        private void LoadResource(ResObj resource)
        {
            //找配置
            ResPackInfo packInfo = GetResPackInfo(resource);
            if (packInfo != null)
            {
                if (packInfo.GetPackType() == (byte)ResPackType.ResPackTypeBundle)
                {
                    BundlePackInfo pi = packInfo as BundlePackInfo;
                    //跟随Resource
                    if (!pi.Outside&& pi.IsNoBundle())
                    {
                        // load from Resources
                        resource.Load();
                    }
                    else
                    {
                        // load from bundle
                        // load bundle
                        BundleService.GetInstance().LoadSync(pi);
                        // load asset from bundle
                        AssetBundle bundle = BundleService.GetInstance().GetBundle(packInfo.Path);
                        if (bundle != null)
                        {
                            if (string.IsNullOrEmpty(resource.Ext))
                            {
                                resource.Ext = pi.GetResExt(resource.Path);
                            }
                            resource.Load(bundle);
                        }
                        else
                        {
                            GGLog.LogE("Loading bundle failed for path:{0}, bundle path:{1}", resource.Path, packInfo.Path);
                        }
                    }
                }
                else
                {
                    //二进制类型 直接根据文件路径 获取
                    resource.Load(packInfo.Path, packInfo.IsOutside(resource.Path));
                }
            }
            else
            {
                //不从属于任何资源包, 从Resources目录下读取 Editor模式 或者保护为未打包的
                resource.Load();
            }
        }

        /// <summary>
        /// 资源是否存在
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        public bool Exists(string resource)
        {
            return _resCache.Exists(resource);
        }

        /// <summary>
        /// UnloadResource的静态封装
        /// </summary>
        /// <param name="ResObj"></param>
        /// <param name="immediately">是否立即释放</param>
        public static void UnloadResource(ResObj ResObj, bool immediately = false)
        {
            GetInstance()._UnloadResource(ResObj, immediately);
        }

        /// <summary>
        /// 释放资源实例 一般二进制文件获取后 就需要卸载
        /// </summary>
        /// <param name="resource">资源实例</param>
        /// <param name="immediately">是否立即释放</param>
        private void _UnloadResource(ResObj resource, bool immediately)
        {
            if (resource == null)
            {
                return;
            }

            string path = resource.Path;
            if (_resCache.Remove(resource, immediately))
            {
                // 未使用cache资源，说明bundle已经被卸载了
                if (!resource.UsingCache)
                {
                    UnloadBundle(path);
                }
            }
        }

        /// <summary>
        /// 执行Resources.UnloadUnusedAssets,GC.Collect
        /// </summary>
        /// <param name="willUseAssets">这些资源在加载过程中或者即将加载，不能卸载他们的bundle</param>
        public void UnloadUnusedAssets(ObjList<string> willUseAssets = null)
        {
            _resCache.UnloadUnusedResources(willUseAssets);
            BundleService.GetInstance().UnloadUnusedBundles(willUseAssets);
            ExtResources.UnloadUnusedAssets();
            System.GC.Collect();
        }
        
        //卸载
        public void UnloadAllAssets()
        {
            BundleService.GetInstance().UnloadAll();
            UnloadUnusedAssets();
        }

        public bool ClearResCachePath()
        {
            string[] clearExtFiles = new string[] { ".json", ".res", ".bytes", ".txt" };
            string[] clearPaths = new string[] { FileUtil.IFSExtractFolder };
            string cachePath = FileUtil.GetCachePath();

            if (Application.isEditor)
            {
                FileUtil.ClearDirectory(cachePath, clearExtFiles, clearPaths);
                return true;
            }
            else
            {
                int tryCount = 0;
                while (true)
                {
                    bool isClearSuc = FileUtil.ClearDirectory(cachePath, clearExtFiles, clearPaths);
                    bool isDirExsit = true;
                    string notClearFile = "";

                    try
                    {
                        if (isClearSuc)
                        {
                            string resCachePath = FileUtil.GetIFSExtractPath();
                            isDirExsit = FileUtil.IsDirectoryExist(resCachePath);
                            if (!isDirExsit)
                            {
                                bool filesClear = true;
                                string[] files = System.IO.Directory.GetFiles(cachePath);
                                for (int i = 0; i < files.Length; i++)
                                {
                                    for (int j = 0; j < clearExtFiles.Length; j++)
                                    {
                                        if (files[i].Contains(clearExtFiles[j]))
                                        {
                                            notClearFile = files[i];
                                            filesClear = false;
                                            break;
                                        }
                                    }

                                    if (!filesClear)
                                        break;
                                }

                                if (filesClear)
                                    return true;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        GGLog.LogE("CleanCache Exception : {0}", ex.ToString());
                    }

                    ++tryCount;

                    if (tryCount >= 3)
                    {
                        GGLog.LogE("CleanCache Res Cache Failed isClearSuc : {0}, isDirExsit : {1}, notClearFile : {2}",
                            isClearSuc, isDirExsit, notClearFile);
                        return false;
                    }
                    else
                    {
                        GGLog.LogE("CleanCache Res Cache Failed [Have Chance : {0}] isClearSuc : {1}, isDirExsit : {2}, notClearFile : {3}",
                            tryCount, isClearSuc, isDirExsit, notClearFile);
                    }
                }
            }

        }

        /// <summary>
        /// 清理缓存目录
        /// </summary>
        /// <returns></returns>
        public ClearCacheResult ClearAllCachePath()
        {
            bool clearUnityCacheSuc = BundleService.CleanCache();
            bool clearResCacheSuc = ClearResCachePath();

            if (clearUnityCacheSuc && clearResCacheSuc)
                return ClearCacheResult.Suc;
            else
            {
                if (clearUnityCacheSuc)
                    return ClearCacheResult.FailClearResCache;
                else if (clearResCacheSuc)
                    return ClearCacheResult.FailClearUnityCache;                
            }

            return ClearCacheResult.FailClearAllCache;
        }

        //判断
        public bool IsBinaryResourceExsitInPkg(string Path)
        {
            string noExtPath = FileUtil.EraseExtension(Path);
            ResPackInfo packInfo = GetResPackInfo(noExtPath);
            if (packInfo != null)
            {
                if (packInfo.GetPackType() == (byte)ResPackType.ResPackTypeBinary)
                {
                    string realPath = FileUtil.CombinePath(packInfo.Path, Path);

                    if (FileUtil.IsExistInIFSExtraFolder(realPath) || FileUtil.IsFileExistInStreamingAssets(realPath))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        
        ResPackInfo GetResPackInfo(ResObj res)
        {
            if (null == _packConfig)
            {
                return null;
            }

            return _packConfig.GetPackInfoForResource(res.Path);
        }

        ResPackInfo GetResPackInfo(string path)
        {
            if (null == _packConfig)
            {
                return null;
            }

            return _packConfig.GetPackInfoForResource(path);
        }

        /// <summary>
        /// 判断Resource下面某个资源存在与否
        /// </summary>
        /// <param name="resourceKey">相对Resource的路径</param>
        /// <returns></returns>
        public bool CheckResourceExist(string resourceKey)
        {
            if(Application.isEditor && _useResPack == false)
            {
                string resourceFullPath = string.Format("{0}/Resources/{1}{2}", Application.dataPath, resourceKey, ".prefab");
                return FileUtil.IsFileExist(resourceFullPath);
            }
            else
            {
                if (_packConfig == null)
                {
                    return false;
                }
                return _packConfig.GetPackInfoForResource(resourceKey) != null;
            }
        }

        /// <summary>
        /// 卸载资源所属bundle
        /// </summary>
        /// <param name="ResObj">资源实例</param>
        private void UnloadBundle(string path)
        {
            BundlePackInfo info = GetResPackInfo(path) as BundlePackInfo;
            if (info != null)
            {
                BundleService.GetInstance().Unload(info);
            }
        }
    };
}