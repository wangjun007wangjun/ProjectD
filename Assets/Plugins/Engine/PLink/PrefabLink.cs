/********************************************************************
  created:  2020-03-31         
  author:    OneJun           
  purpose:   预制体挂接脚本 通过此实例获取元素组件               
*********************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Engine.PLink
{
    /// <summary>
    /// 预制体连接元素定义
    /// </summary>
    [System.Serializable]
    public class PrefabElement
    {
        public Component Component;
        public string Name;
    }

    /// <summary>
    /// 预制体连接，通过此实例获取预制体上的元素
    /// </summary>
    public class PrefabLink : MonoBehaviour
    {   
        /// <summary>
        /// 元素列表
        /// </summary>
        public PrefabElement[] Elements;

        /// <summary>
        /// 获取缓存的元素个数
        /// </summary>
        /// <returns></returns>
        public int GetCacheCnt()
        {
            if(Elements != null)
            {
                return Elements.Length;
            }
            return 0;
        }

        public GameObject GetBindedGo()
        {
            return gameObject;
        }

        public Transform GetBindedTf()
        {
            return transform;
        }


        /// <summary>
        /// 获取缓存的GameObject
        /// </summary>
        /// <param name="index">索引</param>
        /// <returns></returns>
        public GameObject GetCacheGameObject(int index)
        {
            if(Elements != null && Elements.Length > 0)
            {
                if(Elements == null || index < 0 || index >= Elements.Length)
                {
                    return null;
                }
                return Elements[index].Component.gameObject;
            }
            return null;
        }

        /// <summary>
        /// 获取缓存的Transform
        /// </summary>
        /// <param name="index">索引</param>
        /// <returns>获取到的Transform</returns>
        public Transform GetCacheTransform(int index)
        {
            if (Elements != null && Elements.Length > 0)
            {
                if (Elements == null || index < 0 || index >= Elements.Length)
                {
                    return null;
                }
                return Elements[index].Component.transform;
            }
            return null;
        }
        
        /// <summary>
        /// 通过index取component
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Component GetCacheComponent(int index)
        {
            if (Elements == null
                || index < 0
                || index >= Elements.Length)
            {
                return null;
            }
            return Elements[index].Component;
        }

        /// <summary>
        /// 获取缓存的Component
        /// </summary>
        /// <param name="index">索引</param>
        /// <param name="typeName">Component的类型名</param>
        /// <returns>获取到的Component</returns>
        public Component GetCacheComponent(int index, string typeName)
        {
            if (Elements != null && Elements.Length > 0)
            {
                if (Elements == null || index < 0 || index >= Elements.Length)
                {
                    return null;
                }

                return Elements[index].Component;
            }
            return null;
        }
    }

    public static class ExtTransform
    {
        public static PrefabLink ExtGetRootPrefabLink(this Transform tf)
        {
            if(tf == null)
            {
                return null;
            }
            PrefabLink pp = tf.GetComponent<PrefabLink>();
            return pp;
        }
    }
}