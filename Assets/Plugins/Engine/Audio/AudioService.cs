/********************************************************************
  created:  2020-06-05         
  author:    OneJun           

  purpose:   游戏播放背景音乐 和音效服务               
*********************************************************************/
using UnityEngine;
using Engine.Asset;
using Engine.Schedule;
using System.Collections.Generic;
using Engine.Base;

namespace Engine.Audio
{
    /// <summary>
    /// 声音通道类型定义
    /// </summary>
    public enum AudioChannelType
    {
        //背景音乐
        MUSIC = 1,
        //UI音效
        UI = 2,
        //特效通道 各种效果
        EF = 3,
        //对话语音
        VOICE = 4
    }

    public class AudioService : Singleton<AudioService>, IScheduleHandler
    {
        private float _lastMusicVolume = 0.9f;
        private float _lastUIVolume = 0.8f;
        private float _lastEFVolume = 1.0f;
        private float _lastVoiceVolume = 0.8f;

        private float _curMusicVolume = 1f;
        private float _curUIVolume = 1f;
        private float _curEFVolume = 1f;
        private float _curVoiceVolume = 1f;
        private bool _lock = false;

        //音效资源缓存 缓存UI 和特效的
        private ObjDictionary<string, AudioAsset> _clipCacheDic;
        private AudioWorker _worker;
        private BaseAsset _workAsset;
        //动态音效播放
        private ObjDictionary<int, DAudio> _dAudioDic;
        private int _dAudioIndex = 0;
        public AudioWorker Worker
        {
            get
            {
                return _worker;
            }
        }


        public override bool Initialize()
        {
            _workAsset = AssetService.GetInstance().LoadInstantiateAsset("Fixed/AudioWorker", LifeType.Manual);
            if (_workAsset != null)
            {
                _workAsset.RootGo.ExtDontDestroyOnLoad();
                _workAsset.RootGo.ExtSetActive(true);
                _workAsset.RootGo.name = "AudioWorker";
                _workAsset.RootTf.parent = null;
                //
                _worker = _workAsset.RootGo.GetComponent<AudioWorker>();
            }
            //资源池
            _clipCacheDic = new ObjDictionary<string, AudioAsset>(10);

            if (PlayerPrefs.HasKey(System.Enum.GetName(typeof(AudioChannelType), AudioChannelType.MUSIC)))
            {
                float volume = PlayerPrefs.GetFloat(System.Enum.GetName(typeof(AudioChannelType), AudioChannelType.MUSIC), 1);
                _curMusicVolume = volume;
            }

            if (PlayerPrefs.HasKey(System.Enum.GetName(typeof(AudioChannelType), AudioChannelType.UI)))
            {
                float volume = PlayerPrefs.GetFloat(System.Enum.GetName(typeof(AudioChannelType), AudioChannelType.UI), 1);
                _curUIVolume = volume;
            }

            if (PlayerPrefs.HasKey(System.Enum.GetName(typeof(AudioChannelType), AudioChannelType.EF)))
            {
                float volume = PlayerPrefs.GetFloat(System.Enum.GetName(typeof(AudioChannelType), AudioChannelType.EF));
                _curEFVolume = volume;
            }

            if (PlayerPrefs.HasKey(System.Enum.GetName(typeof(AudioChannelType), AudioChannelType.VOICE)))
            {
                float volume = PlayerPrefs.GetFloat(System.Enum.GetName(typeof(AudioChannelType), AudioChannelType.VOICE));
                _curVoiceVolume = volume;
            }

            _lastMusicVolume = _curMusicVolume;
            _lastUIVolume = _curUIVolume;
            _lastEFVolume = _curEFVolume;
            _lastVoiceVolume = _curVoiceVolume;
            //
            if (_worker != null)
            {
                _worker.Initialize(_curMusicVolume, _curUIVolume, _curEFVolume, _curVoiceVolume);
            }
            //挂接全局UI 按钮
            Engine.UGUI.UIButton.PlayAudioHandler = PlayUIAudio;
            this.AddTimer(250, true);
            //动态
            _dAudioDic = new ObjDictionary<int, DAudio>(5);

            return true;
        }

        //--------------------------------音效资源 池化-------------------------
        private AudioClip AcquireAudioClip(string assetPath)
        {
            AudioAsset ap;
            if (_clipCacheDic.TryGetValue(assetPath, out ap))
            {
                return ap.Clip;
            }
            else
            {
                AudioAsset aa = AssetService.GetInstance().LoadAudioAsset(assetPath, LifeType.Manual);
                if (aa != null)
                {
                    _clipCacheDic.Add(assetPath, aa);
                    return aa.Clip;
                }
                else
                {
                    GGLog.LogE("Load Audio Clip File Failed:" + assetPath);
                }
            }
            return null;
        }

        //释放Cache的音效 除什么外
        public void UnloadAudioClipCache(string exclusive = "Common/Audio/Ef_Btn_Click")
        {
            if (_clipCacheDic != null)
            {
                List<string> ks = new List<string>(_clipCacheDic.Keys);
                for (int i = 0; i < ks.Count; i++)
                {
                    if (!ks[i].Equals(exclusive))
                    {
                        AudioAsset aa = _clipCacheDic[ks[i]];
                        AssetService.GetInstance().Unload(aa);
                        _clipCacheDic.Remove(ks[i]);
                    }
                }
            }
        }

        //-------------------------end----------------------

        public override void Uninitialize()
        {
            this.RemoveTimer();
            if (_worker != null)
            {
                _worker.Uninitialize();
                _worker = null;
            }
            if (_workAsset != null)
            {
                AssetService.GetInstance().Unload(_workAsset);
                _workAsset = null;
            }
            Engine.UGUI.UIButton.PlayAudioHandler = null;
            ////保存记录
            //PlayerPrefs.SetFloat(System.Enum.GetName(typeof(AudioChannelType), AudioChannelType.MUSIC), _curMusicVolume);
            //PlayerPrefs.SetFloat(System.Enum.GetName(typeof(AudioChannelType), AudioChannelType.EF), _curEFVolume);
            //PlayerPrefs.SetFloat(System.Enum.GetName(typeof(AudioChannelType), AudioChannelType.UI), _curUIVolume);
            //PlayerPrefs.SetFloat(System.Enum.GetName(typeof(AudioChannelType), AudioChannelType.VOICE), _curVoiceVolume);
            //PlayerPrefs.Save();
            //
            if (_clipCacheDic != null)
            {
                List<string> ks = new List<string>(_clipCacheDic.Keys);
                for (int i = 0; i < ks.Count; i++)
                {
                    AudioAsset aa = _clipCacheDic[ks[i]];
                    AssetService.GetInstance().Unload(aa);
                }
                //
                _clipCacheDic.Clear();
                _clipCacheDic = null;
            }
            //动态音频
            if (_dAudioDic != null)
            {
                List<int> ks = new List<int>(_dAudioDic.Keys);
                for (int i = 0; i < ks.Count; i++)
                {
                    DAudio aa = _dAudioDic[ks[i]];
                    aa.UnInit();
                }
                //
                _dAudioDic.Clear();
                _dAudioDic = null;
            }
        }

        private int releaseCnt = 0;
        void IScheduleHandler.OnScheduleHandle(ScheduleType type, uint id)
        {
            if (_worker != null)
            {
                _worker.LogicUpdate(Time.time);
            }
            //
            releaseCnt++;
            //25秒
            if (releaseCnt > 100)
            {
                releaseCnt = 0;
                //释放一次缓存
                UnloadAudioClipCache();
            }
        }

        /// 挂接全局播放UI 音效
        private void PlayUIAudio(string resName)
        {
            if (string.IsNullOrEmpty(resName))
            {
                return;
            }
            if (_worker != null)
            {
                _worker.PlayUI(AcquireAudioClip(resName));
            }
        }

        //播放
        public void Play(AudioChannelType tt, string resName, bool loop)
        {
            if (_worker == null)
            {
                return;
            }
            if (tt == AudioChannelType.MUSIC)
            {
                _worker.PlayMusic(resName, loop);
            }
            else if (tt == AudioChannelType.EF)
            {
                _worker.PlayEF(AcquireAudioClip(resName));
            }
            else if (tt == AudioChannelType.UI)
            {
                _worker.PlayUI(AcquireAudioClip(resName));
            }
            else if (tt == AudioChannelType.VOICE)
            {
                _worker.PlayVoice(resName);
            }
        }

        //停止单个通道
        public void StopChannel(uint eChannelType)
        {
            if (_worker == null)
            {
                return;
            }
            AudioChannelType tt = (AudioChannelType)eChannelType;
            if (tt == AudioChannelType.MUSIC)
            {
                _worker.PauseMusic();
            }
            else if (tt == AudioChannelType.EF)
            {

            }
            else if (tt == AudioChannelType.UI)
            {

            }
            else if (tt == AudioChannelType.VOICE)
            {
                _worker.StopVocie();
            }
        }

        //继续某个通道
        public void ContinueChannel(uint eChannelType)
        {
            if (_worker == null)
            {
                return;
            }
            AudioChannelType tt = (AudioChannelType)eChannelType;
            if (tt == AudioChannelType.MUSIC)
            {
                _worker.UnPauseMusic();
            }
            else if (tt == AudioChannelType.EF)
            {

            }
            else if (tt == AudioChannelType.UI)
            {

            }
            else if (tt == AudioChannelType.VOICE)
            {
            }
        }
        //关闭所有通道声音
        public void CloseAll()
        {
            if (_lock == false)
            {
                _lastMusicVolume = _curMusicVolume;
                _lastUIVolume = _curUIVolume;
                _lastEFVolume = _curEFVolume;
                _lastVoiceVolume = _curVoiceVolume;
                _curMusicVolume = 0;
                _curUIVolume = 0;
                _curEFVolume = 0;
                _curVoiceVolume = 0;
                if (_worker != null)
                {
                    _worker.CloseAll();
                }
                _lock = true;
            }
        }

        //打开所有通道声音
        public void OpenAll()
        {
            _curMusicVolume = _lastMusicVolume;
            _curUIVolume = _lastUIVolume;
            _curEFVolume = _lastEFVolume;
            _curVoiceVolume = _lastVoiceVolume;
            if (_worker != null)
            {
                _worker.OpenAll(_curMusicVolume, _curUIVolume, _curEFVolume, _curVoiceVolume);
            }
            _lock = false;
        }

        //获取对应通道音量
        public float GetVolumeByChannel(uint eChannelType)
        {
            AudioChannelType tt = (AudioChannelType)eChannelType;
            switch (tt)
            {
                case AudioChannelType.MUSIC:
                    return _curMusicVolume;
                case AudioChannelType.UI:
                    return _curUIVolume;
                case AudioChannelType.VOICE:
                    return _curVoiceVolume;
                case AudioChannelType.EF:
                    return _curEFVolume;
                default:
                    return 1.0f;
            }
        }

        //设置音量
        public void SetVolumeByChannel(uint eChannelType, float in_value)
        {
            if (in_value < 0)
                in_value = 0;
            if (in_value > 1.0f)
                in_value = 1.0f;
            //设置
            if (_worker != null)
            {
                _worker.SetChannelVolume(eChannelType, in_value);
            }
            //记录
            AudioChannelType tt = (AudioChannelType)eChannelType;
            if (tt == AudioChannelType.MUSIC)
            {
                _curMusicVolume = in_value;
                _lastMusicVolume = in_value;
                PlayerPrefs.SetFloat(System.Enum.GetName(typeof(AudioChannelType), AudioChannelType.MUSIC), _curMusicVolume);
            }
            else if (tt == AudioChannelType.EF)
            {
                _curEFVolume = in_value;
                _lastEFVolume = in_value;
                PlayerPrefs.SetFloat(System.Enum.GetName(typeof(AudioChannelType), AudioChannelType.EF), _curEFVolume);
                SetDAudioVolume(_curEFVolume);
                //
            }
            else if (tt == AudioChannelType.UI)
            {
                _curUIVolume = in_value;
                _lastUIVolume = in_value;
                PlayerPrefs.SetFloat(System.Enum.GetName(typeof(AudioChannelType), AudioChannelType.UI), _curUIVolume);
            }
            else if (tt == AudioChannelType.VOICE)
            {
                _curVoiceVolume = in_value;
                _lastVoiceVolume = in_value;
                PlayerPrefs.SetFloat(System.Enum.GetName(typeof(AudioChannelType), AudioChannelType.VOICE), _curVoiceVolume);
            }
            PlayerPrefs.Save();
        }


        //--------------------------动态音效相关-----------------
        private void SetDAudioVolume(float vv)
        {
            //动态音频
            if (_dAudioDic != null)
            {
                List<int> ks = new List<int>(_dAudioDic.Keys);
                for (int i = 0; i < ks.Count; i++)
                {
                    DAudio aa = _dAudioDic[ks[i]];
                    aa.SetVolume(vv);
                }
            }
        }

        public int GenDAudio(string path, bool isLoop)
        {
            _dAudioIndex++;
            DAudio da = new DAudio();
            da.Init(_dAudioIndex, path, _worker.transform, _curEFVolume, _worker.GetEfAMG(), isLoop);
            _dAudioDic.Add(_dAudioIndex, da);
            return _dAudioIndex;
        }

        public void PlayDAudio(int id)
        {
            DAudio da;
            if (_dAudioDic.TryGetValue(id, out da))
            {
                da.Play();
            }
            else
            {
                GGLog.LogE("PlayDAudio Error No Gen");
            }
        }

        public void PauseDAudio(int id)
        {
            DAudio da;
            if (_dAudioDic.TryGetValue(id, out da))
            {
                da.Pause();
            }
            else
            {
                GGLog.LogE("PauseDAudio Error No Gen");
            }
        }

        public void UnPauseDAudio(int id)
        {
            DAudio da;
            if (_dAudioDic.TryGetValue(id, out da))
            {
                da.UnPause();
            }
            else
            {
                GGLog.LogE("PauseDAudio Error No Gen");
            }
        }

        public void StopDAudio(int id)
        {
            DAudio da;
            if (_dAudioDic.TryGetValue(id, out da))
            {
                da.Stop();
            }
            else
            {
                GGLog.LogE("PauseDAudio Error No Gen");
            }
        }

        public void DestoryDAudio(int id)
        {
            DAudio da;
            if (_dAudioDic.TryGetValue(id, out da))
            {
                da.UnInit();
                _dAudioDic.Remove(id);
            }
            else
            {
                GGLog.LogE("DestoryDAudio Error No Gen");
            }
        }
    }
}
