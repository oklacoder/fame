using Xunit;

namespace fame.Tests
{
    public class MessageTests
    {
        [Fact]
        public void Command_EmptyConstructor()
        {
            var cmd = new TestCommand();

            Assert.NotEqual(default, cmd.RefId);
            Assert.NotEqual(default, cmd.DateTimeUtc);
        }

        [Fact]
        public void Query_EmptyConstructor()
        {
            var query = new TestQuery();

            Assert.NotEqual(default, query.RefId);
            Assert.NotEqual(default, query.DateTimeUtc);
        }

        [Fact]
        public void Response_EmptyConstructor()
        {
            var resp = new TestResponse();

            Assert.NotEqual(default, resp.RefId);
            Assert.NotEqual(default, resp.DateTimeUtc);
        }

        [Fact]
        public void Event_EmptyConstructor()
        {
            var evt = new TestEvent();

            Assert.NotEqual(default, evt.RefId);
            Assert.NotEqual(default, evt.DateTimeUtc);
        }
    }
}
