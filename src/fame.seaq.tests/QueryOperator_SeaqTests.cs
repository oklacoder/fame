using fame.Tests;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using static fame.seaq.SeaqPlugin;

namespace fame.seaq.tests
{
    public class QueryOperator_SeaqTests :
        SeaqTestsModule
    {
        public QueryOperator_SeaqTests(
            ITestOutputHelper output) :
            base(output)
        {

        }

        [Fact]
        public async void QueryOperator_CanConfigureAndExecute_HappyPath()
        {
            var services = GetServices();
            var opr = services.GetService<TestQueryOperator>();
            var client = services.GetService<Nest.ElasticClient>();

            Assert.NotNull(client);

            Assert.Contains(
                opr.Plugins,
                x => x.Equals(
                    typeof(SeaqPlugin).FullName,
                    StringComparison.OrdinalIgnoreCase));

            var msg = new TestQuery();

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
            var doc = m.GetObjectAsMessage<TestQuery>();

            Assert.NotNull(doc);
        }

        [Fact]
        public async void QueryOperator_CanConfigureAndExecute_Error()
        {
            var services = GetServices();
            var opr = services.GetService<TestQueryOperator>();
            var client = services.GetService<Nest.ElasticClient>();

            Assert.NotNull(client);

            Assert.Contains(
                opr.Plugins,
                x => x.Equals(
                    typeof(SeaqPlugin).FullName,
                    StringComparison.OrdinalIgnoreCase));

            var args = new TestQueryArgs { ShouldThrow = true };
            var msg = new TestQuery(args);

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
            var doc = m.GetObjectAsMessage<TestQuery>();

            Assert.NotNull(doc);
        }

    }
}
