using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace fame.Tests
{
    public class CommandOperatorTests
    {
        
        [Fact]
        public async void CommandOperator_HappyPath()
        {
            var opr = new TestCommandOperator();
            var msg = new TestCommand();

            var resp = await opr.SafeHandle<TestResponse>(msg);

            Assert.NotNull(resp);
            Assert.Equal(msg.RefId, resp.SourceId);
            Assert.True(resp.IsValid);
            Assert.True(resp.Successful);
        }

        [Fact]
        public async void CommandOperator_Invalidates()
        {
            var opr = new TestCommandOperator();
            var args = new TestCommandArgs
            {
                IsValid = false
            };
            var msg = new TestCommand(args);

            var resp = await opr.SafeHandle<TestResponse>(msg);

            Assert.NotNull(resp);
            Assert.Equal(msg.RefId, resp.SourceId);
            Assert.False(resp.IsValid);
            Assert.False(resp.Successful);
            Assert.NotEmpty(resp.Messages);
        }

        [Fact]
        public async void CommandOperator_Errors()
        {
            var opr = new TestCommandOperator();
            var args = new TestCommandArgs
            {
                ShouldThrow = true
            };
            var msg = new TestCommand(args);

            var resp = await opr.SafeHandle<TestResponse>(msg);

            Assert.NotNull(resp);
            Assert.Equal(msg.RefId, resp.SourceId);
            Assert.False(resp.Successful);
            Assert.NotEmpty(resp.Messages);
        }
        [Fact]
        public async void CommandOperator_ErrorsWhenTypeConversionFails()
        {
            var opr = new TestCommandOperator();
            var msg = new TestCommand();

            //await Assert.ThrowsAsync<InvalidCastException>(async () => await opr.SafeHandle<JunkResponse>(msg));

            var resp = await opr.SafeHandle<JunkResponse>(msg);

            Assert.NotNull(resp);
            Assert.Equal(msg.RefId, resp.SourceId);
            Assert.False(resp.Successful);
            Assert.NotEmpty(resp.Messages);
        }
    }
}
