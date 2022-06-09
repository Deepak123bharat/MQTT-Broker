using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace MQTTPublisher
{
    class Publisher
    {
        static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            var currentPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var certificate = new X509Certificate2(Path.Combine(currentPath, "certificate.pfx"), "", X509KeyStorageFlags.Exportable);

            var client = new MqttFactory().CreateMqttClient();
            var opetions = new MqttClientOptionsBuilder()
                .WithClientId(Guid.NewGuid().ToString())
                .WithTcpServer("localhost", 8883)
                .WithTls(o =>
                {
                   o.UseTls = true;
                   o.SslProtocol = SslProtocols.Tls12;
                   o.Certificates = new List<X509Certificate>
                   {
                        certificate
                   };
                   o.AllowUntrustedCertificates = true;
                   o.IgnoreCertificateChainErrors = true;
                   o.IgnoreCertificateRevocationErrors = true;
                })
                .WithCleanSession()
                .Build();

            await client.ConnectAsync(opetions, CancellationToken.None);

            Log.Logger.Information("Write a message to publish");
            string message = Console.ReadLine();

            await PublishMessgeAsync(client, message);

            await client.DisconnectAsync();
        }

        private static async Task PublishMessgeAsync(IMqttClient client, string messagePayload)
        {
            var message = new MqttApplicationMessageBuilder()
                            .WithTopic("Teltonika")
                            .WithPayload(messagePayload)
                            .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                            .Build();

            if(client.IsConnected)
            {
                await client.PublishAsync(message);
                Log.Logger.Information("MQTT message is published.");
                Console.ReadLine();
            }
        }
    }
}
