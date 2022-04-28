/********************************************************************
    created:	2021-12-14				
    author:		OneJun						
    purpose:	得分信息								
*********************************************************************/
using System;
using System.Collections.Generic;
using Engine.TimeSync;
using Engine.UGUI;
using Net;
using UnityEngine;

namespace Data
{
    [System.Serializable]
    public class ScoreInfo
    {
        public int musicId;
        public string musicName;
        public int bestScore = 0;
    }
    [System.Serializable]
    public class ScoreInfoStorage
    {
        public List<ScoreInfo> ScoreInfoList = new List<ScoreInfo>();
    }
    [System.Serializable]
    public class ScoreData
    {
        private ScoreInfoStorage _storage;

        public void Load()
        {
            _storage = DataService.GetInstance().ReUseDataFromLocal<ScoreInfoStorage>();
            if (_storage.ScoreInfoList == null)
            {
                _storage.ScoreInfoList = new List<ScoreInfo>();
            }
        }
        public void Dump()
        {
            DataService.GetInstance().DumpDataToLocal<ScoreInfoStorage>(_storage);
        }

        public int GetScoreInfoById(int id)
        {
            if (_storage == null)
            {
                Debug.Log("为空,id:"+id.ToString());
            }
            if (_storage.ScoreInfoList != null)
            {
                ScoreInfo info = _storage.ScoreInfoList.Find((a) => { return a.musicId == id; });
                if (info != null)
                {
                    return info.bestScore;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return 0;
            }
        }
        public void SaveScore(int id, int score)
        {
            if (_storage.ScoreInfoList == null)
            {
                _storage.ScoreInfoList = new List<ScoreInfo>();
            }
            ScoreInfo info = _storage.ScoreInfoList.Find((a) => { return a.musicId == id; });
            if (info != null)
            {
                if (info.bestScore < score)
                {
                    info.bestScore = score;
                    Dump();
                    DumpNewScoreToSvr(info);
                }
            }
            else
            {
                ScoreInfo infoTemp = new ScoreInfo();
                infoTemp.bestScore = score;
                infoTemp.musicId = id;

                _storage.ScoreInfoList.Add(infoTemp);

                Dump();
                DumpNewScoreToSvr(infoTemp);
            }
        }

        private void DumpNewScoreToSvr(ScoreInfo info)
        {
            //存储到服务器
            AddNewRankReq req = new AddNewRankReq();
            RankPlayerInfo score = new RankPlayerInfo();
            score.name = DataService.GetInstance().Me.PlayerName;
            score.score = info.bestScore;
            score.music = info.musicId + DataService.GetInstance().Model * 100;
            score.update_time = TimeHelper.GetCurDateTimeStr();
            req.score = score;
            NetService.GetInstance().SendNetPostReq(NetDeclare.UpdateRankAPI, req, this.OnSaveScoreSvrRsp);
        }

        private void OnSaveScoreSvrRsp(bool isError, string rspJsonStr)
        {
            if (isError)
            {
                UICommon.GetInstance().ShowBubble(rspJsonStr);
                return;
            }
            AddNewRankRsp rsp = JsonUtility.FromJson<AddNewRankRsp>(rspJsonStr);
            if (rsp.code != 2000)
            {
                UICommon.GetInstance().ShowBubble(rsp.message);

                GLog.LogE("code:" + rsp.code + "   message:" + rsp.message);
                UICommon.GetInstance().CleanWaiting();
                return;
            }
        }
    }
}