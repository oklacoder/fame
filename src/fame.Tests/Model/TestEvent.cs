namespace fame.Tests
{
    public class TestEvent :
        BaseEvent
    {
        public new TestEventArgs Args => base.Args as TestEventArgs;
        public TestEvent()
        {

        }
        public TestEvent(
            TestEventArgs args)
        {
            base.Args = args;
        }
    }
}
