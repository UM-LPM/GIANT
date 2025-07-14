using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using System.Text;

namespace WebAPI.Models
{
    public class MqttClientService : IHostedService, IDisposable
    {
        public static string Server = "broker.emqx.io";
        public static int Port = 1883;
        public static string ClientId = "WebApiClient_" + System.Guid.NewGuid().ToString();
        public static string Topic = "giant/webapi/logs";

        private readonly IMqttClient _mqttClient;
        private readonly MqttClientOptions _options;
        private readonly ILogger<MqttClientService> _logger;

        public MqttClientService(ILogger<MqttClientService> logger)
        {
            _logger = logger;
            var factory = new MqttFactory();
            _mqttClient = factory.CreateMqttClient();

            // Configure MQTT client options
            _options = new MqttClientOptionsBuilder()
                .WithTcpServer(Server, Port) // Replace with your broker address
                .WithClientId(ClientId)
                .Build();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Attempting to connect to MQTT broker...");
            try
            {
                await _mqttClient.ConnectAsync(_options, cancellationToken);
                _logger.LogInformation("Successfully connected to MQTT broker.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to MQTT broker.");
                throw;
            }
        }

        public async Task PublishAsync(string topic, string payload)
        {
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(Encoding.UTF8.GetBytes(payload))
                .Build();

            await _mqttClient.PublishAsync(message);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("MQTT Client Service is stopping.");
            await _mqttClient.DisconnectAsync(new MqttClientDisconnectOptions(), cancellationToken);
            _logger.LogInformation("MQTT Client is disconnected.");
        }

        public void Dispose()
        {
            _mqttClient?.Dispose();
        }

        public IMqttClient GetMqttClient()
        {
            return _mqttClient;
        }
    }
}
