using System.Collections.Generic;

namespace PIF.EBP.Core.Messaging.DTOs
{
    public class MessagingRequestDto
    {
        public Exchange exchange { get; set; }
        public Queue queue { get; set; }
        public BindSettings bindSettings { get; set; }
        public List<Message> messages { get; set; }
    }

    public class Message
    {
        public string MessageRoutingKey { get; set; }
        public object MessageContant { get; set; }
    }

    public class Exchange
    {
        public string Name { get; set; }
        public string Type { get; set; } = ExchangeType.Direct;
        public long? MessageTTL { get; set; } = null;
    }

    public class Queue
    {
        public string Name { get; set; }
        public int? MaxLength { get; set; } = null;
        public string DlxExchange { get; set; }
    }

    public class BindSettings
    {
        public Dictionary<string, object> Headers { get; set; }
        public string bindingKey { get; set; }
    }

    public static class ExchangeType
    {
        public const string Direct = "direct";
        public const string Fanout = "fanout";
        public const string Topic = "topic";
        public const string Headers = "headers";
        public const string Default = "default";
    }
}
