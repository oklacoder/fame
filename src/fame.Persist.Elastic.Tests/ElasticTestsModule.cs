using fame.Tests;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nest;
using System;

namespace fame.Persist.Elastic.Tests
{
    public class ElasticTestsModule
    {
        IConfiguration config;
        protected const int ElasticConsistencyDelay = 200;

        protected ServiceProvider GetServices()
        {
            var services = new ServiceCollection();

            config = new ConfigurationBuilder()
               .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
               .AddJsonFile("testConfig.json").Build();
            services.AddSingleton<IConfiguration>(config);

            var conn = new ConnectionSettings(new Uri("http://localhost:9200"));
            conn.BasicAuthentication("elastic", "elastic");
            var client = new Nest.ElasticClient(conn);
            services.AddSingleton(client);

            services.AddSingleton<IFamePlugin, ElasticPlugin>();

            services.AddSingleton<TestCommandOperator>();
            services.AddSingleton<TestQueryOperator>();
            services.AddSingleton<TestEventOperator>();
            services.AddSingleton<TestResponseOperator>();

            return services.BuildServiceProvider();
        }

        protected string GetIndexForMessage(object obj)
        {
            return ElasticPlugin.GetIndexNameFromObject(obj, config.GetValue<string>("ElasticServer:IndexPrefix"));
        }
    }
}
