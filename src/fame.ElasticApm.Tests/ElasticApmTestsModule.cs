using fame.Tests;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace fame.ElasticApm.Tests
{
    public class ElasticApmTestsModule
    {
        protected const string tran_index = "apm-7.15.0-transaction*";
        protected const string span_index = "apm-7.15.0-span*";

        protected ServiceProvider GetServices()
        {
            var services = new ServiceCollection();

            var config = new ConfigurationBuilder()
               .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
               .AddJsonFile("testConfig.json").Build();
            services.AddSingleton<IConfiguration>(config);

            services.AddSingleton<IFamePlugin, ElasticApmPlugin>();

            services.AddSingleton<TestCommandOperator>();
            services.AddSingleton<TestQueryOperator>();
            services.AddSingleton<TestEventOperator>();
            services.AddSingleton<TestResponseOperator>();

            return services.BuildServiceProvider();
        }
    }

}
