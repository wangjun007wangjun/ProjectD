/********************************************************************
  created:  2020-05-18         
  author:    OneJun           

  purpose:  资产同步异步加载器                
*********************************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using Engine.Asset;
using Engine.Base;
using Engine.Res;
using UnityEngine;

namespace Engine.Asset
{
    public class AssetLoader : MonoBehaviour
    {
        private static readonly object InstructionEnd = new object();
        private const int LoadStateLoading = 0;
        private const int LoadStateSuccess = 1;
        private const int LoadStateFail = 2;

        private struct TaskData
        {
            public AssetData Data;
            public int LoadBundleState;
            public ResObjRequest Request;
        }

        private AssetHolder _assetManager;
        private ArrayList<AssetData> _data;
        private int _loadAssetBundlePriority;
        private ArrayList<TaskData> _resourceRequesting;
        private ResObjRequest _resourceDiscardRequest;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="assetManager">资产管理器</param>
        public void Initialize(AssetHolder assetManager)
        {
            _assetManager = assetManager;
            _data = new ArrayList<AssetData>();
            _resourceRequesting = new ArrayList<TaskData>();
            StartCoroutine(AsynchronousLoad());
        }

        /// <summary>
        /// 反初始化
        /// </summary>
        public void Uninitialize()
        {
            StopAllCoroutines();

            _assetManager = null;
            _data.Clear();
            _data = null;

            _resourceRequesting = null;
            _resourceDiscardRequest = null;
        }
        public void AddTask(ref AssetData assetData)
        {
            //非阻塞
            if (assetData.Priority != LoadPriority.Wait)
            {
                //进行中 重复
                for (int i = 0; i < _resourceRequesting.Count; i++)
                {
                    TaskData loadData = _resourceRequesting[i];
                    if (assetData.Type == loadData.Data.Type &&
                        assetData.Name.Equals(loadData.Data.Name, StringComparison.OrdinalIgnoreCase) &&
                        (assetData.Callback == loadData.Data.Callback))
                    {
                        loadData.Data = assetData;
                        _resourceRequesting.RemoveAt(i);

                        bool insert = false;
                        for (int j = _resourceRequesting.Count - 1; j >= 0; j--)
                        {
                            if (_resourceRequesting[j].Data.Priority <= loadData.Data.Priority)
                            {
                                _resourceRequesting.Insert(j + 1, loadData);
                                insert = true;
                                break;
                            }
                        }
                        if (!insert)
                        {
                            _resourceRequesting.Insert(0, loadData);
                        }
                        return;
                    }
                }
            }

            //重复？
            for (int i = 0; i < _data.Count; i++)
            {
                AssetData data = _data[i];
                if (assetData.Callback == data.Callback &&
                    assetData.Type == data.Type &&
                    assetData.Name.Equals(data.Name, StringComparison.OrdinalIgnoreCase))
                {
                    _data.RemoveAt(i);
                    break;
                }
            }

            for (int i = _data.Count - 1; i >= 0; --i)
            {
                //插入前
                if (_data[i].Priority <= assetData.Priority)
                {
                    _data.Insert(i + 1, assetData);
                    return;
                }
            }
            _data.Insert(0, assetData);
        }

        public void CancelTask(IAssetLoadCallback callback, string assetName)
        {
            if (callback == null)
            {
                GGLog.LogE("Loader.CancelTask : invalid parameter");
                return;
            }

            for (int i = _resourceRequesting.Count - 1; i >= 0; --i)
            {
                TaskData loadData = _resourceRequesting[i];
                if (loadData.Data.Callback != callback ||
                    (!string.IsNullOrEmpty(assetName) && !loadData.Data.Name.Equals(assetName, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                if (loadData.Request != null)
                {
                    if (_resourceDiscardRequest != null)
                    {
                        GGLog.LogE("Loader.ClearTask : _resourceDiscardRequest != null");
                    }

                    _resourceDiscardRequest = loadData.Request;
                }

                _resourceRequesting.RemoveAt(i);
            }

            for (int i = _data.Count - 1; i >= 0; --i)
            {
                AssetData data = _data[i];
                if (data.Callback == callback &&
                    (string.IsNullOrEmpty(assetName) || data.Name.Equals(assetName, StringComparison.OrdinalIgnoreCase)))
                {
                    _data.RemoveAt(i);
                }
            }
        }
        #region Work
        private IEnumerator AsynchronousLoad()
        {
            ObjList<string> stringList = new ObjList<string>();
            ArrayList<AssetData> assetDataList = new ArrayList<AssetData>();

            IEnumerator loadBundleEnumerator = AsynchronousLoad_LoadAssetBundle(stringList, assetDataList);
            IEnumerator loadResourceEnumerator = AsynchronousLoad_LoadResource();
            IEnumerator instantiateEnumerator = AsynchronousLoad_InstantiateResource();
            if (loadBundleEnumerator == null || loadResourceEnumerator == null || instantiateEnumerator == null)
            {
                yield break;
            }

            while (true)
            {
                yield return null;
                while (_resourceDiscardRequest != null)
                {
                    if (!_resourceDiscardRequest.isDone)
                    {
                        yield return null;
                        continue;
                    }
                    if (_resourceDiscardRequest.resource != null)
                    {
                        ResService.UnloadResource(_resourceDiscardRequest.resource);
                    }
                    _resourceDiscardRequest = null;
                    yield return null;
                }

                if (_resourceRequesting.Count > 0)
                {
                    while (instantiateEnumerator.MoveNext())
                    {
                        if (instantiateEnumerator.Current == InstructionEnd)
                        {
                            break;
                        }
                        yield return null;
                    }
                }

                if (_resourceRequesting.Count > 0)
                {
                    while (loadResourceEnumerator.MoveNext())
                    {
                        if (loadResourceEnumerator.Current == InstructionEnd)
                        {
                            break;
                        }
                        yield return null;
                    }
                }

                _loadAssetBundlePriority = AsynchronousLoad_CalculatePriority();
                if (_loadAssetBundlePriority != int.MaxValue &&
                    (_loadAssetBundlePriority < LoadPriority.Preprocess ||
                    (_resourceDiscardRequest == null && _resourceRequesting.Count == 0)))
                {
                    while (loadBundleEnumerator.MoveNext())
                    {
                        if (loadBundleEnumerator.Current == InstructionEnd)
                        {
                            break;
                        }
                        yield return null;
                    }
                }
            }
        }

        private int AsynchronousLoad_CalculatePriority()
        {
            int priority = int.MaxValue;
            for (int i = 0; i < _resourceRequesting.Count; i++)
            {
                TaskData loadData = _resourceRequesting[i];
                if (loadData.Data.Priority < priority)
                {
                    priority = loadData.Data.Priority;
                }
            }
            for (int i = 0; i < _data.Count; i++)
            {
                AssetData data = _data[i];
                if (data.Priority != LoadPriority.Wait && data.Priority < priority)
                {
                    priority = data.Priority;
                    break;
                }
            }
            return priority;
        }

        private IEnumerator AsynchronousLoad_LoadAssetBundle(ObjList<string> stringList, ArrayList<AssetData> assetDataList)
        {
            while (true)
            {
                stringList.Clear();
                assetDataList.Clear();
                for (int i = 0; i < _data.Count;)
                {
                    AssetData data = _data[i];
                    if (data.Priority != _loadAssetBundlePriority)
                    {
                        ++i;
                        continue;
                    }
                    _data.RemoveAt(i);
                    if (_assetManager.GetCacheCount(data.Name) >= data.Count)
                    {
                        if (data.Callback != null)
                        {
                            assetDataList.Add(data);
                        }
                        continue;
                    }
                    TaskData loadData;
                    loadData.Data = data;
                    loadData.LoadBundleState = LoadStateLoading;
                    loadData.Request = null;
                    bool insert = false;
                    for (int j = _resourceRequesting.Count - 1; j >= 0; --j)
                    {
                        if (_resourceRequesting[j].Data.Priority <= data.Priority)
                        {
                            _resourceRequesting.Insert(j + 1, loadData);
                            insert = true;
                            break;
                        }
                    }
                    if (!insert)
                    {
                        _resourceRequesting.Insert(0, loadData);
                    }
                    stringList.Add(data.Filename);
                    if (_loadAssetBundlePriority >= LoadPriority.Preprocess)
                    {
                        break;
                    }
                }
                yield return null;
                if (stringList.Count > 0)
                {
                    OnBundleLoadCompleted(stringList, true);
                }
                yield return null;
                for (int i = 0; i < assetDataList.Count; i++)
                {
                    AssetData data = assetDataList[i];
                    if (data.Callback != null)
                    {
                        data.Callback.OnLoadAssetCompleted(data.Name, AssetLoadResult.Success, null);
                        yield return null;
                    }
                }
                yield return InstructionEnd;
            }
        }

        private IEnumerator AsynchronousLoad_LoadResource()
        {
            while (true)
            {
                int find = 0;
                for (; find < _resourceRequesting.Count; find++)
                {
                    if (_resourceRequesting[find].LoadBundleState != LoadStateLoading)
                    {
                        break;
                    }
                }

                if (find >= _resourceRequesting.Count)
                {
                    yield return InstructionEnd;
                    continue;
                }

                TaskData loadData = _resourceRequesting[find];
                if (_assetManager.GetCacheCount(loadData.Data.Name) >= loadData.Data.Count)
                {
                    _resourceRequesting.RemoveAt(find);

                    if (loadData.Data.Callback != null)
                    {
                        loadData.Data.Callback.OnLoadAssetCompleted(loadData.Data.Name, AssetLoadResult.Success, null);
                        yield return null;
                    }
                    yield return InstructionEnd;
                    continue;
                }

                if (loadData.LoadBundleState == LoadStateFail)
                {
                    GGLog.LogE("Loader.AsynchronousLoad_Resource : load bundle failed - {0}", loadData.Data.Name);
                    _resourceRequesting.RemoveAt(find);
                    if (loadData.Data.Callback != null)
                    {
                        loadData.Data.Callback.OnLoadAssetCompleted(loadData.Data.Name, AssetLoadResult.BundleFail, null);
                        yield return null;
                    }
                    yield return InstructionEnd;
                    continue;
                }
                loadData.Request = ResService.GetInstance().GetResourceAsync(loadData.Data.Filename);
                _resourceRequesting[find] = loadData;
                yield return InstructionEnd;
            }
        }

        private IEnumerator AsynchronousLoad_InstantiateResource()
        {
            while (true)
            {
                int find = 0;
                for (; find < _resourceRequesting.Count; find++)
                {
                    if (_resourceRequesting[find].Request != null)
                    {
                        break;
                    }
                }
                if (find >= _resourceRequesting.Count)
                {
                    yield return InstructionEnd;
                    continue;
                }
                TaskData loadData = _resourceRequesting[find];
                if (!loadData.Request.isDone)
                {
                    yield return null;
                    continue;
                }
                _resourceRequesting.RemoveAt(find);
                if (loadData.Request.resource == null)
                {
                    GGLog.LogE("Loader.AsynchronousLoad_InstantiateResource : load failed - {0}", loadData.Data.Name);
                    if (loadData.Data.Callback != null)
                    {
                        loadData.Data.Callback.OnLoadAssetCompleted(loadData.Data.Name, AssetLoadResult.ResourceFail, null);
                        yield return null;
                    }
                    yield return InstructionEnd;
                    continue;
                }
                if (loadData.Request.resource.Content == null)
                {
                    GGLog.LogE("Loader.AsynchronousLoad_InstantiateResource : load failed - {0}", loadData.Data.Name);
                    ResService.UnloadResource(loadData.Request.resource);

                    if (loadData.Data.Callback != null)
                    {
                        loadData.Data.Callback.OnLoadAssetCompleted(loadData.Data.Name, AssetLoadResult.ResourceFail, null);
                        yield return null;
                    }
                    yield return InstructionEnd;
                    continue;
                }
                if (_assetManager.AddCache(loadData.Data, loadData.Request.resource) == null)
                {
                    GGLog.LogE("Loader.AsynchronousLoad_Resource : add cache failed - {0}", loadData.Data.Name);
                    ResService.UnloadResource(loadData.Request.resource);

                    if (loadData.Data.Callback != null)
                    {
                        loadData.Data.Callback.OnLoadAssetCompleted(loadData.Data.Name, AssetLoadResult.ResourceFail, null);
                        yield return null;
                    }
                    yield return InstructionEnd;
                    continue;
                }
                yield return null;
                //个数
                if (_assetManager.GetCacheCount(loadData.Data.Name) < loadData.Data.Count)
                {
                    loadData.Request.resource = ResService.GetResource(loadData.Data.Filename);
                    _resourceRequesting.Insert(find, loadData);

                    yield return InstructionEnd;
                    continue;
                }
                if (loadData.Data.Callback != null)
                {
                    loadData.Data.Callback.OnLoadAssetCompleted(loadData.Data.Name, AssetLoadResult.Success, null);
                    yield return null;
                }
                yield return InstructionEnd;
            }

        }

        private void OnBundleLoadCompleted(ObjList<string> resourcePath, bool succeed)
        {
            if (resourcePath == null || resourcePath.Count == 0)
            {
                GGLog.LogE("Loader.OnBundleLoadCompleted : invalid parameter");
                return;
            }
            for (int i = 0; i < _resourceRequesting.Count; i++)
            {
                TaskData loadData = _resourceRequesting[i];
                if (loadData.LoadBundleState != LoadStateLoading || !resourcePath.Contains(loadData.Data.Filename))
                {
                    continue;
                }
                loadData.LoadBundleState = succeed ? LoadStateSuccess : LoadStateFail;
                _resourceRequesting[i] = loadData;
            }
        }
        #endregion
    }
}
