using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Engine.UGUI
{
    public class UIToggleGroup : UIComponent
    {
        [Header("开关互斥锁")]
        public ToggleGroup Group;
        [Header("所有开关")]
        public Toggle[] Toggles;
        [Header("所有开关描述文本")]
        public UnityEngine.UI.Text[] ToggleTexts;
        [Header("互斥模式")]
        public bool IsMutexMode = true;
        //
        private System.Action<int, bool> _handler;
        //初始化
        public override void Initialize(UIForm form)
        {
            if (_isInited)
            {
                return;
            }
            base.Initialize(form);
        }

        public void InitData(List<string> items, List<int> defaultOnIndxs, bool isMutMode)
        {
            List<string> list = items;
            List<int> defaultOnIdxs = defaultOnIndxs;
            //
            IsMutexMode = isMutMode;
            //
            if (list == null || list.Count == 0)
            {
                //隐藏
                if (Toggles != null)
                {
                    for (int i = 0; i < Toggles.Length; i++)
                    {
                        Toggles[i].gameObject.SetActive(false);
                    }
                }
                return;
            }
            //重置事件
            if (Toggles != null)
            {
                for (int i = 0; i < Toggles.Length; i++)
                {
                    Toggles[i].onValueChanged.RemoveAllListeners();
                }
            }
            _handler = null;

            //互斥模式
            if (IsMutexMode)
            {
                if (Group != null)
                {
                    Group.allowSwitchOff = false;
                    if (defaultOnIdxs == null || defaultOnIdxs.Count == 0)
                    {
                        Group.allowSwitchOff = true;
                    }
                }
                if (Toggles != null)
                {
                    for (int i = 0; i < Toggles.Length; i++)
                    {
                        Toggles[i].group = Group;
                    }
                }
            }
            else
            {
                if (Toggles != null)
                {
                    for (int i = 0; i < Toggles.Length; i++)
                    {
                        Toggles[i].group = null;
                    }
                }
            }
            //显示文本
            if (ToggleTexts != null)
            {
                for (int i = 0; i < ToggleTexts.Length; i++)
                {
                    if (i < list.Count)
                    {
                        ToggleTexts[i].text = list[i];
                    }
                }
            }
            //开关显示 控制
            if (Toggles != null)
            {
                for (int i = 0; i < Toggles.Length; i++)
                {
                    //包含有没有
                    if (i < list.Count)
                    {
                        Toggles[i].gameObject.SetActive(true);
                        //默认是否打开
                        if (defaultOnIdxs != null)
                        {
                            bool isFind = false;
                            for (int j = 0; j < defaultOnIdxs.Count; j++)
                            {
                                //默认 打开里面有对应索引
                                if (i == defaultOnIdxs[j])
                                {
                                    Toggles[i].isOn = true;
                                    isFind = true;
                                    break;
                                }
                            }
                            if (isFind == false)
                            {
                                Toggles[i].isOn = false;
                            }
                        }
                        else
                        {
                            Toggles[i].isOn = false;
                        }
                    }
                    else
                    {
                        Toggles[i].gameObject.SetActive(false);
                    }
                }
            }

        }

        public void InitHandler(System.Action<int, bool> callBack)
        {
            if (Toggles != null)
            {
                for (int i = 0; i < Toggles.Length; i++)
                {
                    if (Toggles[i].gameObject.activeSelf)
                    {
                        int idx = i;
                        Toggles[i].onValueChanged.AddListener(delegate (bool isOn)
                        {
                            this.OnToggleEvent(idx, isOn);
                        });
                    }
                }
            }
            _handler = callBack;
        }

        private void OnToggleEvent(int idx, bool isOn)
        {
            if (IsMutexMode)
            {
                if (_handler != null && isOn)
                {
                    _handler(idx, isOn);
                }
            }
            else
            {
                if (_handler != null)
                {
                    _handler(idx, isOn);
                }
            }
        }

        private void OnDestroy()
        {
            BelongedForm = null;
            if (Toggles != null)
            {
                for (int i = 0; i < Toggles.Length; i++)
                {
                    Toggles[i].onValueChanged.RemoveAllListeners();
                }
            }
            _handler = null;
        }

        //关闭
        public override void OnClose()
        {
            if (Toggles != null)
            {
                for (int i = 0; i < Toggles.Length; i++)
                {
                    Toggles[i].onValueChanged.RemoveAllListeners();
                }
            }
            _handler = null;
        }

        //隐藏
        public override void OnHide()
        {

        }

        //显示
        public override void OnAppear()
        {

        }
    }
}
