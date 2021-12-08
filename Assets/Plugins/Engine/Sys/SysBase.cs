using Engine.Base;

namespace Engine.Sys
{
    public abstract class SysBase
    {
        /// <summary>
        /// 创建
        /// </summary>
        public void Create()
        {
            OnInitializeSys();
        }

        /// <summary>
        /// 销毁
        /// </summary>
        public void Destroy()
        {
            OnUninitializeSys();
        }

        /// <summary>
        /// 初始化模块
        /// </summary>
        protected abstract void OnInitializeSys();

        /// <summary>
        /// 反初始化模块
        /// </summary>
        protected abstract void OnUninitializeSys();


        /// <summary>
        /// 游戏状态改变处理
        /// </summary>
        /// <param name="srcSt">上一个状态</param>
        /// <param name="curSt">当前状态</param>
        /// <param name="usrData">自定义数据</param>
        public virtual void OnStateChanged(string srcSt, string curSt, object usrData)
        {

        }

    }
}
