using fame.Tests;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nest;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace fame.ElasticApm.Tests
{

    public class ResponseOperator_ElasticApmTests :
        ElasticApmTestsModule
    {


        [Fact]
        public async void ResponseOperator_CanConfigureAndExecute_HappyPath()
        {
            var services = GetServices();
            var opr = services.GetService<TestResponseOperator>();

            Assert.Equal(1, opr.Plugins.Count());
            Assert.Equal(typeof(ElasticApmPlugin).FullName, opr.Plugins.FirstOrDefault());

            var msg = new TestResponse();
            var config = new ConfigurationBuilder()
               .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
               .AddJsonFile("testConfig.json").Build();


            var resp = await opr.SafeHandle<TestResponse>(msg);

            Assert.NotNull(resp);
            Assert.Equal(msg.RefId, resp.SourceId);
            Assert.True(resp.IsValid);
            Assert.True(resp.Successful);

            await Task.Delay(5000);

            //check for transaction by msg.refId => spans by transactionId

            var conn = new ConnectionSettings(new Uri("http://localhost:9200"));
            conn.BasicAuthentication("elastic", "elastic");
            var client = new Nest.ElasticClient(conn);

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

            var hasExecutionSpan = spans.Any(x => x?.span?.name.Equals(ElasticApmPlugin.execution_key) is true);

            Assert.True(hasExecutionSpan);
        }

        [Fact]
        public async void ResponseOperator_CanConfigureAndExecute_Error()
        {
            var services = GetServices();
            var opr = services.GetService<TestResponseOperator>();

            var args = new TestResponseArgs() { ShouldThrow = true };
            var msg = new TestResponse(args);
            var config = new ConfigurationBuilder()
               .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
               .AddJsonFile("testConfig.json").Build();


            var resp = await opr.SafeHandle<TestResponse>(msg);

            Assert.NotNull(resp);
            Assert.Equal(msg.RefId, resp.SourceId);
            Assert.False(resp.Successful);

            await Task.Delay(5000);

            //check for transaction by msg.refId => spans by transactionId

            var conn = new ConnectionSettings(new Uri("http://localhost:9200"));
            conn.BasicAuthentication("elastic", "elastic");
            var client = new Nest.ElasticClient(conn);

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

            var hasExecutionSpan = spans.Any(x => x?.span?.name.Equals(ElasticApmPlugin.execution_key) is true);

            Assert.True(hasExecutionSpan);
        }
    }

}
