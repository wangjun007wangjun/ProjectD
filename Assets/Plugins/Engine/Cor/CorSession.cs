/********************************************************************
    created:	2020-04-15 				
    author:		OneJun						
    purpose:	单个协程							
*********************************************************************/
using System.Collections;
using UnityEngine;
using System.Security.Cryptography;
using System;
using Engine.Base;

namespace Engine.Cor
{
    public class CorSession
    {
        public uint Id;
        public string Tag;
        public IEnumerator CorFunc;
        public bool IsRunning = false;
        
        //内部回调
        public Action<CorSession> InnerFinishedHandler;
        //外部回调
        public Action<uint> OutFinishedHander;

        //驱动
        public IEnumerator DriveWrapper()
        {
            IEnumerator e = CorFunc;
            while (IsRunning)
            {
                if (e != null && e.MoveNext())
                {
                    yield return e.Current;
                }
                else
                {
                    IsRunning = false;
                }
            }

            if (InnerFinishedHandler != null)
            {
                InnerFinishedHandler(this);
            }
        }
    }
}
