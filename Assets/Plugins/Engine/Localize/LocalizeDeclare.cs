using System.Collections;
using System.Collections.Generic;
using Engine.Base;
using UnityEngine;

namespace Engine.Localize
{
    //单条国际化配置
    [System.Serializable]
    public class TextEntry
    {
        public string Id;
        public string Value;
    }

    //国际化段定义
    [System.Serializable]
    public class TextSection
    {
        //分类名
        [SerializeField]
        public string SectionName;
        //语言类型key
        [SerializeField]
        public string LanguageKey;

        [SerializeField]
        private List<TextEntry> _entryList = new List<TextEntry>();
        //文本字典
        private Dictionary<string, string> _textDic;

        /// <summary>
        /// 添加一行
        /// </summary>
        /// <param name="entry">行</param>
        public void AddEntry(TextEntry entry)
        {
            _entryList.Add(entry);
        }

        /// <summary>
        /// 映射
        /// </summary>
        public void DoMap()
        {
            if (_entryList == null || _entryList.Count <= 0)
            {
                return;
            }
            _textDic = new Dictionary<string, string>();
            for (int i = 0; i < _entryList.Count; i++)
            {
                try
                {
                    _textDic.Add(_entryList[i].Id, _entryList[i].Value);
                }
                catch (System.Exception ex)
                {
                    GGLog.LogE("Map Text Error Id:" + _entryList[i].Id);
                    GGLog.LogE(ex.ToString());
                }
            }
        }

        /// <summary>
        /// 获取对应文本
        /// </summary>
        /// <returns></returns>
        public string GetText(string id)
        {
            if (_textDic == null)
            {
                return string.Empty;
            }
            string vv = string.Empty;
            if (_textDic.TryGetValue(id, out vv))
            {

            }
            return vv;
        }

        public string[] GetAllTextId()
        {
            List<string> allKeys = new List<string>(_textDic.Keys);
            return allKeys.ToArray();
        }

    }

    //游戏文本工具
    public class TextUtils
    {
        /// <summary>
        /// 系统语言定义 简称
        /// </summary>
        /// <param name="local"></param>
        /// <returns></returns>
        public static string SystemLanguageToKey(SystemLanguage local)
        {
            string result = "en";
            switch (local)
            {
                case SystemLanguage.Chinese:
                    result = "cn";
                    break;
                case SystemLanguage.English:
                    result = "en";
                    break;
                case SystemLanguage.French:
                    result = "fr";
                    break;
                case SystemLanguage.German:
                    result = "de";
                    break;
                case SystemLanguage.Japanese:
                    result = "jp";
                    break;
                case SystemLanguage.Korean:
                    result = "ko";
                    break;
                case SystemLanguage.Russian:
                    result = "ru";
                    break;
                case SystemLanguage.ChineseSimplified:
                    result = "cn";
                    break;
                case SystemLanguage.ChineseTraditional:
                    result = "cn-tr";
                    break;
                case SystemLanguage.Arabic:
                    result = "ar";
                    break;
                case SystemLanguage.Spanish:
                case SystemLanguage.Catalan:
                    result = "es";
                    break;
                case SystemLanguage.Portuguese:
                    result = "pt";
                    break;
                case SystemLanguage.Italian:
                    result = "it";
                    break;
                case SystemLanguage.Dutch:
                    result = "du";
                    break;
                case SystemLanguage.Thai:
                    result = "th";
                    break;
                case SystemLanguage.Unknown:
                    result = "en";
                    break;
            }
            return result;
        }
    }
}
