/********************************************************************
	created:	2019-5-15
	author:		OneJun
	
	purpose:	动态音效播放
*********************************************************************/
using Engine.Asset;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Engine.Base;

namespace Engine.Audio
{
    public class DAudio
    {
        //动态Id
        public int Id = 0;
        //
        private AudioSource _source;
        private AudioAsset _asset;
        private GameObject _sourceGo;

        public void Init(int id,string audioPath, Transform parent, float volume, AudioMixerGroup ag, bool isLoop)
        {
            Id = id;
            _sourceGo = new GameObject("DAudio_" + id.ToString() + "_" + audioPath);
            _source = _sourceGo.AddComponent<AudioSource>();
            _source.playOnAwake = false;
            _source.loop = isLoop;
            _source.outputAudioMixerGroup = ag;
            _source.volume = volume;
            _sourceGo.transform.parent = parent;
            _asset = AssetService.GetInstance().LoadAudioAsset(audioPath);
            if (_asset != null)
            {
                _source.clip = _asset.Clip;
            }
            else
            {
                GGLog.LogE("Load DAudio Clip File Failed:" + audioPath);
            }
        }

        public void UnInit()
        {
            if (_source != null)
            {
                _source.Stop();
                _source.clip = null;
                _source = null;
            }
            if (_asset != null)
            {
                AssetService.GetInstance().Unload(_asset);
                _asset = null;
            }
            if (_sourceGo != null)
            {
                GameObject.Destroy(_sourceGo);
                _sourceGo = null;
            }
        }

        public void Play()
        {
            if (_source != null)
            {
                if (!_source.isPlaying)
                {
                    _source.Play();
                }
            }
        }

        public void Pause()
        {
            if (_source != null)
            {
                _source.Pause();
            }
        }

        public void UnPause()
        {
            if (_source != null)
            {
                if (!_source.isPlaying)
                {
                    _source.Play();
                }
                else
                {
                    _source.UnPause();
                }
            }
        }


        public void Stop()
        {
            if (_source != null)
            {
                _source.Stop();
            }
        }

        public void SetVolume(float vv)
        {
            if (_source != null)
            {
                _source.volume = vv;
            }
        }
    }
}


