using System.IO;
using UnityEngine;

namespace Utils
{
    public class ErrorLogger : MonoBehaviour
    {
        public static ErrorLogger Instance;

        private string logFilePath;

        void Awake()
        {
            logFilePath = "error_log.txt";

            // Singleton pattern
            if (Instance != null)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(this);
            }

            // Subscribe to log message event
            Application.logMessageReceived += HandleLog;
        }

        void OnDestroy()
        {
            // Unsubscribe when the object is destroyed
            Application.logMessageReceived -= HandleLog;
        }

        async void HandleLog(string logString, string stackTrace, LogType type)
        {
            // Only logs, errors and exceptions (not warnings) are sent to MQTT broker to prevent spamming.
            if (type != LogType.Warning)
            {
                await MqttNetLogger.Log(logString, MqttNetLogger.MapLogTypeToMqttLogType(type));
            }

            if (type == LogType.Error || type == LogType.Exception)
            {
                string logEntry = $"{System.DateTime.Now}: {type}\n{logString}\n{stackTrace}\n";
                File.AppendAllText(logFilePath, logEntry);
            }

            if(type == LogType.Exception){
                DebugSystem.LogError(logString);
            }
        }
    }
}