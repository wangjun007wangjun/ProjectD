using UnityEditor;
using UnityEngine;
using System;
using System.Collections;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.IO;
public class BMFontTool
{
    public static Regex BMCharMatch =
        new Regex(
            @"char id=(?<id>\d+)\s+x=(?<x>\d+)\s+y=(?<y>\d+)\s+width=(?<width>\d+)\s+height=(?<height>\d+)\s+xoffset=(?<xoffset>\d+)\s+yoffset=(?<yoffset>\d+)\s+xadvance=(?<xadvance>\d+)\s+");

    public static Regex BMInfoMatch =
        new Regex(@"common lineHeight=(?<lineHeight>\d+)\s+.*scaleW=(?<scaleW>\d+)\s+scaleH=(?<scaleH>\d+)");

    public const string BMFontExt = ".fnt";
    public const string FontExt = ".fontsettings";

    public static string GetConfPath()
    {
        UnityEngine.Object obj = Selection.activeObject;
        string cfgPath = AssetDatabase.GetAssetPath(obj);
        if (!cfgPath.EndsWith(".fnt"))
        {
            Debug.LogError("请选择.fnt文件！！");
            return null;
        }
        return cfgPath;
    }

    [MenuItem("Assets/创建BM字体")]
    public static void CreateFromBMFont()
    {
        string cfgPath = GetConfPath();
        if (null == cfgPath) return;

        string name = Path.GetFileNameWithoutExtension(cfgPath);
        int lineHeight = 1;
        // 创建材质
        Material mat = new Material(Shader.Find("UI/Default Font"))
        {
            name = name,
            mainTexture = AssetDatabase.LoadAssetAtPath<Texture>(cfgPath.Replace(BMFontExt, ".png")),
        };
        mat.SetTexture("_MainTex", mat.mainTexture);
        // 创建字体
        Font customFont = new Font(name)
        {
            material = mat,
            characterInfo = ParseBMFont(cfgPath, ref lineHeight).ToArray(),
        };
        
        // 修改行高
        SerializedObject serializedFont = new SerializedObject(customFont);
        SetLineHeight(serializedFont, lineHeight);
        serializedFont.ApplyModifiedProperties();
        // 保存
        AssetDatabase.CreateAsset(mat, cfgPath.Replace(BMFontExt, ".mat"));
        AssetDatabase.CreateAsset(customFont, cfgPath.Replace(BMFontExt, FontExt));
    }

    [MenuItem("Assets/更新BM字体")]
    public static void BuildFromBMFont()
    {
        string cfgPath = GetConfPath();
        if (null == cfgPath) return;

        string fontPath = cfgPath.Replace(BMFontExt, FontExt);
        if (!File.Exists(fontPath)) return;

        Font customFont = AssetDatabase.LoadAssetAtPath<Font>(fontPath);
        int lineHeight = 1;
        List<CharacterInfo> chars = ParseBMFont(cfgPath, ref lineHeight);
        SerializeFont(customFont, chars, lineHeight);
        Debug.Log("字体更新完成", customFont);
    }

    public static List<CharacterInfo> ParseBMFont(string path, ref int lineHeight)
    {
        List<CharacterInfo> chars = new List<CharacterInfo>();
        using (StreamReader reader = new StreamReader(path))
        {
            // 文字贴图的宽、高
            float texWidth = 1;
            float texHeight = 1;

            string line = reader.ReadLine();
            
            while (line != null)
            {   
                Debug.Log(line);
                if (line.Contains("char id="))
                {
                    Debug.Log("6666");
                    Match match = BMCharMatch.Match(line);
                    if (match != Match.Empty)
                    {
                        
                        int id = Convert.ToInt32(match.Groups["id"].Value);
                        int x = Convert.ToInt32(match.Groups["x"].Value);
                        int y = Convert.ToInt32(match.Groups["y"].Value);
                        int width = Convert.ToInt32(match.Groups["width"].Value);
                        int height = Convert.ToInt32(match.Groups["height"].Value);
                        int xoffset = Convert.ToInt32(match.Groups["xoffset"].Value);
                        int yoffset = Convert.ToInt32(match.Groups["yoffset"].Value);
                        int xadvance = Convert.ToInt32(match.Groups["xadvance"].Value);
                        // 转换为Unity UV坐标
                        float uvMinX = x / texWidth;
                        float uvMaxX = (x + width) / texWidth;
                        float uvMaxY = 1 - (y / texHeight);
                        float uvMinY = (texHeight - height - y) / texHeight;

                        // Unity字体UV的是 [左下(0, 0) - 右上(1, 1)]
                        // BMFont的UV是 [左上(0,0) - 右下(1, 1)]
                        CharacterInfo info = new CharacterInfo
                        {
                            // 字符的Unicode值
                            index = id,
                            uvBottomLeft = new Vector2(uvMinX, uvMinY),
                            uvBottomRight = new Vector2(uvMaxX, uvMinY),
                            uvTopLeft = new Vector2(uvMinX, uvMaxY),
                            uvTopRight = new Vector2(uvMaxX, uvMaxY),
                            minX = xoffset,
                            minY = -height / 2, // 居中对齐
                            glyphWidth = width,
                            glyphHeight = height,
                            // The horizontal distance from the origin of this character to the origin of the next character.
                            advance = xadvance,
                        };
                        chars.Add(info);
                    }
                }
                else if (line.IndexOf("scaleW=", StringComparison.Ordinal) != -1)
                {
                    Match match = BMInfoMatch.Match(line);
                    if (match != Match.Empty)
                    {
                        lineHeight = Convert.ToInt32(match.Groups["lineHeight"].Value);
                        texWidth = Convert.ToInt32(match.Groups["scaleW"].Value);
                        texHeight = Convert.ToInt32(match.Groups["scaleH"].Value);
                    }
                }
                line = reader.ReadLine();
            }
        }
        return chars;
    }

    public static void SetLineHeight(SerializedObject font, float height)
    {
        font.FindProperty("m_LineSpacing").floatValue = height;
    }

    /// <summary>
    /// 序列化自定义字体
    /// </summary>
    /// <param name="font">字体资源</param>
    /// <param name="chars">全部字符信息</param>
    /// <param name="lineHeight">显示的行高</param>
    public static SerializedObject SerializeFont(Font font, List<CharacterInfo> chars, float lineHeight)
    {
        SerializedObject serializedFont = new SerializedObject(font);
        SetLineHeight(serializedFont, lineHeight);
        SerializeFontCharInfos(serializedFont, chars);
        serializedFont.ApplyModifiedProperties();
        return serializedFont;
    }

    /// <summary>
    /// 序列化字体中的全部字符信息
    /// </summary>
    public static void SerializeFontCharInfos(SerializedObject font, List<CharacterInfo> chars)
    {
        SerializedProperty charRects = font.FindProperty("m_CharacterRects");
        charRects.arraySize = chars.Count;
        for (int i = 0; i < chars.Count; ++i)
        {
            CharacterInfo info = chars[i];
            SerializedProperty prop = charRects.GetArrayElementAtIndex(i);
            SerializeCharInfo(prop, info);
        }
    }

    /// <summary>
    /// 序列化一个字符信息
    /// </summary>
    public static void SerializeCharInfo(SerializedProperty prop, CharacterInfo charInfo)
    {
        prop.FindPropertyRelative("index").intValue = charInfo.index;
        prop.FindPropertyRelative("uv").rectValue = charInfo.uv;
        prop.FindPropertyRelative("vert").rectValue = charInfo.vert;
        prop.FindPropertyRelative("advance").floatValue = charInfo.advance;
        prop.FindPropertyRelative("flipped").boolValue = false;
    }


    // [MenuItem("Assets/分类图集")]
    // static void SliceSprite()
    // {
    //     Texture2D image = Selection.activeObject as Texture2D;//获取旋转的对象
    //     string rootPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(image));//获取路径名称
    //     string path = rootPath + "/" + image.name + ".PNG";//图片路径名称


    //     TextureImporter texImp = AssetImporter.GetAtPath(path) as TextureImporter;//获取图片入口


    //     AssetDatabase.CreateFolder(rootPath, image.name);//创建文件夹


    //     foreach (SpriteMetaData metaData in texImp.spritesheet)//遍历小图集
    //     {
    //         Texture2D myimage = new Texture2D((int)metaData.rect.width, (int)metaData.rect.height);

    //         //abc_0:(x:2.00, y:400.00, width:103.00, height:112.00)
    //         for (int y = (int)metaData.rect.y; y < metaData.rect.y + metaData.rect.height; y++)//Y轴像素
    //         {
    //             for (int x = (int)metaData.rect.x; x < metaData.rect.x + metaData.rect.width; x++)
    //                 myimage.SetPixel(x - (int)metaData.rect.x, y - (int)metaData.rect.y, image.GetPixel(x, y));
    //         }


    //         //转换纹理到EncodeToPNG兼容格式
    //         if (myimage.format != TextureFormat.ARGB32 && myimage.format != TextureFormat.RGB24)
    //         {
    //             Texture2D newTexture = new Texture2D(myimage.width, myimage.height);
    //             newTexture.SetPixels(myimage.GetPixels(0), 0);
    //             myimage = newTexture;
    //         }
    //         var pngData = myimage.EncodeToPNG();


    //         //AssetDatabase.CreateAsset(myimage, rootPath + "/" + image.name + "/" + metaData.name + ".PNG");
    //         File.WriteAllBytes(rootPath + "/" + image.name + "/" + metaData.name + ".PNG", pngData);
    //         // 刷新资源窗口界面
    //         AssetDatabase.Refresh();
    //     }
    // }
}