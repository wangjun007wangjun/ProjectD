using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Cfg
{
    /// <summary>
    ///     Excel Converter
    /// </summary>
    public static partial class EEConverter
    {
        public static void GenerateCSharpFiles(string excelPath, string csPath, string inspectDir, string cfgAssetDir)
        {
            try
            {
                excelPath = excelPath.Replace("\\", "/");
                csPath = csPath.Replace("\\", "/");

                var tmpPath = Environment.CurrentDirectory + "/EasyExcelTmp/";
                var tmpEditorPath = Environment.CurrentDirectory + "/EasyExcelTmp/Editor/";

                if (Directory.Exists(tmpPath))
                    Directory.Delete(tmpPath, true);

                Directory.CreateDirectory(tmpPath);
                Directory.CreateDirectory(tmpEditorPath);

                excelPath = excelPath.Replace("\\", "/");
                csPath = csPath.Replace("\\", "/");
                if (!csPath.EndsWith("/"))
                    csPath += "/";

                var csChanged = false;
                var filePaths = Directory.GetFiles(excelPath);
                for (var i = 0; i < filePaths.Length; ++i)
                {
                    var excelFilePath = filePaths[i].Replace("\\", "/");
                    if (i + 1 < filePaths.Length)
                        UpdateProgressBar(i + 1, filePaths.Length, "");
                    else
                        ClearProgressBar();
                    if (!IsExcelFile(excelFilePath))
                        continue;
                    var newCsDict = ToCSharpArray(excelFilePath);
                    foreach (var newCs in newCsDict)
                    {
                        var cSharpFileName = GetCSharpFileName(newCs.Key);
                        var tmpCsFilePath = tmpPath + cSharpFileName;
                        var csFilePath = csPath + cSharpFileName;
                        var shouldWrite = true;
                        if (File.Exists(csFilePath))
                        {
                            var oldCs = File.ReadAllText(csFilePath);
                            shouldWrite = oldCs != newCs.Value;
                        }

                        if (!shouldWrite)
                            continue;
                        csChanged = true;
                        File.WriteAllText(tmpCsFilePath, newCs.Value, Encoding.UTF8);
                    }
                    //inspect
                    var newInspectorDict = ToCSharpInspectorArray(excelFilePath);
                    foreach (var newCs in newInspectorDict)
                    {
                        var inspectorFileName = GetSheetInspectorFileName(newCs.Key);
                        var tmpInspFilePath = tmpEditorPath + inspectorFileName;
                        var csFilePath = inspectDir + inspectorFileName;
                        var shouldWrite = true;
                        if (File.Exists(csFilePath))
                        {
                            var oldCs = File.ReadAllText(csFilePath);
                            shouldWrite = oldCs != newCs.Value;
                        }

                        if (!shouldWrite)
                            continue;
                        csChanged = true;
                        File.WriteAllText(tmpInspFilePath, newCs.Value, Encoding.UTF8);
                    }
                }

                if (csChanged)
                {
                    EditorPrefs.SetBool(csChangedKey, true);
                    var files = Directory.GetFiles(tmpPath);
                    foreach (var s in files)
                    {
                        var p = s.Replace("\\", "/");
                        File.Copy(s, csPath + p.Substring(p.LastIndexOf("/", StringComparison.Ordinal)), true);
                    }
                    files = Directory.GetFiles(tmpEditorPath);
                    foreach (var s in files)
                    {
                        var p = s.Replace("\\", "/");
                        File.Copy(s, inspectDir + p.Substring(p.LastIndexOf("/", StringComparison.Ordinal)), true);
                    }
                    Directory.Delete(tmpPath, true);
                    AssetDatabase.Refresh();
                    Debug.Log("Scripts are generated, wait for generating assets...");
                }
                else
                {
                    Debug.Log("No CSharp files changed, begin generating assets...");
                    ClearProgressBar();
                    GenerateScriptableObjects(excelPath, cfgAssetDir);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
                EditorPrefs.SetBool(csChangedKey, false);
                ClearProgressBar();
                AssetDatabase.Refresh();
            }
        }

        private static Dictionary<string, string> ToCSharpArray(string excelPath)
        {
            var lst = new Dictionary<string, string>();
            var book = EEWorkbook.Load(excelPath);
            if (book == null)
                return lst;
            foreach (var sheet in book.sheets)
            {
                if (sheet == null)
                    continue;
                if (!IsValidSheet(sheet))
                {
                    Debug.Log(string.Format("Skipped sheet {0} in file {1}.", sheet.name, Path.GetFileName(excelPath)));
                    continue;
                }
                var sheetData = ToSheetData(sheet);
                var csTxt = ToCSharp(sheetData, sheet.name);
                lst.Add(sheet.name, csTxt);
            }
            return lst;
        }

        //转换成代码
        private static string ToCSharp(SheetData sheetData, string sheetName)
        {
            try
            {
                var rowDataClassName = GetRowDataClassName(sheetName);
                var sheetClassName = GetSheetClassName(sheetName);
                var csFile = new StringBuilder(2048);
                csFile.Append("//------------------------------------------\n");
                csFile.Append("//auto-generated\n");
                csFile.Append("//-------------------------------------------");

                csFile.Append("\nusing System;\nusing System.Collections.Generic;\nusing UnityEngine;\n\n");
                csFile.Append(string.Format("namespace {0}\n", S_NameSpace));
                csFile.Append("{\n");
                csFile.Append("\t[Serializable]\n");
                csFile.Append("\tpublic class " + rowDataClassName + " : EERowData\n");
                csFile.Append("\t{\n");

                var columnCount = 0;
                for (var col = 1; col < sheetData.ColumnCount; col++)
                {
                    if (string.IsNullOrEmpty(sheetData.Get(S_NameRowIndex, col)))
                        break;
                    columnCount++;
                }

                // Get field names
                var fieldsName = new string[columnCount];
                for (var col = 0; col < columnCount; col++)
                    fieldsName[col] = sheetData.Get(S_NameRowIndex, col + 1);

                // Get field types and Key column
                var fieldsLength = new string[columnCount];
                var fieldsExplain = new string[columnCount];
                var fieldsType = new string[columnCount];
                string keyFieldNameFull = "";
                string keyFieldName = "";
                string keyFieldType = "";

                for (var col = 0; col < columnCount; col++)
                {
                    var cellInfo = sheetData.Get(S_TypeRowIndex, col + 1);
                    var explainInfo = sheetData.Get(S_ExplainIndex, col + 1);
                    fieldsLength[col] = null;
                    fieldsType[col] = cellInfo;
                    fieldsExplain[col] = explainInfo;
                    if (cellInfo.EndsWith("]"))
                    {
                        var startIndex = cellInfo.IndexOf('[');
                        fieldsLength[col] = cellInfo.Substring(startIndex + 1, cellInfo.Length - startIndex - 2);
                        fieldsType[col] = cellInfo.Substring(0, startIndex);
                    }

                    var varName = fieldsName[col];
                    var varLen = fieldsLength[col];
                    var varType = fieldsType[col];
                    if (varName.EndsWith(":Key") || varName.EndsWith(":key") || varName.EndsWith(":KEY"))
                    {
                        var splits = varName.Split(':');
                        if ((varType.Equals("int") || varType.Equals("string")) && varLen == null)
                        {
                            keyFieldNameFull = varName;
                            fieldsName[col] = splits[0];
                            keyFieldName = fieldsName[col];
                            keyFieldType = varType;
                        }
                    }
                }

                if (string.IsNullOrEmpty(keyFieldNameFull))
                    Debug.LogError("Cannot find Key column in sheet " + sheetName);

                for (var col = 0; col < columnCount; col++)
                {
                    var varName = fieldsName[col];
                    var varLen = fieldsLength[col];
                    var varType = fieldsType[col];
                    var varExplain = fieldsExplain[col];
                    bool isKeyField = !string.IsNullOrEmpty(keyFieldName) && keyFieldName == varName;
                    if (IsSupportedColumnType(varType))
                    {
                        if (!string.IsNullOrEmpty(varExplain))
                        {
                            csFile.AppendFormat("\t\t/// <summary>\n\t\t///{0}\n\t\t/// </summary>\n", varExplain);
                        }
                        if (isKeyField)
                            csFile.Append("\t\t[EEKeyField]\n");
                        csFile.Append("\t\t[SerializeField]\n");
                        if (varLen == null)
                        {
                            csFile.AppendFormat("\t\tpublic {0} {1};\n", varType, varName);
                            //csFile.AppendFormat("\t\tpublic {0} {1} {{ get {{ return _{1}; }} }}\n\n", varType, varName);
                        }
                        else
                        {
                            csFile.AppendFormat("\t\tpublic {0}[] {1};\n", varType, varName);
                            //csFile.AppendFormat("\t\tpublic {0}[] {1} {{ get {{ return _{1}; }} }}\n\n", varType, varName);
                        }
                    }
                }

                csFile.AppendFormat("\n\t\tpublic {0}()" + "\n", rowDataClassName);
                csFile.Append("\t\t{" + "\n");
                csFile.Append("\t\t}\n");

                csFile.Append("\n#if UNITY_EDITOR\n");
                csFile.AppendFormat("\t\tpublic {0}(List<List<string>> sheet, int row, int column)" + "\n", rowDataClassName);
                csFile.Append("\t\t{" + "\n");
                //csFile.Append("\t\t\tcolumn = base._init(sheet, row, column);\n");
                for (var col = 0; col < columnCount; col++)
                {
                    var varType = fieldsType[col];
                    var varLen = fieldsLength[col];
                    var varName = fieldsName[col];
                    if (!string.IsNullOrEmpty(keyFieldNameFull) && varName == keyFieldNameFull)
                        varName = keyFieldName;
                    //varName = "_" + varName;

                    if (varType.Equals("int") || varType.Equals("float") || varType.Equals("double") ||
                        varType.Equals("long") || varType.Equals("bool"))
                    {
                        if (varLen == null)
                        {
                            csFile.Append("\t\t\t" + varType + ".TryParse(sheet[row][column++], out " + varName + ");\n");
                        }
                        else
                        {
                            csFile.Append("\t\t\tstring " + varName + "ArrayString=sheet[row][column++];" + "\n");
                            csFile.Append("\t\t\tif(!string.IsNullOrEmpty(" + varName + "ArrayString))\n");
                            csFile.Append("\t\t\t{\n");
                            csFile.Append("\t\t\t\tstring[] " + varName + "Array = " + varName + "ArrayString.Split(\',\');" + "\n");
                            csFile.Append("\t\t\tint " + varName + "Count = " + varName + "Array.Length;" + "\n");
                            csFile.Append("\t\t\t" + varName + " = new " + varType + "[" + varName + "Count];\n");
                            csFile.Append("\t\t\tfor(int i = 0; i < " + varName + "Count; i++)\n");
                            csFile.Append("\t\t\t\t" + varType + ".TryParse(" + varName + "Array[i], out " + varName + "[i]);\n");
                            csFile.Append("\t\t\t}\n");
                        }
                    }
                    else if (varType.Equals("string"))
                    {
                        if (varLen == null)
                        {
                            csFile.Append("\t\t\t" + varName + " = sheet[row][column++] ?? \"" + /*varDefault + */"\";\n");
                        }
                        else
                        {
                            csFile.Append("\t\t\tstring " + varName + "ArrayString=sheet[row][column++];" + "\n");
                            csFile.Append("\t\t\tif(!string.IsNullOrEmpty(" + varName + "ArrayString))\n");
                            csFile.Append("\t\t\t{\n");
                            csFile.Append("\t\t\t\tstring[] " + varName + "Array = " + varName + "ArrayString.Split(\',\');" + "\n");
                            csFile.Append("\t\t\t\tint " + varName + "Count = " + varName + "Array.Length;" + "\n");
                            csFile.Append("\t\t\t\t" + varName + " = new " + varType + "[" + varName + "Count];\n");
                            csFile.Append("\t\t\t\tfor(int i = 0; i < " + varName + "Count; i++)\n");
                            csFile.Append("\t\t\t\t\t" + varName + "[i] = " + varName + "Array[i];\n");
                            csFile.Append("\t\t\t}\n");
                        }
                    }
                }

                //csFile.Append("\t\t\treturn column;\n");
                csFile.Append("\t\t}\n#endif\n");

                csFile.Append("\t}\n\n");

                // EERowDataCollection class
                csFile.Append("\tpublic class " + sheetClassName + " : EERowDataCollection\n");
                csFile.Append("\t{\n");
                csFile.AppendFormat("\t\t[SerializeField]\n\t\tprivate List<{0}> _entryList = new List<{0}>();\n\n", rowDataClassName);
                if (!string.IsNullOrEmpty(keyFieldType) && !string.IsNullOrEmpty(keyFieldName))
                {
                    csFile.AppendFormat("\t\tprivate Dictionary<{0}, {1}> _entryDic;\n\n", keyFieldType, rowDataClassName);
                }
                csFile.AppendFormat("\t\tpublic override void AddEntry(EERowData data)\n\t\t{{\n\t\t\t_entryList.Add(data as {0});\n\t\t}}\n\n", rowDataClassName);
                csFile.Append("\t\tpublic override int GetEntryCount()\n\t\t{\n\t\t\treturn _entryList.Count;\n\t\t}\n\n");
                //生成根据索引获取
                csFile.AppendFormat("\t\tpublic {0} GetEntryByIndex(int index)\n", rowDataClassName);
                csFile.Append("\t\t{\n");
                csFile.AppendFormat("\t\t\treturn _entryList[index] as {0};\n\n", rowDataClassName);
                csFile.Append("\t\t}\n\n");

                //生成key获取数据函数
                if (!string.IsNullOrEmpty(keyFieldType) && (!string.IsNullOrEmpty(keyFieldName)))
                {
                    csFile.AppendFormat("\t\tpublic {0} GetEntryByKey({1} kId)\n", rowDataClassName, keyFieldType);
                    csFile.Append("\t\t{\n");
                    csFile.AppendFormat("\t\t\t{0} result;\n", rowDataClassName);
                    csFile.Append("\t\t\tif (_entryDic.TryGetValue(kId, out result)){\n");
                    csFile.Append("\t\t\t\treturn result;\n\t\t\t}\n");
                    csFile.Append("\t\t\t return null;\n");
                    csFile.Append("\t\t}\n\n");
                }

                if (!string.IsNullOrEmpty(keyFieldType) && (!string.IsNullOrEmpty(keyFieldName)))
                {
                    csFile.Append("\t\tpublic override void DoMapEntry()\n");
                    csFile.Append("\t\t{\n");
                    csFile.Append("\t\t\tif (_entryList == null || _entryList.Count == 0)\n");
                    csFile.Append("\t\t\t{\n\t\t\t\treturn;\n\t\t\t}\n");
                    csFile.AppendFormat("\t\t\t_entryDic = new Dictionary<{0}, {1}>(_entryList.Count);\n", keyFieldType, rowDataClassName);
                    csFile.Append("\t\t\tfor (int i = 0; i < _entryList.Count; i++)\n");
                    csFile.Append("\t\t\t{\n");
                    csFile.AppendFormat("\t\t\t\t_entryDic.Add(_entryList[i].{0}, _entryList[i]);\n", keyFieldName);
                    csFile.Append("\t\t\t}\n");
                    csFile.Append("\t\t}\n");
                }
                //end

                //
                csFile.Append("\t}\n");

                csFile.Append("}\n");

                return csFile.ToString();
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.ToString());
            }

            return "";
        }

        private static Dictionary<string, string> ToCSharpInspectorArray(string excelPath)
        {
            var lst = new Dictionary<string, string>();
            var book = EEWorkbook.Load(excelPath);
            if (book == null)
                return lst;
            foreach (var sheet in book.sheets)
            {
                if (sheet == null)
                    continue;
                if (!IsValidSheet(sheet))
                    continue;
                var csTxt = ToCSharpInspector(sheet.name);
                lst.Add(sheet.name, csTxt);
            }
            return lst;
        }

        //资产检查器 样式
        private static string ToCSharpInspector(string sheetName)
        {
            try
            {
                var inspectorClassName = GetSheetInspectorClassName(sheetName);
                var csFile = new StringBuilder(1024);
                csFile.Append("//----------------------------\n");
                csFile.Append("//auto-generated\n");
                csFile.Append("//----------------------------");
                csFile.Append("\nusing UnityEditor;\n\n");
                csFile.Append(string.Format("namespace {0}\n", S_NameSpace));
                csFile.Append("{\n");
                csFile.Append(string.Format("\t[CustomEditor(typeof({0}))]\n", sheetName));
                csFile.Append("\tpublic class " + inspectorClassName + " : EEAssetInspector\n");
                csFile.Append("\t{\n");

                //创建
                csFile.Append(string.Format("\t\t[MenuItem(@\"Cfg/{0}创建Asset\", false, 1000)]\n", sheetName));
                csFile.Append("\t\tpublic static void CreateAsset()\n");
                csFile.Append("\t\t{\n");
                csFile.Append(string.Format("\t\t\tCfgEditor.CreateAsset<{0}>();\n", sheetName));
                csFile.Append("\t\t}\n");


                //导出菜单json
                csFile.Append(string.Format("\t\t[MenuItem(@\"Cfg/{0}导出Json\", false, 1000)]\n", sheetName));
                csFile.Append("\t\tpublic static void ExportJson()\n");
                csFile.Append("\t\t{\n");
                csFile.Append(string.Format("\t\t\tCfgEditor.ExportCfgAssetToJsonFile<{0}>();\n", sheetName));
                csFile.Append("\t\t}\n");

                //导入菜单json
                csFile.Append(string.Format("\t\t[MenuItem(@\"Cfg/{0}加载Json\", false, 1000)]\n", sheetName));
                csFile.Append("\t\tpublic static void ImportFromJson()\n");
                csFile.Append("\t\t{\n");
                csFile.Append(string.Format("\t\t\tCfgEditor.LoadCfgJsonToAsset<{0}>();\n", sheetName));
                csFile.Append("\t\t}\n");


                //
                csFile.Append("\t}\n");
                csFile.Append("}\n");

                return csFile.ToString();
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.ToString());
            }

            return "";
        }
    }
}