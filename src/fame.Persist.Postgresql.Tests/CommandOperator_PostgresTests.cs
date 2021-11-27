using Bogus;
using fame.Tests;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace fame.Persist.Postgresql.Tests
{
    public class FakerService
    {
        public static IEnumerable<Property> GetFakeProperties(
            int qty = 1000)
        {
            Randomizer.Seed = new Random(8675309);

            var f = new Faker<Property>()
                .RuleFor(x => x.PropertyId, f => f.Random.Guid())
                .RuleFor(x => x.Address1, f => f.Address.StreetAddress())
                .RuleFor(x => x.Address2, f => f.Address.SecondaryAddress())
                .RuleFor(x => x.City, f => f.Address.City())
                .RuleFor(x => x.State, f => f.Address.State())
                .RuleFor(x => x.PostalCode, f => f.Address.ZipCode())
                .RuleFor(x => x.Country, (f, x) => f.Address.Country());

            var recentWork = new Faker<PropertyWork>()
                .RuleFor(x => x.WorkId, f => Guid.NewGuid())
                .RuleFor(x => x.ScheduledStartDateUtc, f => f.Date.Recent(10).ToUniversalTime())
                .RuleFor(x => x.ActualStartDateUtc, (f, u) => f.Random.Int(0, 100) > 95 ? null : f.Date.Soon(0, u.ScheduledStartDateUtc).ToUniversalTime())
                .RuleFor(x => x.CompletedDateUtc, (f, u) => !u.ActualStartDateUtc.HasValue ? null : f.Date.Soon(0, u.ActualStartDateUtc).ToUniversalTime())
                .RuleFor(x => x.CancelledDateUtc, (f, u) => u.CompletedDateUtc.HasValue ? null : f.Date.Soon(0, u.ActualStartDateUtc).ToUniversalTime());
            var currentWork = new Faker<PropertyWork>()
                .RuleFor(x => x.WorkId, f => Guid.NewGuid())
                .RuleFor(x => x.ScheduledStartDateUtc, f => f.Date.Recent(10).ToUniversalTime())
                .RuleFor(x => x.ActualStartDateUtc, (f, u) => f.Date.Soon(0, u.ScheduledStartDateUtc).ToUniversalTime())
                .RuleFor(x => x.CompletedDateUtc, (f, u) => null);
            var upcomingWork = new Faker<PropertyWork>()
                .RuleFor(x => x.WorkId, f => Guid.NewGuid())
                .RuleFor(x => x.ScheduledStartDateUtc, f => f.Date.Recent(10).ToUniversalTime())
                .RuleFor(x => x.ActualStartDateUtc, (f, u) => null)
                .RuleFor(x => x.CompletedDateUtc, (f, u) => null);


            var o = new Faker<PropertyOwner>()
                .RuleFor(x => x.OwnerId, f => Guid.NewGuid())
                .RuleFor(x => x.DisplayName, f => $"{f.Name.FirstName()} {f.Name.LastName()}")
                .RuleFor(x => x.Email, (f, u) => f.Internet.Email(u.DisplayName.Split(" ").FirstOrDefault(), u.DisplayName.Split(" ").LastOrDefault()))
                .RuleFor(x => x.Phone, f => f.Phone.PhoneNumber("###-###-####"));

            var fakes = f.Generate(qty);

            foreach (var p in fakes)
            {
                var owner = o.Generate();

                var wQty = new Random().Next(0, 10);

                var work = Enumerable.Range(0, wQty).Select(x =>
                {
                    var typVal = new Random().Next(0, 100);
                    var resp = x switch
                    {
                        int n when (n < 45) => recentWork.Generate(), //recent
                        int n when (n >= 45 && n < 55) => currentWork.Generate(), //current
                        int n when (n >= 55) => upcomingWork.Generate(), //upcoming
                        _ => upcomingWork.Generate()
                    };

                    resp.PropertyId = p.PropertyId;
                    return resp;
                }).ToList();

                p.Work = work;
                p.Owner = owner;
            }

            return fakes.ToList();
        }
    }
    public class Property 
    {
        public Guid PropertyId { get; set; }

        public string? Address1 { get; set; }
        public string? Address2 { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }

        public PropertyOwner? Owner { get; set; }
        public IEnumerable<PropertyWork> Work { get; set; } = Array.Empty<PropertyWork>();
    }

    public class PropertyWork
    {
        public Guid PropertyId { get; set; }
        public Guid WorkId { get; set; }
        public DateTime? ScheduledStartDateUtc { get; set; }
        public DateTime? ActualStartDateUtc { get; set; }
        public DateTime? CompletedDateUtc { get; set; }
        public DateTime? CancelledDateUtc { get; set; }
    }

    public class PropertyOwner
    {
        public Guid OwnerId { get; set; }

        public string? DisplayName { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }

    }
    public class CreateProperty :
    BaseCommand
    {
        
        public new CreatePropertyArgs Args
        {
            get => base.Args as CreatePropertyArgs;
            set => base.Args = value;
        }

        public CreateProperty()
        {

        }

        public CreateProperty(
            string userId,
            CreatePropertyArgs args) :
            base(userId, args)
        {

        }

        public override bool Validate(out IEnumerable<string> messages)
        {
            if (Args is null)
            {
                messages = new[] { "Args could not be parsed." };
                return false;
            }
            else
            {
                messages = Array.Empty<string>();
                return true;
            }
        }
    }
    public class CreatePropertyArgs :
        BaseCommandArgs
    {
        public Property Property { get; set; }
    }
    public class PostgresTestsModule
    {
        IConfiguration config;
        PostgresPluginConfig _config;
        protected const int ConsistencyDelay = 1000;
        private readonly ITestOutputHelper _output;


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
            _output = output;
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

            var fac = new LoggerFactory(new[] { new XUnitLoggerProvider(_output) });
            services.AddSingleton<ILoggerFactory>(fac);
            
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
        public async void CommandOperator_CanConfigureAndExecuteDerivedCommand_HappyPath()
        {
            var services = GetServices();
            var opr = services.GetService<TestCommandOperator>();

            var prop = FakerService.GetFakeProperties(1).FirstOrDefault();

            var args = new CreatePropertyArgs() { Property = prop };
            var cmd = new CreateProperty("", args);
            await opr.SafeHandle<TestResponse>(cmd);

            BaseCommand msg;

            while (opr.AnyPluginsProcessing is true)
            {
                await Task.Delay(ConsistencyDelay);
            }

            using (var context = GetContext())
            {
                msg = await context.Commands.FirstOrDefaultAsync(x => x.RefId == cmd.RefId);
            }

            var type = msg.GetType().FullName;

            Assert.NotNull(msg);
            Assert.Equal(type, typeof(CreateProperty).FullName);
            Assert.NotNull(msg.CompletedDateUtc);
            Assert.Null(msg.ValidationFailedDateUtc);
            Assert.Null(msg.ErrorDateUtc);
            Assert.Null(msg.ErrorMessage);
            Assert.Null(msg.ErrorStackTrace);
            Assert.NotNull(msg.Args);
        }
        [Fact]
        public async void CommandOperator_CanConfigureAndExecuteDerivedCommand_HappyPath_Multiple()
        {
            var services = GetServices();
            var opr = services.GetService<TestCommandOperator>();
            var count = 1;

            var props = FakerService.GetFakeProperties(count);

            var success = true;

            foreach(var prop in props)
            {
                var args = new CreatePropertyArgs() { Property = prop };
                var cmd = new CreateProperty("", args);
                var resp = await opr.SafeHandle<TestResponse>(cmd);
                success = success && resp.Successful is true;
            }

            while(opr.AnyPluginsProcessing is true)
            {
                await Task.Delay(ConsistencyDelay);
            }

            int actualCount = 0;

            

            using (var context = GetContext())
            {
                actualCount = await context.Set<CreateProperty>().CountAsync();
            }

            Assert.True(success);
            Assert.True(count <= actualCount);
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
            while (opr.AnyPluginsProcessing is true)
            {
                await Task.Delay(ConsistencyDelay);
            }

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

            while (opr.AnyPluginsProcessing is true)
            {
                await Task.Delay(ConsistencyDelay);
            }

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

            while (opr.AnyPluginsProcessing is true)
            {
                await Task.Delay(ConsistencyDelay);
            }

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

            while (opr.AnyPluginsProcessing is true)
            {
                await Task.Delay(ConsistencyDelay);
            }

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

            while (opr.AnyPluginsProcessing is true)
            {
                await Task.Delay(ConsistencyDelay);
            }

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

            while (opr.AnyPluginsProcessing is true)
            {
                await Task.Delay(ConsistencyDelay);
            }

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

            while (opr.AnyPluginsProcessing is true)
            {
                await Task.Delay(ConsistencyDelay);
            }

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

            while (opr.AnyPluginsProcessing is true)
            {
                await Task.Delay(ConsistencyDelay);
            }

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

            while (opr.AnyPluginsProcessing is true)
            {
                await Task.Delay(ConsistencyDelay);
            }

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
