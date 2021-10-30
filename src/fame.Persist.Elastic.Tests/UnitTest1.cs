using fame.Tests;
using Microsoft.Extensions.Configuration;
using System;
using Xunit;

namespace fame.Persist.Elastic.Tests
{
    public class CommandOperator_ElasticApmTests
    {
        [Fact]
        public async void CommandOperator_CanConfigureAndExecute_HappyPath()
        {
            var opr = new TestCommandOperator();
            var msg = new TestCommand();

            var config = new ConfigurationBuilder()
               .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
               .AddJsonFile("testConfig.json").Build();

            fame.Persist.Elastic.ElasticPlugin.Configure(config);
            fame.Persist.Elastic.ElasticPlugin.Enroll(opr);

            var resp = await opr.SafeHandle<TestResponse>(msg);

            Assert.NotNull(resp);
            Assert.Equal(msg.RefId, resp.SourceId);
            Assert.True(resp.IsValid);
            Assert.True(resp.Successful);
        }
    }
}
