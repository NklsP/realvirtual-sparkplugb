using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.TopicTemplate;
using realvirtual;
using System;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class SparkplugBInferface : InterfaceBaseClass
{
    [Tooltip("Address of MQTT broker")]
    public string Broker = "broker.hivemq.com"; //! Address of the MQTT Broker

    [Tooltip("Port number for the MQTT Broker")]
    public int Port = 1883; //! Port number for the MQTT Broker

    private bool mqttConnectionStatus = false;
    private bool lastConnectionStatus = false;
    private MqttTopicTemplate receivTemplate;


    public override void OpenInterface()
    {
        Connect();
        base.OpenInterface();
    }

    public override void CloseInterface()
    {
        Console.WriteLine($"Disconnect here on {nameof(CloseInterface)}");
    }

    void Update()
    {
        CheckStatus();
    }

    private void FixedUpdate()
    {

    }

    private async Task Connect()
    {
        var factory = new MqttFactory();

        using (var mqttClient = factory.CreateMqttClient())
        {
            var parameters = new MqttClientOptionsBuilderTlsParameters()
            {
                UseTls = true,
                SslProtocol = SslProtocols.Tls12
            };

            // Use builder classes where possible in this project.
            var clientOptions = new MqttClientOptionsBuilder()
                        .WithTcpServer(Broker, Port)
                        .Build();

            // This will throw an exception if the server is not available.
            // The result from this message returns additional data which was sent
            // from the server. Please refer to the MQTT protocol specification for details.
            mqttClient.ConnectedAsync += MqttClient_ConnectedAsync;
            mqttClient.DisconnectedAsync += MqttClient_DisconnectedAsync;

            var response = await mqttClient.ConnectAsync(clientOptions, CancellationToken.None);

            receivTemplate = new MqttTopicTemplate($"nicholas123/UNITY/#");

            // Subscribe to topics
            var mqttSubscribeOptions = factory.CreateSubscribeOptionsBuilder()
                .WithTopicTemplate(receivTemplate)
                .Build();

            mqttClient.ApplicationMessageReceivedAsync += MqttClient_ApplicationMessageReceivedAsync;

            var responsesubscribe = await mqttClient.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);

            Debug.Log(nameof(Connect));
        }
    }

    private async Task MqttClient_ApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs arg)
    {
       Debug.Log($"{arg.ApplicationMessage.ConvertPayloadToString()}");
    }

    private async Task MqttClient_DisconnectedAsync(MqttClientDisconnectedEventArgs arg)
    {
        mqttConnectionStatus = false;
        Debug.Log($"{nameof(MqttClient_DisconnectedAsync)}");
    }

    private async Task MqttClient_ConnectedAsync(MqttClientConnectedEventArgs arg)
    {
        mqttConnectionStatus = true;
        Debug.Log($"{nameof(MqttClient_ConnectedAsync)}");
    }

    void CheckStatus()
    {
        if (mqttConnectionStatus != lastConnectionStatus)
        {
            if (mqttConnectionStatus)
            {
                OnConnected();
            }
            else
            {
                OnDisconnected();
            }
        }
    }
}
