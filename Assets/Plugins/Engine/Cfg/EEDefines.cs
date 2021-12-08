using System;
using UnityEngine;

namespace Cfg
{
    /// <summary>
    /// excel 当行
    /// </summary>
    [Serializable]
    public abstract class EERowData
    {
        public object GetKeyFieldValue()
        {
            var keyField = EEUtility.GetRowDataKeyField(GetType());
            return keyField == null ? null : keyField.GetValue(this);
        }
    }

    /// <summary>
    /// 游戏配置基础对象
    /// </summary>
    public abstract class CfgResObj : ScriptableObject
    {
        //映射成支持 key 访问
        public abstract void DoMapEntry();
    }

    /// <summary>
    /// excel 行数据集
    /// </summary>
    [Serializable]
    public abstract class EERowDataCollection : CfgResObj
    {
        public string ExcelFileName;
        public string ExcelSheetName;
        public string KeyFieldName;
        public abstract void AddEntry(EERowData data);
        public abstract int GetEntryCount();

    }

    /// <summary>
    /// excel 字段特性
    /// </summary>
    public class EEKeyFieldAttribute : Attribute
    {

    }

    /// <summary>
    /// excel 评论字段特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class EECommentAttribute : Attribute
    {
        public readonly string content;

        public EECommentAttribute(string text)
        {
            content = text;
        }
    }

}
