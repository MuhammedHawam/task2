using PIF.EBP.Application.Db.Model;
using System;
using System.Configuration;
using System.Data.Entity;

namespace PIF.EBP.Application.Db.Context
{
    public class LogDbContext : DbContext
    {
        public static string connString
        {
            get
            {
                if (ConfigurationManager.ConnectionStrings["AzureDbConnectionString"] == null)
                {
                    throw new InvalidOperationException("Connection string 'AzureDbConnectionString' not found.");
                }
                string ConnStr = ConfigurationManager.ConnectionStrings["AzureDbConnectionString"].ConnectionString;
                return ConnStr;
            }

        }
        public LogDbContext() : base(connString)
        {

        }

        public DbSet<LogTrace> LogTraces { get; set; }
    }
}
