/********************************************************************
  created:  2020-05-29         
  author:    OneJun           

  purpose:   同步时间处理机制 事件接收               
*********************************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Engine.Base;
using UnityEngine;

namespace Engine.Event
{
    /// <summary>
    /// 事件参数接口
    /// </summary>
    public interface EventArg
    {
        
    }

    /// <summary>
    /// 空事件参数
    /// </summary>
    public struct EmptyEventArg : EventArg { }

    public class EventDeclare
    {

    }

    /// <summary>
    /// 事件接收器
    /// </summary>
    public class EventHandler
    {
        /// <summary>
        /// 事件ID
        /// </summary>
        private uint _id;
        /// <summary>
        /// 事件接收对象
        /// </summary>
        private object _instance;
        /// <summary>
        /// 事件接收函数名
        /// </summary>
        private string _methodName;
        /// <summary>
        /// 事件接收函数
        /// </summary>
        private MethodInfo _mi;

        private static CircleBuffer<object[]> _argObject;

        /// <summary>
        /// 有效性
        /// </summary>
        /// <value></value>
        public bool Valid
        {
            get
            {
                return _instance != null && _mi != null;
            }
        }

        /// <summary>
        /// 判断是否相等
        /// </summary>
        /// <param name="id">事件ID</param>
        /// <returns>是否相等</returns>
        public bool IsEqual(uint id)
        {
            return id == _id;
        }

        /// <summary>
        /// 判断是否相等
        /// </summary>
        /// <param name="instance">事件接收对象</param>
        /// <returns>是否相等</returns>
        public bool IsEqual(object instance)
        {
            return instance == _instance;
        }

        /// <summary>
        /// 判断是否相等
        /// </summary>
        /// <param name="id">事件ID</param>
        /// <param name="instance">事件接收对象</param>
        /// <returns>是否相等</returns>
        public bool IsEqual(uint id, object instance)
        {
            return id == _id && instance == _instance;
        }

        /// <summary>
        /// 创建事件接收器
        /// </summary>
        /// <param name="id">事件ID</param>
        /// <param name="instance">事件接收对象</param>
        /// <param name="methodName">事件接收函数名</param>
        /// <returns>创建成功/失败</returns>
        public bool Create(uint id, object instance, string methodName)
        {
            if (instance == null || methodName == null)
            {
                return false;
            }

            _id = id;
            _instance = instance;
            _methodName = methodName;
            _mi = instance.GetType().GetMethod(_methodName, BindingFlags.Instance | BindingFlags.NonPublic);

            return _mi != null;
        }

        /// <summary>
        /// 销毁事件接收器
        /// </summary>
        public void Destroy()
        {
            _instance = null;
            _mi = null;
        }

        /// <summary>
        /// 调用接收器
        /// </summary>
        /// <param name="arg">事件参数</param>
        public void Invoke(ref EventArg arg)
        {
            if (_instance == null || _mi == null)
            {
                return;
            }

            if (_argObject == null)
            {
                _argObject = new CircleBuffer<object[]>(100);

                for (int i = 0; i < 100; i++)
                {
                    _argObject.Push(new object[1]);
                }
            }

            object[] argObjects;
            if (!_argObject.Pop(out argObjects))
            {
                GGLog.LogE("EventHandle - no enough arg objet");
                return;
            }

            argObjects[0] = arg;

            try
            {
                _mi.Invoke(_instance, argObjects);
            }
            catch (Exception e)
            {
                GGLog.LogE("EventHandler.Invoke : exception {0} at {1}:{2}:{3}", e, _id, _instance.GetType().Name, _mi.Name);
                throw;
            }

            argObjects[0] = null;
            _argObject.Push(argObjects);
        }
    }
}