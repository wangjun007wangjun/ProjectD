/********************************************************************
  created:  2020-03-31         
  author:    OneJun  

  purpose:  单一资源定义                
*********************************************************************/
using System;
using System.Collections;
using Engine.Base;
using Engine.Res;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

namespace Engine.Res
{
    public class ResObj
    {
        /// <summary>
        /// 引用计数
        /// </summary>
        public int RefCnt;

        /// <summary>
        /// 正在缓存资源
        /// </summary>
        public bool UsingCache;

        /// <summary>
        /// 路径，不带扩展名
        /// </summary>
        public string Path;

        /// <summary>
        /// 原始路径
        /// </summary>
        public string OriginPath;

        /// <summary>
        /// 资源文件名
        /// </summary>
        public string Name;

        /// <summary>
        /// 资源扩展名
        /// </summary>
        public string Ext;

        /// <summary>
        /// 资源内容 or BinaryObject
        /// </summary>
        Object _content;
        public Object Content
        {
            get
            {
                return _content;
            }
        }

        /// <summary>
        /// 重置属性
        /// </summary>
        /// <param name="path"></param>
        void Reset(string path)
        {
            RefCnt = 1;
            UsingCache = false;

            OriginPath = path;
            Path = FileUtil.EraseExtension(path);

            Name = FileUtil.EraseExtension(FileUtil.GetFullName(path));
            Ext = FileUtil.GetExtension(path);

            _content = null;
        }


        ///构造函数
        ResObj(string path)
        {
            Reset(path);
        }

        //Resource 方式读取
        #region Resources 方式读取

        /// <summary>
        /// 加载
        /// </summary>
        public void Load()
        {
            if (Application.isEditor)
            {
                //编辑器模式 二进制文件 //失败从StreamingAsset读取
                if (!string.IsNullOrEmpty(Ext))
                {
                    Load(string.Format("{0}/Resources", Application.dataPath), "");
                    if (_content == null)
                    {
                        if (FileUtil.IsFileExistInStreamingAssets(OriginPath))
                        {
                            byte[] data = FileUtil.ReadFileInStreamingAssets(OriginPath);
                            if (data != null)
                            {
                                OnBinaryLoaded(data);
                            }
                        }
                    }
                }
                else
                {
                    _content = ExtResources.Load(Path);
                    if (_content == null)
                    {
                        GGLog.LogE("Resource.Load Fail:" + Path);
                    }
                    else
                    {
                        PostProcessContent();
                    }
                }
            }
            else
            {
                _content = ExtResources.Load(Path);
                if (_content == null)
                {
                    GGLog.LogE("Resource.Load Fail:" + Path);
                }
                else
                {
                    PostProcessContent();
                }
            }
        }

        public IEnumerator LoadAsync(Action<ResObj> complete = null, Action<float> progress = null)
        {
            if (Application.isEditor)
            {
                if (!string.IsNullOrEmpty(Ext))
                {
                    string pp = FileUtil.CombinePaths("file://", Application.dataPath);
                    yield return LoadAsync(pp, "", complete, progress);
                }
                else
                {
                    ResourceRequest request = ExtResources.LoadAsync(Path);
                    yield return request;

                    if (request != null)
                    {
                        _content = request.asset;
                        PostProcessContent();
                    }

                    if (complete != null)
                    {
                        complete(this);
                    }
                }
            }
            else
            {
                ResourceRequest request = ExtResources.LoadAsync(Path);
                yield return request;

                if (request != null)
                {
                    _content = request.asset;
                    PostProcessContent();
                }

                if (complete != null)
                {
                    complete(this);
                }
            }
        }
        #endregion

        #region IFS or StreamingAssets 目录读取单一资源

        /// <summary>
        /// 从IFS或StreamingAssets目录同步加载资源
        /// </summary>
        public void Load(string pkgPath, bool outside)
        {
            if (outside)
            {
                Load(FileUtil.GetIFSExtractPath(), pkgPath);
            }
            else
            {
                // load from StreamingAssets
                if (Application.platform == RuntimePlatform.Android && Application.isEditor == false)
                {
                    string realPath = FileUtil.CombinePath(pkgPath, OriginPath);
                    byte[] data = FileUtil.ReadFileInStreamingAssets(realPath);
                    if (data != null)
                    {
                        OnBinaryLoaded(data);
                    }
                }
                else
                {
                    Load(FileUtil.GetStreamingAssetsPath(), pkgPath);
                }
            }
        }

        /// <summary>
        /// 从IFS或StreamingAssets目录异步加载资源
        /// </summary>
        /// <param name="complete"></param>
        /// <param name="progress"></param>
        /// <returns></returns>
        public IEnumerator LoadAsync(string pkgPath, bool outside, Action<ResObj> complete = null, Action<float> progress = null)
        {
            string realPath = FileUtil.CombinePath(pkgPath, OriginPath);
            if (outside)
            {
                yield return LoadAsync(FileUtil.GetIFSExtractPathWithHeader(), pkgPath, complete, progress);
            }
            else
            {
                yield return LoadAsync(FileUtil.GetStreamingAssetsPathWithHeader(""), pkgPath, complete, progress);
            }
        }

        #endregion

        /// <summary>
        /// 从指定目录加载二进制资源
        /// </summary>
        /// <param name="directory">目录</param>
        void Load(string directory, string pkgPath)
        {
            string fullPath = FileUtil.CombinePath(directory, OriginPath);
            byte[] data = FileUtil.ReadFile(fullPath);
            if (data != null)
            {
                OnBinaryLoaded(data);
            }
        }

        /// <summary>
        /// 从指定目录异步加载资源
        /// </summary>
        /// <param name="directory">目录，必须带header，因为要使用www加载</param>
        /// <returns></returns>
        IEnumerator LoadAsync(string directory, string pkgPath, Action<ResObj> complete, Action<float> progress = null)
        {
            UnityWebRequest www = new UnityWebRequest(FileUtil.CombinePaths(directory, pkgPath, OriginPath));
            yield return www.SendWebRequest();
            while (!www.isDone)
            {
                if (progress != null)
                {
                    progress(www.downloadProgress);
                }

                yield return null;
            }

            if (www != null && www.isDone && !string.IsNullOrEmpty(www.error))
            {
                OnBinaryLoaded(www.downloadHandler.data);
            }

            if (complete != null)
            {
                complete(this);
            }
            if (www != null)
            {
                www.Dispose();
                www = null;
            }
        }

        #region AssetBundle 内部读取

        /// <summary>
        /// 从AssetBundle 同步加载资源
        /// </summary>
        /// <param name="assetBundle"></param>
        public void Load(AssetBundle assetBundle)
        {
            try
            {
                //Ext为prefab U5 AssetBundle存储的路径相对了Project
                if (string.IsNullOrEmpty(Ext))
                {
                    Ext = ".prefab";
                }
                string u5path = "Assets/Resources/" + Path + Ext;
                //
                _content = assetBundle.LoadAsset(u5path);
                PostProcessContent();
                OnBundleAssetLoaded();
            }
            catch (Exception e)
            {
                GGLog.LogE("Load asset failed, path:{0}, error:{1}", Path, e.Message);
            }
        }

        /// <summary>
        /// 异步从AssetBundle加载资源
        /// </summary>
        /// <param name="assetBundle"></param>
        /// <returns></returns>
        public IEnumerator LoadAsync(AssetBundle assetBundle, Action<ResObj> complete, Action<float> progress = null)
        {
            string u5path = "Assets/Resources/" + Path + Ext;
            AssetBundleRequest request = assetBundle.LoadAssetAsync(u5path, typeof(Object));
            while (!request.isDone)
            {
                if (progress != null)
                {
                    progress(request.progress);
                }
                yield return null;
            }

            try
            {
                if (request != null && request.asset != null)
                {
                    _content = request.asset;
                    PostProcessContent();
                }

                OnBundleAssetLoaded();
            }
            catch (MissingReferenceException e)
            {
                // 加载过程中调用了AssetBundle.Unload()
                GGLog.LogE("Bundle is unloaded when loading asset asynchronous. exception:{0}", e.ToString());
            }
            finally
            {
                if (complete != null)
                {
                    complete(this);
                }
            }
        }

        /// <summary>
        /// bundle中资源加载完成
        /// </summary>
        void OnBundleAssetLoaded()
        {
            BundleService.GetInstance().OnAssetLoaded(Path);
        }

        #endregion

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Unload()
        {
            BinaryObject binaryObject = _content as BinaryObject;
            if (binaryObject != null)
            {
                binaryObject.m_data = null;
            }
            _content = null;
        }

        /// <summary>
        /// 二进制内容加载完成
        /// </summary>
        /// <param name="data"></param>
        void OnBinaryLoaded(byte[] data)
        {
            BinaryObject binaryObject = ScriptableObject.CreateInstance<BinaryObject>();
            binaryObject.m_data = data;
            binaryObject.name = Name;

            _content = binaryObject;
        }

        /// <summary>
        /// 加载后处理Content
        /// </summary>
        void PostProcessContent()
        {
            //TextAsset需要转换为CBinaryObject，方便统一从各种位置读取资源
            if (_content != null && _content.GetType() == typeof(TextAsset))
            {
                BinaryObject binaryObject = ScriptableObject.CreateInstance<BinaryObject>();
                TextAsset textAsset = _content as TextAsset;
                if (null != textAsset)
                {
                    binaryObject.m_data = textAsset.bytes;
                }
                _content = binaryObject;
            }
        }

        /// <summary>
        /// 根据后缀取资源类型
        /// </summary>
        /// <returns></returns>
        Type GetContentType()
        {
            Type contentType = null;

            if (string.Equals(Ext, ".prefab", StringComparison.OrdinalIgnoreCase))
            {
                contentType = typeof(GameObject);
            }
            else if (string.Equals(Ext, ".bytes", StringComparison.OrdinalIgnoreCase)
                || string.Equals(Ext, ".xml", StringComparison.OrdinalIgnoreCase))
            {
                contentType = typeof(TextAsset);
            }
            else if (string.Equals(Ext, ".asset", StringComparison.OrdinalIgnoreCase))
            {
                contentType = typeof(ScriptableObject);
            }
            else if (string.Equals(Ext, ".png", StringComparison.OrdinalIgnoreCase)
                || string.Equals(Ext, ".tga", StringComparison.OrdinalIgnoreCase))
            {
                contentType = typeof(Texture);
            }
            else if (string.Equals(Ext, ".mp3", StringComparison.OrdinalIgnoreCase))
            {
                contentType = typeof(AudioClip);
            }

            return contentType;
        }

        #region Pool化

        // ResObj对象池
        static ObjList<ResObj> _pool = new ObjList<ResObj>(5);

        // 取实例
        public static ResObj Get(string path)
        {
            ResObj r = null;
            if (_pool.Count > 0)
            {
                r = _pool[_pool.Count - 1];
                r.Reset(path);

                _pool.RemoveAt(_pool.Count - 1);
            }
            else
            {
                r = new ResObj(path);
            }

            return r;
        }

        /// <summary>
        /// 回收
        /// </summary>
        /// <param name="r"></param>
        public static void Recycle(ResObj r)
        {
            if (r != null)
            {
                r.Reset(null);

                if (!_pool.Contains(r))
                {
                    _pool.Add(r);
                }
            }
        }

        #endregion
    }


    //资源异步加载request 定义
    public class ResObjRequest
    {
        // 是否完成
        // true: 加载完成，或加载错误
        // false: 加载中
        public bool isDone;

        // 加载进度：0 ~ 1f
        public float progress;

        // 资源对象
        public ResObj resource;
    }
}