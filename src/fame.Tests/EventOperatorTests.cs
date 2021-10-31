using Xunit;

namespace fame.Tests
{
    public class EventOperatorTests
    {

        [Fact]
        public async void EventOperator_HappyPath()
        {
            var opr = new TestEventOperator();
            var msg = new TestEvent();

            var resp = await opr.SafeHandle<TestResponse>(msg);

            Assert.NotNull(resp);
            Assert.Equal(msg.RefId, resp.SourceId);
            Assert.True(resp.IsValid);
            Assert.True(resp.Successful);
        }

        [Fact]
        public async void EventOperator_Errors()
        {
            var opr = new TestEventOperator();
            var args = new TestEventArgs
            {
                ShouldThrow = true
            };
            var msg = new TestEvent(args);

            var resp = await opr.SafeHandle<TestResponse>(msg);

            Assert.NotNull(resp);
            Assert.Equal(msg.RefId, resp.SourceId);
            Assert.False(resp.Successful);
            Assert.NotEmpty(resp.Messages);
        }
        [Fact]
        public async void EventOperator_ErrorsWhenTypeConversionFails()
        {
            var opr = new TestEventOperator();
            var msg = new TestEvent();

            //await Assert.ThrowsAsync<InvalidCastException>(async () => await opr.SafeHandle<JunkResponse>(msg));

            var resp = await opr.SafeHandle<JunkResponse>(msg);

            Assert.NotNull(resp);
            Assert.Equal(msg.RefId, resp.SourceId);
            Assert.False(resp.Successful);
            Assert.NotEmpty(resp.Messages);
        }
    }
}
