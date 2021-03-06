using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Engine.Http;
using Engine.Base;

namespace Engine.NetAsset
{
    public class NetAssetSession
    {
        public uint SessionId;
        public bool IsOver;

        //进度
        private System.Action<float> _handler;
        private Dictionary<uint,string> _httpSessionIds = new Dictionary<uint, string>();
        private int _totalCnt = 0;
        private int _finishedCnt = 0;
        private int _failCnt = 0;

        /// <summary>
        /// 准备一组网络文件
        /// </summary>
        /// <param name="urls"></param>
        /// <param name="finishhandler"></param>
        public void PrepareNetAsset(List<string> prepareUrls, System.Action<float> progresHandler)
        {
            if (prepareUrls == null)
            {
                GGLog.LogE("PrepareNetAsset Error Arg");
                IsOver = true;
                return;
            }
            //
            List<string> urls = new List<string>();
            for (int i = 0; i < prepareUrls.Count; i++)
            {
                //去重复
                string cc = prepareUrls[i];
                if (!urls.Contains(cc))
                {
                    urls.Add(cc);
                }
            }
            //
            if (urls.Count <= 0)
            {
                IsOver = true;
                GGLog.LogE("PrepareNetAsset Empty Urls");
                if (progresHandler != null)
                {
                    progresHandler(1.0f);
                }
                return;
            }
            //
            _finishedCnt = 0;
            _totalCnt = urls.Count;
            _httpSessionIds.Clear();
            _handler = progresHandler;
            _failCnt = 0;
            IsOver = false;
            //
            for (int i = 0; i < urls.Count; i++)
            {
                string key = Engine.Res.FileUtil.GetMd5(urls[i].ToLower());
                string downloadPath = Engine.Res.FileUtil.CombinePath(NetAssetService.S_CachedNetAssetDirectory, key + ".bytes");
                //是否有了
                NetAssetInfo info = NetAssetService.GetInstance().GetCachedNetAssetInfoByKey(key);
                string filePath = Engine.Res.FileUtil.CombinePath(NetAssetService.S_CachedNetAssetDirectory, key + ".bytes");
                if (info != null && (Engine.Res.FileUtil.IsFileExist(filePath)))
                {
                    _finishedCnt = _finishedCnt + 1;
                    float pp = _finishedCnt / (float)_totalCnt;
                    if (_handler != null)
                    {
                        _handler(pp);
                    }
                    //Over 
                    if (_finishedCnt >= _totalCnt)
                    {
                        if (_handler != null)
                        {
                            _handler(1.0f);
                        }
                        IsOver = true;
                    }
                }
                else
                {
                    //没有 就下载
                    uint sid= HttpService.GetInstance().RegisteFileHttpDownload(urls[i], downloadPath,this.OnHttpSessionCallBack);
                    _httpSessionIds.Add(sid, key);
                }
            }
        }

        //
        public void StopSession()
        {
            _finishedCnt = 0;
            _totalCnt = 0;
            _handler = null;
            _failCnt = 0;
            IsOver = true;
            if (_httpSessionIds != null)
            {
                foreach(uint idid in _httpSessionIds.Keys)
                {
                    HttpService.GetInstance().UnRegisteFileHttpDownload(idid);
                }
                _httpSessionIds.Clear();
            }
        }

        //单个文件下载回调
        private void OnHttpSessionCallBack(bool isError, uint sessionId, bool isEnd, float progress)
        {
            if (isError)
            {
                _failCnt = _failCnt+1;
            }
            //添加到缓存记录
            if (isEnd && (!isError))
            {
                if (_httpSessionIds.ContainsKey(sessionId))
                {
                    string kk = string.Empty;
                    _httpSessionIds.TryGetValue(sessionId, out kk);
                    NetAssetService.GetInstance().AddCachedNetAssetInfoDirect(kk);
                }
            }

            //进度
            if (isEnd)
            {
                _finishedCnt = _finishedCnt + 1;
                float pp = _finishedCnt / (float)_totalCnt;
                if (_handler != null)
                {
                    if (_finishedCnt >= _totalCnt)
                    {
                        _handler(1.0f);
                    }
                    else
                    {
                        _handler(pp);
                    }
                }
                //Over 
                if (_finishedCnt >= _totalCnt)
                {
                    IsOver = true;
                    //
                    NetAssetService.GetInstance().CleanSession();
                }
            }
        }
    }
}
