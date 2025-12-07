using Newtonsoft.Json;
using PIF.EBP.Core.Messaging;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;

namespace PIF.EBP.Integrations.RabbitMQ.Implementation
{
    public class RabbitMQService : IMessageQueueService, IDisposable
    {
        private readonly IRabbitMqConnectionFactory _rabbitMqConnectionFactory;
        private IConnection _connection;
        private IModel _channel;

        public RabbitMQService()
        {
            var jsonConfiguration = ConfigurationManager.AppSettings["RabbitMqConfig"];
            var options = JsonConvert.DeserializeObject<RabbitBaseOptions>(jsonConfiguration);
            _rabbitMqConnectionFactory = RabbitMqConnectionFactory.From(options);

            InitializeConnection();
        }

        private void InitializeConnection()
        {
            try
            {
                _connection = _rabbitMqConnectionFactory.CreateConnection();
                _channel = _connection.CreateModel();
                _channel.BasicQos(0, 1, false);
                _channel.ConfirmSelect();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
        }

        public async Task SendMessageAsync<T>(string exchangeName, string routingKey, T message, Dictionary<string, object> headers = null, byte priority = 9)
        {
            try
            {
                var properties = _channel.CreateBasicProperties();
                properties.Headers = headers;
                properties.Priority = priority;
                properties.Persistent = true;

                var json = JsonConvert.SerializeObject(message);
                var body = Encoding.UTF8.GetBytes(json);

                await Task.Run(() => _channel.BasicPublish(exchange: exchangeName, routingKey: routingKey, basicProperties: properties, body: body));
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
        }

        public async Task<List<byte[]>> ReceiveMessagesAsync(string queueName)
        {
            var messages = new List<byte[]>();
            try
            {
                await Task.Run(() =>
                {
                    BasicGetResult result;
                    while ((result = _channel.BasicGet(queueName, false)) != null)
                    {
                        var body = result.Body.ToArray();
                        messages.Add(body);
                        _channel.BasicAck(result.DeliveryTag, false);
                    }
                });
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }

            return messages;
        }

        public async Task DeclareExchangeAsync(string exchangeName, string exchangeType, long? messageTTL)
        {
            try
            {
                Dictionary<string, object> args = new Dictionary<string, object>();
                if (messageTTL.HasValue && messageTTL > 0)
                {
                    args = new Dictionary<string, object> { { "x-message-ttl", messageTTL } };
                }

                await Task.Run(() => _channel.ExchangeDeclare(exchange: exchangeName, type: exchangeType, durable: true, arguments: args));
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
        }

        public async Task DeclareQueueAsync(string queueName, int? maxLength, string dlxExchange)
        {
            try
            {
                Dictionary<string, object> args = new Dictionary<string, object>();
                if (maxLength.HasValue && maxLength > 0)
                {
                    args.Add("x-max-length", maxLength);
                }
                if (!string.IsNullOrEmpty(dlxExchange))
                {
                    args.Add("x-dead-letter-exchange", dlxExchange);
                }

                await Task.Run(() => _channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: args));
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
        }

        public async Task BindQueueToExchangeAsync(string queueName, string exchangeName, string bindingKey, Dictionary<string, object> headers = null)
        {
            try
            {
                await Task.Run(() => _channel.QueueBind(queue: queueName, exchange: exchangeName, routingKey: bindingKey, arguments: headers));
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
        }

        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
}