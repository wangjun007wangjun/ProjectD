using UnityEngine;
using UnityEngine.EventSystems;
using Engine.Base;
using UnityEngine.UI;

namespace Engine.UGUI
{
    /// <summary>
    /// UI窗口层次切换管理
    /// </summary>
    public class UGUIRoot : Singleton<UGUIRoot>
    {
        private static string sFormCameraName = "Camera_Form";
        //LayerMask.NameToLayer("UI");
        private const int Const_FormCameraMaskLayer = 5;
        private const int Const_FormCameraDepth = 10;

        //Form列表
        private ObjList<UIForm> _forms;

        //Form打开顺序号
        private int _formOpenOrder;

        //Form序列号，可以作为每个Form的唯一标识
        private int _formSequence;

        //UI元素 Root
        private GameObject _uiRoot;

        public delegate void OnFormSortedDelegate(ObjList<UIForm> inForms);
        public OnFormSortedDelegate OnFormSortedHandler = null;
        //UI输入事件系统
        private EventSystem _uiInputEventSystem;

        //Camera
        private Camera _formCamera;
        public Camera FormCamera
        {
            get
            {
                return _formCamera;
            }
        }

        //Forms相关操作
        private bool _needSortForms = false;
        private bool _needUpdateRaycasterAndHide = false;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns>初始化成功/失败</returns>
        public override bool Initialize()
        {
            _forms = new ObjList<UIForm>();
            _formOpenOrder = 1;
            _formSequence = 0;
            //创建UIRoot
            CreateUIRoot();
            //创建EventSystem
            CreateEventSystem();
            //创建Camera
            CreateCamera();
            return true;
        }

        private void CreateUIRoot()
        {
            _uiRoot = new GameObject("UGUIRoot");
            GameObject.DontDestroyOnLoad(_uiRoot);
        }

        private void CreateEventSystem()
        {
            _uiInputEventSystem = UnityEngine.Object.FindObjectOfType<EventSystem>();
            if (_uiInputEventSystem == null)
            {
                GameObject eventSystem = new GameObject("EventSystem");
                _uiInputEventSystem = eventSystem.AddComponent<EventSystem>();
                eventSystem.AddComponent<StandaloneInputModule>();
            }
            _uiInputEventSystem.gameObject.transform.parent = _uiRoot.transform;
        }

        private void CreateCamera()
        {
            GameObject cameraObject = new GameObject(sFormCameraName);
            cameraObject.transform.SetParent(_uiRoot.transform, true);
            cameraObject.transform.localPosition = Vector3.zero;
            cameraObject.transform.localRotation = Quaternion.identity;
            cameraObject.transform.localScale = Vector3.one;
            //
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 50;
            camera.clearFlags = CameraClearFlags.Color;

            camera.cullingMask = 1 << Const_FormCameraMaskLayer;
            camera.depth = Const_FormCameraDepth;
            camera.useOcclusionCulling = false;
            //camera.allowDynamicResolution = false;
            camera.allowMSAA = false;
            camera.allowHDR = false;
            _formCamera = camera;
        }


        /// <summary>
        /// 反初始化
        /// </summary>
        public override void Uninitialize()
        {
            if (_forms != null)
            {
                //清理
                for (int i = 0; i < _forms.Count; i++)
                {
                    UIForm cur = _forms[i];
                    if (cur != null)
                    {
                        //转为关闭状态
                        if (cur.TurnToClosed(true))
                        {
                            cur.OnRemove();
                            _forms[i] = null;
                        }
                    }
                }
                _forms.Clear();
                _forms = null;
            }

            _formOpenOrder = 1;
            _formSequence = 0;
            if (_uiRoot != null)
            {
                GameObject.Destroy(_uiRoot);
                _uiRoot = null;
            }
            _formCamera = null;
            _uiInputEventSystem = null;
        }

        //统一驱动
        public void CustomUpdate()
        {
            for (int i = 0; i < _forms.Count;)
            {
                UIForm cur = _forms[i];
                //cur.CustomUpdate();
                if (cur.IsNeedClose())
                {
                    //转为关闭状态成功
                    if (cur.TurnToClosed(false))
                    {
                        //真正关闭
                        cur.OnRemove();
                        _forms.RemoveAt(i);
                        _needSortForms = true;
                        continue;
                    }
                }
                else if (cur.IsClosed())
                {
                    //移除
                    cur.OnRemove();
                    _forms.RemoveAt(i);
                    _needSortForms = true;
                    continue;
                }
                i++;
            }
            if (_needSortForms)
            {
                ProcessFormList(true, true);
            }
            else if (_needUpdateRaycasterAndHide)
            {
                ProcessFormList(false, true);
            }
            _needSortForms = false;
            _needUpdateRaycasterAndHide = false;
        }

        #region 输入控制
        //返回EventSystem
        public EventSystem GetEventSystem()
        {
            return _uiInputEventSystem;
        }

        public void DisableInput()
        {
            if (_uiInputEventSystem != null)
            {
                _uiInputEventSystem.gameObject.ExtSetActive(false);
            }
        }

        public void EnableInput()
        {
            if (_uiInputEventSystem != null)
            {
                _uiInputEventSystem.gameObject.ExtSetActive(true);
            }
        }
        #endregion

        #region 打开关闭控制
        private void OnUIFormEvent(UIFormEventType eType)
        {
            if (eType == UIFormEventType.VisibleChanged)
            {
                _needUpdateRaycasterAndHide = true;
            }
            if (eType == UIFormEventType.PriorityChanged)
            {
                _needSortForms = true;
            }
        }

        private UIForm GetUnClosedForm(string formPath)
        {
            for (int i = 0; i < _forms.Count; i++)
            {
                if (_forms[i] != null)
                {
                    if (_forms[i].FormPath.Equals(formPath) && !_forms[i].IsClosed())
                    {
                        return _forms[i];
                    }
                }
            }
            return null;
        }
        public void CloseGroupForm(int group)
        {
            if (group == 0)
            {
                return;
            }
            for (int i = 0; i < _forms.Count; i++)
            {
                if (_forms[i].GroupId == group)
                {
                    _forms[i].Close();
                }
            }
        }

        public UIForm OpenForm(UIForm formCpt, bool useCamera = true)
        {
            if (formCpt == null)
            {
                GGLog.LogE("Open Form Error Null Arg");
                return null;
            }
            UIForm old;
            //检查同名Form是否存在
            old = GetUnClosedForm(formCpt.FormPath);
            //只有一个 重新打开
            if (old != null && old.IsSingleton)
            {
                GGLog.LogD("Open Singleton Form" + formCpt.FormPath);
                //更新sequence
                old.Open(_formSequence, _formOpenOrder, true);
                _formSequence++;
                _formOpenOrder++;
                _needSortForms = true;
                return old;
            }
            //
            GameObject formGo = formCpt.gameObject;
            if (formGo == null)
            {
                GGLog.LogE("Form " + formCpt.FormPath + " Open Fail!!!");
                return null;
            }
            if (!formGo.activeSelf)
            {
                formGo.ExtSetActive(true);
            }
            if (formGo.transform.parent != _uiRoot.transform)
            {
                formGo.transform.SetParent(_uiRoot.transform);
            }
            //设置参数
            if (formCpt != null)
            {
                formCpt.Open(useCamera ? _formCamera : null, _formSequence, _formOpenOrder, false);
                //close 同组Form
                if (formCpt.GroupId > 0)
                {
                    CloseGroupForm(formCpt.GroupId);
                }
                //监听
                formCpt.EventHandler += this.OnUIFormEvent;
                _forms.Add(formCpt);
            }
            _formSequence++;
            _formOpenOrder++;
            _needSortForms = true;
            return formCpt;
        }

        public void CloseForm(UIForm form)
        {
            if (form == null)
            {
                return;
            }
            if (_forms == null)
            {
                return;
            }
            for (int i = 0; i < _forms.Count; i++)
            {
                if (_forms[i] == form)
                {
                    _forms[i].Close();
                }
            }
        }

        private void ProcessFormList(bool sort, bool handleInputAndHide)
        {
            if (sort)
            {
                _forms.Sort();

                if (true)   //(m_formOpenOrder > 10)
                {
                    for (int i = 0; i < _forms.Count; i++)
                    {
                        _forms[i].SetDisplayOrder(i + 1);
                    }
                    _formOpenOrder = _forms.Count + 1;
                }
            }
            if (handleInputAndHide)
            {
                UpdateFormHided();
                UpdateFormRaycaster();
            }
            if (OnFormSortedHandler != null)
            {
                OnFormSortedHandler(_forms);
            }
        }

        private void UpdateFormHided()
        {
            bool needHide = false;

            for (int i = _forms.Count - 1; i >= 0; i--)
            {
                if (needHide)
                {
                    _forms[i].Hide(UIFormHideFlag.HideByOtherForm, false);
                }
                else
                {
                    _forms[i].Appear(UIFormHideFlag.HideByOtherForm, false);
                }

                if (!needHide && !_forms[i].IsHided() && _forms[i].IsHideUnderForms)
                {
                    needHide = true;
                }
            }
        }

        private void UpdateFormRaycaster()
        {
            bool respondInput = true;
            //从后开始
            for (int i = _forms.Count - 1; i >= 0; i--)
            {
                if (_forms[i].IsDisableInput || _forms[i].IsHided())
                {
                    continue;
                }

                GraphicRaycaster graphicRaycaster = _forms[i].GetGraphicRaycaster();
                if (graphicRaycaster != null)
                {
                    graphicRaycaster.enabled = respondInput;
                }

                if (_forms[i].IsModal && respondInput)
                {
                    respondInput = false;
                }
            }
        }
        #endregion

    }
}

