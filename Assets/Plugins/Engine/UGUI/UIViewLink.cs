/********************************************************************
  created:  2020-03-17         
  author:    OneJun           
  purpose:  UIView链接 获取内部cache组件                
*********************************************************************/
using UnityEngine;
using System;

namespace Engine.UGUI
{
    public class UIViewLink : MonoBehaviour
    {
        /// <summary>
        /// 窗口预制体上UI元素定义
        /// </summary>
        [Serializable]
        public class UIElement
        {
            /// <summary>
            /// 组件
            /// </summary>
            public Component Component;
            /// <summary>
            /// 名称
            /// </summary>
            public string Name;
        }

        /// <summary>
        /// 窗口Form所包含的UI控件列表
        /// </summary>
        public UIElement[] Elements;

        /// <summary>
        /// 获取缓存的GameObject
        /// </summary>
        /// <param name="index">索引</param>
        /// <returns>GameObject</returns>
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

                return Elements[index].Component.gameObject.GetComponent(typeName);
            }
            return null;
        }
    }
}