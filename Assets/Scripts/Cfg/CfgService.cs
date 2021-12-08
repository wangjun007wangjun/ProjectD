/********************************************************************
  created:  2020-06-05         
  author:    OneJun           
  purpose:  游戏配置数据 持有                
*********************************************************************/
using Engine.Base;
using Engine.Res;
using UnityEngine;

namespace Cfg
{
    public class CfgService : Singleton<CfgService>
    {
        //外部存储配置档的目录
        private static string S_CacheDirPath = Engine.Res.FileUtil.CombinePath(Engine.Res.FileUtil.GetCachePath(), "CfgCache");
        private static string S_CfgAssetResDir = "Cfgs";

        //缓存的游戏配置列表
        private ObjDictionary<string, CfgResObj> _cachedCfgs;

        public override bool Initialize()
        {
            _cachedCfgs = new ObjDictionary<string, CfgResObj>(20);
            return true;
        }

        public override void Uninitialize()
        {
            _cachedCfgs.Clear();
            _cachedCfgs = null;
        }

        /// <summary>
        /// 获取配置 对外
        /// </summary>
        /// <typeparam name="T">配置类型</typeparam>
        public T GetCfg<T>() where T : CfgResObj
        {
            string nameKey = typeof(T).Name;
            CfgResObj so;
            if (_cachedCfgs.TryGetValue(nameKey, out so))
            {
                return so as T;
            }
            else
            {
                return LoadOneCfg<T>();
            }
        }

        /// <summary>
        /// 缓存配置到本地
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void DumpCfg<T>(T cfgSo = null) where T : CfgResObj
        {
            string name = typeof(T).Name;
            CfgResObj so = cfgSo;
            if (so == null)
            {
                if (_cachedCfgs.TryGetValue(name, out so))
                {
                    so = so as T;
                }
                else
                {
                    GLog.LogE("DumpCfg Not Exit!");
                    return;
                }
            }
            //先判断外部是否存在
            string savePath = Engine.Res.FileUtil.CombinePaths(S_CacheDirPath, name + ".cfg");
            string jsonData = JsonUtility.ToJson(so);
            byte[] byteData = System.Text.Encoding.UTF8.GetBytes(jsonData);
            Engine.Res.FileUtil.WriteFile(savePath, byteData, true);
        }

        //加载一个配置
        private T LoadOneCfg<T>() where T : CfgResObj
        {
            string cfgName = typeof(T).Name;
            if (_cachedCfgs.ContainsKey(cfgName))
            {
                GLog.LogE("Repeat Load Cfg:" + cfgName);
                return _cachedCfgs[cfgName] as T;
            }
            //构建一个
            T so = null;
            bool externalSucc = false;
            //先判断外部是否存在
            string externalFilePath = Engine.Res.FileUtil.CombinePath(S_CacheDirPath, cfgName + ".cfg");
            if (Engine.Res.FileUtil.IsFileExist(externalFilePath))
            {
                string jsonStr = Engine.Res.FileUtil.ReadFileText(externalFilePath);
                if (!string.IsNullOrEmpty(jsonStr))
                {
                    so = ScriptableObject.CreateInstance<T>();
                    JsonUtility.FromJsonOverwrite(jsonStr, so);
                    externalSucc = true;
                    //map
                    so.DoMapEntry();
                    //add cache
                    _cachedCfgs.Add(cfgName, so);
                }
                else
                {
                    externalSucc = false;
                }
            }
            //从Resource目录读取ScriptableObject 资源文件
            if (externalSucc != false)
            {
                return so;
            }
            ResObj obj = ResService.GetResource(S_CfgAssetResDir + "/" + cfgName);
            if (obj != null)
            {
                so = obj.Content as T;
                //map
                so.DoMapEntry();
                _cachedCfgs.Add(cfgName, so);
            }
            else
            {
                GLog.LogE("Load Cfg Asset Error");
            }
            return so;
        }

        public void ClearAll()
        {
            if (_cachedCfgs != null)
            {
                _cachedCfgs.Clear();
            }
        }
    }
}
