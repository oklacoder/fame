using Xunit;

namespace fame.Tests
{
    public class QueryOperatorTests
    {
        [Fact]
        public async void QueryOperator_HappyPath()
        {
            var opr = new TestQueryOperator();
            var msg = new TestQuery();

            var resp = await opr.SafeHandle<TestResponse>(msg);

            Assert.NotNull(resp);
            Assert.Equal(msg.RefId, resp.SourceId);
            Assert.True(resp.IsValid);
            Assert.True(resp.Successful);
        }

        [Fact]
        public async void QueryOperator_Errors()
        {
            var opr = new TestQueryOperator();
            var args = new TestQueryArgs
            {
                ShouldThrow = true
            };
            var msg = new TestQuery(args);

            var resp = await opr.SafeHandle<TestResponse>(msg);

            Assert.NotNull(resp);
            Assert.Equal(msg.RefId, resp.SourceId);
            Assert.False(resp.Successful);
            Assert.NotEmpty(resp.Messages);
        }
        [Fact]
        public async void QueryOperator_ErrorsWhenTypeConversionFails()
        {
            var opr = new TestQueryOperator();
            var msg = new TestQuery();

            //await Assert.ThrowsAsync<InvalidCastException>(async () => await opr.SafeHandle<JunkResponse>(msg));

            var resp = await opr.SafeHandle<JunkResponse>(msg);

            Assert.NotNull(resp);
            Assert.Equal(msg.RefId, resp.SourceId);
            Assert.False(resp.Successful);
            Assert.NotEmpty(resp.Messages);
        }
    }
}
