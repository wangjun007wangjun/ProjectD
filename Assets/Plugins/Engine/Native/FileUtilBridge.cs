/********************************************************************
	created:	24:11:2017   19:15
	author:		OneJun
	purpose:	系统文件操作 功能对接
*********************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using Engine.Base;
using Engine.Res;

namespace Engine.Native
{
    public static class FileUtilAndroid
    {
        // private static AndroidJavaClass javaBridge =
        //    new AndroidJavaClass("com.jw.plugin.FileUtilBridge");

        public static bool Android_IsFileExistInStreamingAssets(string fileName)
        {
            // if (javaBridge != null)
            // {
            //    return javaBridge.CallStatic<bool>("UnityIsFileExistInStreamingAssets", fileName);
            // }
            return false;
        }

        public static byte[] Android_ReadFileInAssets(string fileName)
        {
            // if (javaBridge != null)
            // {
            //     return javaBridge.CallStatic<byte[]>("UnityReadFileInAssets", fileName);
            // }
            // else
            // {
            //     return null;
            // }
            return null;
        }
    }

    public class JWEngineFileUtilProxy : FileUtilInterface
    {
        public bool Android_IsFileExistInStreamingAssets(string fileName)
        {
            return FileUtilAndroid.Android_IsFileExistInStreamingAssets(fileName);
        }

        public byte[] Android_ReadFileInAssets(string fileName)
        {
            return FileUtilAndroid.Android_ReadFileInAssets(fileName);
        }
    }


    public class FileUtilBridge : MonoBehaviour
    {
        //构建
        private static GameObject _sGo = null;
        //创建
        public static FileUtilBridge CreateBridge()
        {
            FileUtilBridge ret = null;
            if (_sGo == null)
            {
                _sGo = new GameObject("FileUtilBridge");
                _sGo.transform.parent = Engine.Base.SingletonManager.MonoSingletonGo.transform;
            }
            ret = _sGo.ExtAddComponent<FileUtilBridge>(true);
            return ret;
        }
        //销毁
        public static void DestroyBridge()
        {
            if (_sGo != null)
            {
                Destroy(_sGo);
                _sGo = null;
            }
        }

        //初始化
        public void Initialize()
        {

        }

        //反初始化
        public void Uninitialize()
        {
            return;
        }

        //------------------------回调by UnitySendMessage------------------------


        //----------------------end----------------
        #region Native Interface
#if UNITY_EDITOR || UNITY_STANDALONE
        public bool IsFileExistInStreamingAssets(string fileName)
        {
            return Engine.Res.FileUtil.IsFileExistInStreamingAssets(fileName);
        }

        public byte[] ReadFileInStreamingAssets(string fileName)
        {
            return Engine.Res.FileUtil.ReadFileInStreamingAssets(fileName);
        }

#elif UNITY_IOS
         
        public bool IsFileExistInStreamingAssets(string fileName)
        {
            return JWEngine.Res.FileUtil.IsFileExistInStreamingAssets(fileName);
        }

        public byte[] ReadFileInStreamingAssets(string fileName)
        {
            return JWEngine.Res.FileUtil.ReadFileInStreamingAssets(fileName);
        }

#elif UNITY_ANDROID

        public bool IsFileExistInStreamingAssets(string fileName)
        {
            return FileUtilAndroid.Android_IsFileExistInStreamingAssets(fileName);
        }

        public byte[] ReadFileInStreamingAssets(string fileName)
        {
            return FileUtilAndroid.Android_ReadFileInAssets(fileName);
        }
#endif
        #endregion
    }
}
