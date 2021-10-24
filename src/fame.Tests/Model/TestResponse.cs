namespace fame.Tests
{
    public class TestResponse :
        BaseResponse
    {
        public new TestResponseArgs Args => base.Args as TestResponseArgs;
        public TestResponse()
        {

        }
        public TestResponse(
            TestResponseArgs args)
        {
            base.Args = args;
        }
    }
}
