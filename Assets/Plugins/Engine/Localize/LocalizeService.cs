using UnityEngine;
using Engine.Base;
using Engine.Res;

namespace Engine.Localize
{
    public class LocalizeService : Singleton<LocalizeService>
    {
        //外部存储配置档的目录
        private static string S_CacheDirPath = Engine.Res.FileUtil.CombinePath(Engine.Res.FileUtil.GetIFSExtractPath(), "Localize");
        private static string S_TextAssetResDir = "Localize";

        //已经加载的国际化文本
        private TextSection _loadedSection = new TextSection();
        //当前语言
        private SystemLanguage _curLanguage = SystemLanguage.English;
        //语言改变回调
        private System.Action _languageChangedHandler;


        public override bool Initialize()
        {

            // //读取默认语言
            // var result = Application.systemLanguage;
            // if (PlayerPrefs.HasKey("GameLanguage"))
            // {
            //     string saveValue = PlayerPrefs.GetString("GameLanguage");
            //     result = (SystemLanguage)System.Enum.Parse(typeof(SystemLanguage), saveValue);
            // }
            //_curLanguage = result;

            _curLanguage = SystemLanguage.Chinese;
            LoadLanguageSection(_curLanguage);
            //
            return true;
        }

        public override void Uninitialize()
        {
            _loadedSection = null;
        }

        /// <summary>
        /// 切换语言
        /// </summary>
        public void SwitchLanguage(SystemLanguage sl)
        {
            if (_curLanguage == sl)
            {
                return;
            }
            //重新加载语言包
            _loadedSection = null;
            LoadLanguageSection(sl);
            //记录
            PlayerPrefs.SetString("GameLanguage", sl.ToString());
            _curLanguage = sl;
            //通知
            _languageChangedHandler?.Invoke();
        }

        /// <summary>
        /// 重载语言
        /// </summary>
        public void ReloadLanguage()
        {
            //重新加载语言包
            _loadedSection = null;
            LoadLanguageSection(_curLanguage);
            //记录
            PlayerPrefs.SetString("GameLanguage", _curLanguage.ToString());
            //通知
            _languageChangedHandler?.Invoke();
        }
        /// <summary>
        /// 添加语言变更回调
        /// </summary>
        /// <param name="callback">回调</param>
        public void AddLanguageChangedCallback(System.Action callback)
        {
            _languageChangedHandler += callback;
        }

        /// <summary>
        /// 移除语言变更回调
        /// </summary>
        /// <param name="callback"></param>
        public void RemoveLanguageChangedCallback(System.Action callback)
        {
            _languageChangedHandler -= callback;
        }


        /// <summary>
        /// 根据定位获取 国际化文本
        /// </summary>
        /// <param name="uri">地址 eg:LC_MENU_YES</param>
        /// <returns></returns>
        public string GetTextById(string textId)
        {
            if (string.IsNullOrEmpty(textId))
            {
                GGLog.LogE("GetTextByURI Error Uri");
                return string.Empty;
            }
            if (_loadedSection != null)
            {
                return _loadedSection.GetText(textId);
            }
            return string.Empty;
        }

        /// <summary>
        /// 获取所有本地化文本Id
        /// </summary>
        /// <returns></returns>
        public string[] GetAllTextId()
        {
            return _loadedSection.GetAllTextId();
        }

        /// <summary>
        /// 加载语言包 游戏加载时候 或者进入特殊pve
        /// </summary>
        /// <param name="sectionName">包类型</param>
        private void LoadLanguageSection(SystemLanguage sl)
        {
            string fullName = "text" + "_" + TextUtils.SystemLanguageToKey(sl); ;
            fullName = fullName.ToLower();
            TextSection gameTextSection = new TextSection();
            //编辑器
            if (!Application.isPlaying)
            {
                TextAsset tt = Resources.Load(S_TextAssetResDir + "/" + fullName) as TextAsset;
                if (tt != null)
                {
                    JsonUtility.FromJsonOverwrite(tt.text, gameTextSection);
                    //生成映射
                    gameTextSection.DoMap();
                    //add cache
                    _loadedSection = gameTextSection;
                }
                return;
            }

            //
            bool isOutOk = false;
            //先判断外部是否存在 资源更新支持
            string externalFilePath = Engine.Res.FileUtil.CombinePath(S_CacheDirPath, fullName + ".bytes");
            if (Engine.Res.FileUtil.IsFileExist(externalFilePath))
            {
                string jsonStr = Engine.Res.FileUtil.ReadFileText(externalFilePath);
                if (!string.IsNullOrEmpty(jsonStr))
                {
                    JsonUtility.FromJsonOverwrite(jsonStr, gameTextSection);
                    isOutOk = true;
                    //生成映射
                    gameTextSection.DoMap();
                    //add cache
                    _loadedSection = gameTextSection;
                }
                else
                {
                    isOutOk = false;
                }
            }
            //从Resource目录读取ScriptableObject 资源文件
            if (isOutOk != false)
            {
                return;
            }
            ResObj obj = ResService.GetResource(S_TextAssetResDir + "/" + fullName);
            if (obj != null)
            {
                BinaryObject cc = obj.Content as BinaryObject;
                string ss = System.Text.Encoding.UTF8.GetString(cc.m_data);
                JsonUtility.FromJsonOverwrite(ss, gameTextSection);
                //生成映射
                gameTextSection.DoMap();
                //add cache
                _loadedSection = gameTextSection;
            }
            else
            {
                GGLog.LogE("Load Text Bytes Error :" + fullName);
            }
        }
    }

}

