/********************************************************************
  created:  2020-05-26         
  author:    OneJun           

  purpose:  AssetBundle信息定义                
*********************************************************************/
using System;
using System.Collections.Generic;
using Engine.Base;
using Engine.Res;
using UnityEngine;

namespace Engine.Res
{
    /// <summary>
    /// bundle生命周期
    /// </summary>
    [Serializable]
    public enum EBundleLife
    {
        /// <summary>
        /// 缓存，在UnloadUnusedBundles时卸载
        /// </summary>
        Cache,
        /// <summary>
        /// 用时加载，用完释放,默认选项
        /// </summary>
        Immediate,
        /// <summary>
        /// 常驻，更新资源后加载，Logout卸载
        /// </summary>
        Resident,
    }

    /// <summary>
    /// bundle 包标记 标记AssetBundle 的打包方式 加载方式等
    /// </summary>
    [Serializable]
    public enum EBundleFlag
    {
        None = 0,
        UnCompress = 1, //非压缩的Bundle
        Resident = 1 << 1,  //常驻的bundle ，只有在没有依赖父Bundle时才有效
        LZMA = 1 << 2,      //LZMA压缩方式
        LZ4 = 1 << 3,       //ChunkBasedCompression压缩方式
        UnityScene = 1 << 4,        //是否是Unity场景
        NoBundle = 1 << 5,          //表示资源制件 可以打包但是同样存在Resource目录
        PreLoad = 1 << 6,           //预加载
        Optional = 1 << 7,          //跳过
    }

    /// <summary>
    /// Bundle包信息定义
    /// </summary>
    [Serializable]
    public class BundlePackInfo : ResPackInfo
    {
        /// <summary>
        /// 标记
        /// </summary>
        [SerializeField]
        public int Flags;
        /// <summary>
        /// 内存方式
        /// </summary>
        [SerializeField]
        public EBundleLife Life;
        /// <summary>
        /// 状态
        /// </summary>
        [SerializeField]
        public string State;
        /// <summary>
        /// 依赖包Name,逗号分隔
        /// </summary>
        [SerializeField]
        public string DependencyNames;


        /// <summary>
        /// 依赖AssetBundle包信息列表
        /// </summary>
        [SerializeField]
        public List<BundlePackInfo> Dependencies = null;

        /// <summary>
        /// 是否存储在用户磁盘 通过check location 设置了内部列表位置
        /// </summary>
        /// <value></value>
        public bool Outside
        {
            get
            {
                return (Count > 0 && Resources[0].Outside);
            }
        }

        /// <summary>
        /// 资源预制体 assetbundle
        /// </summary>
        /// <returns></returns>
        public override byte GetPackType()
        {
            return (byte)ResPackType.ResPackTypeBundle;
        }

        /// <summary>
        /// 读二进制配置初始化
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        public override void Read(byte[] data, ref int offset)
        {
            base.Read(data, ref offset);
			Path = Path.ToLower ();
            // Flags
            Flags = MemoryOperator.ReadInt(data, ref offset);

            // Life
            Life = (EBundleLife)MemoryOperator.ReadShort(data, ref offset);
            
            // Dependency
            int hasDependency = MemoryOperator.ReadByte(data, ref offset);
            if (hasDependency == 1)
            {
                DependencyNames = MemoryOperator.ReadString(data, ref offset);
            }            
        }

        /// <summary>
        /// 处理依赖关系
        /// </summary>
        /// <param name="loaded"></param>
        public override void ProcessDependency(List<ResPackInfo> loaded)
        {
            if (string.IsNullOrEmpty(DependencyNames) || loaded == null || loaded.Count == 0)
            {
                return;
            }

            string[] tags = DependencyNames.Split(',');
            for (int j = 0; j < tags.Length; j++)
            {
                string tag = tags[j];

                for (int i = 0; i < loaded.Count; i++)
                {
                    BundlePackInfo info = loaded[i] as BundlePackInfo;
                    if (info == null)
                    {
                        continue;
                    }

                    if (info.Path.Equals(tag,System.StringComparison.OrdinalIgnoreCase))
                    {
                        if (Dependencies == null)
                        {
                            Dependencies = new List<BundlePackInfo>(1);
                        }
                        Dependencies.Add(info);
                    }
                }
            }
            if (Dependencies == null)
            {
                GGLog.LogE("Specify DependencyNames {0}, but no dependency instance. bundle:{1}", DependencyNames, Path);
            }
        }

        /// <summary>
        /// 检查标识位
        /// </summary>
        /// <param name="flag"></param>
        /// <returns></returns>
        public bool HasFlag(EBundleFlag flag)
        {
            int flagInt = (int)flag;
            return (Flags & flagInt) > 0;
        }

        /// <summary>
        /// 表示资源包的资源 出档时候存在于Resource 目录
        /// </summary>
        /// <returns></returns>
        public bool IsNoBundle()
        {
            return HasFlag(EBundleFlag.NoBundle); 
        }

        /// <summary>
        /// 位置检查
        /// </summary>
        /// <returns></returns>
        protected override bool CheckLocation()
        {
            if (!base.CheckLocation())
            {
                bool outside = FileUtil.IsExistInIFSExtraFolder(Path);
                //内部资源都标记在外面
                for (int i = 0; i < Count; i++)
                {
                    ResInfo r = Resources[i];
                    r.Outside = outside;
                }
            }
            return true;
        }

        /// <summary>
        /// 写二进制配置
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        public override void Write(byte[] data, ref int offset)
        {
            base.Write(data, ref offset);
            // Flags
            MemoryOperator.WriteInt(Flags, data, ref offset);

            // Life
            MemoryOperator.WriteShort((short)Life, data, ref offset);
            
            // Dependency
            if (string.IsNullOrEmpty(DependencyNames))
            {
                MemoryOperator.WriteByte(0, data, ref offset);
            }
            else
            {
                MemoryOperator.WriteByte(1, data, ref offset);
                MemoryOperator.WriteString(DependencyNames, data, ref offset);
            }
        }
    }
}