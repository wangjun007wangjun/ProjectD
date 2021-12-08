/********************************************************************
  created:  2020-05-22         
  author:    OneJun           

  purpose:   资源信息定义 代表resources目录下一个资源的信息               
*********************************************************************/
using UnityEngine;
using System;

namespace Engine.Res
{
    [Serializable]
    public class ResInfo
    {
        /// <summary>
        /// 资源在Resources目录下的完整路径(不以"/"开头，不带扩展名)
        /// </summary>
        [SerializeField]
        public string Path;

        /// <summary>
        /// 资源扩展名（带".")
        /// </summary>
        [SerializeField]
        public string Ext;

        /// <summary>
        /// 是否在磁盘空间
        /// </summary>
        [SerializeField]
        public bool Outside;

        /// <summary>
        /// 工程目录中的相对路径
        /// </summary>
        [SerializeField]
        public string RelativePath;

        /// <summary>
        /// 不从Resources目录删除
        /// </summary>
        [SerializeField]
        public bool Keep;
    }
}