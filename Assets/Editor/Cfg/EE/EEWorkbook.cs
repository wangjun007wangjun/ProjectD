using System;
using System.Collections.Generic;
using System.IO;
using OfficeOpenXml;
using UnityEngine;

namespace Cfg
{
    public class EEWorkbook
    {
        public readonly List<EEWorksheet> sheets = new List<EEWorksheet>();

        private EEWorkbook()
        {
        }

        private EEWorkbook(ExcelWorkbook workbook)
        {
            try
            {
                foreach (var sheet in workbook.Worksheets)
                    sheets.Add(new EEWorksheet(sheet));
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }

        public EEWorksheet AddWorksheet(string name)
        {
            var sheet = new EEWorksheet { name = name };
            sheets.Add(sheet);
            return sheet;
        }

        public void Dump()
        {
            try
            {
                foreach (var t in sheets)
                    t.Dump();
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }

        public static EEWorkbook Load(string path)
        {
            try
            {
                if (!File.Exists(path))
                {
                    Debug.LogError("Cannot find file " + path);
                    return null;
                }
                var file = new FileInfo(path);
                using (var ep = new ExcelPackage(file))
                {
                    var workbook = new EEWorkbook(ep.Workbook);
                    return workbook;
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }

            return null;
        }

        public static EEWorkbook Create(string firstSheetName = "Sheet 1")
        {
            try
            {
                using (var ep = new ExcelPackage())
                {
                    ep.Workbook.Worksheets.Add(firstSheetName);
                    var workbook = new EEWorkbook(ep.Workbook);
                    return workbook;
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
                return null;
            }
        }

        public void SaveToFile(string path)
        {
            try
            {
                var output = new FileInfo(path);
                using (var ep = new ExcelPackage())
                {
                    foreach (var s in sheets)
                    {
                        var sheet = ep.Workbook.Worksheets.Add(s.name);
                        s.CopyTo(sheet);
                    }

                    ep.SaveAs(output);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }
    }
}