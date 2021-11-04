using fame.Tests;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using static fame.seaq.SeaqPlugin;

namespace fame.seaq.tests
{

    public class CommandOperator_SeaqTests :
        SeaqTestsModule
    {
        public CommandOperator_SeaqTests(
            ITestOutputHelper output) :
            base(output)
        {

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