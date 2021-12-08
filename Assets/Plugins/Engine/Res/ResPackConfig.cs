/********************************************************************
  created:  2020-05-25         
  author:    OneJun           

  purpose:   资源配置信息类型描述类 单个游戏的所有资源信息 包括二进制包 和AB包 类似主Manifest               
*********************************************************************/
using System;
using System.Collections.Generic;
using Engine.Base;
using UnityEngine;

namespace Engine.Res
{
    [Serializable]
    public class ResPackConfig
    {
        [SerializeField]
        public List<ResPackInfo> PackInfo = new List<ResPackInfo>();

        private ObjDictionary<string, ResPackInfo> _resourceMap = null;

        public ResPackConfig()
        {
            PackInfo = new List<ResPackInfo>();
        }

        /// <summary>
        /// 根据二进制初始化
        /// </summary>
        /// <param name="bbs"></param>
        /// <param name="isSub"></param>
        public ResPackConfig(byte[] bbs,bool isSub = false)
        {
            if(bbs == null || bbs.Length == 0)
            {
                GGLog.LogE("Init ResPackConfig Error No Bytes");
                return;
            }
            int offset = 0;
            Read(bbs,ref offset);

            if(!isSub)
            {
                //后处理下依赖
                for (int i = 0; i < PackInfo.Count; i++)
                {
                    ResPackInfo resPackInfo = PackInfo[i];
                    if(resPackInfo != null)
                    {
                        resPackInfo.ProcessDependency(this.PackInfo);
                    }
                }
            }
        }

        public void ReloadDataBytes(byte[] bbs)
        {
            if(bbs == null || bbs.Length <= 0)
            {
                GGLog.LogE("ResPackConfig ReloadDataBytes Error No Bytes");
                return;
            }

            PackInfo.Clear();
            _resourceMap.Clear();
            int offset = 0;
            Read(bbs, ref offset);

            //后处理下依赖
            for (int i = 0; i < PackInfo.Count; i++)
            {
                ResPackInfo resPackInfo = PackInfo[i];
                if(resPackInfo != null)
                {
                    resPackInfo.ProcessDependency(this.PackInfo);
                }
            }
        }

        /// <summary>
        /// 合并
        /// </summary>
        /// <param name="other"></param>
        public void MergeWithOther(ResPackConfig other)
        {
            if (_resourceMap == null || other == null)
                return;

            //先清理
            for (int i = 0; i < other.PackInfo.Count; ++i)
            {
                ResPackInfo resPackInfo = other.PackInfo[i];
                //
                for (int j = PackInfo.Count - 1; j >= 0; j--)
                {
                    if (PackInfo[j].Path == resPackInfo.Path)
                    {
                        PackInfo.RemoveAt(j);
                    }
                }
            }
            //添加
            for (int i = 0; i < other.PackInfo.Count; ++i)
            {
                ResPackInfo resPackInfo = other.PackInfo[i];
                PackInfo.Add(resPackInfo);

                List<ResInfo> resInfos = resPackInfo.Resources;
                for (int resIndex = 0; resIndex < resInfos.Count; ++resIndex)
                {
                    ResInfo res = resInfos[resIndex];
                    if (!_resourceMap.ContainsKey(res.Path))
                    {
                        _resourceMap.Add(res.Path, resPackInfo);
                    }
                    else
                    {
                        //Log.LogE("Pack Config {0} and {1}:{2}had same res : {3}", "Main", "Other", resPackInfo.Path, res.Path);
                        //替换
                        GGLog.LogD("Reload ResPackInfo:");
                        _resourceMap.Remove(res.Path);
                        _resourceMap.Add(res.Path, resPackInfo);
                    }
                }
            }

            //后处理下依赖
            for (int i = 0; i <PackInfo.Count; ++i)
            {
                ResPackInfo resPackInfo =PackInfo[i];
                if (resPackInfo != null)
                    resPackInfo.ProcessDependency(this.PackInfo);
            }
        }

        /// <summary>
        /// 读取配置初始化 包列表信息
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        public void Read(byte[] data, ref int offset, bool procDependency = false)
        {
            int count = MemoryOperator.ReadShort(data, ref offset);

            int resCount = 0;

            for (int i = 0; i < count; i++)
            {
                ResPackInfo packInfo = CreatePackInfo(data, ref offset);
                if (packInfo != null)
                {
                    packInfo.Read(data, ref offset);

                    if (procDependency)
                    {
                        packInfo.ProcessDependency(PackInfo);
                    }

                    PackInfo.Add(packInfo);
                    resCount += packInfo.Resources.Count;
                }
            }

            if (_resourceMap == null)
            {
                _resourceMap = new ObjDictionary<string, ResPackInfo>(resCount, StringComparer.OrdinalIgnoreCase);
            }

            for (int i = 0; i < PackInfo.Count; ++i)
            {
                AddToMap(PackInfo[i]);
            }
        }

        /// <summary>
        /// 添加包信息
        /// </summary>
        /// <param name="packInfo"></param>
        public void AddPackInfo(ResPackInfo packInfo)
        {
            if (packInfo == null)
            {
                return;
            }

            if (_resourceMap == null)
            {
                _resourceMap = new ObjDictionary<string, ResPackInfo>(StringComparer.OrdinalIgnoreCase);
            }

            packInfo.ProcessDependency(PackInfo);
            PackInfo.Add(packInfo);

            AddToMap(packInfo);
        }

        /// <summary>
        /// ͨ获取包信息
        /// </summary>
        /// <param name="path">包名</param>
        /// <returns></returns>
        public ResPackInfo GetPackInfo(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            for (int i = 0; i < PackInfo.Count; i++)
            {
                if (path.Equals(PackInfo[i].Path))
                {
                    return PackInfo[i];
                }
            }

            return null;
        }

        /// <summary>
        /// ͨ获取资源对应的包信息
        /// </summary>
        /// <param name="resourcePath">资源路径</param>
        /// <returns></returns>
        public ResPackInfo GetPackInfoForResource(string resourcePath)
        {
            if (string.IsNullOrEmpty(resourcePath))
            {
                return null;
            }

            ResPackInfo packInfo = null;
            if (_resourceMap != null)
            {
                _resourceMap.TryGetValue(resourcePath, out packInfo);
            }

            return packInfo;
        }

        /// <summary>
        /// 创建包信息
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        ResPackInfo CreatePackInfo(byte[] data, ref int offset)
        {
            // type
            int type = MemoryOperator.ReadByte(data, ref offset);
            switch ((ResPackType)type)
            {
                case ResPackType.ResPackTypeBundle:
                    return new BundlePackInfo();
                case ResPackType.ResPackTypeBinary:
                    return new BinaryPackInfo();
                default:
                    break;
            }
            return null;
        }

        /// <summary>
        /// 添加资源映射
        /// </summary>
        /// <param name="packInfo"></param>
        void AddToMap(ResPackInfo packInfo)
        {
            for (int i = 0; i < packInfo.Resources.Count; i++)
            {
                ResInfo r = packInfo.Resources[i];
                try
                {
                    _resourceMap.Add(r.Path, packInfo);
                }
                catch (Exception e)
                {
                    GGLog.LogE("Add {0} to map error:{1}", r.Path, e.Message);
                }
            }
        }
    }
}