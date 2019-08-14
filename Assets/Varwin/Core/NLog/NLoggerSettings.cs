using System.IO;
using NLog;
using NLog.Config;
using NLog.Targets;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace NLogger
{
    public static class NLoggerSettings
    {
        public static bool LogDebug = false;
        private static object[] _objects;

        public static void Init()
        {
            if (File.Exists(Application.persistentDataPath + "/process.log"))
            {
                File.Delete(Application.persistentDataPath + "/process.log");
            }

            if (File.Exists(Application.persistentDataPath + "/error.log"))
            {
                File.Delete(Application.persistentDataPath + "/error.log");
            }

            LoggingConfiguration config = new LoggingConfiguration();

            FileTarget processLogFile = new FileTarget("process") {FileName = Application.persistentDataPath + "/process.log"};
            FileTarget errorLogFile = new FileTarget("error") {FileName = Application.persistentDataPath + "/error.log"};

            MethodCallTarget unityDebugLog = new MethodCallTarget("logconsole", LogEventAction);

            config.AddRule(LogLevel.Debug, LogLevel.Debug, unityDebugLog);
            config.AddRule(LogLevel.Info, LogLevel.Info, unityDebugLog);
            config.AddRule(LogLevel.Error, LogLevel.Error, unityDebugLog);
            config.AddRule(LogLevel.Fatal, LogLevel.Fatal, unityDebugLog);

            if (LogDebug)
            {
                config.AddRule(LogLevel.Debug, LogLevel.Debug, processLogFile);
            }

            config.AddRule(LogLevel.Info, LogLevel.Info, processLogFile);
            config.AddRule(LogLevel.Error, LogLevel.Debug, processLogFile);
            config.AddRule(LogLevel.Fatal, LogLevel.Info, processLogFile);

            config.AddRule(LogLevel.Error, LogLevel.Error, errorLogFile);
            config.AddRule(LogLevel.Fatal, LogLevel.Fatal, errorLogFile);

            LogManager.Configuration = config;
        }

        private static void LogEventAction(LogEventInfo logEventInfo, object[] objects)
        {
            
            
            if (LogDebug && logEventInfo.Level == LogLevel.Debug)
            {
                Debug.Log(logEventInfo.Message);
            }

            if (logEventInfo.Level == LogLevel.Info)
            {
                Debug.Log(logEventInfo.Message);
            }

            if (logEventInfo.Level == LogLevel.Error)
            {
                Debug.LogError(logEventInfo.Message);
            }

            if (logEventInfo.Level == LogLevel.Fatal)
            {
                Debug.LogError(logEventInfo.Message);
            }
        }
    }
}