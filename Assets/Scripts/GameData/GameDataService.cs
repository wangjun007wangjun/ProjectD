/********************************************************************
  created:  2020-03-13         
  author:    OneJun           
  purpose:   游戏运行时数据持有               
*********************************************************************/
using UnityEngine;
using System;

namespace GameData
{
    public class GameDataService 
    {
        //游戏数据本地缓存目录
        private static readonly string S_SaveDir = Engine.Res.FileUtil.CombinePath(Engine.Res.FileUtil.GetCachePath(), "GameData");

       
        /// <summary>
        /// 保存游戏运行时数据到本地
        /// </summary>
        /// <returns></returns>
        public void DumpDataToLocal<T>(T obj) where T : new()
        {
            string name = typeof(T).Name;
            //先判断外部是否存在

            //可用ID区分用户本地文件文件夹
            // string savePath = Base.Res.FileUtil.CombinePaths(S_SaveDir,Id, name + ".data");

            string savePath = Engine.Res.FileUtil.CombinePaths(S_SaveDir, name + ".data");
            string jsonData = JsonUtility.ToJson(obj);
            byte[] byteData = System.Text.Encoding.UTF8.GetBytes(jsonData);
            Engine.Res.FileUtil.WriteFile(savePath, byteData, true);
        }

        /// <summary>
        /// 根据本地存储的数据重置运行时数据 加载完成后记得 重新设置 共有的成员
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public T ReUseDataFromLocal<T>() where T : new()
        {
            string name = typeof(T).Name;
            string savePath = Engine.Res.FileUtil.CombinePaths(S_SaveDir, name + ".data");
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
