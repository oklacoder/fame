using fame.Tests;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using Xunit;
using Xunit.Abstractions;

namespace fame.Persist.Postgresql.Tests
{
    public class PostgresTestsModule
    {
        IConfiguration config;
        protected const int ElasticConsistencyDelay = 200;

        public PostgresTestsModule(
            ITestOutputHelper output)
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .MinimumLevel.Verbose()
                .WriteTo.TestOutput(output, Serilog.Events.LogEventLevel.Verbose)
                //.WriteTo.Console()
                //.WriteTo.File(@"C:\temp\logs\tests\fame.persist.seaq\")
                .CreateLogger();

            
            _ = new TestCommand();
            _ = new TestEvent();
            _ = new TestQuery();
            _ = new TestResponse();
        }

        protected ServiceProvider GetServices()
        {
            var services = new ServiceCollection();
            var _config = new PostgresPluginConfig();

            config = new ConfigurationBuilder()
               .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
               .AddJsonFile("testConfig.json").Build();
            services.AddSingleton<IConfiguration>(config);

            config.GetSection(PostgresPluginConfig.PostgresPluginConfig_Key).Bind(_config);

            
            services.AddSingleton<IFamePlugin, PostgresPlugin>();

            services.AddSingleton<TestCommandOperator>();
            services.AddSingleton<TestQueryOperator>();
            services.AddSingleton<TestEventOperator>();
            services.AddSingleton<TestResponseOperator>();

            return services.BuildServiceProvider();
        }

    }
    public class UnitTest1 :
        PostgresTestsModule
    {
        public UnitTest1(
            ITestOutputHelper output) :
            base(output)
        {

        }
        [Fact]
        public async void CommandOperator_CanConfigureAndExecute_HappyPath()
        {
            var services = GetServices();
            var opr = services.GetService<TestCommandOperator>();

            var args = new TestCommandArgs() { IsValid = true, ShouldThrow = false };
            var cmd = new TestCommand(args);
            await opr.SafeHandle<TestResponse>(cmd);
        }
        [Fact]
        public async void EventOperator_CanConfigureAndExecute_HappyPath()
        {
            var services = GetServices();
            var opr = services.GetService<TestEventOperator>();

            var args = new TestEventArgs() { ShouldThrow = false };
            var cmd = new TestEvent(args);
            await opr.SafeHandle<TestResponse>(cmd);
        }
        [Fact]
        public async void QueryOperator_CanConfigureAndExecute_HappyPath()
        {
            var services = GetServices();
            var opr = services.GetService<TestQueryOperator>();

            var args = new TestQueryArgs() { ShouldThrow = false };
            var cmd = new TestQuery(args);
            await opr.SafeHandle<TestResponse>(cmd);
        }
        [Fact]
        public async void ResponseOperator_CanConfigureAndExecute_HappyPath()
        {
            var services = GetServices();
            var opr = services.GetService<TestResponseOperator>();

            var args = new TestResponseArgs() { ShouldThrow = false };
            var cmd = new TestResponse(args);
            await opr.SafeHandle<TestResponse>(cmd);
        }
    }
}
