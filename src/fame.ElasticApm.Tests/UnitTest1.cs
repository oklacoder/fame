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
    public class TransactionResult
    {
        //public DateTime @timestamp { get; set; }
        public Transaction transaction { get; set; }

    }
    public class Transaction
    {
        public bool sampled { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
    }
    public class SpanResult
    {
        public Span span { get; set; }
        public Transaction transaction { get; set; }
    }
    public class Span
    {
        public string name { get; set; }
        public string type { get; set; }
        public long id { get; set; }
        public long parent { get; set; }
    }

    public class CommandOperator_ElasticApmTests
    {
        const string tran_index = "apm-6.7.1-transaction*";
        const string span_index = "apm-6.7.1-span*";


        private ServiceCollection GetServices()
        {
            var services = new ServiceCollection();

            var config = new ConfigurationBuilder()
               .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
               .AddJsonFile("testConfig.json").Build();
            services.AddSingleton<IConfiguration>(config);

            return services;
        }


        [Fact]
        public async void CommandOperator_CanConfigureAndExecute_HappyPath()
        {
            var opr = new TestCommandOperator();
            var msg = new TestCommand();
            var config = new ConfigurationBuilder()
               .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
               .AddJsonFile("testConfig.json").Build();

            ElasticApmPlugin.Configure(config);
            ElasticApmPlugin.Enroll(opr);

            var resp = await opr.SafeHandle<TestResponse>(msg);

            Assert.NotNull(resp);
            Assert.Equal(msg.RefId, resp.SourceId);
            Assert.True(resp.IsValid);
            Assert.True(resp.Successful);

            await Task.Delay(15000);

            //check for transaction by msg.refId => spans by transactionId

            var conn = new ConnectionSettings(new Uri("http://localhost:9200"));
            conn.BasicAuthentication("elastic", "elastic");
            var client = new Nest.ElasticClient(conn);
            
            var qResp = client.Search<TransactionResult>(x => x.Size(100).AllTypes().Index(tran_index).Query(q => q.Match(m => m.Field("transaction.name").Query(msg.RefId.ToString()))));

            var tran = qResp.Documents.FirstOrDefault();

            var qSpanResp = client.Search<SpanResult>(x => x.AllTypes().Index(span_index).Query(q => q.Match(m => m.Field("transaction.id").Query(tran?.transaction?.id))));

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
            var opr = new TestCommandOperator();
            var args = new TestCommandArgs() { IsValid = false };
            var msg = new TestCommand(args);
            var config = new ConfigurationBuilder()
               .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
               .AddJsonFile("testConfig.json").Build();

            ElasticApmPlugin.Configure(config);
            ElasticApmPlugin.Enroll(opr);

            var resp = await opr.SafeHandle<TestResponse>(msg);

            Assert.NotNull(resp);
            Assert.Equal(msg.RefId, resp.SourceId);
            Assert.False(resp.IsValid);
            Assert.False(resp.Successful);

            await Task.Delay(15000);

            //check for transaction by msg.refId => spans by transactionId

            var conn = new ConnectionSettings(new Uri("http://localhost:9200"));
            conn.BasicAuthentication("elastic", "elastic");
            var client = new Nest.ElasticClient(conn);

            var qResp = client.Search<TransactionResult>(x => x.Size(100).AllTypes().Index(tran_index).Query(q => q.Match(m => m.Field("transaction.name").Query(msg.RefId.ToString()))));

            var tran = qResp.Documents.FirstOrDefault();

            var qSpanResp = client.Search<SpanResult>(x => x.AllTypes().Index(span_index).Query(q => q.Match(m => m.Field("transaction.id").Query(tran?.transaction?.id))));

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
            var opr = new TestCommandOperator();
            var args = new TestCommandArgs() { ShouldThrow = true };
            var msg = new TestCommand(args);
            var config = new ConfigurationBuilder()
               .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
               .AddJsonFile("testConfig.json").Build();

            ElasticApmPlugin.Configure(config);
            ElasticApmPlugin.Enroll(opr);

            var resp = await opr.SafeHandle<TestResponse>(msg);

            Assert.NotNull(resp);
            Assert.Equal(msg.RefId, resp.SourceId);
            Assert.False(resp.Successful);

            await Task.Delay(15000);

            //check for transaction by msg.refId => spans by transactionId

            var conn = new ConnectionSettings(new Uri("http://localhost:9200"));
            conn.BasicAuthentication("elastic", "elastic");
            var client = new Nest.ElasticClient(conn);

            var qResp = client.Search<TransactionResult>(x => x.Size(100).AllTypes().Index(tran_index).Query(q => q.Match(m => m.Field("transaction.name").Query(msg.RefId.ToString()))));

            var tran = qResp.Documents.FirstOrDefault();

            var qSpanResp = client.Search<SpanResult>(x => x.AllTypes().Index(span_index).Query(q => q.Match(m => m.Field("transaction.id").Query(tran?.transaction?.id))));

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

    public class QueryOperator_ElasticApmTests
    {
        const string tran_index = "apm-6.7.1-transaction*";
        const string span_index = "apm-6.7.1-span*";

        [Fact]
        public async void QueryOperator_CanConfigureAndExecute_HappyPath()
        {
            var opr = new TestQueryOperator();
            var msg = new TestQuery();
            var config = new ConfigurationBuilder()
               .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
               .AddJsonFile("testConfig.json").Build();

            ElasticApmPlugin.Configure(config);
            ElasticApmPlugin.Enroll(opr);

            var resp = await opr.SafeHandle<TestResponse>(msg);

            Assert.NotNull(resp);
            Assert.Equal(msg.RefId, resp.SourceId);
            Assert.True(resp.IsValid);
            Assert.True(resp.Successful);

            await Task.Delay(15000);

            //check for transaction by msg.refId => spans by transactionId

            var conn = new ConnectionSettings(new Uri("http://localhost:9200"));
            conn.BasicAuthentication("elastic", "elastic");
            var client = new Nest.ElasticClient(conn);

            var qResp = client.Search<TransactionResult>(x => x.Size(100).AllTypes().Index(tran_index).Query(q => q.Match(m => m.Field("transaction.name").Query(msg.RefId.ToString()))));

            var tran = qResp.Documents.FirstOrDefault();

            var qSpanResp = client.Search<SpanResult>(x => x.AllTypes().Index(span_index).Query(q => q.Match(m => m.Field("transaction.id").Query(tran?.transaction?.id))));

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
        public async void CommandOperator_CanConfigureAndExecute_Error()
        {
            var opr = new TestQueryOperator();
            var args = new TestQueryArgs() { ShouldThrow = true };
            var msg = new TestQuery(args);
            var config = new ConfigurationBuilder()
               .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
               .AddJsonFile("testConfig.json").Build();

            ElasticApmPlugin.Configure(config);
            ElasticApmPlugin.Enroll(opr);

            var resp = await opr.SafeHandle<TestResponse>(msg);

            Assert.NotNull(resp);
            Assert.Equal(msg.RefId, resp.SourceId);
            Assert.False(resp.Successful);

            await Task.Delay(15000);

            //check for transaction by msg.refId => spans by transactionId

            var conn = new ConnectionSettings(new Uri("http://localhost:9200"));
            conn.BasicAuthentication("elastic", "elastic");
            var client = new Nest.ElasticClient(conn);

            var qResp = client.Search<TransactionResult>(x => x.Size(100).AllTypes().Index(tran_index).Query(q => q.Match(m => m.Field("transaction.name").Query(msg.RefId.ToString()))));

            var tran = qResp.Documents.FirstOrDefault();

            var qSpanResp = client.Search<SpanResult>(x => x.AllTypes().Index(span_index).Query(q => q.Match(m => m.Field("transaction.id").Query(tran?.transaction?.id))));

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

    public class EventOperator_ElasticApmTests
    {
        const string tran_index = "apm-6.7.1-transaction*";
        const string span_index = "apm-6.7.1-span*";

        [Fact]
        public async void EventOperator_CanConfigureAndExecute_HappyPath()
        {
            var opr = new TestEventOperator();
            var msg = new TestEvent();
            var config = new ConfigurationBuilder()
               .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
               .AddJsonFile("testConfig.json").Build();

            ElasticApmPlugin.Configure(config);
            ElasticApmPlugin.Enroll(opr);

            var resp = await opr.SafeHandle<TestResponse>(msg);

            Assert.NotNull(resp);
            Assert.Equal(msg.RefId, resp.SourceId);
            Assert.True(resp.IsValid);
            Assert.True(resp.Successful);

            await Task.Delay(15000);

            //check for transaction by msg.refId => spans by transactionId

            var conn = new ConnectionSettings(new Uri("http://localhost:9200"));
            conn.BasicAuthentication("elastic", "elastic");
            var client = new Nest.ElasticClient(conn);

            var qResp = client.Search<TransactionResult>(x => x.Size(100).AllTypes().Index(tran_index).Query(q => q.Match(m => m.Field("transaction.name").Query(msg.RefId.ToString()))));

            var tran = qResp.Documents.FirstOrDefault();

            var qSpanResp = client.Search<SpanResult>(x => x.AllTypes().Index(span_index).Query(q => q.Match(m => m.Field("transaction.id").Query(tran?.transaction?.id))));

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
        public async void EventOperator_CanConfigureAndExecute_Error()
        {
            var opr = new TestEventOperator();
            var args = new TestEventArgs() { ShouldThrow = true };
            var msg = new TestEvent(args);
            var config = new ConfigurationBuilder()
               .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
               .AddJsonFile("testConfig.json").Build();

            ElasticApmPlugin.Configure(config);
            ElasticApmPlugin.Enroll(opr);

            var resp = await opr.SafeHandle<TestResponse>(msg);

            Assert.NotNull(resp);
            Assert.Equal(msg.RefId, resp.SourceId);
            Assert.False(resp.Successful);

            await Task.Delay(15000);

            //check for transaction by msg.refId => spans by transactionId

            var conn = new ConnectionSettings(new Uri("http://localhost:9200"));
            conn.BasicAuthentication("elastic", "elastic");
            var client = new Nest.ElasticClient(conn);

            var qResp = client.Search<TransactionResult>(x => x.Size(100).AllTypes().Index(tran_index).Query(q => q.Match(m => m.Field("transaction.name").Query(msg.RefId.ToString()))));

            var tran = qResp.Documents.FirstOrDefault();

            var qSpanResp = client.Search<SpanResult>(x => x.AllTypes().Index(span_index).Query(q => q.Match(m => m.Field("transaction.id").Query(tran?.transaction?.id))));

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

    public class ResponseOperator_ElasticApmTests
    {
        const string tran_index = "apm-6.7.1-transaction*";
        const string span_index = "apm-6.7.1-span*";

        [Fact]
        public async void ResponseOperator_CanConfigureAndExecute_HappyPath()
        {
            var opr = new TestResponseOperator();
            var msg = new TestResponse();
            var config = new ConfigurationBuilder()
               .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
               .AddJsonFile("testConfig.json").Build();

            ElasticApmPlugin.Configure(config);
            ElasticApmPlugin.Enroll(opr);

            var resp = await opr.SafeHandle<TestResponse>(msg);

            Assert.NotNull(resp);
            Assert.Equal(msg.RefId, resp.SourceId);
            Assert.True(resp.IsValid);
            Assert.True(resp.Successful);

            await Task.Delay(15000);

            //check for transaction by msg.refId => spans by transactionId

            var conn = new ConnectionSettings(new Uri("http://localhost:9200"));
            conn.BasicAuthentication("elastic", "elastic");
            var client = new Nest.ElasticClient(conn);

            var qResp = client.Search<TransactionResult>(x => x.Size(100).AllTypes().Index(tran_index).Query(q => q.Match(m => m.Field("transaction.name").Query(msg.RefId.ToString()))));

            var tran = qResp.Documents.FirstOrDefault();

            var qSpanResp = client.Search<SpanResult>(x => x.AllTypes().Index(span_index).Query(q => q.Match(m => m.Field("transaction.id").Query(tran?.transaction?.id))));

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
            var opr = new TestResponseOperator();
            var args = new TestResponseArgs() { ShouldThrow = true };
            var msg = new TestResponse(args);
            var config = new ConfigurationBuilder()
               .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
               .AddJsonFile("testConfig.json").Build();

            ElasticApmPlugin.Configure(config);
            ElasticApmPlugin.Enroll(opr);

            var resp = await opr.SafeHandle<TestResponse>(msg);

            Assert.NotNull(resp);
            Assert.Equal(msg.RefId, resp.SourceId);
            Assert.False(resp.Successful);

            await Task.Delay(15000);

            //check for transaction by msg.refId => spans by transactionId

            var conn = new ConnectionSettings(new Uri("http://localhost:9200"));
            conn.BasicAuthentication("elastic", "elastic");
            var client = new Nest.ElasticClient(conn);

            var qResp = client.Search<TransactionResult>(x => x.Size(100).AllTypes().Index(tran_index).Query(q => q.Match(m => m.Field("transaction.name").Query(msg.RefId.ToString()))));

            var tran = qResp.Documents.FirstOrDefault();

            var qSpanResp = client.Search<SpanResult>(x => x.AllTypes().Index(span_index).Query(q => q.Match(m => m.Field("transaction.id").Query(tran?.transaction?.id))));

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
