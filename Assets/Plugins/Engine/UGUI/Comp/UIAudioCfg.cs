/********************************************************************
  created:  2020-06-05         
  author:    OneJun           

  purpose:  UI 音效配置                
*********************************************************************/
using System;
using UnityEngine;

namespace Engine.UGUI
{
    public class UIAudioCfg : MonoBehaviour
    {
        [Serializable]
        public class AudioData
        {
            public string Name;
            public string ResName;
        }

        public AudioData[] AudioDatas;

        public bool GetAudio(string audioName,out string resName)
        {
            resName = null;
            if(string.IsNullOrEmpty(audioName))
            {
                return false;
            }

            for (int i = 0; i < AudioDatas.Length; i++)
            {
                if(string.Compare(AudioDatas[i].Name,audioName,StringComparison.OrdinalIgnoreCase) == 0)
                {
                    resName = AudioDatas[i].ResName;
                    return true;
                }
            }

            return false;
        }
    }
}