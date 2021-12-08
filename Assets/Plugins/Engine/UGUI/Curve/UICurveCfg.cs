/********************************************************************
  created:  2020-03-31         
  author:    OneJun           
  purpose:   UI动画曲线 配置               
*********************************************************************/
using UnityEngine;
using System;

namespace Engine.UGUI
{
    public class UICurveCfg : MonoBehaviour
    {
        [Serializable]
        public class CurveData
        {
            public string Name;
            public AnimationCurve Curve;
        }

        public CurveData[] CurveDatas;

        public AnimationCurve GetCurve(string curveName)
        {
            if(string.IsNullOrEmpty(curveName))
            {
                return null;
            }

            for (int i = 0; i < CurveDatas.Length; i++)
            {
                if (string.Compare(CurveDatas[i].Name, curveName, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return CurveDatas[i].Curve;
                }
            }
            return null;
        }
    }
}