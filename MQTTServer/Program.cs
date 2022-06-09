using Serilog;
using System;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using MQTTnet.Protocol;
using MQTTnet.Server;
using MQTTnet;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

namespace MQTTServer
{
    class Server
    {
        [Obsolete]
        static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            var currentPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var certificate = new X509Certificate2(Path.Combine(currentPath, "certificate.pfx"), "", X509KeyStorageFlags.Exportable);

            var optionsBuilder = new MqttServerOptionsBuilder()
                .WithoutDefaultEndpoint() // This call disables the default unencrypted endpoint on port 1883
                .WithEncryptedEndpoint()
                .WithEncryptedEndpointPort(8883)
                .WithEncryptionCertificate(certificate.Export(X509ContentType.Pfx))
                .WithEncryptionSslProtocol(SslProtocols.Tls12);

            var server = new MqttFactory().CreateMqttServer();
            await server.StartAsync(optionsBuilder.Build());

            
            if(server.IsStarted)
            {
                Log.Logger.Information("Server is started on port 8883");
            }
            Log.Logger.Information("Press Enter to exit.");

            while(true)
            {
            }
            //Console.ReadLine();

            // Stop the server after reading any key in terminal
            //await server.StopAsync();
        }
    }
}
