using MQTTnet;
using MQTTnet.Client;
using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Security.Authentication;
using MQTTnet.Client.Options;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using System.IO;
using System.Reflection;

namespace MQTTSubscriber
{
    class Subscriber
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

            using var client = new MqttFactory().CreateMqttClient();
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

            client.UseApplicationMessageReceivedHandler(e =>
            {
                Log.Logger.Information("Received message.");
                Log.Logger.Information(Encoding.UTF8.GetString(e.ApplicationMessage.Payload));
                //Console.WriteLine(JsonSerializer.Serialize(e));
            });

            client.UseConnectedHandler(async e =>
            {
                OnConnected(e);
                var topicFilter = new MqttTopicFilterBuilder()
                    .WithTopic("Teltonika")
                    .Build();

                await client.SubscribeAsync(topicFilter);
                Log.Logger.Information("client is subscribed to topic Teltonika");

            });
            client.UseDisconnectedHandler(e =>
            {
                OnDisconnected(e);
            });


            await client.ConnectAsync(opetions, CancellationToken.None);

            Console.ReadLine();

            await client.DisconnectAsync();

        }

        private static void OnDisconnected(MqttClientDisconnectedEventArgs e)
        {
            Log.Logger.Information("successfully disconnected.");
        }

        private static void OnConnected(MqttClientConnectedEventArgs e)
        {
            Log.Logger.Information("Successfully connected.");
        }
    }
}
