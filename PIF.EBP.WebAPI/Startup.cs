using Microsoft.Extensions.DependencyInjection;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Owin;
using PIF.EBP.WebAPI.Middleware.Exceptions;
using PIF.EBP.WebAPI.Middleware.Logging;
using System;
using System.Configuration;
using System.Threading.Tasks;
using System.Web.Cors;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using System.Linq;
using PIF.EBP.Core.DependencyInjection;
using Castle.Windsor;
using PIF.EBP.WebAPI.Resolvers;
using Castle.MicroKernel.Registration;
using PIF.EBP.WebAPI.Modules;
using Castle.Facilities.Logging;
using Castle.Services.Logging.Log4netIntegration;
using Newtonsoft.Json.Serialization;
using System.Net;
using PIF.EBP.Application.Db.Context;
using PIF.EBP.Application;
using PIF.EBP.Integrations.RedisCache;
using PIF.EBP.Core.Caching;
using Castle.MicroKernel.Lifestyle;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk;
using PIF.EBP.Core.Helpers;
using System.Net.Http;
using PIF.EBP.Core.Utilities;
using PIF.EBP.Core.Shared;
using System.Net.Http.Headers;
using System.Text;

[assembly: OwinStartup(typeof(PIF.EBP.WebAPI.Startup))]
namespace PIF.EBP.WebAPI
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var services = new ServiceCollection();

            var container = new WindsorContainer();

            WindsorContainerProvider.Container = container;

            container.AddFacility<LoggingFacility>(f => f.LogUsing<Log4netFactory>().WithConfig("log4net.config"));

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            IServiceManagement<IOrganizationService> orgServiceManagement =
            ServiceConfigurationFactory.CreateManagement<IOrganizationService>(
            new Uri(ConfigurationManager.AppSettings["CrmOrganizationService"]));

            AuthenticationCredentials credentials = new AuthenticationCredentials();
            var encryptedUserName = ConfigurationManager.AppSettings["CrmUserName"];
            var encryptedPassword = ConfigurationManager.AppSettings["CrmPassword"];

            credentials.ClientCredentials.UserName.UserName = CryptoUtils.Decrypt(encryptedUserName);
            credentials.ClientCredentials.UserName.Password = CryptoUtils.Decrypt(encryptedPassword);

            AuthenticationCredentials authCredentials = orgServiceManagement.Authenticate(credentials);

            Uri serviceUri = new Uri(ConfigurationManager.AppSettings["CrmOrganizationService"]);


            container.Install(new CrmModule(orgServiceManagement, authCredentials));
            container.Register(Component.For<GlobalExceptionHandler>().ImplementedBy<GlobalExceptionHandler>().LifestyleTransient(),
                                Component.For<LogDbContext>().ImplementedBy<LogDbContext>().LifestyleScoped(),
                                Component.For<ITypedCache>().UsingFactoryMethod(() => new RedisCacheService()).LifestyleSingleton(),
                                Component.For(typeof(ICacheManagerBase<,>)).ImplementedBy(typeof(CacheManagerBase<,>)).LifestyleScoped(),
                                Component.For(typeof(ICacheManagerBase<>)).ImplementedBy(typeof(CacheManagerBase<>)).LifestyleScoped(),
                                Component.For<HttpClientServiceFactory>().ImplementedBy<HttpClientServiceFactory>().LifestyleTransient(),
                                Component.For<HttpClient>().UsingFactoryMethod(() =>
                                {
                                    var client = new HttpClient
                                    {
                                        BaseAddress = new Uri(ConfigurationManager.AppSettings["EsmServerUrl"])
                                    };
                                    // Adding Basic Authentication header
                                    var byteArray = Encoding.ASCII.GetBytes($"{ConfigurationManager.AppSettings["EsmUsername"]}:{ConfigurationManager.AppSettings["EsmPassword"]}");
                                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                                    return client;

                                }).LifestyleScoped().Named(ApplicationConsts.ESMHttpClientName)
                                );
            ConfigureServices(services, container);

            


            var config = new HttpConfiguration();
            config.Services.Replace(typeof(IExceptionHandler), container.Resolve<GlobalExceptionHandler>());
            config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            config.Formatters.JsonFormatter.UseDataContractJsonSerializer = false;

            //This is for DEV only
            bool.TryParse(ConfigurationManager.AppSettings["DevelopmentMode"], out bool developmentMode);
            if (developmentMode)
            {
                ServicePointManager.ServerCertificateValidationCallback +=
                (sender, cert, chain, sslPolicyErrors) => true;
            }
            

            var serviceProvider = services.BuildServiceProvider();

            config.DependencyResolver = new WindsorDependencyResolver(container);
            config.MapHttpAttributeRoutes();
            var allowedOriginsConfig = ConfigurationManager.AppSettings["Origins"];

            config.MessageHandlers.Add(container.Resolve<LoggingMiddleware>());

            if (!string.IsNullOrEmpty(allowedOriginsConfig))
            {
                var allowedOrigins = allowedOriginsConfig
                    .Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);

                var corsPolicy = new CorsPolicy()
                {
                    AllowAnyOrigin = true,
                    AllowAnyHeader = true,
                    AllowAnyMethod = true,
                    SupportsCredentials = true,
                    ExposedHeaders = { "Authorization" }
                };
                foreach (var origin in allowedOrigins)
                    corsPolicy.Origins.Add(origin);

                var policyProvider = new CorsPolicyProvider()
                {
                    PolicyResolver = (context) => Task.FromResult(corsPolicy)
                };
                var corsOptions = new CorsOptions()
                {
                    PolicyProvider = policyProvider
                };

                app.UseCors(corsOptions);
            }

            app.Use(async (context, next) =>
            {
                // Create a child container for each request or scope
                using (var scope = container.BeginScope())
                {
                    await next.Invoke();
                }
            });

            app.Use(async (context, next) =>
            {
                context.Response.Headers.Remove("x-powered-by");
                context.Response.Headers.Remove("server");
                await next.Invoke();
            });


            app.UseWebApi(config);
        }

        private void ConfigureServices(IServiceCollection services, IWindsorContainer container)
        {
            //Registering Transient Services
            RegisterDependencies(typeof(ITransientDependency), container);

            //Registering Scoped Services
            RegisterDependencies(typeof(IScopedDependency), container);

            //Registering Transient Services
            RegisterDependencies(typeof(ISingletonDependency), container);
        }


        //Func<Type, Type, IServiceCollection> addDependencyFunc, was using microsoft dependency injection and pass the dependency registration in method call
        private void RegisterDependencies(Type dependencyType, IWindsorContainer container)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.FullName.ToLower().Contains("pif")).ToList();

            var types = assemblies.SelectMany(s => s.GetTypes())
                                 .Where(t => dependencyType.IsAssignableFrom(t) && t.IsClass && !t.IsAbstract);

            foreach (var type in types)
            {
                var interfaceType = type.GetInterfaces().FirstOrDefault(i => i != dependencyType && !i.FullName.ToLower().Contains("system"));

                if (dependencyType == typeof(ITransientDependency))
                {
                    container.Register(Component.For(interfaceType ?? type).ImplementedBy(type).LifestyleTransient());
                }
                if (dependencyType == typeof(IScopedDependency))
                {
                    container.Register(Component.For(interfaceType ?? type).ImplementedBy(type).LifestyleScoped());
                }
                if (dependencyType == typeof(ISingletonDependency))
                {
                    container.Register(Component.For(interfaceType ?? type).ImplementedBy(type).LifestyleSingleton());
                }
            }
        }
    }
}