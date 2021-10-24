using System.Collections.Generic;

namespace fame.Tests
{
    public class TestCommand :
        BaseCommand
    {
        public new TestCommandArgs Args => base.Args as TestCommandArgs;
        public TestCommand()
        {

        }
        public TestCommand(
            TestCommandArgs args)
        {
            base.Args = args;
        }

        public override bool Validate(out IEnumerable<string> messages)
        {
            
            if (Args is not null && Args.IsValid is not true)
            {
                messages = new[] { "Manually marked as invalid for testing." };
                return false;
            }
            else
            {
                return base.Validate(out messages);
            }
        }
    }
}
