/********************************************************************
    created:	2021-12-14				
    author:		OneJun						
    purpose:	得分信息								
*********************************************************************/
using System.Collections.Generic;
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
    Debug.Log("为空");
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
                }
            }
            else
            {
                ScoreInfo infoTemp = new ScoreInfo();
                infoTemp.bestScore = score;
                infoTemp.musicId = id;

                _storage.ScoreInfoList.Add(infoTemp);
                
                Dump();
            }
        }
    }
}