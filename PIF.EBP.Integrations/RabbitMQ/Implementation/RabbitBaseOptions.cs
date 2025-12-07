using RabbitMQ.Client;
using System;
using System.Net.Security;
using System.Security.Authentication;

namespace PIF.EBP.Integrations.RabbitMQ.Implementation
{
    public class RabbitBaseOptions
    {
        public string Host { get; set; } = string.Empty;

        public string VirtualHost { get; set; } = string.Empty;

        public string User { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public int Port { get; set; } = AmqpTcpEndpoint.UseDefaultPort;
        public TimeSpan RequestedHeartbeat { get; set; } = TimeSpan.FromSeconds(60);
        public RabbitSslOptions Tls { get; set; } = new RabbitSslOptions();
    }

    public class RabbitSslOptions
    {
        public bool Enabled { get; set; }
        public SslProtocols Protocols { get; set; } = SslProtocols.Tls12;
        public SslPolicyErrors AcceptablePolicyErrors { get; set; } = SslPolicyErrors.None;
        public RemoteCertificateValidationCallback CertificateValidationCallback { get; set; }

    }
}
