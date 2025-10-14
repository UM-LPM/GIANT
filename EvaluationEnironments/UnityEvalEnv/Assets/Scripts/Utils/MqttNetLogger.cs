using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Server;
using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using UnityEngine;

namespace Utils
{
    public static class MqttNetLogger
    {
        private static string Server = "broker.emqx.io";
        private static int Port = 1883;
        private static string ClientId = "UnityMqttNet_" + System.Guid.NewGuid().ToString();
        private static string Topic = "giant/unity/logs";

        private static IMqttClient mqttClient;

        public static async Task Connect()
        {
            var factory = new MqttFactory();
            mqttClient = factory.CreateMqttClient();
            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(Server, Port)
                .WithClientId(ClientId)
                .Build();
            await mqttClient.ConnectAsync(options);
            await Log("Unity MQTT Logger initialized");
        }

        public static async System.Threading.Tasks.Task Log(string message, MqttNetLogType logType = MqttNetLogType.Log)
        {
            var msgPayload = new MqttNetLogObject(message, logType);

            if (mqttClient.IsConnected)
            {
                var msg = new MqttApplicationMessageBuilder()
                    .WithTopic(Topic)
                    .WithPayload(System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(msgPayload)))
                    .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                    .Build();
                await mqttClient.PublishAsync(msg);
            }
        }

        public static MqttNetLogType MapLogTypeToMqttLogType(LogType logType)
        {
            return logType switch
            {
                LogType.Error => MqttNetLogType.Error,
                LogType.Assert => MqttNetLogType.Assert,
                LogType.Warning => MqttNetLogType.Warning,
                LogType.Log => MqttNetLogType.Log,
                LogType.Exception => MqttNetLogType.Exception,
                _ => MqttNetLogType.Log
            };
        }

        public static async void Disconnect()
        {
            await mqttClient.DisconnectAsync();
        }

        public static bool IsConnected()
        {
            return mqttClient != null && mqttClient.IsConnected;
        }
    }

    [Serializable]
    public class MqttNetLogObject
    {
        public string message;
        public MqttNetLogType logType;
        public long timestamp;

        public int timeZoneOffset = 2; // Example offset (GMT+2), adjust as needed

        public MqttNetLogObject(string message, MqttNetLogType logType)
        {
            this.message = message;
            this.timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            this.logType = logType;
        }

        public override string ToString()
        {
            var dateTime = DateTimeOffset.FromUnixTimeSeconds(timestamp).ToUniversalTime().AddHours(timeZoneOffset);
            return $"{dateTime:yyyy-MM-dd HH:mm:ss} [{logType}] - {message}";
        }
    }

    public enum MqttNetLogType
    {
        Error,
        Assert,
        Warning,
        Log,
        Exception
    }
}