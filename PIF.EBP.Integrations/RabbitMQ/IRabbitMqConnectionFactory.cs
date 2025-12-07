using RabbitMQ.Client;
using System;

namespace PIF.EBP.Integrations.RabbitMQ
{
    public interface IRabbitMqConnectionFactory : IDisposable
    {
        string UserName { get; }

        string Password { get; }

        string VirtualHost { get; }

        IConnection CurrentConnection { get; }
        IModel CurrentChannel { get; }
        IConnection CreateConnection();
        IModel CreateModel();
    }
}
