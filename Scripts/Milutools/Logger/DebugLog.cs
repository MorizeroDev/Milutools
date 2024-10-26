using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Milutools.Logger
{
    internal static class DebugLog
    {
#if UNITY_EDITOR
        private enum LogLevel
        {
            Info, Warning, Error
        }

        private const string LogLevelKey = "_unity_debug_milutools_log_level";
        
        private static LogLevel _logLevel = LogLevel.Info;

        [InitializeOnEnterPlayMode]
        private static void ReadConfig()
        {
            _logLevel = (LogLevel)EditorPrefs.GetInt(LogLevelKey, (int)LogLevel.Warning);
        }
        
        [MenuItem("Milutools/Log Level/Error", false, 2)]
        private static void SwitchLogLevelError()
        {
            SwitchLogLevel(LogLevel.Error);
        }
        
        [MenuItem("Milutools/Log Level/Warning", false, 1)]
        private static void SwitchLogLevelWarning()
        {
            SwitchLogLevel(LogLevel.Warning);
        }
        
        [MenuItem("Milutools/Log Level/Info", false, 0)]
        private static void SwitchLogLevelInfo()
        {
            SwitchLogLevel(LogLevel.Info);
        }
        
        private static void SwitchLogLevel(LogLevel level)
        {
            _logLevel = level;
            EditorPrefs.SetInt(LogLevelKey, (int)level);
            EditorUtility.DisplayDialog("Milutools", "Switched Milutools log level to " + level, "OK");
        }
        
        internal static void Log(string content)
        {
            if (_logLevel > LogLevel.Info)
            {
                return;
            }
            Debug.Log(content);
        }
        
        internal static void LogWarning(string content)
        {
            if (_logLevel > LogLevel.Warning)
            {
                return;
            }
            Debug.LogWarning(content);
        }
        
        internal static void LogError(string content)
        {
            if (_logLevel > LogLevel.Error)
            {
                return;
            }
            Debug.LogError(content);
        }
#else
        internal static void Log(string content)
        {
            
        }
        
        internal static void LogWarning(string content)
        {
            
        }
        
        internal static void LogError(string content)
        {
            
        }
#endif
    }
}
