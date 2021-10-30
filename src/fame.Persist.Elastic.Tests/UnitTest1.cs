using fame.Tests;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using Xunit;

namespace fame.Persist.Elastic.Tests
{
    public class CommandOperator_ElasticApmTests
    {

        private ServiceProvider GetServices()
        {
            var services = new ServiceCollection();

            var config = new ConfigurationBuilder()
               .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
               .AddJsonFile("testConfig.json").Build();
            services.AddSingleton<IConfiguration>(config);

            services.AddSingleton<IFamePlugin, ElasticPlugin>();

            services.AddSingleton<TestCommandOperator>();
            services.AddSingleton<TestQueryOperator>();
            services.AddSingleton<TestEventOperator>();
            services.AddSingleton<TestResponseOperator>();

            return services.BuildServiceProvider();
        }

        [Fact]
        public async void CommandOperator_CanConfigureAndExecute_HappyPath()
        {
            var services = GetServices();
            var opr = services.GetService<TestCommandOperator>();

            var msg = new TestCommand();

            Assert.Equal(1, opr.Plugins.Count());
            Assert.Equal(typeof(ElasticPlugin).FullName, opr.Plugins.FirstOrDefault());

            //fame.Persist.Elastic.ElasticPlugin.Configure(config);
            //fame.Persist.Elastic.ElasticPlugin.Enroll(opr);

            var resp = await opr.SafeHandle<TestResponse>(msg);

            Assert.NotNull(resp);
            Assert.Equal(msg.RefId, resp.SourceId);
            Assert.True(resp.IsValid);
            Assert.True(resp.Successful);
        }
    }
}
