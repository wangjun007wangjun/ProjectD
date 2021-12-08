/********************************************************************
	created:	2020/02/16
	author:		OneJun
	
	purpose:	TabBar 选择条目
*********************************************************************/
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Engine.UGUI
{
    [RequireComponent(typeof(UIButton))]
    public class UITabItem : UIComponent
    {

        [SerializeField]
        [Header("正常显示对象(简单模式2个GO切换)")]
        private GameObject _normalStateGo;

        [SerializeField]
        [Header("选中显示对象(简单模式2个GO切换)")]
        private GameObject _selStateGo;


        [SerializeField]
        [Header("正常图标对象")]
        private GameObject _normalIconGo = null;

        [SerializeField]
        [Header("选中图标对象")]
        private GameObject _selectedIconGo = null;

        [SerializeField]
        [Header("选中显示背景图片")]
        private Image _selectedBgImageCom = null;

        [SerializeField]
        [Header("主TMP文本")]
        private UnityEngine.UI.Text _tmpTextCom = null;

        [SerializeField]
        [Header("主UGUI文本")]
        private UnityEngine.UI.Text _uguiTextCom = null;

        [SerializeField]
        [Header("正常文本Size")]
        private int _normalTextSize = 20;

        [SerializeField]
        [Header("选中态文本Size")]
        private int _selectedTextSize = 20;

        [SerializeField]
        [Header("正常文本颜色")]
        private Color _normalTextColor = Color.white;
        [SerializeField]
        [Header("选中文本颜色")]
        private Color _selectedTextColor = Color.black;

        [SerializeField]
        [Header("自定义点击索引")]
        public int ClickTag = -1;



        [HideInInspector]
        public int ItemIndex = 0;
        //是否打开
        private bool _isSelected = false;
        private UIButton _buttonCom;
        private UITabBar _tabBarCom;

        public void Init(UITabBar group, int index)
        {
            _tabBarCom = group;
            _isSelected = false;
            ItemIndex = index;
            RefreshShow(false);
        }

        //动态创建使用
        public void DynInit(UITabBar group, int index, string name)
        {
            _buttonCom = this.transform.GetComponent<UIButton>();
            _tabBarCom = group;
            ItemIndex = index;
            _buttonCom.IsAutoClickAction = false;
            _buttonCom.onClick.AddListener(this.OnClick);
            _isSelected = false;
            if (_uguiTextCom != null || _tmpTextCom)
            {
                _uguiTextCom.text = name;
                _tmpTextCom.text = name;
            }
            RefreshShow(false);
        }

        //动态释放使用
        public void UnInit()
        {
            _buttonCom.onClick.RemoveListener(this.OnClick);
        }

        /// <summary>
        /// 当前的选中转态
        /// </summary>
        public bool SelState
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    RefreshShow(true);
                }
            }
        }

        private void OnClick()
        {
            if (_tabBarCom != null)
            {
                _tabBarCom.DealTabItemSelected(this.ItemIndex, this.ClickTag);
            }
        }


        private void Awake()
        {
            _buttonCom = this.transform.GetComponent<UIButton>();
        }

        private void Start()
        {
            _buttonCom.onClick.AddListener(this.OnClick);
        }

        private void OnDestroy()
        {
            _buttonCom.onClick.RemoveAllListeners();
            _buttonCom = null;
        }

        private void RefreshShow(bool isAni = false)
        {
            // if (_tmpTextCom != null)
            // {
            //     _tmpTextCom.fontStyle = _isSelected ? FontStyles.Bold : FontStyles.Normal;
            //     _tmpTextCom.fontSize = _isSelected ? _selectedTextSize : _normalTextSize;
            // }
            if (_uguiTextCom != null)
            {
                _uguiTextCom.fontStyle = _isSelected ? UnityEngine.FontStyle.Bold : UnityEngine.FontStyle.Normal;
                _uguiTextCom.fontSize = _isSelected ? _selectedTextSize : _normalTextSize;
            }
            if (_selectedIconGo != null)
            {
                _selectedIconGo.SetActive(_isSelected);
            }
            if (_normalIconGo != null)
            {
                _normalIconGo.SetActive(!_isSelected);
            }



            //动画部分
            if (isAni)
            {
                if (_selectedBgImageCom != null)
                {
                    _selectedBgImageCom.DOFade(_isSelected ? 1 : 0, 0.2f).SetEase(Ease.InOutSine).SetLink(_selectedBgImageCom.gameObject);
                }
                Color targetColor = _isSelected ? _selectedTextColor : _normalTextColor;
                if (_tmpTextCom != null)
                {
                    _tmpTextCom.DOColor(targetColor, 0.2f).SetEase(Ease.InOutSine).SetLink(_tmpTextCom.gameObject);
                }
                if (_uguiTextCom != null)
                {
                    _uguiTextCom.DOColor(targetColor, 0.2f).SetEase(Ease.InOutSine).SetLink(_tmpTextCom.gameObject);
                }
            }
            else
            {
                if (_selectedBgImageCom != null)
                {
                    _selectedBgImageCom.DOFade(_isSelected ? 1 : 0, 0.1f).SetEase(Ease.InOutSine).SetLink(_selectedBgImageCom.gameObject);
                }
                if (_tmpTextCom != null)
                {
                    _tmpTextCom.color = _isSelected ? _selectedTextColor : _normalTextColor;
                }
                if (_uguiTextCom != null)
                {
                    _tmpTextCom.color = _isSelected ? _selectedTextColor : _normalTextColor;
                }
            }

            //
            //简单模式
            if (_normalStateGo != null && _selStateGo != null)
            {
                _normalStateGo.SetActive(!_isSelected);
                _selStateGo.SetActive(_isSelected);
            }
        }


    }
}