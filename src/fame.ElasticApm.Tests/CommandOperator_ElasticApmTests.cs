using fame.Tests;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nest;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace fame.ElasticApm.Tests
{

    public class CommandOperator_ElasticApmTests :
        ElasticApmTestsModule
    {

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
                    typeof(ElasticApmPlugin).FullName,
                    StringComparison.OrdinalIgnoreCase));

            var msg = new TestCommand();

            var resp = await opr.SafeHandle<TestResponse>(msg);

            Assert.NotNull(resp);
            Assert.Equal(msg.RefId, resp.SourceId);
            Assert.True(resp.IsValid);
            Assert.True(resp.Successful);

            await Task.Delay(WaitForElastic);

            //check for transaction by msg.refId => spans by transactionId

            
            var qResp = await client.SearchAsync<TransactionResult>(x => x.Size(100).Index(tran_index).Query(q => q.Match(m => m.Field("transaction.name").Query(msg.RefId.ToString()))));

            var tran = qResp.Documents.FirstOrDefault();

            var qSpanResp = await client.SearchAsync<SpanResult>(x => x.Index(span_index).Query(q => q.Match(m => m.Field("transaction.id").Query(tran?.transaction?.id))));

            var spans = qSpanResp.Documents;

            Assert.NotNull(qResp);
            Assert.NotNull(qResp.Documents);
            Assert.NotEmpty(qResp.Documents);

            Assert.NotNull(qSpanResp);
            Assert.NotNull(spans);
            Assert.NotEmpty(spans);
            Assert.Equal(2, spans.Count);

            var hasValidationSpan = spans.Any(x => x?.span?.name.Equals(ElasticApmPlugin.validation_key) is true);
            var hasExecutionSpan = spans.Any(x => x?.span?.name.Equals(ElasticApmPlugin.execution_key) is true);

            Assert.True(hasValidationSpan);
            Assert.True(hasExecutionSpan);
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
                    typeof(ElasticApmPlugin).FullName,
                    StringComparison.OrdinalIgnoreCase));

            var args = new TestCommandArgs() { IsValid = false };
            var msg = new TestCommand(args);

            var resp = await opr.SafeHandle<TestResponse>(msg);

            Assert.NotNull(resp);
            Assert.Equal(msg.RefId, resp.SourceId);
            Assert.False(resp.IsValid);
            Assert.False(resp.Successful);

            await Task.Delay(WaitForElastic);

            var qResp = client.Search<TransactionResult>(x => x.Size(100).Index(tran_index).Query(q => q.Match(m => m.Field("transaction.name").Query(msg.RefId.ToString()))));

            var tran = qResp.Documents.FirstOrDefault();

            var qSpanResp = client.Search<SpanResult>(x => x.Index(span_index).Query(q => q.Match(m => m.Field("transaction.id").Query(tran?.transaction?.id))));

            var spans = qSpanResp.Documents;

            Assert.NotNull(qResp);
            Assert.NotNull(qResp.Documents);
            Assert.NotEmpty(qResp.Documents);

            Assert.NotNull(qSpanResp);
            Assert.NotNull(spans);
            Assert.NotEmpty(spans);
            Assert.Equal(1, spans.Count);

            var hasValidationSpan = spans.Any(x => x?.span?.name.Equals(ElasticApmPlugin.validation_key) is true);
            var hasExecutionSpan = spans.Any(x => x?.span?.name.Equals(ElasticApmPlugin.execution_key) is true);

            Assert.True(hasValidationSpan);
            //Assert.True(hasExecutionSpan);
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
                    typeof(ElasticApmPlugin).FullName,
                    StringComparison.OrdinalIgnoreCase));

            var args = new TestCommandArgs() { ShouldThrow = true };
            var msg = new TestCommand(args);

            var resp = await opr.SafeHandle<TestResponse>(msg);

            Assert.NotNull(resp);
            Assert.Equal(msg.RefId, resp.SourceId);
            Assert.False(resp.Successful);

            await Task.Delay(WaitForElastic);

            var qResp = client.Search<TransactionResult>(x => x.Size(100).Index(tran_index).Query(q => q.Match(m => m.Field("transaction.name").Query(msg.RefId.ToString()))));

            var tran = qResp.Documents.FirstOrDefault();

            var qSpanResp = client.Search<SpanResult>(x => x.Index(span_index).Query(q => q.Match(m => m.Field("transaction.id").Query(tran?.transaction?.id))));

            var spans = qSpanResp.Documents;

            Assert.NotNull(qResp);
            Assert.NotNull(qResp.Documents);
            Assert.NotEmpty(qResp.Documents);

            Assert.NotNull(qSpanResp);
            Assert.NotNull(spans);
            Assert.NotEmpty(spans);
            Assert.Equal(2, spans.Count);

            var hasValidationSpan = spans.Any(x => x?.span?.name.Equals(ElasticApmPlugin.validation_key) is true);
            var hasExecutionSpan = spans.Any(x => x?.span?.name.Equals(ElasticApmPlugin.execution_key) is true);

            Assert.True(hasValidationSpan);
            Assert.True(hasExecutionSpan);
        }
    }

}
