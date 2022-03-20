using fame.Tests;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nest;
using seaq;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;
using static fame.seaq.SeaqPlugin;

namespace fame.seaq.tests
{
    public class SeaqTestsModule
    {
        IConfiguration config;
        protected const int ElasticConsistencyDelay = 200;

        public SeaqTestsModule(
            ITestOutputHelper output)
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .MinimumLevel.Verbose()
                .WriteTo.TestOutput(output, Serilog.Events.LogEventLevel.Verbose)
                //.WriteTo.Console()
                //.WriteTo.File(@"C:\temp\logs\tests\fame.persist.seaq\")
                .CreateLogger();

            _searchableTypes = FieldNameUtilities.GetAllSearchableTypes().ToDictionary(t => t.FullName, t => t);
            _ = new TestCommand();
            _ = new TestEvent();
            _ = new TestQuery();
            _ = new TestResponse();
        }

        protected static Dictionary<string, Type> _searchableTypes;

        protected Type TryGetSearchType(
            string typeFullName)
        {
            if (_searchableTypes.TryGetValue(typeFullName, out var type))
            {
                return type;
            }
            else
            {
                return typeof(BaseDocument);
            }
        }

        protected ServiceProvider GetServices()
        {
            var services = new ServiceCollection();
            var _config = new SeaqPluginConfig();

            config = new ConfigurationBuilder()
               .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
               .AddJsonFile("testConfig.json").Build();
            services.AddSingleton<IConfiguration>(config);

            config.GetSection(SeaqPluginConfig.SeaqPluginConfig_Key).Bind(_config);

            var args = new ClusterArgs(
                _config?.ClusterScope,
                _config?.ClusterUrl,
                _config?.ClusterUser,
                _config?.ClusterPass,
                _config?.ClusterBypassCertificateValidation == true);
            var cluster = Cluster.Create(args);


            var conn = new ConnectionSettings(
                new Elasticsearch.Net.SingleNodeConnectionPool(new Uri(_config?.ClusterUrl)),
                (a, b) => new DefaultSeaqElasticsearchSerializer(TryGetSearchType));
            conn.BasicAuthentication(_config?.ClusterUser, _config?.ClusterPass);
            conn.ServerCertificateValidationCallback((a,b,c,d) => true);
            var client = new Nest.ElasticClient(conn);
            services.AddSingleton(client);

            services.AddSingleton<IFamePlugin, SeaqPlugin>();

            services.AddSingleton<TestCommandOperator>();
            services.AddSingleton<TestQueryOperator>();
            services.AddSingleton<TestEventOperator>();
            services.AddSingleton<TestResponseOperator>();

            return services.BuildServiceProvider();
        }

        protected string GetIndexForMessage(IMessage msg)
        {
            var w = new MessageWrapper(msg);
            return string.Format("{0}_{1}", "seaqplugin_tests", w.IndexName);
        }
    }
}
