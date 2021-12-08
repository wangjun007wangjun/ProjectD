/********************************************************************
  created:  2020-03-26         
  author:    OneJun           
  purpose:  UI工具类                
*********************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Engine.UGUI
{
    public class UIUtility
    {
        /// <summary>
        /// 用于隐藏GameObject的Layer
        /// </summary>
        public const int c_hideLayer = 31;
        /// <summary>
        /// UI层
        /// </summary>
        public const int c_uiLayer = 5;
        /// <summary>
        /// Default层
        /// </summary>
        public const int c_defaultLayer = 0;
        /// <summary>
        /// UI_BottomBG层
        /// </summary>
        public const int c_UIBottomBg = 18;

        /// <summary>
        /// 遍历获取UI组件，在Go非active状态下依然有效
        /// </summary>
        /// <param name="go"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetComponentInChildren<T>(GameObject go) where T : Component
        {
            if(go == null)
            {
                return null;
            }
            T t =go.GetComponent<T>();
            if(t != null)
            {
                return t;
            }

            Transform trans = go.transform;
            int count = trans.childCount;

            for (int i = 0; i < count; i++)
            {
                t = GetComponentInChildren<T>(trans.GetChild(i).gameObject);
                if(t != null)
                {
                    return t;
                }
            }
            return null;
        }

        /// <summary>
        /// 遍历获取UI组件数组，在Go非active状态下依然有效
        /// </summary>
        /// <param name="go"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static void GetComponentsInChildren<T>(GameObject go, T[] components, ref int count) where T : Component
        {
            T t = go.GetComponent<T>();
            if(t != null)
            {
                components[count] = t;
                count++;
            }

            for (int i = 0; i < go.transform.childCount; i++)
            {
                GetComponentsInChildren<T>(go.transform.GetChild(i).gameObject, components, ref count);
            }
        }

        /// <summary>
        /// 屏幕坐标转世界坐标
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="screenPoint"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static Vector3 ScreenToWorldPoint(Camera camera, Vector2 screenPoint, float z)
        {
            return ((camera == null) ? new Vector3(screenPoint.x, screenPoint.y, z) : camera.ViewportToWorldPoint(new Vector3(screenPoint.x / Screen.width, screenPoint.y / Screen.height, z)));
        }

        /// <summary>
        /// 世界坐标转屏幕坐标
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="worldPoint"></param>
        /// <returns></returns>
        public static Vector2 WorldToScreenPoint(Camera camera, Vector3 worldPoint)
        {
            return ((camera == null) ? new Vector2(worldPoint.x,worldPoint.y) : (Vector2)camera.WorldToScreenPoint(worldPoint));
        }

        /// <summary>
        /// 世界坐标转UI坐标
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="camera"></param>
        /// <param name="worldPos"></param>
        /// <returns></returns>
        public static Vector3 WorldToUIPoint(Canvas canvas, Camera camera, Vector3 worldPos)
        {
            Vector2 pos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, camera.WorldToScreenPoint(worldPos), canvas.worldCamera, out pos);
            return pos;
        }

        /// <summary>
        /// 设置GameObject的Layer
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="layer"></param>
        public static void SetGameObjectLayer(GameObject gameObject, int layer)
        {
            gameObject.layer = layer;
            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                SetGameObjectLayer(gameObject.transform.GetChild(i).gameObject,layer);
            }
        }

        public static float ValueInRange(float value, float min, float max)
        {
            if(value < min)
            {
                return min;
            }
            else if(value > max)
            {
                return max;
            }
            else
            {
                return value;
            }
        }
    }
}