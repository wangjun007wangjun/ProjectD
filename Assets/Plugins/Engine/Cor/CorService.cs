/********************************************************************
    created:	2020-04-15 				
    author:		OneJun						
    purpose:	非 monob 提供协程服务 								
*********************************************************************/
using System.Collections;
using UnityEngine;
using System;
using Engine.Base;
namespace Engine.Cor
{
    public class CorService : MonoSingleton<CorService>
    {
        private uint _currentId = 1;
        private readonly ArrayList<CorSession> _sessions = new ArrayList<CorSession>(20);

        public override bool Initialize()
        {
            return true;
        }

        public override void Uninitialize()
        {
            _currentId = 1;
            for (int i = 0; i < _sessions.Count; ++i)
            {
                _sessions[i].IsRunning = false;
                GGLog.LogE("Coroutine start but not stop! {0}", _sessions[i].CorFunc);
            }
            _sessions.Clear();
        }

        /// <summary>
        /// 启动协程
        /// </summary>
        /// <param name="coroutine">函数</param>
        /// <returns>协程Id</returns>
        public uint StartCoroutineSession(IEnumerator coroutine,Action<uint> OnFinishedCallback=null)
        {
            uint id = 0;
            StartGlobalCoroutine(coroutine, null, OnFinishedCallback, ref id);
            return id;
        }

        /// <summary>
        /// 启动带tag 
        /// </summary>
        /// <param name="tag">tag</param>
        /// <param name="coroutine">函数</param>
        /// <returns>Id</returns>
        public uint StartCoroutineSessionWithTag(string tag, IEnumerator coroutine)
        {
            uint id = 0;
            StartGlobalCoroutine(coroutine, tag, null, ref id);
            return id;
        }

        /// <summary>
        /// 启动协程
        /// </summary>
        /// <param name="coroutine">协程函数</param>
        /// <param name="OnFinishedCallback">协程结束回调,包括强制终止</param>
        /// <returns>成功返回协程ID</returns>
        public uint StartCoroutineSession(IEnumerator coroutine, string tag, Action<uint> OnFinishedCallback)
        {
            uint id = 0;
            StartGlobalCoroutine(coroutine, tag, OnFinishedCallback, ref id);
            return id;
        }

        //do
        private Coroutine StartGlobalCoroutine(IEnumerator coroutine, string tag, Action<uint> OnFinishedCallback, ref uint id)
        {
            if (coroutine == null)
            {
                id = 0;
                return null;
            }

            CorSession data = new CorSession();
            data.Id = _currentId++;
            data.CorFunc = coroutine;
            data.IsRunning = true;
            data.Tag = tag;
            data.OutFinishedHander = OnFinishedCallback;
            data.InnerFinishedHandler = OnSessionFinished;
            _sessions.Add(data);
            id = data.Id;
            //real
            return StartCoroutine(data.DriveWrapper());
        }


        private void OnSessionFinished(CorSession data)
        {
            if (data.OutFinishedHander != null)
            {
                data.OutFinishedHander(data.Id);
            }
            data.InnerFinishedHandler = null;
            data.OutFinishedHander = null;
            _sessions.Remove(data);
        }

        /// <summary>
        /// 停止协程
        /// </summary>
        /// <param name="id">协程Id</param>
        public void StopCoroutineSession(uint id)
        {
            if (id == 0)
                return;

            for (int i = 0; i < _sessions.Count; ++i)
            {
                if (_sessions[i].Id == id)
                {
                    _sessions[i].IsRunning = false;
                    break;
                }
            }
        }

        /// <summary>
        /// 停止所有协程
        /// </summary>
        public void StopAllSession()
        {
            for (int i = 0; i < _sessions.Count; ++i)
            {
                _sessions[i].IsRunning = false;
            }
        }

        /// <summary>
        /// 停止一类
        /// </summary>
        /// <param name="tag"></param>
        public void StopAllSessionWithTag(string tag)
        {
            for (int i = 0; i < _sessions.Count; ++i)
            {
                CorSession coroutineData = _sessions[i];
                if (coroutineData.Tag != null && coroutineData.Tag == tag)
                {
                    _sessions[i].IsRunning = false;
                }
            }
        }
    }
}
