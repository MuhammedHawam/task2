using PIF.EBP.Application.Db.Context;
using PIF.EBP.Application.Db.Model;
using System;

namespace PIF.EBP.Application.Db.Implementation
{
    public class LogDbAppService : ILogDbAppService
    {
        private readonly LogDbContext _dbLogContext;
        public LogDbAppService(LogDbContext dbLogContext)
        {
            _dbLogContext = dbLogContext;
        }
        public void CreateLog(Exception exception)
        {
            var oLogTrace = new LogTrace { CorrelationId=Guid.NewGuid().ToString(),Details = exception.Message, RequestDateTime = DateTime.Now};
            _dbLogContext.LogTraces.Add(oLogTrace);
            _dbLogContext.SaveChanges();

        }
    }
}
