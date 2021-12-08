/********************************************************************
  created:  2020-05-22         
  author:    OneJun           

  purpose:   简单对象池               
*********************************************************************/

using UnityEngine;

namespace Engine.Base
{
    /// <summary>
    /// 池中对象
    /// </summary>
    public interface IPoolObject
    {
        /// <summary>
        /// 创建
        /// </summary>
        /// <param name="param1">外部参数1</param>
        /// <param name="param2">外部参数2</param>
        void PoolCreate(object param1 = null, object param2 = null);

        /// <summary>
        /// 销毁
        /// </summary>
        void PoolDestroy();

        /// <summary>
        /// 构造
        /// </summary>
        void PoolConstructor();

        /// <summary>
        /// 析构
        /// </summary>
        void PoolDestructor();
    }

    /// <summary>
    /// 单一类型对象池
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    public class ObjectPool<T> where T : IPoolObject, new()
    {
        private ArrayList<T> _objects;

        /// <summary>
        /// 对象数量
        /// </summary>
        /// <value></value>
        public int Count
        {
            get
            {
                return null == _objects ? 0 : _objects.Count;
            }
        }

        /// <summary>
        /// 是否已经初始化
        /// </summary>
        public bool IsInitialize
        {
            get
            {
                return _objects != null;
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="initCount">初始数量</param>
        /// <param name="param1">自定义参数1（将传递给PoolCreate)</param>
        /// <param name="param2">自定义参数2（将传递给PoolCeate)</param>
        public void Initialize(int initCount, object param1 = null, object param2 = null)
        {
            if (initCount > 0)
            {
                _objects = new ArrayList<T>(initCount, 4);
                for (int i = 0; i < initCount; i++)
                {
                    T obj = new T();
                    obj.PoolCreate(param1, param2);

                    _objects.Add(obj);
                }
            }
            else
            {
                _objects = new ArrayList<T>(8, 4);
            }
        }

        /// <summary>
        /// 反初始化
        /// </summary>
        public void Uninitialize()
        {
            if (_objects == null)
            {
                return;
            }

            for (int i = 0; i < _objects.Count; i++)
            {
                _objects[i].PoolDestroy();
            }

            _objects.Clear();
            _objects = null;
        }

        /// <summary>
        /// 追加对象
        /// </summary>
        /// <param name="count">追加数量</param>
        /// <param name="param1">自定义参数1（将传递给PoolCreate）</param>
        /// <param name="param2">自定义参数2（将传递给PoolCreate）</param>
        public void Append(int count, object param1 = null, object param2 = null)
        {
            if (count <= 0 || _objects == null)
            {
                return;
            }

            _objects.ExpandCapacity(count);
            for (; count > 0; --count)
            {
                T obj = new T();
                obj.PoolCreate(param1, param2);
                _objects.Add(obj);
            }
        }

        /// <summary>
        /// 创建对象
        /// </summary>
        /// <returns>对象</returns>
        public T CreateObject()
        {
            if (_objects == null)
            {
                GGLog.LogE("CreateObject but _objects is null...maybe not Initialize it");
                return default(T);
            }

            if (_objects.Count == 0)
            {
                T obj = new T();
                obj.PoolCreate();
                obj.PoolConstructor();

                return obj;
            }

            int index = _objects.Count - 1;
            T ob = _objects[index];
            _objects.RemoveAt(index);

            ob.PoolConstructor();

            return ob;
        }

        /// <summary>
        /// 销毁对象
        /// </summary>
        /// <param name="obj">对象</param>
        public void DestroyObject(T obj)
        {
            obj.PoolDestructor();
            _objects.Add(obj);
        }
    }
}