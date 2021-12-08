/********************************************************************
    created:	2020-04-29 				
    author:		OneJun						
    purpose:	游戏国际化文本 处理							
*********************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Cfg;
using Engine.Localize;

public static class LocalizeEditor
{
    //gametext excel 文件
    private static string LocalizeExcelFilePath = Application.dataPath + "/../../GD/Localize.xlsx";
    //转档输出 目录
    private static string LocalizeResOutDir = Application.dataPath + "/Resources/Localize";


    [MenuItem(@"Localize/国际化测试")]
    public static void ShowLocalizeWnd()
    {
        LocalizeToolWnd.ShowWindow();
    }


    [MenuItem(@"Localize/导入国际化 Excel", false, 100)]
    public static void ImportExcel()
    {
        Debug.Log("----------0-----------");
        EEWorkbook book = EEWorkbook.Load(LocalizeExcelFilePath);
        if (book == null)
        {
            Debug.LogError("加载国际化文本出错!");
            return;
        }
        EditorUtility.DisplayProgressBar("处理国际化Excel", "Dealing.....", 0.0f);
        for (int i = 0; i < book.sheets.Count; i++)
        {
            DealOneSheet(book.sheets[i]);
        }
        LocalizeService.GetInstance().ReloadLanguage();
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
    }

    /// <summary>
    /// 处理单页
    /// </summary>
    private static void DealOneSheet(EEWorksheet sheet)
    {
        //分类名字
        string sectionName = sheet.name;

        // //获取所有Id
        // List<string> textIdList=new List<string>();
        // //第一行开始
        // for (int i = 1; i < sheet.RowCount; i++)
        // {   
        //     //第一列 表示Id
        //     string idName=sheet.GetCell(i,1).value;
        //     textIdList.Add(idName);
        // }

        //语言列表
        List<string> languageList = new List<string>();
        List<int> languageColIndexs = new List<int>();

        for (int j = 2; j < sheet.ColumnCount; j++)
        {
            string lanName = sheet.GetCell(0, j).value;
            languageList.Add(lanName);
            languageColIndexs.Add(j);
        }

        //每个语言 建立一个 国际化段
        for (int i = 0; i < languageList.Count; i++)
        {
            //语言名字
            string languageName = languageList[i];
            TextSection ss = new TextSection();
            ss.SectionName = sectionName.ToLower();
            ss.LanguageKey = languageName.ToLower();
            //填充单个 从第一行开始
            int colIndex = languageColIndexs[i];

            for (int j = 1; j < sheet.RowCount; j++)
            {
                TextEntry entry = new TextEntry();
                entry.Id = sheet.GetCellValue(j, 1);
                entry.Value = sheet.GetCellValue(j, colIndex);
                if (!string.IsNullOrEmpty(entry.Id) && !string.IsNullOrEmpty(entry.Value))
                {
                    entry.Id = entry.Id.ToLower();
                    ss.AddEntry(entry);
                }
            }
            //输出json文件
            string outPath = LocalizeResOutDir + "/" + "text" + "_" + languageName.ToLower() + ".bytes";
            string json = JsonUtility.ToJson(ss);
            byte[] data = System.Text.Encoding.UTF8.GetBytes(json);
            Engine.Res.FileUtil.WriteFile(outPath, data, true);
            Debug.Log("Localize Export  " + outPath);
        }
    }
}

