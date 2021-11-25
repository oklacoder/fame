using Bogus;
using fame.Tests;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using static fame.seaq.SeaqPlugin;

namespace fame.seaq.tests
{
    public class FakerService
    {
        public static IEnumerable<Property> GetFakeProperties(
            int qty = 1000)
        {
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
                });

                p.Work = work;
                p.Owner = owner;
            }

            return fakes;
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


    public class CommandOperator_SeaqTests :
        SeaqTestsModule
    {
        public CommandOperator_SeaqTests(
            ITestOutputHelper output) :
            base(output)
        {

        }
        [Fact]
        public async void CommandOperator_CanConfigureAndExecuteDerivedCommand_HappyPath()
        {
            var services = GetServices();
            var opr = services.GetService<TestCommandOperator>();
            var client = services.GetService<Nest.ElasticClient>();

            Assert.NotNull(client);

            Assert.Contains(
                opr.Plugins,
                x => x.Equals(
                    typeof(SeaqPlugin).FullName,
                    StringComparison.OrdinalIgnoreCase));

            var prop = FakerService.GetFakeProperties(1).FirstOrDefault();

            var args = new CreatePropertyArgs() { Property = prop };
            var msg = new CreateProperty("", args);
            var resp = await opr.SafeHandle<TestResponse>(msg);

            Assert.NotNull(resp);
            Assert.Equal(msg.RefId, resp.SourceId);
            Assert.True(resp.IsValid);
            Assert.True(resp.Successful);

            var idx = GetIndexForMessage(msg);
            Assert.NotNull(idx);

            //let elastic percolate...
            await Task.Delay(ElasticConsistencyDelay);

            var qResp = client.Search<MessageWrapper>(s => s.Index(idx).Query(q => q.Match(m => m.Field("message.refId").Query(msg.RefId.ToString()))));

            Assert.True(qResp.IsValid);
            Assert.NotNull(qResp.Documents);
            Assert.NotEmpty(qResp.Documents);

            var m = qResp.Documents.FirstOrDefault();
            var doc = m.GetObjectAsMessage<TestCommand>();

            Assert.NotNull(doc);
            Assert.NotNull(doc.CompletedDateUtc);
            Assert.Null(doc.ErrorDateUtc);
            Assert.Null(doc.ErrorMessage);
            Assert.Null(doc.ErrorStackTrace);
            Assert.NotNull(doc.FinishedDateUtc);
        }

        [Fact]
        public async void CommandOperator_CanConfigureAndExecute_HappyPath()
        {
            var services = GetServices();
            var opr = services.GetService<TestCommandOperator>();
            var client = services.GetService<Nest.ElasticClient>();

            Assert.NotNull(client);

            Assert.Contains(
                opr.Plugins,
                x => x.Equals(
                    typeof(SeaqPlugin).FullName,
                    StringComparison.OrdinalIgnoreCase));

            var msg = new TestCommand();

            var resp = await opr.SafeHandle<TestResponse>(msg);

            Assert.NotNull(resp);
            Assert.Equal(msg.RefId, resp.SourceId);
            Assert.True(resp.IsValid);
            Assert.True(resp.Successful);

            var idx = GetIndexForMessage(msg);
            Assert.NotNull(idx);

            //let elastic percolate...
            await Task.Delay(ElasticConsistencyDelay);

            var qResp = client.Search<MessageWrapper>(s => s.Index(idx).Query(q => q.Match(m => m.Field("message.refId").Query(msg.RefId.ToString()))));

            Assert.True(qResp.IsValid);
            Assert.NotNull(qResp.Documents);
            Assert.NotEmpty(qResp.Documents);

            var m = qResp.Documents.FirstOrDefault();
            var doc = m.GetObjectAsMessage<TestCommand>();

            Assert.NotNull(doc);
            Assert.NotNull(doc.CompletedDateUtc);
            Assert.Null(doc.ErrorDateUtc);
            Assert.Null(doc.ErrorMessage);
            Assert.Null(doc.ErrorStackTrace);
            Assert.NotNull(doc.FinishedDateUtc);
        }

        [Fact]
        public async void CommandOperator_CanConfigureAndExecute_Error()
        {
            var services = GetServices();
            var opr = services.GetService<TestCommandOperator>();
            var client = services.GetService<Nest.ElasticClient>();

            Assert.NotNull(client);

            Assert.Contains(
                opr.Plugins,
                x => x.Equals(
                    typeof(SeaqPlugin).FullName,
                    StringComparison.OrdinalIgnoreCase));

            var args = new TestCommandArgs { ShouldThrow = true };
            var msg = new TestCommand(args);

            var resp = await opr.SafeHandle<TestResponse>(msg);

            Assert.NotNull(resp);
            Assert.Equal(msg.RefId, resp.SourceId);
            Assert.False(resp.Successful);

            var idx = GetIndexForMessage(msg);
            Assert.NotNull(idx);

            //let elastic percolate...
            await Task.Delay(ElasticConsistencyDelay);

            var qResp = client.Search<MessageWrapper>(s => s.Index(idx).Query(q => q.Match(m => m.Field("message.refId").Query(msg.RefId.ToString()))));

            Assert.True(qResp.IsValid);
            Assert.NotNull(qResp.Documents);
            Assert.NotEmpty(qResp.Documents);

            var m = qResp.Documents.FirstOrDefault();
            var doc = m.GetObjectAsMessage<TestCommand>();

            Assert.NotNull(doc);
            Assert.Null(doc.CompletedDateUtc);
            Assert.NotNull(doc.ErrorDateUtc);
            Assert.NotNull(doc.ErrorMessage);
            Assert.NotNull(doc.ErrorStackTrace);
            Assert.NotNull(doc.FinishedDateUtc);
        }

        [Fact]
        public async void CommandOperator_CanConfigureAndExecute_Invalid()
        {
            var services = GetServices();
            var opr = services.GetService<TestCommandOperator>();
            var client = services.GetService<Nest.ElasticClient>();

            Assert.NotNull(client);

            Assert.Contains(
                opr.Plugins,
                x => x.Equals(
                    typeof(SeaqPlugin).FullName,
                    StringComparison.OrdinalIgnoreCase));

            var args = new TestCommandArgs { IsValid = false };
            var msg = new TestCommand(args);

            var resp = await opr.SafeHandle<TestResponse>(msg);

            Assert.NotNull(resp);
            Assert.Equal(msg.RefId, resp.SourceId);
            Assert.False(resp.IsValid);

            var idx = GetIndexForMessage(msg);
            Assert.NotNull(idx);

            //let elastic percolate...
            await Task.Delay(ElasticConsistencyDelay);

            var qResp = client.Search<MessageWrapper>(s => s.Index(idx).Query(q => q.Match(m => m.Field("message.refId").Query(msg.RefId.ToString()))));

            Assert.True(qResp.IsValid);
            Assert.NotNull(qResp.Documents);
            Assert.NotEmpty(qResp.Documents);

            var m = qResp.Documents.FirstOrDefault();
            var doc = m.GetObjectAsMessage<TestCommand>();

            Assert.NotNull(doc);
            Assert.NotNull(doc.ValidationFailedDateUtc);
            Assert.Null(doc.CompletedDateUtc);
            Assert.Null(doc.ErrorDateUtc);
            Assert.Null(doc.ErrorMessage);
            Assert.Null(doc.ErrorStackTrace);
            Assert.NotNull(doc.FinishedDateUtc);
        }
    }
}
