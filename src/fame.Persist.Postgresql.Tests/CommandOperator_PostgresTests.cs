using fame.Tests;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace fame.Persist.Postgresql.Tests
{

    public class PostgresTestsModule
    {
        IConfiguration config;
        PostgresPluginConfig _config;
        protected const int ConsistencyDelay = 1000;

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
            _config = new PostgresPluginConfig();

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


        protected ContextBase GetContext()
        {
            return new ContextBase(_config.PostgresqlConnection);
        }


    }
    public class CommandOperator_PostgresTests :
        PostgresTestsModule
    {
        public CommandOperator_PostgresTests(
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

            BaseCommand msg;

            await Task.Delay(ConsistencyDelay);

            using (var context = GetContext())
            {
                msg = await context.Commands.FirstOrDefaultAsync(x => x.RefId == cmd.RefId);
            }

            var type = msg.GetType().FullName;

            Assert.NotNull(msg);
            Assert.Equal(type, typeof(TestCommand).FullName);
            Assert.NotNull(msg.CompletedDateUtc);
            Assert.Null(msg.ValidationFailedDateUtc);
            Assert.Null(msg.ErrorDateUtc);
            Assert.Null(msg.ErrorMessage);
            Assert.Null(msg.ErrorStackTrace);
            Assert.NotNull(msg.Args);
        }
        [Fact]
        public async void CommandOperator_CanConfigureAndExecute_Invalid()
        {
            var services = GetServices();
            var opr = services.GetService<TestCommandOperator>();

            var args = new TestCommandArgs() { IsValid = false, ShouldThrow = false };
            var cmd = new TestCommand(args);
            await opr.SafeHandle<TestResponse>(cmd);

            BaseCommand msg;

            await Task.Delay(ConsistencyDelay);

            using (var context = GetContext())
            {
                msg = await context.Commands.FirstOrDefaultAsync(x => x.RefId == cmd.RefId);
            }

            var type = msg.GetType().FullName;

            Assert.NotNull(msg);
            Assert.Equal(type, typeof(TestCommand).FullName);
            Assert.Null(msg.CompletedDateUtc);
            Assert.NotNull(msg.ValidationFailedDateUtc);
            Assert.Null(msg.ErrorDateUtc);
            Assert.Null(msg.ErrorMessage);
            Assert.Null(msg.ErrorStackTrace);
            Assert.NotNull(msg.Args);
        }
        [Fact]
        public async void CommandOperator_CanConfigureAndExecute_Error()
        {
            var services = GetServices();
            var opr = services.GetService<TestCommandOperator>();

            var args = new TestCommandArgs() { IsValid = true, ShouldThrow = true };
            var cmd = new TestCommand(args);
            await opr.SafeHandle<TestResponse>(cmd);

            BaseCommand msg;

            await Task.Delay(ConsistencyDelay);

            using (var context = GetContext())
            {
                msg = await context.Commands.FirstOrDefaultAsync(x => x.RefId == cmd.RefId);
            }

            var type = msg.GetType().FullName;

            Assert.NotNull(msg);
            Assert.Equal(type, typeof(TestCommand).FullName);
            Assert.Null(msg.CompletedDateUtc);
            Assert.Null(msg.ValidationFailedDateUtc);
            Assert.NotNull(msg.ErrorDateUtc);
            Assert.NotNull(msg.ErrorMessage);
            Assert.NotNull(msg.ErrorStackTrace);
            Assert.NotNull(msg.Args);
        }
    }
    public class EventOperator_PostgresTests :
        PostgresTestsModule
    {

        public EventOperator_PostgresTests(
            ITestOutputHelper output) :
            base(output)
        {

        }
        [Fact]
        public async void EventOperator_CanConfigureAndExecute_HappyPath()
        {
            var services = GetServices();
            var opr = services.GetService<TestEventOperator>();

            var args = new TestEventArgs() { ShouldThrow = false };
            var cmd = new TestEvent(args);
            await opr.SafeHandle<TestResponse>(cmd);

            BaseEvent msg;

            await Task.Delay(ConsistencyDelay);

            using (var context = GetContext())
            {
                msg = await context.Events.FirstOrDefaultAsync(x => x.RefId == cmd.RefId);
            }

            var type = msg.GetType().FullName;

            Assert.NotNull(msg);
            Assert.Equal(type, typeof(TestEvent).FullName);
            Assert.NotNull(msg.Args);
        }
        [Fact]
        public async void EventOperator_CanConfigureAndExecute_Error()
        {
            var services = GetServices();
            var opr = services.GetService<TestEventOperator>();

            var args = new TestEventArgs() { ShouldThrow = true };
            var cmd = new TestEvent(args);
            await opr.SafeHandle<TestResponse>(cmd);

            BaseEvent msg;

            await Task.Delay(ConsistencyDelay);

            using (var context = GetContext())
            {
                msg = await context.Events.FirstOrDefaultAsync(x => x.RefId == cmd.RefId);
            }

            var type = msg.GetType().FullName;

            Assert.NotNull(msg);
            Assert.Equal(type, typeof(TestEvent).FullName);
            Assert.NotNull(msg.Args);
        }
    }
    public class QueryOperator_PostgresTests :
        PostgresTestsModule
    {

        public QueryOperator_PostgresTests(
            ITestOutputHelper output) :
            base(output)
        {

        }
        [Fact]
        public async void QueryOperator_CanConfigureAndExecute_HappyPath()
        {
            var services = GetServices();
            var opr = services.GetService<TestQueryOperator>();

            var args = new TestQueryArgs() { ShouldThrow = false };
            var cmd = new TestQuery(args);
            await opr.SafeHandle<TestResponse>(cmd);

            BaseQuery msg;

            await Task.Delay(ConsistencyDelay);

            using (var context = GetContext())
            {
                msg = await context.Queries.FirstOrDefaultAsync(x => x.RefId == cmd.RefId);
            }

            var type = msg.GetType().FullName;

            Assert.NotNull(msg);
            Assert.Equal(type, typeof(TestQuery).FullName);
            Assert.NotNull(msg.CompletedDateUtc);
            Assert.Null(msg.ErrorDateUtc);
            Assert.NotNull(msg.Args);
        }
        [Fact]
        public async void QueryOperator_CanConfigureAndExecute_Error()
        {
            var services = GetServices();
            var opr = services.GetService<TestQueryOperator>();

            var args = new TestQueryArgs() { ShouldThrow = true };
            var cmd = new TestQuery(args);
            await opr.SafeHandle<TestResponse>(cmd);

            BaseQuery msg;

            await Task.Delay(ConsistencyDelay);

            using (var context = GetContext())
            {
                msg = await context.Queries.FirstOrDefaultAsync(x => x.RefId == cmd.RefId);
            }

            var type = msg.GetType().FullName;

            Assert.NotNull(msg);
            Assert.Equal(type, typeof(TestQuery).FullName);
            Assert.Null(msg.CompletedDateUtc);
            Assert.NotNull(msg.Args);
            Assert.NotNull(msg.ErrorDateUtc);
            Assert.NotNull(msg.ErrorMessage);
            Assert.NotNull(msg.ErrorStackTrace);
        }
    }
    public class ResponseOperator_PostgresTests :
        PostgresTestsModule
    {

        public ResponseOperator_PostgresTests(
            ITestOutputHelper output) :
            base(output)
        {

        }
        [Fact]
        public async void ResponseOperator_CanConfigureAndExecute_HappyPath()
        {
            var services = GetServices();
            var opr = services.GetService<TestResponseOperator>();

            var args = new TestResponseArgs() { ShouldThrow = false };
            var cmd = new TestResponse(args);
            await opr.SafeHandle<TestResponse>(cmd);

            BaseResponse msg;

            await Task.Delay(ConsistencyDelay);

            using (var context = GetContext())
            {
                msg = await context.Responses.FirstOrDefaultAsync(x => x.RefId == cmd.RefId);
            }

            var type = msg.GetType().FullName;

            Assert.NotNull(msg);
            Assert.Equal(type, typeof(TestResponse).FullName);
            Assert.NotNull(msg.Args);
        }
        [Fact]
        public async void ResponseOperator_CanConfigureAndExecute_Error()
        {
            var services = GetServices();
            var opr = services.GetService<TestResponseOperator>();

            var args = new TestResponseArgs() { ShouldThrow = true };
            var cmd = new TestResponse(args);
            await opr.SafeHandle<TestResponse>(cmd);

            BaseResponse msg;

            await Task.Delay(ConsistencyDelay);

            using (var context = GetContext())
            {
                msg = await context.Responses.FirstOrDefaultAsync(x => x.RefId == cmd.RefId);
            }

            var type = msg.GetType().FullName;

            Assert.NotNull(msg);
            Assert.Equal(type, typeof(TestResponse).FullName);
            Assert.NotNull(msg.Args);
        }
    }
}
