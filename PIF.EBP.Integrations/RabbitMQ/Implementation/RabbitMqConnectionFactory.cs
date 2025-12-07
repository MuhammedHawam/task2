using PIF.EBP.Core.Utilities;
using RabbitMQ.Client;
using System;
using System.Security.Authentication;
using System.Threading;

namespace PIF.EBP.Integrations.RabbitMQ.Implementation
{
    public class RabbitMqConnectionFactory : IRabbitMqConnectionFactory
    {
        private readonly IConnectionFactory _connectionFactory;
        private readonly Lazy<IConnection> _lazyConnection;
        private readonly Lazy<IModel> _lazyChannel;

        private RabbitMqConnectionFactory(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;

            _lazyConnection = new Lazy<IConnection>(() =>
                _connectionFactory.CreateConnection(), LazyThreadSafetyMode.ExecutionAndPublication);

            _lazyChannel = new Lazy<IModel>(() =>
                CurrentConnection.CreateModel(), LazyThreadSafetyMode.ExecutionAndPublication);
        }

        public static IRabbitMqConnectionFactory From(RabbitBaseOptions options)
        {
            Guard.AssertArgumentNotNull(options);

            return new RabbitMqConnectionFactory(FromConfig(options));
        }

        private static IConnectionFactory FromConfig(RabbitBaseOptions baseOptions)
        {
            Guard.AssertArgumentNotNull(baseOptions);

            return new ConnectionFactory
            {
                HostName = baseOptions.Host,
                Port = baseOptions.Port,
                VirtualHost = baseOptions.VirtualHost,
                UserName = baseOptions.User,
                Password = baseOptions.Password,
                RequestedHeartbeat = baseOptions.RequestedHeartbeat,
                Ssl = new SslOption
                {
                    Enabled = baseOptions.Tls.Enabled,
                    ServerName = baseOptions.Host,
                    Version = baseOptions.Tls.Enabled ? baseOptions.Tls.Protocols : SslProtocols.None,
                    AcceptablePolicyErrors = baseOptions.Tls.AcceptablePolicyErrors,
                    CertificateValidationCallback = baseOptions.Tls.CertificateValidationCallback
                }
            };
        }

        public string UserName => _connectionFactory.UserName;
        public string Password => _connectionFactory.Password;
        public string VirtualHost => _connectionFactory.VirtualHost;

        public IConnection CurrentConnection => _lazyConnection.Value;

        public IModel CurrentChannel => _lazyChannel.Value;

        public IConnection CreateConnection() => _connectionFactory.CreateConnection();

        public IModel CreateModel() => CurrentConnection.CreateModel();

        public void Dispose()
        {
            if (_lazyChannel.IsValueCreated)
            {
                _lazyChannel.Value.Dispose();
            }

            if (_lazyConnection.IsValueCreated)
            {
                _lazyConnection.Value.Dispose();
            }
        }
    }
}
