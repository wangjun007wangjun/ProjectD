using UnityEngine;
using Engine.Asset;
using Engine.Base;

namespace Engine.UGUI
{
    /// <summary>
    /// 视图控制类
    /// </summary>
    public abstract class UIViewClass : UIViewAsset
    {
        private readonly string _className;

        public string ClassName
        {
            get
            {
                return _className;
            }
        }

        /// <summary>
        /// 当前创建预制体挂接组件
        /// </summary>
        protected UIView View;

        /// <summary>
        /// 所属窗口控制器
        /// </summary>
        private UIFormClass ParentFormController;

        /// <summary>
        /// 动作处理器
        /// </summary>
        public System.Action<string, object> ActionHandler;

        /// <summary>
        /// 构造函数
        /// </summary>
        protected UIViewClass()
        {
            _className = GetType().Name;
        }

        public void ActiveView(bool active)
        {
            if (View != null)
            {
                View.gameObject.SetActive(active);
            }
        }

        public void SendAction(string key, object param = null)
        {
            if (ActionHandler != null)
            {
                ActionHandler(key, param);
            }
        }
        #region 外部不要调用
        public void Create(UIFormClass formVC, Transform parentTf, System.Action<string, object> acHander = null, object parameter = null)
        {
            ActionHandler = acHander;
            ParentFormController = formVC;
            //挂接
            RootTf.parent = parentTf;
            RootTf.ExtSetLocalEulerAnglesXYZ(0, 0, 0);
            RootTf.ExtSetLocalPositionXYZ(0, 0, 0);
            RootTf.ExtSetLocalScaleXYZ(1.0f, 1.0f, 1.0f);
            RootGo.ExtSetActive(true);
            //组件设置
            formVC.AddSubView(View);
            //
            // 逻辑初始化
            OnInitialize(parameter);
        }

        /// <summary>
        /// 销毁，请不要使用
        /// </summary>
        public void Destroy()
        {
            // 逻辑反初始化
            OnUninitialize();
            //移除
            ParentFormController.RemoveSubView(View);
            // 释放View资源
            AssetService.GetInstance().Unload(this);
            //
            ActionHandler = null;
            ParentFormController = null;
        }

        /// <summary>
        /// 资源 创建 回调
        /// </summary>
        public sealed override void OnViewAssetCreate()
        {
            if (RootGo == null)
            {
                GGLog.LogE("UIViewClass.PoolCreate error - failed to create : {0}", GetPath());
                return;
            }
            //获取组件
            View = RootGo.ExtGetComponent<UIView>();
            if (View == null)
            {
                GGLog.LogE("UIViewClass.PoolCreate error - failed to get UIPrefab component : {0}", GetPath());
                return;
            }
            View.Controller = this;
            OnResourceLoaded();
        }

        /// <summary>
        /// 资源池销毁对象
        /// </summary>
        public sealed override void OnViewAssetDestroy()
        {
            OnResourceUnLoaded();
            if (View != null)
            {
                View.Controller = null;
                View = null;
            }
        }

        /// <summary>
        /// 内部控件操作 外部禁止使用
        /// </summary>
        /// <param name="id">操作ID标识</param>
        /// <param name="param">操作参数</param>
        public void Action(string id, object param = null)
        {
            OnAction(id, param);
        }

        /// <summary>
        /// 持有者调用 更新UI
        /// </summary>
        /// <param name="id">更新ID标识</param>
        /// <param name="param">更新参数</param>
        public void UpdateUI(string id, object param)
        {
            OnUpdateUI(id, param);
        }



        #endregion


        #region 子类重写

        /// <summary>
        /// 视图被聚焦可见
        /// </summary>
        public virtual void OnViewFocused(bool isFocus)
        {

        }

        /// <summary>
        /// 资源加载后回调
        /// </summary>
        protected virtual void OnResourceLoaded()
        {
        }

        /// <summary>
        /// 资源卸载后回调
        /// </summary>
        protected virtual void OnResourceUnLoaded()
        {

        }

        /// <summary>
        /// 逻辑初始化
        /// </summary>
        /// <param name="parameter">初始化参数</param>
        protected virtual void OnInitialize(object parameter)
        {
        }

        /// <summary>
        /// 逻辑反初始化
        /// </summary>
        protected virtual void OnUninitialize()
        {
        }

        /// <summary>
        /// 更新UI
        /// </summary>
        /// <param name="id">更新ID标识</param>
        /// <param name="param">更新参数</param>
        protected virtual void OnUpdateUI(string id, object param)
        {

        }

        /// <summary>
        /// 操作
        /// </summary>
        /// <param name="id">操作ID标识</param>
        /// <param name="param">操作参数</param>
        protected virtual void OnAction(string id, object param)
        {

        }

        #endregion

        /// <summary>
        /// 添加自身的组件 无用
        /// </summary>
        /// <typeparam name="T">组件类型</typeparam>
        /// <returns>组件</returns>
        protected T AddComponent<T>() where T : Component
        {
            return null != RootGo ? RootGo.ExtAddComponent<T>() : null;
        }

        /// <summary>
        /// 获取自身的组件 无用
        /// </summary>
        /// <typeparam name="T">组件类型</typeparam>
        /// <returns>组件</returns>
        protected T GetComponent<T>() where T : Component
        {
            return null != RootGo ? RootGo.ExtGetComponent<T>() : null;
        }

        /// <summary>
        /// 获取组件
        /// </summary>
        /// <typeparam name="T">组件类型</typeparam>
        /// <param name="index">index下标</param>
        /// <returns>组件</returns>
        protected T GetComponent<T>(int index) where T : Component
        {
            Component com = View.GetCacheComponent(index);
            if (com == null)
            {
                GameObject gameObject = GetGameObject(index);
                com = gameObject.ExtGetComponent<T>();
            }

            return com ? com as T : null;
        }

        /// <summary>
        /// 获取组件
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        protected Component GetComponent(int index)
        {
            return View.GetCacheComponent(index);
        }

        /// <summary>
        /// 获取组件
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        protected Component GetComponent(int index, string typeName)
        {
            return View.GetCacheComponent(index, typeName);
        }

        /// <summary>
        /// 获取Transform
        /// </summary>
        /// <param name="index">index下标</param>
        /// <returns>Transform</returns>
        protected Transform GetTransform(int index)
        {
            return View.GetCacheTransform(index);
        }

        /// <summary>
        /// 获取GameObject
        /// </summary>
        /// <param name="index">index下标</param>
        /// <returns>GameObject</returns>
        protected GameObject GetGameObject(int index)
        {
            return View.GetCacheGameObject(index);
        }

    }
}