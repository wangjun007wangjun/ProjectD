using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Cfg
{
    /// <summary>
    ///     Excel Converter
    /// </summary>
    public static partial class EEConverter
    {
        //是否需要重新生成cs key
        public const string csChangedKey = "EasyExcelCSChanged";

        //名称行
        private static int S_NameRowIndex = 0;
        //类型定义行
        private static int S_TypeRowIndex = 1;
        //注释行
        private static int S_ExplainIndex = 2;
        //数据开始行
        private static int S_DataStartIndex = 3;
        //命名空间
        private static string S_NameSpace = "Cfg";

        //行数据模型 类名称
        public static string GetRowDataClassName(string sheetName, bool includeNameSpace = false)
        {
            return (includeNameSpace ? S_NameSpace + "." : null) + sheetName + "Entry";
        }

        //sheet 类名称
        public static string GetSheetClassName(string sheetName)
        {
            return sheetName;
        }

        //sheet类 inspect
        public static string GetSheetInspectorClassName(string sheetName)
        {
            return sheetName + "Inspector";
        }

        //asset文件名
        public static string GetAssetFileName(string sheetName)
        {
            return sheetName + ".asset";
        }

        //c#代码名称
        public static string GetCSharpFileName(string sheetName)
        {
            // The file name must not differ from the name of ScriptableObject class
            return GetSheetClassName(sheetName) + ".cs";
        }

        public static string GetSheetInspectorFileName(string sheetName)
        {
            return GetSheetInspectorClassName(sheetName) + ".cs";
        }
        //

        private static SheetData ToSheetData(EEWorksheet sheet)
        {
            var sheetData = new SheetData();
            for (var i = 0; i < sheet.RowCount; i++)
            {
                var rowData = new List<string>();
                for (var j = 0; j < sheet.ColumnCount; j++)
                {
                    var cell = sheet.GetCell(i, j);
                    rowData.Add(cell != null ? cell.value : "");
                }

                sheetData.Table.Add(rowData);
            }

            sheetData.RowCount = sheet.RowCount;
            sheetData.ColumnCount = sheet.ColumnCount;

            return sheetData;
        }

        private static SheetData ToSheetDataRemoveEmptyColumn(EEWorksheet sheet)
        {
            var validNameColumns = new List<int>();
            for (var column = 0; column < sheet.ColumnCount; column++)
            {
                var cellValue = sheet.GetCellValue(S_NameRowIndex, column);
                if (!string.IsNullOrEmpty(cellValue))
                    validNameColumns.Add(column);
            }
            var validTypeColumns = new List<int>();
            for (var column = 0; column < validNameColumns.Count; column++)
            {
                var cellValue = sheet.GetCellValue(S_TypeRowIndex, validNameColumns[column]);
                if (IsSupportedColumnType(cellValue))
                    validTypeColumns.Add(validNameColumns[column]);
            }

            var sheetData = new SheetData();
            for (var i = 0; i < sheet.RowCount; i++)
            {
                var rowData = new List<string>();
                foreach (var c in validTypeColumns)
                {
                    var cell = sheet.GetCell(i, c);
                    rowData.Add(cell != null ? cell.value : "");
                }

                sheetData.Table.Add(rowData);
            }

            sheetData.RowCount = sheet.RowCount;
            sheetData.ColumnCount = validTypeColumns.Count;

            return sheetData;
        }

        private static bool IsValidSheet(EEWorksheet sheet)
        {
            if (sheet == null || sheet.RowCount <= S_TypeRowIndex || sheet.ColumnCount < 1)
                return false;
            int validColumnCount = 0;
            for (int col = 0; col < sheet.ColumnCount; col++)
            {
                string varType = sheet.GetCellValue(S_TypeRowIndex, col);
                if (string.IsNullOrEmpty(varType) || varType.Equals(" ") || varType.Equals("\r"))
                    continue;
                if (IsSupportedColumnType(varType))
                {
                    string varName = sheet.GetCellValue(S_NameRowIndex, col);
                    if (!string.IsNullOrEmpty(varName))
                        validColumnCount++;
                }
            }

            return validColumnCount > 0;
        }

        private static bool IsExcelFile(string filePath)
        {
            return EEUtility.IsExcelFileSupported(filePath);
        }

        private static bool IsSupportedColumnType(string type)
        {
            return !string.IsNullOrEmpty(type) && (type.Contains("int") || type.Contains("float") ||
                    type.Contains("double") || type.Contains("long") ||
                    type.Contains("string") || type.Contains("bool") ||
                    type.Contains("JObject"));
        }

        private static bool isDisplayingProgress;

        private static void UpdateProgressBar(int progress, int progressMax, string desc)
        {
            var title = "Importing...[" + progress + " / " + progressMax + "]";
            var value = progress / (float)progressMax;
            EditorUtility.DisplayProgressBar(title, desc, value);
            isDisplayingProgress = true;
        }

        private static void ClearProgressBar()
        {
            if (!isDisplayingProgress) return;
            try
            {
                EditorUtility.ClearProgressBar();
            }
            catch (Exception)
            {
                // ignored
            }

            isDisplayingProgress = false;
        }

        private class SheetData
        {
            private readonly List<List<string>> table = new List<List<string>>();
            public int ColumnCount;
            public int RowCount;

            public List<List<string>> Table
            {
                get { return table; }
            }

            public string Get(int row, int column)
            {
                return table[row][column];
            }

            public void Set(int row, int column, string value)
            {
                table[row][column] = value;
            }
        }
    }
}