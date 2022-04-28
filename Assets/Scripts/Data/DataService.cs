/********************************************************************
	created:	2020/02/14
	author:		OneJun
	purpose:	游戏运行时数据持有 这里的数据主要来自服务器
*********************************************************************/
using Engine.Base;
using UnityEngine;
using System;
namespace Data
{
    public class DataService : Singleton<DataService>
    {
        //游戏数据本地缓存目录
        private static readonly string S_SaveDir = Engine.Res.FileUtil.CombinePath(Engine.Res.FileUtil.GetCachePath(), "GameData");
        //用户信息自己
        public UserInfo Me;
        public ScoreData Score;
        public MusicDataCfgList MusicDataCfgList = null;
        //游戏模式
        public int Model = 1;
        public override bool Initialize()
        {
            Me = new UserInfo();
            Score = new ScoreData();
            Model = 1;
            return true;
        }

        public override void Uninitialize()
        {
            Me = null;
            Score = null;
            Model = 1;
        }

        public void ResetData()
        {
            Me = new UserInfo();
            Score = new ScoreData();
            Model = 1;
        }

        public void DelLocalSaveData()
        {
            Engine.Res.FileUtil.DeleteDirectory(S_SaveDir);
        }

        /// <summary>
        /// 保存游戏运行时数据到本地
        /// </summary>
        /// <returns></returns>
        public void DumpDataToLocal<T>(T obj) where T : new()
        {
            if (Me == null || Me.PlayerId == -1)
            {
                GLog.LogE("DumpData Must User Login");
                return;
            }
            string meId = Me.PlayerId.ToString();
            string name = typeof(T).Name;
            //先判断外部是否存在
            string savePath = Engine.Res.FileUtil.CombinePaths(S_SaveDir, meId, Model.ToString(), name + ".data");
            string jsonData = JsonUtility.ToJson(obj);
            byte[] byteData = System.Text.Encoding.UTF8.GetBytes(jsonData);
            Engine.Res.FileUtil.WriteFile(savePath, byteData, true);

        }

        /// <summary>
        /// 检查是否含有存档数据
        /// </summary>
        /// <param name="obj"></param>
        /// <typeparam name="T"></typeparam>
        public bool CheckIsHaveLocalDumpData<T>() where T : new(){
            if (Me == null || Me.PlayerId == -1)
            {
                GLog.LogE("CheckIsHaveLocalDumpData Must User Login");
                return false;
            }
            string meId = Me.PlayerId.ToString();
            string name = typeof(T).Name;
            //先判断外部是否存在
            string savePath = Engine.Res.FileUtil.CombinePaths(S_SaveDir, meId, name + ".data");
            if(Engine.Res.FileUtil.IsFileExist(savePath)){
                return true;
            }else{
                return false;
            }
        }

        /// <summary>
        /// 删除单个存档数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void DeleteLocalDumpData<T>()where T : new(){
             if (Me == null || Me.PlayerId == -1)
            {
                GLog.LogE("DeleteLocalDumpData Must User Login");
                return;
            }   
            string meId = Me.PlayerId.ToString();
            string name = typeof(T).Name;
            //先判断外部是否存在
            string savePath = Engine.Res.FileUtil.CombinePaths(S_SaveDir, meId, name + ".data");
            Engine.Res.FileUtil.DeleteFile(savePath);
        }

        /// <summary>
        /// 根据本地存储的数据重置运行时数据 加载完成后记得 重新设置 共有的成员
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public T ReUseDataFromLocal<T>() where T : new()
        {
            if (Me == null || Me.PlayerId == -1)
            {
                GLog.LogE("ReUseDataFromLocal Must User Login");
                return default(T);
            }
            string meId = Me.PlayerId.ToString();
            string name = typeof(T).Name;
            string savePath = Engine.Res.FileUtil.CombinePaths(S_SaveDir, meId, Model.ToString(), name + ".data");
            string jsonStr = Engine.Res.FileUtil.ReadFileText(savePath);
            try
            {
                if (!string.IsNullOrEmpty(jsonStr))
                {
                    T ret = new T();
                    JsonUtility.FromJsonOverwrite(jsonStr, ret);
                    return ret;
                }
                else
                {
                    return new T();
                }
            }
            catch (Exception ex)
            {
                GLog.LogE(ex.ToString());
                throw (ex);
            }
        }

    }
}

