using PIF.EBP.Core.DependencyInjection;
using PIF.EBP.Core.Messaging;
using PIF.EBP.Core.Messaging.DTOs;
using PIF.EBP.WebAPI.Middleware.ActionFilter;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;

namespace PIF.EBP.WebAPI.Controllers
{
    [RoutePrefix("Messaging")]
    [ApiResponseWrapper]
    public class MessagingController : BaseController
    {
        private readonly IMessageQueueService _messageQueueService;

        public MessagingController()
        {
            _messageQueueService = WindsorContainerProvider.Container.Resolve<IMessageQueueService>();
        }

        [HttpPost]
        [Route("send-messages")]
        public async Task<IHttpActionResult> SendMessages(MessagingRequestDto messagingRequestDto)
        {
            await _messageQueueService.DeclareExchangeAsync(messagingRequestDto.exchange.Name, messagingRequestDto.exchange.Type, messagingRequestDto.exchange.MessageTTL);
            await _messageQueueService.DeclareQueueAsync(messagingRequestDto.queue.Name, messagingRequestDto.queue.MaxLength, messagingRequestDto.queue.DlxExchange);
            await _messageQueueService.BindQueueToExchangeAsync(messagingRequestDto.queue.Name, messagingRequestDto.exchange.Name, messagingRequestDto.bindSettings.bindingKey, messagingRequestDto.bindSettings.Headers);

            List<Task> messagesToSend = new List<Task>();
            foreach (Message message in messagingRequestDto.messages)
            {
                messagesToSend.Add(_messageQueueService.SendMessageAsync(messagingRequestDto.exchange.Name, message.MessageRoutingKey, message.MessageContant));
            }
            await Task.WhenAll(messagesToSend);

            return Ok();
        }

        [HttpGet]
        [Route("receive-messages")]
        public async Task<IHttpActionResult> ReceiveMessages(string queueName)
        {
            MessagingResponseDto messagingResponseDto = new MessagingResponseDto();
            messagingResponseDto.Messages = await _messageQueueService.ReceiveMessagesAsync(queueName);

            return Ok(messagingResponseDto);
        }
    }
}