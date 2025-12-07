using Castle.Core.Logging;
using PIF.EBP.Application.Db;
using PIF.EBP.Application.Shared.Resources;
using PIF.EBP.Core.DependencyInjection;
using PIF.EBP.Core.Exceptions;
using PIF.EBP.WebAPI.Response;

using System;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Web.Http.ExceptionHandling;
using static PIF.EBP.Application.Shared.Enums;

namespace PIF.EBP.WebAPI.Middleware.Exceptions
{
    //todo: this should only log errors. handling the error in response should be in the response wrapper
    public class GlobalExceptionHandler : ExceptionHandler
    {
        private readonly ILogger _logger;
        private ILogDbAppService _logDbAppService;
        public GlobalExceptionHandler(ILogger logger)
        {
            _logger = logger;
        }

        public override void Handle(ExceptionHandlerContext context)
        {
            LogException(context.Exception);

            if (context.Exception is UserFriendlyException)
            {
                var MessageEn = Messages.ResourceManager.GetString(context.Exception.Message);
                var MessageAr = MessagesAR.ResourceManager.GetString(context.Exception.Message);
                var placeholder = ((UserFriendlyException)context.Exception).Placeholder;
                if (!string.IsNullOrEmpty(placeholder))
                {
                    MessageEn= string.Format(MessageEn, placeholder);
                    MessageAr= string.Format(MessageAr, placeholder);
                }

                MessageEn = MessageEn ?? context.Exception.Message.ToString().Trim();
                MessageAr = MessageAr ?? context.Exception.Message.ToString().Trim();
                var apiResponseError = new ApiResponseError(MessageEn, MessageAr, context.Exception.Message);
                var response = context.Request.CreateResponse(HttpStatusCode.OK,
                new ApiResponse(((UserFriendlyException)context.Exception).HttpStatusCode.GetHashCode(),
                "Error",
                ((UserFriendlyException)context.Exception).CustomData ?? null,
                apiResponseError));
                context.Result = new ErrorMessageResult(response);
            }
            else
            {
                var apiResponseError = new ApiResponseError(Messages.MsgUnexpectedError, MessagesAR.MsgUnexpectedError, "MsgUnexpectedError");

                var response = context.Request.CreateResponse(HttpStatusCode.InternalServerError,
                    new ApiResponse(HttpStatusCode.InternalServerError.GetHashCode(), "Error", null, apiResponseError));
                context.Result = new ErrorMessageResult(response);
            }

        }

        private void LogException(Exception exception)
        {
            string logDest = ConfigurationManager.AppSettings.Get("LogDestination");

            if (logDest.Equals(LogDestination.SQL.ToString()))
                LogSQL(exception);

            else if (logDest.Equals(LogDestination.File.ToString()))
                LogFile(exception);

            else
            {
                LogSQL(exception);
                LogFile(exception);
            }
        }
        private void LogSQL(Exception exception)
        {
            _logDbAppService = WindsorContainerProvider.Container.Resolve<ILogDbAppService>();
            _logDbAppService.CreateLog(exception);

        }
        private void LogFile(Exception exception)
        {
            _logger.Error(exception.Message + "StackTrace= "+exception.StackTrace + " Source= "+exception.Source, exception);
            if (exception.InnerException != null)
            {
                LogFile(exception.InnerException);
            }
        }

    }
}