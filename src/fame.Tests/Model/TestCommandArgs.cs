namespace fame.Tests
{
    public class TestCommandArgs :
        BaseCommandArgs
    {
        public bool IsValid { get; set; } = true;
        public bool ShouldThrow { get; set; } = false;
    }
}
