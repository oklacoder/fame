using Xunit;

namespace fame.Tests
{
    public class ResponseOperatorTests
    {
        [Fact]
        public async void ResponseOperator_HappyPath()
        {
            var opr = new TestResponseOperator();
            var msg = new TestResponse();

            var resp = await opr.SafeHandle<TestResponse>(msg);

            Assert.NotNull(resp);
            Assert.Equal(msg.RefId, resp.SourceId);
            Assert.True(resp.IsValid);
            Assert.True(resp.Successful);
        }

        [Fact]
        public async void ResponseOperator_Errors()
        {
            var opr = new TestResponseOperator();
            var args = new TestResponseArgs
            {
                ShouldThrow = true
            };
            var msg = new TestResponse(args);

            var resp = await opr.SafeHandle<TestResponse>(msg);

            Assert.NotNull(resp);
            Assert.Equal(msg.RefId, resp.SourceId);
            Assert.False(resp.Successful);
            Assert.NotEmpty(resp.Messages);
        }
        [Fact]
        public async void ResponseOperator_ErrorsWhenTypeConversionFails()
        {
            var opr = new TestResponseOperator();
            var msg = new TestResponse();

            //await Assert.ThrowsAsync<InvalidCastException>(async () => await opr.SafeHandle<JunkResponse>(msg));

            var resp = await opr.SafeHandle<JunkResponse>(msg);

            Assert.NotNull(resp);
            Assert.Equal(msg.RefId, resp.SourceId);
            Assert.False(resp.Successful);
            Assert.NotEmpty(resp.Messages);
        }
    }
}
