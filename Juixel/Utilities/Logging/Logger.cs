using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Utilities.Threading;

namespace Utilities.Logging
{
    public static class Logger
    {
        public static bool SaveLogs = true;

        private const int Log_Flush_Count = 50;

        private static LockingList<string> _LogRecords = new LockingList<string>();
        private static LockingList<LogInfo> _Logs = new LockingList<LogInfo>();

        private static string LogDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "\\Logs\\");
        private static string LogFileName = Path.Combine(LogDirectory,
            $"{Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location)}___{DateTime.Now}".Replace(' ', '-').Replace('/', '_').Replace(':', '_'));

        public static void Log(object Value, bool Error = false, bool Record = false, ConsoleColor? Color = null, string Title = null)
        {
            string Log = Value.ToString();
            if (Title != null)
                Log = $"[{Title}] " + Log;
            Log = $"[{DateTime.Now}] " + Log;
            if (Error)
            {
                Log = "[Error] " + Log;
                if (Color == null)
                    Color = ConsoleColor.Red;
            }
            if (Color == null)
                Color = ConsoleColor.White;
            _Logs.Add(new LogInfo
            {
                Log = Log,
                Color = Color.Value
            });

            if (SaveLogs)
                _LogRecords.Add(Log);
        }

        public static void StepLog()
        {
            LogInfo[] Logs = _Logs.ToArrayAndClear();
            for (int i = 0; i < Logs.Length; i++)
            {
                LogInfo Log = Logs[i];
                if (Console.ForegroundColor != Log.Color)
                    Console.ForegroundColor = Log.Color;
                Console.WriteLine(Log.Log);
            }

            if (_LogRecords.Count >= Log_Flush_Count && SaveLogs)
                FlushLog();
        }

        public static void FlushLog()
        {
            string[] Records = _LogRecords.ToArrayAndClear();
            if (!Directory.Exists(LogDirectory))
                Directory.CreateDirectory(LogDirectory);
            File.AppendAllLines(LogFileName, Records);
        }

        private struct LogInfo
        {
            public string Log;
            public ConsoleColor Color;
        }

        public enum LogType
        {
            None,
            Minimal,
            Verbose
        }
    }
}
