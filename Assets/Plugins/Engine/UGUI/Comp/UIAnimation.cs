using System.Collections;
using System.Collections.Generic;
using Engine.Base;
using UnityEngine;

namespace Engine.UGUI
{
    /// <summary>
    /// 事件类型
    /// </summary>
    public enum UIAnimationEventType
    {
        AnimationStart,
        AnimationEnd,
    }
    public class UIAnimation : UIComponent
    {
        /// <summary>
        /// Unity Animation
        /// </summary>
        private Animation _animation;

        /// <summary>
        /// 当前正在播放的动画状态
        /// </summary>
        private AnimationState _currentAnimationState;
        private float _currentAnimationTime;

        public override void Initialize(UIForm form)
        {
            if (_isInited)
            {
                return;
            }
            base.Initialize(form);

            _animation = this.gameObject.GetComponent<Animation>();
            if (_animation != null)
            {
                if (_animation.playAutomatically && _animation.clip != null)
                {
                    _currentAnimationState = _animation[_animation.clip.name];
                    _currentAnimationTime = 0;
                    //派发事件
                    DispatchAnimationEvent(UIAnimationEventType.AnimationStart);
                }
            }
            else
            {
                GGLog.LogE("UIAnimation Need Aniamtion Component");
            }
        }

        /// <summary>
        /// Update is called every frame, if the MonoBehaviour is enabled.
        /// </summary>
        void Update()
        {
            if (_currentAnimationState == null)
            {
                return;
            }
            if (_currentAnimationState.wrapMode != WrapMode.Loop
            && _currentAnimationState.wrapMode != WrapMode.PingPong
            && _currentAnimationState.wrapMode != WrapMode.ClampForever
            )
            {
                if (_currentAnimationTime >= _currentAnimationState.length)
                {
                    DispatchAnimationEvent(UIAnimationEventType.AnimationEnd);
                    _currentAnimationState = null;
                    _currentAnimationTime = 0;
                }
                else
                {
                    _currentAnimationTime += Time.deltaTime;
                }
            }
        }

        /// <summary>
        /// 播放动画
        /// </summary>
        /// <param name="animName"></param>
        /// <param name="forceRewind"></param>
        public void PlayAnimation(string animName, bool forceRewind)
        {
            if (_currentAnimationState != null && _currentAnimationState.name.Equals(animName) && !forceRewind)
            {
                return;
            }

            if (_currentAnimationState != null)
            {
                _animation.Stop(_currentAnimationState.name);
                _currentAnimationState = null;
                _currentAnimationTime = 0;
            }

            _currentAnimationState = _animation[animName];
            _currentAnimationTime = 0;

            if (_currentAnimationState != null)
            {
                _animation.Play(animName);

                //派发事件
                DispatchAnimationEvent(UIAnimationEventType.AnimationStart);
            }
        }

        /// <summary>
        /// 停止播放动画
        /// </summary>
        /// <param name="animName"></param>
        public void StopAnimation(string animName)
        {
            if (_currentAnimationState == null || !_currentAnimationState.name.Equals(animName))
            {
                return;
            }

            _animation.Stop(animName);

            //派发事件
            DispatchAnimationEvent(UIAnimationEventType.AnimationEnd);

            _currentAnimationState = null;
            _currentAnimationTime = 0;
        }

        /// <summary>
        /// 返回当前播放的动画名
        /// </summary>
        /// <returns></returns>
        public string GetCurrentAnimation()
        {
            return (_currentAnimationState == null) ? null : _currentAnimationState.name;
        }

        /// <summary>
        /// 指定动画是否处于停止播放状态
        /// </summary>
        /// <param name="animationName"></param>
        /// <returns></returns>
        public bool IsAnimationStopped(string animationName)
        {
            if (string.IsNullOrEmpty(animationName) || _currentAnimationState == null || _currentAnimationTime == 0)
            {
                return true;
            }

            return (!string.Equals(_currentAnimationState.name, animationName));
        }

        //派发动画相关事件
        private void DispatchAnimationEvent(UIAnimationEventType animationEventType)
        {
            
        }

        /// <summary>
        /// 设置动画速度，1为正常速度
        /// </summary>
        /// <param name="animName"></param>
        /// <param name="speed"></param>
        public void SetAnimationSpeed(string animName, float speed)
        {
            if (_animation != null && _animation[animName] != null)
            {
                _animation[animName].speed = speed;
            }
        }
    }
}