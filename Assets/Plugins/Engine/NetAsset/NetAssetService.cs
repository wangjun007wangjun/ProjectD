using UnityEngine;
using System;
using Engine.Schedule;
using Engine.Base;
using System.Collections.Generic;

namespace Engine.NetAsset
{
    public class NetAssetService : Singleton<NetAssetService>, IScheduleHandler
    {
        private const int C_CachedNetAssetMaxAmount = 10000;
        public static string S_CachedNetAssetDirectory = Engine.Res.FileUtil.CombinePath(Engine.Res.FileUtil.GetCachePath(), "NetAssetCache");
        private static string S_CachedNetAssetInfoSetFileFullPath = Engine.Res.FileUtil.CombinePath(S_CachedNetAssetDirectory, "NetAssetCacheSet.bytes");
        private static byte[] S_Buffer = new byte[C_CachedNetAssetMaxAmount * 100];
        //
        private NetAssetInfoSet _cachedNetAssetInfoSet;

        //当前分配ID
        private uint _currentId = 1;
        //列表
        private readonly ArrayList<NetAssetSession> _prepareSessions = new ArrayList<NetAssetSession>(2);
        //
        private bool _isNeedSaveInfoSet = false;

        public override bool Initialize()
        {
            _cachedNetAssetInfoSet = new NetAssetInfoSet();
            //
            if (!Engine.Res.FileUtil.IsDirectoryExist(S_CachedNetAssetDirectory))
            {
                Engine.Res.FileUtil.CreateDirectory(S_CachedNetAssetDirectory);
            }
            //
            if (Engine.Res.FileUtil.IsFileExist(S_CachedNetAssetInfoSetFileFullPath))
            {
                byte[] data = Engine.Res.FileUtil.ReadFile(S_CachedNetAssetInfoSetFileFullPath);
                int offset = 0;
                _cachedNetAssetInfoSet.Read(data, ref offset);
            }
            this.AddTimer(120000, true);
            return true;
        }

        public override void Uninitialize()
        {
            if (_prepareSessions != null)
            {
                for (int i = 0; i < _prepareSessions.Count; i++)
                {
                    NetAssetSession session = _prepareSessions[i];
                    session.StopSession();
                    _prepareSessions[i] = session;
                }
                _prepareSessions.Clear();
            }
            this.RemoveTimer();
            if (_cachedNetAssetInfoSet != null)
            {
                //保存记录
                int offset = 0;
                _cachedNetAssetInfoSet.Write(S_Buffer, ref offset);
                if (!Engine.Res.FileUtil.IsFileExist(S_CachedNetAssetInfoSetFileFullPath))
                {
                    Engine.Res.FileUtil.WriteFile(S_CachedNetAssetInfoSetFileFullPath, S_Buffer, 0, offset);
                }
                _cachedNetAssetInfoSet = null;
            }
        }

        //---------------------------------------Image 相关-------------------------
        /// 获取Cached 图片纹理
        public Texture2D GetCachedNetImage(string url, float validDays, int width = 0, int height = 0)
        {
            string key = Engine.Res.FileUtil.GetMd5(url.ToLower());

            NetAssetInfo cachedInfo = _cachedNetAssetInfoSet.GetCachedNetAssetInfo(key);
            //验证是否过期
            if (cachedInfo == null || (DateTime.Now - cachedInfo.LastModifyTime).TotalDays >= validDays)
            {
                return null;
            }
            //
            string imageFilePath = Engine.Res.FileUtil.CombinePath(S_CachedNetAssetDirectory, key + ".bytes");
            if (!Engine.Res.FileUtil.IsFileExist(imageFilePath))
            {
                return null;
            }
            //
            byte[] data = Engine.Res.FileUtil.ReadFile(imageFilePath);
            if (data == null || data.Length <= 0)
            {
                return null;
            }
            else
            {
                int w = width;
                int h = height;
                if (width == 0)
                {
                    //使用记录的
                    w = cachedInfo.ImageWidth;
                    h = cachedInfo.ImageHeight;
                }
                if (w == 0)
                {
                    w = 64;
                    h = 64;
                }
                Texture2D texture2D = null;
                texture2D = new Texture2D(w, h, TextureFormat.RGBA32, false);
                texture2D.filterMode = FilterMode.Bilinear;
                texture2D.wrapMode = TextureWrapMode.Clamp;
                texture2D.LoadImage(data);
                return texture2D;
            }
        }

        /// <summary>
        /// 添加网络图片数据到缓存
        /// </summary>
        /// <param name="url"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="data"></param>
        public void AddCachedNetImage(string url, int width, int height, byte[] data)
        {
            string key = Engine.Res.FileUtil.GetMd5(url.ToLower());

            NetAssetInfo info = _cachedNetAssetInfoSet.GetCachedNetAssetInfo(key);
            if (info != null)
            {
                info.LastModifyTime = DateTime.Now;
            }
            else
            {
                int curCnt = _cachedNetAssetInfoSet.GetCachedNetAssetCnt();
                //添加
                if (curCnt >= C_CachedNetAssetMaxAmount)
                {
                    string removeKey = _cachedNetAssetInfoSet.RemoveEarliestNetAssetInfo();
                    if (!string.IsNullOrEmpty(removeKey))
                    {
                        string removePath = Engine.Res.FileUtil.CombinePath(S_CachedNetAssetDirectory, removeKey + ".bytes");
                        if (Engine.Res.FileUtil.IsFileExist(removePath))
                        {
                            Engine.Res.FileUtil.DeleteFile(removePath);
                        }
                    }
                }
                //
                NetAssetInfo newInfo = new NetAssetInfo();
                newInfo.Key = key;
                newInfo.AssetType = NetAssetType.Image;
                newInfo.ImageWidth = width;
                newInfo.ImageHeight = height;
                newInfo.LastModifyTime = DateTime.Now;
                _cachedNetAssetInfoSet.AddNetAssetInfo(key, newInfo);
            }

            _cachedNetAssetInfoSet.SortNetAssetInfo();

            //保存数据
            string filePath = Engine.Res.FileUtil.CombinePath(S_CachedNetAssetDirectory, key + ".bytes");
            if (Engine.Res.FileUtil.IsFileExist(filePath))
            {
                Engine.Res.FileUtil.DeleteFile(filePath);
            }
            Engine.Res.FileUtil.WriteFile(filePath, data);
            //记录保存
            _isNeedSaveInfoSet = true;
        }

        /// <summary>
        /// 直接添加信息
        /// </summary>
        /// <param name="key"></param>
        public void AddCachedNetAssetInfoDirect(string key)
        {
            NetAssetInfo info = _cachedNetAssetInfoSet.GetCachedNetAssetInfo(key);
            if (info != null)
            {
                info.LastModifyTime = DateTime.Now;
            }
            else
            {
                int curCnt = _cachedNetAssetInfoSet.GetCachedNetAssetCnt();
                //添加 挤掉旧的
                if (curCnt >= C_CachedNetAssetMaxAmount)
                {
                    string removeKey = _cachedNetAssetInfoSet.RemoveEarliestNetAssetInfo();
                    if (!string.IsNullOrEmpty(removeKey))
                    {
                        string removePath = Engine.Res.FileUtil.CombinePath(S_CachedNetAssetDirectory, removeKey + ".bytes");
                        if (Engine.Res.FileUtil.IsFileExist(removePath))
                        {
                            Engine.Res.FileUtil.DeleteFile(removePath);
                        }
                    }
                }
                NetAssetInfo newInfo = new NetAssetInfo();
                newInfo.Key = key;
                newInfo.LastModifyTime = DateTime.Now;
                _cachedNetAssetInfoSet.AddNetAssetInfo(key, newInfo);
            }
            _cachedNetAssetInfoSet.SortNetAssetInfo();
        }


        /// 获取一个记录
        public NetAssetInfo GetCachedNetAssetInfoByKey(string key)
        {
            return _cachedNetAssetInfoSet.GetCachedNetAssetInfo(key);
        }

        //
        public NetAssetInfo GetCachedNetAssetInfoByUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return null;
            }
            string key = Engine.Res.FileUtil.GetMd5(url.ToLower());
            return _cachedNetAssetInfoSet.GetCachedNetAssetInfo(key);
        }

        //------------------准备网络资源--------------------------
        public uint PrepareNetAssets(List<string> prepareUrls, System.Action<float> progresHandler)
        {
            if (prepareUrls == null)
            {
                return 0;
            }
            NetAssetSession ss = new NetAssetSession();
            ss.SessionId = _currentId++;

            ss.PrepareNetAsset(prepareUrls, progresHandler);
            if (ss != null && (ss.IsOver == false))
            {
                _prepareSessions.Add(ss);
            }
            return ss.SessionId;
        }

        //反注册
        public void UnPrepareNetAssets(uint sid)
        {
            for (int i = 0; i < _prepareSessions.Count; i++)
            {
                NetAssetSession session = _prepareSessions[i];
                if (session.SessionId == sid)
                {
                    _prepareSessions[i] = session;
                    session.StopSession();
                    break;
                }
            }
            CleanSession();
        }

        void IScheduleHandler.OnScheduleHandle(ScheduleType type, uint id)
        {
            //
            if (_isNeedSaveInfoSet)
            {
                _isNeedSaveInfoSet = false;
                //保存记录
                int offset = 0;
                _cachedNetAssetInfoSet.Write(S_Buffer, ref offset);
                if (Engine.Res.FileUtil.IsFileExist(S_CachedNetAssetInfoSetFileFullPath))
                {
                    Engine.Res.FileUtil.DeleteFile(S_CachedNetAssetInfoSetFileFullPath);
                }
                Engine.Res.FileUtil.WriteFile(S_CachedNetAssetInfoSetFileFullPath, S_Buffer, 0, offset);
            }
            //
            if (_prepareSessions == null || _prepareSessions.Count <= 0)
            {
                return;
            }
            CleanSession();
        }

        public void CleanSession()
        {
            for (int i = _prepareSessions.Count - 1; i >= 0; --i)
            {
                if (_prepareSessions[i].IsOver)
                {
                    _prepareSessions.RemoveAt(i);
                }
            }
            //
            if (_prepareSessions.Count <= 0)
            {
                //保存记录
                int offset = 0;
                _cachedNetAssetInfoSet.Write(S_Buffer, ref offset);
                if (Engine.Res.FileUtil.IsFileExist(S_CachedNetAssetInfoSetFileFullPath))
                {
                    Engine.Res.FileUtil.DeleteFile(S_CachedNetAssetInfoSetFileFullPath);
                }
                Engine.Res.FileUtil.WriteFile(S_CachedNetAssetInfoSetFileFullPath, S_Buffer, 0, offset);
            }
        }
    }
}
