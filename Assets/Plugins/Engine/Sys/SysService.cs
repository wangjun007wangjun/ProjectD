using Engine.Base;
using Engine.State;
using System;

namespace Engine.Sys
{
    public class SysService : Singleton<SysService>,IStateCallback
    {
        private ArrayList<SysBase> _modules;
        public override bool Initialize()
        {
            StateService.GetInstance().AddCallback(this);
            _modules = new ArrayList<SysBase>();
            return true;
        }

        public override void Uninitialize()
        {
            for (int i = _modules.Count - 1; i >= 0; --i)
            {
                _modules[i].Destroy();
            }
            _modules.Clear();
            _modules.Release();
            _modules = null;
            StateService.GetInstance().RemoveCallback(this);
        }

        public void OnStateChanged(string srcSt, string curSt, object usrData)
        {
            for (int i = 0; i < _modules.Count; i++)
            {
                _modules[i].OnStateChanged(srcSt, curSt, usrData);
            }
        }

        /// <summary>
        /// 统一创建系统
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public SysBase Create<T>() where T : SysBase, new()
        {
            T module = new T();
            module.Create();

            _modules.Add(module);
            return module;
        }

        /// <summary>
        /// 获取系统 一般情况下不需要 
        /// APP开发过程中 由于存在内嵌系统 相关view 可以用 
        /// 原则上一个系统的界面 基本都是 Form 或则PopUp
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public SysBase GetSys<T>() where T : SysBase
        {
            for (int i = 0; i < _modules.Count; i++)
            {
                SysBase mm = _modules[i];
                if (mm.GetType() == typeof(T))
                {
                    return mm;
                }
            }
            return null;
        }
    }
}
