namespace fame.Tests
{
    public class TestQuery :
        BaseQuery
    {
        public new TestQueryArgs Args => base.Args as TestQueryArgs;
        public TestQuery()
        {

        }
        public TestQuery(
            TestQueryArgs args)
        {
            base.Args = args;
        }
    }
}
