using PIF.EBP.Core.DependencyInjection;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PIF.EBP.Core.Messaging
{
    public interface IMessageQueueService : ITransientDependency
    {
        Task SendMessageAsync<T>(string exchangeName, string routingKey, T message, Dictionary<string, object> headers = null, byte priority = 9);
        Task<List<byte[]>> ReceiveMessagesAsync(string queueName);
        Task DeclareExchangeAsync(string exchangeName, string exchangeType, long? messageTTL);
        Task DeclareQueueAsync(string queueName, int? maxLength, string dlxExchange);
        Task BindQueueToExchangeAsync(string queueName, string exchangeName, string bindingKey, Dictionary<string, object> headers = null);
    }
}
