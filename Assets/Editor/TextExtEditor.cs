using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.UI;
using UnityEngine.UI;

public static class TextExt
{
    public class TextStyle
    {
        public int fontSize;
        public string color;
        public bool isOutline;

        public string outlineColor;
        public Vector2 outlineDistance;

        public bool isShadow;
        public string shaowColor;
        public Vector2 shadowDistance;

        public TextStyle(int fontSize, string color, bool isOutline, string outlineColor = "", float xDistance = 1, float yDistance = -1,
            bool isShadow = false, string shadowColor = "", float shadowOffsetX = 1.0f,float shadowOffsetY = -1.0f)
        {
            this.fontSize = fontSize;
            this.color = color;
            this.isOutline = isOutline;

            this.outlineColor = outlineColor;
            this.outlineDistance = new Vector2(xDistance, yDistance);

            this.isShadow = isShadow;
            this.shaowColor = shadowColor;
            this.shadowDistance = new Vector2(shadowOffsetX, shadowOffsetY);
        }
    }

    static List<TextStyle> _styles = new List<TextStyle>();

    static TextExt()
    {
        //弹窗标题文本
        _styles.Add(new TextStyle(50, "#FFFFFF", true, "#5A1BBF", 2.0f, -2.0f));
        //内容文本
        _styles.Add(new TextStyle(28, "#FFFFFF", false));
        //标签文本
        _styles.Add(new TextStyle(28, "#FFEC5F", false));
        //绿色按钮大
        _styles.Add(new TextStyle(46, "#FFFFFF", true, "01BE3A", 2f, -2f));
        //绿色按钮小
        _styles.Add(new TextStyle(30, "#FFFFFF", false));
        //蓝色按钮大
        _styles.Add(new TextStyle(40, "#FFFFFF", true, "1EA9FA", 2, -2));
        //蓝色按钮小
        _styles.Add(new TextStyle(30, "#FFFFFF", true, "1EA9FA", 2, -2));
        //黄色按钮大
        _styles.Add(new TextStyle(30, "#FFFFFF", false, "", 1, -1, true, "000000", 0, -3f));
        //黄色按钮小
        _styles.Add(new TextStyle(26, "#FFFFFF", false));
        //TabBar选中
        _styles.Add(new TextStyle(40, "#FFFFFF", false));
        //Tab未选中
        _styles.Add(new TextStyle(35, "#FFFFFF", false));
        //描述文本
        _styles.Add(new TextStyle(28, "#FFFFFF", false));
        //数字文本
        _styles.Add(new TextStyle(26, "#FFFFFF", true, "000000", 0, -3f));
    }

    [MenuItem("CONTEXT/Text/弹窗标题", false, 0)]
    private static void Style00(MenuCommand command)
    {
        SetTextStyle(command, 0);
    }
    [MenuItem("CONTEXT/Text/内容文本", false, 1)]
    private static void Style01(MenuCommand command)
    {
        SetTextStyle(command, 1);
    }
    [MenuItem("CONTEXT/Text/标签文本", false, 2)]
    private static void Style02(MenuCommand command)
    {
        SetTextStyle(command, 2);
    }
    [MenuItem("CONTEXT/Text/蓝色按钮大", false, 3)]
    private static void Style03(MenuCommand command)
    {
        SetTextStyle(command, 3);
    }
    [MenuItem("CONTEXT/Text/蓝色按钮小", false, 4)]
    private static void Style04(MenuCommand command)
    {
        SetTextStyle(command, 4);
    }

    [MenuItem("CONTEXT/Text/蓝色按钮大", false, 5)]
    private static void Style05(MenuCommand command)
    {
        SetTextStyle(command, 5);
    }
    [MenuItem("CONTEXT/Text/蓝色按钮小", false, 6)]
    private static void Style06(MenuCommand command)
    {
        SetTextStyle(command, 6);
    }
    [MenuItem("CONTEXT/Text/黄色按钮大", false, 7)]
    private static void Style07(MenuCommand command)
    {
        SetTextStyle(command, 7);
    }
    [MenuItem("CONTEXT/Text/黄色按钮小", false, 8)]
    private static void Style08(MenuCommand command)
    {
        SetTextStyle(command, 8);
    }

    [MenuItem("CONTEXT/Text/TabBar选中文本", false, 9)]
    private static void Style09(MenuCommand command)
    {
        SetTextStyle(command, 9);
    }
    [MenuItem("CONTEXT/Text/TabBar未选中文本", false, 10)]
    private static void Style10(MenuCommand command)
    {
        SetTextStyle(command, 10);
    }
    [MenuItem("CONTEXT/Text/描述文本", false, 11)]
    private static void Style11(MenuCommand command)
    {
        SetTextStyle(command, 11);
    }
    [MenuItem("CONTEXT/Text/数字文本", false, 12)]
    private static void Style12(MenuCommand command)
    {
        SetTextStyle(command, 12);
    }

    private static void SetTextStyle(MenuCommand command, int index)
    {
        Text label = (Text)command.context;
        // label.font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Resources/Font/GameFont.TTF");
        TextStyle style = _styles[index];
        if (style == null)
        {
            return;
        }
        label.fontSize = style.fontSize;
        Color fontColor;
        ColorUtility.TryParseHtmlString(style.color, out fontColor);
        label.color = fontColor;
        label.raycastTarget = false;
        label.supportRichText = false;
        if (style.isOutline)
        {
            Outline outline = label.transform.GetComponent<Outline>();
            if (outline == null)
            {
                outline = label.gameObject.AddComponent<Outline>();
            }

            Color effectColor;
            ColorUtility.TryParseHtmlString(style.outlineColor, out effectColor);
            outline.effectColor = effectColor;

            outline.effectDistance = style.outlineDistance;
        }
        else
        {
            Outline outline = label.gameObject.GetComponent<Outline>();
            if (outline != null)
            {
                if (outline.GetType() == typeof(Outline))
                {
                    GameObject.DestroyImmediate(outline);
                }
            }
        }
        //
        if (style.isShadow)
        {

            Shadow shadow = label.gameObject.GetComponent<Shadow>();
            if (shadow == null)
            {
                shadow = label.gameObject.AddComponent<Shadow>();
            }
            else
            {
                //
                if (shadow.GetType() == typeof(Outline))
                {
                    GameObject.DestroyImmediate(shadow);
                    shadow = label.gameObject.AddComponent<Shadow>();
                }
            }
            Color effectColor;
            ColorUtility.TryParseHtmlString(style.shaowColor, out effectColor);
            shadow.effectColor = effectColor;

            shadow.effectDistance = style.shadowDistance;
        }
        else
        {
            Shadow ss = label.gameObject.GetComponent<Shadow>();
            if (ss.GetType() != typeof(Outline))
            {
                GameObject.DestroyImmediate(ss);
            }
        }
    }

}