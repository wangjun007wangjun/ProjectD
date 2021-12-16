/********************************************************************
	created:	2019-5-5
	author:		OneJun
	
	purpose:	游戏音乐 音效播放
*********************************************************************/
using Engine.Asset;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Engine.Base;

namespace Engine.Audio
{
    public class AudioWorker : MonoBehaviour
    {
        //播放音乐
        public AudioSource M1;
        public AudioSource M2;
        //UI音效播放
        public AudioSource UI;
        //特效音效播放
        public AudioSource EF;
        //声音播放
        public AudioSource VO;
        //混合器
        public AudioMixer Mixer;

        //音乐
        //音乐淡入淡出 
        private const float MusicFadeInterval = 0.2f;
        private AudioAsset _lastMusicAsset;
        private AudioAsset _curMusicAsset;
        private AudioSource _lastMusicSource;
        private AudioSource _curMusicSource;
        //语音
        private AudioAsset _curVoiceAsset;
        private float _curVoiceEndTime;

        private void Start()
        {
            if (M1 == null)
            {
                M1 = transform.Find("M1").GetComponent<AudioSource>();
            }
            if (M2 == null)
            {
                M2 = transform.Find("M2").GetComponent<AudioSource>();
            }
            if (UI == null)
            {
                UI = transform.Find("UI").GetComponent<AudioSource>();
            }
            if (EF == null)
            {
                EF = transform.Find("EF").GetComponent<AudioSource>();
            }
            if (VO == null)
            {
                VO = transform.Find("VO").GetComponent<AudioSource>();
            }
        }

        private float ResetV2DB(float vv)
        {
            float rel = Mathf.Clamp01(vv);
            float db = rel * 80;
            return -(80.0f - db);
        }

        //初始化各通道音量
        public void Initialize(float mV, float uiV, float efV, float voV)
        {
            if (Mixer != null)
            {
                //主音量
                Mixer.SetFloat("MASTER", ResetV2DB(1.0f));
                Mixer.SetFloat("MUSIC", ResetV2DB(mV));
                Mixer.SetFloat("UI", ResetV2DB(uiV));
                Mixer.SetFloat("EF", ResetV2DB(efV));
                Mixer.SetFloat("VOICE", ResetV2DB(voV));
            }
        }

        public void Uninitialize()
        {
            if (M1 != null)
            {
                M1.clip = null;
            }
            if (M2 != null)
            {
                M2.clip = null;
            }
            if (VO != null)
            {
                VO.clip = null;
            }
            if (_lastMusicAsset != null)
            {
                AssetService.GetInstance().Unload(_lastMusicAsset);
                _lastMusicAsset = null;
            }
            if (_curMusicAsset != null)
            {
                AssetService.GetInstance().Unload(_curMusicAsset);
                _curMusicAsset = null;
            }
            if (_curVoiceAsset != null)
            {
                AssetService.GetInstance().Unload(_curVoiceAsset);
                _curVoiceAsset = null;
            }
            _curMusicSource = null;
            _lastMusicSource = null;
        }

        public void PlayUI(AudioClip clip)
        {
            if (clip == null)
            {
                return;
            }
            if (UI != null)
            {
                UI.PlayOneShot(clip);
            }
        }

        public void PlayEF(AudioClip clip)
        {
            if (clip == null)
            {
                return;
            }
            if (EF != null)
            {
                EF.PlayOneShot(clip);
            }
        }

        public AudioMixerGroup GetEfAMG()
        {
            if (EF != null)
            {
                return EF.outputAudioMixerGroup;
            }
            return null;
        }

        public void PlayVoice(string voiceRes)
        {
            if (VO == null)
            {
                return;
            }
            VO.Stop();
            VO.clip = null;
            if (_curVoiceAsset != null)
            {
                AssetService.GetInstance().Unload(_curVoiceAsset);
                _curVoiceAsset = null;
            }
            //
            _curVoiceAsset = AssetService.GetInstance().LoadAudioAsset(voiceRes,LifeType.Trust);
            if (null != _curVoiceAsset && _curVoiceAsset.Clip != null)
            {
                _curVoiceEndTime = Time.time + _curVoiceAsset.Clip.length;
                //
                VO.clip = _curVoiceAsset.Clip;
                VO.Play();
            }
            else
            {
                GGLog.LogE("Load Audio Voice File Error:" + voiceRes);
            }
        }


        public void PlayMusic(string musicRes, bool isLoop = true)
        {
            if (_curMusicSource != null)
            {
                //上一个还未淡出完整
                if (_lastMusicSource != null)
                {
                    _lastMusicSource.volume = 0;
                    _lastMusicSource.Stop();
                    _lastMusicSource.loop = false;
                    _lastMusicSource.clip = null;
                    if (_lastMusicAsset != null)
                    {
                        AssetService.GetInstance().Unload(_lastMusicAsset);
                        _lastMusicAsset = null;
                    }
                    _lastMusicSource = null;
                }
                //接管
                _lastMusicSource = _curMusicSource;
                _lastMusicAsset = _curMusicAsset;
                //
                if (_curMusicSource == M1)
                {
                    _curMusicSource = M2;
                }
                else
                {
                    _curMusicSource = M1;
                }
            }
            else
            {
                _curMusicSource = M1;
            }
            //
            _curMusicAsset = AssetService.GetInstance().LoadAudioAsset(musicRes);
            if (null != _curMusicAsset && _curMusicAsset.Clip != null)
            {
                _curMusicSource.clip = _curMusicAsset.Clip;
                //准备淡入
                _curMusicSource.volume = 0.0f;
                _curMusicSource.loop = isLoop;
                _curMusicSource.Play();
            }
            else
            {
                GGLog.LogE("Load Audio Music File Error:" + musicRes);
                _curMusicSource = null;
                _curMusicAsset = null;
            }
        }

        //停止语音
        public void StopVocie()
        {
            VO.Stop();
            VO.clip = null;
            if (_curVoiceAsset != null)
            {
                AssetService.GetInstance().Unload(_curVoiceAsset);
                _curVoiceAsset = null;
            }
        }

        //暂停
        public void PauseMusic()
        {
            if (_curMusicSource != null)
            {
                _curMusicSource.Pause();
            }
        }

        public void UnPauseMusic()
        {
            if (_curMusicSource != null)
            {
                _curMusicSource.UnPause();
            }
        }
        public AudioSource GetCurMusicSource()
        {
            return _curMusicSource;
        }
        public void SetChannelVolume(uint eChannelType, float vv)
        {
            if (Mixer == null)
            {
                return;
            }
            AudioChannelType tt = (AudioChannelType)eChannelType;
            if (tt == AudioChannelType.MUSIC)
            {
                Mixer.SetFloat("MUSIC", ResetV2DB(vv));
            }
            else if (tt == AudioChannelType.EF)
            {
                Mixer.SetFloat("EF", ResetV2DB(vv));
            }
            else if (tt == AudioChannelType.UI)
            {
                Mixer.SetFloat("UI", ResetV2DB(vv));
            }
            else if (tt == AudioChannelType.VOICE)
            {
                Mixer.SetFloat("VOICE", ResetV2DB(vv));
            }
        }

        //打开所有通道
        public void OpenAll(float mV, float uiV, float efV, float voV)
        {
            if (Mixer != null)
            {
                //主音量
                Mixer.SetFloat("MASTER", ResetV2DB(1.0f));
                Mixer.SetFloat("MUSIC", ResetV2DB(mV));
                Mixer.SetFloat("UI", ResetV2DB(uiV));
                Mixer.SetFloat("EF", ResetV2DB(efV));
                Mixer.SetFloat("VOICE", ResetV2DB(voV));
            }
        }

        //关闭所有通道
        public void CloseAll()
        {
            if (Mixer != null)
            {
                //主音量
                Mixer.SetFloat("MASTER", ResetV2DB(0.0f));
            }
        }

        //驱动更新 主要用于释放资源
        public void LogicUpdate(float curTime)
        {
            //语音结束
            if (VO != null && VO.isPlaying && _curVoiceEndTime < curTime)
            {
                VO.Stop();
                VO.clip = null;
                if (_curVoiceAsset != null)
                {
                    AssetService.GetInstance().Unload(_curVoiceAsset);
                    _curVoiceAsset = null;
                }
            }
            //上一个音乐淡出
            if (_lastMusicSource != null)
            {
                if (_lastMusicSource.volume > 0)
                {
                    _lastMusicSource.volume -= MusicFadeInterval;
                }
                else
                {
                    _lastMusicSource.volume = 0;
                    _lastMusicSource.Stop();
                    _lastMusicSource.loop = false;
                    _lastMusicSource.clip = null;
                    if (_lastMusicAsset != null)
                    {
                        AssetService.GetInstance().Unload(_lastMusicAsset);
                        _lastMusicAsset = null;
                    }
                    //
                    _lastMusicSource = null;
                }
            }
            //当前淡入
            if (_curMusicSource != null && _curMusicSource.isPlaying)
            {
                if (_curMusicSource.volume < 1.0f)
                {
                    _curMusicSource.volume += MusicFadeInterval;
                }
            }
        }
    }
}
