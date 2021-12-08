/********************************************************************
	created:	15:1:2018   15:08
	author:		OneJun
	purpose:	调节相机适配 分辨率
*********************************************************************/
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace MonoB.Msic
{
    public enum CameraAdapterType
    {
        BaseWithWidth,
        BaseWithHeight,
        Full,
    }

    public class CameraAdapter : MonoBehaviour
    {
        public CameraAdapterType AdapterType = CameraAdapterType.Full;
        public float OrthographicSize = 6.4f;
        public float ManualWidth = 720.0f;
        public float ManualHeight = 1280.0f;
        public Camera Cam;

        private float _oriFieldOfView = 1.0f;

        void Awake()
        {
            if (Cam == null)
            {
                Cam = gameObject.GetComponent<Camera>();
            }
            if (Cam != null)
            {
                _oriFieldOfView = Cam.fieldOfView;
            }
        }

        private void OnEnable()
        {
            DoAdapter();
        }


        void DoAdapter()
        {
            if (Cam == null)
            {
                Cam = gameObject.GetComponent<Camera>();
            }
            if (null == Cam)
            {
                return;
            }
            float sw = (float)Screen.width;
            float sh = (float)Screen.height;

#if UNITY_EDITOR
            string[] res = UnityStats.screenRes.Split('x');
           GLog.LogD(int.Parse(res[0]) + " " + int.Parse(res[1]));
            sw = int.Parse(res[0]);
            sh = int.Parse(res[1]);
#endif
            //正交模式
            if (Cam.orthographic)
            {
                float oriSize = OrthographicSize;
                float hwRatio = ManualHeight / ManualWidth;
                float curHWRatio = sh / sw;
                float changedSize = oriSize * (curHWRatio / hwRatio);
                Cam.orthographicSize = changedSize;
            }
            else
            {
                //透视模式
                float designAspect = ManualWidth / ManualHeight;
                float deviceAspect = sw / sh;
                float activeAspect = 0;
                if (AdapterType == CameraAdapterType.Full)
                {
                    float wr = sw / ManualWidth;
                    float hr = sh / ManualHeight;
                    float activeH = sh;
                    float activeW = sw;
                    float offR = 0f;
                    if (wr > hr)
                    {
                        activeH = ManualHeight * wr;
                        offR = (activeH - sh) / sh;
                        if (deviceAspect > designAspect)
                        {
                            float changedSize = _oriFieldOfView * (1.0f - offR);
                            Cam.fieldOfView = changedSize;
                        }
                    }
                    else
                    {
                        activeW = ManualWidth * hr;
                        offR = (activeW - sw) / sw;
                        //透视模式
                        if (deviceAspect > designAspect)
                        {
                            float changedSize = _oriFieldOfView * (1.0f - offR);
                            Cam.fieldOfView = changedSize;
                        }
                    }
                }
                else if (AdapterType == CameraAdapterType.BaseWithWidth)
                {
                    float wr = sw / ManualWidth;
                    float activeh = ManualHeight * wr;
                    activeAspect = sw / activeh;
                    //透视模式
                    float changedSize = _oriFieldOfView * (activeAspect / designAspect);
                    Cam.fieldOfView = changedSize;
                }
                else if (AdapterType == CameraAdapterType.BaseWithHeight)
                {
                    float hr = sh / ManualHeight;
                    float activew = ManualWidth * hr;
                    activeAspect = activew / sh;
                    //透视模式
                    float changedSize = _oriFieldOfView * (activeAspect / designAspect);
                    Cam.fieldOfView = changedSize;
                }
            }
        }

        [ContextMenu("执行")]
        public void Reposition()
        {
            DoAdapter();
        }
    }
}