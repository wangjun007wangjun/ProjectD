/********************************************************************
  created:  2020-06-08         
  author:    OneJun           

  purpose:   引擎日志输出               
*********************************************************************/
using System;
using System.IO;
using System.Text;

namespace Engine.Base
{
    /// <summary>
    /// Log单例定义
    /// </summary>
    public class GGLog : Singleton<GGLog>
    {
        /// <summary>
        /// 日志类型定义
        /// </summary>
        public enum Type
        {
            Debug,
            Warning,
            Error,
            None,
        }

        private StringBuilder _stringBuilder;
        private StringBuilder _stringHelper;
        private StreamWriter _writer;

        //日志等级
        private static Type LogLevel = Type.None;
        private string logPath;

        public override bool Initialize()
        {
            return true;
        }

        ///合并路径
        private  string CombinePath(string path1, string path2)
        {
            if (path1.LastIndexOf('/') != path1.Length - 1)
            {
                path1 += "/";
            }

            if (path2.IndexOf('/') == 0)
            {
                path2 = path2.Substring(1);
            }
            return path1 + path2;
        }

        /// <summary>
        /// 开始
        /// </summary>
        /// <param name="logType">日志类型</param>
        /// <param name="logFileDir">日志文件目录 默认不写文件 </param>
        public void SetConfig(Type logType, string logFileDir=null)
        {
            //日志等级初始化
            LogLevel = logType;
            if (logType == Type.None)
            {
                return;
            }

            if (string.IsNullOrEmpty(logFileDir))
            {
                return;
            }

            logPath = CombinePath(logFileDir, "JWLog");
            //
            if (!Directory.Exists(logPath))
            {
                Directory.CreateDirectory(logPath);
            }
            //删除非今天的日志文件Release
            DateTime now = DateTime.Now;
            string logFileName = string.Format("JWEngineLog_{0}_{1}_{2}.log",now.Year, now.Month, now.Day);
            string[] files = System.IO.Directory.GetFiles(logPath);
            for (int i = 0; i < files.Length; i++)
            {
                if (!files[i].Contains(logFileName))
                {
                    File.Delete(files[i]);
                    break;
                }
            }
            //本地文件记录
            _stringBuilder = new StringBuilder(128);
            _stringHelper = new StringBuilder(128);
        }

        public override void Uninitialize()
        {
            if (_writer != null)
            {
                try
                {
                    _writer.Flush();
                    _writer.BaseStream.Flush();
                    _writer.Close();
                    _writer = null;
                }
                catch
                {
                    // ignored
                }
            }
            _stringBuilder = null;
            _stringHelper = null;
        }

       
        public static void LogD(string content)
        {
            GetInstance().OutputLog(Type.Debug, content);
        }

       
        public static void LogD(string format, params object[] args)
        {
            GetInstance().OutputLog(Type.Debug, format, args);
        }

       
        public static void LogW(string content)
        {
            GetInstance().OutputLog(Type.Warning, content);
        }

       
        public static void LogW(string format, params object[] args)
        {
            GetInstance().OutputLog(Type.Warning, format, args);
        }

       
        public static void LogE(string content)
        {
            GetInstance().OutputLog(Type.Error, content);
        }

       
        public static void LogE(string format, params object[] args)
        {
            GetInstance().OutputLog(Type.Error, format, args);
        }

        /// <summary>
        /// 输出日志
        /// </summary>
        /// <param name="type"></param>
        /// <param name="content"></param>
        private void OutputLog(Type type, string content)
        {
            if (type < LogLevel)
            {
                return;
            }

            switch (type)
            {
                case Type.Debug:
                    UnityEngine.Debug.Log(content);
                    break;

                case Type.Warning:
                    UnityEngine.Debug.LogWarning(content);
                    break;

                case Type.Error:
                    UnityEngine.Debug.LogError(content);
                    break;
            }

            if (_writer == null)
            {
                OpenFile();
            }

            if (_writer != null)
            {
                BuildLog(type);
                _stringBuilder.Append(content);
                try
                {
                    _writer.WriteLine(_stringBuilder.ToString());
                    _writer.Flush();
                    _writer.BaseStream.Flush();
                }
                catch
                {
                    // ignored
                }
            }
        }

        private void OutputLog(Type type, string format, params object[] args)
        {
            if (type < LogLevel)
            {
                return;
            }
            if (_stringHelper == null)
            {
                return;
            }
            if (!string.IsNullOrEmpty(format)&& args!=null && args.Length > 0)
            {
                _stringHelper.Length = 0;
                _stringHelper.AppendFormat(format, args);
                OutputLog(type, _stringHelper.ToString());
            }
        }

        private void OpenFile()
        {
            DateTime now = DateTime.Now;

            string logFile = string.Format("{0}/JWEngineLog_{1}_{2}_{3}.log", logPath, now.Year, now.Month, now.Day);
            try
            {
                if (!File.Exists(logFile))
                {
                    FileStream fs = new FileStream(logFile, FileMode.OpenOrCreate);
                    _writer = new StreamWriter(fs);
                }
                else
                {
                    var fs = new FileStream(logFile, FileMode.Append);//改为追加
                    _writer = new StreamWriter(fs);
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void BuildLog(Type type)
        {
            if (_stringBuilder == null)
            {
                return;
            }
            _stringBuilder.Length = 0;

            DateTime now = DateTime.Now;
            _stringBuilder.Append('[');

            StringBuilderOperator.AppendInt(_stringBuilder, now.Hour, 2);
            _stringBuilder.Append(':');

            StringBuilderOperator.AppendInt(_stringBuilder, now.Minute, 2);
            _stringBuilder.Append(':');

            StringBuilderOperator.AppendInt(_stringBuilder, now.Second, 2);

            switch (type)
            {
                case Type.Debug:
                    _stringBuilder.Append("]D:");
                    break;

                case Type.Warning:
                    _stringBuilder.Append("]W:");
                    break;

                case Type.Error:
                    _stringBuilder.Append("]E:");
                    break;
            }
        }

    }
}
