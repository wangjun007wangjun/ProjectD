using System.Collections.Generic;
using Engine.Base;
using UnityEngine;

namespace Engine.State
{
    public interface IState
    {
        /// <summary>
        ///     初始化状态
        /// </summary>
        void InitializeState();

        /// <summary>
        /// 反初始化状态
        /// </summary>
        void UninitializeState();

        /// <summary>
        /// 状态名
        /// </summary>
        /// <returns></returns>
        string Name();

        /// <summary>
        /// 状态进入状态栈
        /// </summary>
        void OnStateEnter(object usrData = null);

        /// <summary>
        /// 状态退出状态栈
        /// </summary>
        void OnStateLeave();

        /// <summary>
        /// 状态切换结束
        /// </summary>
        void OnStateChanged(string srcSt,string curSt,object usrData);

    }

    /// <summary>
    /// 状态接口
    /// </summary>
    public interface IStateCallback
    {
        void OnStateChanged(string srcSt, string curSt, object usrData);
    }

    /// <summary>
    /// 游戏状态服务 
    /// </summary>
    public class StateService : Singleton<StateService>
    {
        //注册的转态列表
        private ObjDictionary<string, IState> _registedState = new ObjDictionary<string, IState>();
        //当前状态
        private IState _curState = null;

        private readonly List<IStateCallback> _callback = new List<IStateCallback>();

        /// <summary>
        /// 当前状态
        /// </summary>
        public string CurrentState
        {
            get
            {
                return (_curState!=null)? _curState.Name() : string.Empty;
            }
        }

        public override bool Initialize()
        {
            return true;
        }

        public override void Uninitialize()
        {
            _curState = null;
            if (_registedState != null)
            {
                foreach(KeyValuePair<string,IState> kvp in _registedState)
                {
                    kvp.Value.UninitializeState();
                }
                _registedState.Clear();
                _registedState = null;
            }
            
        }

        /// <summary>
        /// 注册游戏状态
        /// </summary>
        /// <param name="state"></param>
        public void RegisteState(IState state)
        {
            if (_registedState == null)
            {
                _registedState = new ObjDictionary<string, IState>();
            }
            string name = state.Name();
            if (string.IsNullOrEmpty(name))
            {
                GGLog.LogE("StateService: RegisteState Error St Name");
                return;
            }
            if (_registedState.ContainsKey(name))
            {
                GGLog.LogE("StateService:Already Registed:"+name);
                return;
            }
            if (state != null)
            {
                state.InitializeState();
            }
            _registedState.Add(name, state);
        }


        /// <summary>
        /// 添加状态回调
        /// </summary>
        /// <param name="callback">回调接口</param>
        public void AddCallback(IStateCallback callback)
        {
            if (callback == null)
            {
                GGLog.LogE("StateService.AddCallback : invalid parameter");
                return;
            }

            if (_callback.Contains(callback))
            {
                GGLog.LogE("StateService.AddCallback : duplicate add call back - {0}", callback.GetType().FullName);
                return;
            }

            _callback.Add(callback);
        }

        /// <summary>
        /// 移除状态回调
        /// </summary>
        /// <param name="callback">回调接口</param>
        public void RemoveCallback(IStateCallback callback)
        {
            if (callback == null)
            {
                GGLog.LogE("StateService.RemoveCallback : invalid parameter");
                return;
            }
            _callback.Remove(callback);
        }


        /// <summary>
        /// 切换状态
        /// </summary>
        /// <param name="targetState">目标状态</param>
        /// <param name="userData">用户自定义数据</param>
        public void ChangeState(string targetState, object userData = null)
        {
            if (string.IsNullOrEmpty(targetState))
            {
                GGLog.LogE("StateService.ChangeState : invalid parameter");
                return;
            }
            if (string.Equals(this.CurrentState,targetState))
            {
                GGLog.LogE("StateService.ChangeState : already existed");
                return;
            }
            //
            string fromState = this.CurrentState;
            if (_curState != null)
            {
                _curState.OnStateLeave();
                _curState = null;
            }
            //
            IState cState = null;
            if (_registedState.TryGetValue(targetState, out cState))
            {
                _curState = cState;
                _curState.OnStateEnter(userData);
            }
            else
            {
                GGLog.LogE("StateService.ChangeState : not registed");
                return;
            }
            //结束
            string toState = cState.Name();
            List<string> kks = new List<string>(_registedState.Keys);
            for (int i = 0; i < kks.Count; i++)
            {
                IState ap = _registedState[kks[i]];
                string stName = ap.Name();
                //非原状态 和非新状态 告知状态
                if (ap != null && (!stName.Equals(fromState)) && (!stName.Equals(toState)))
                {
                    ap.OnStateChanged(fromState, toState, userData);
                }
            }
            //call back
            for (int j = 0; j < _callback.Count; j++)
            {
                _callback[j].OnStateChanged(fromState, toState, userData);
            }
        }
    }
}
