using System;
using System.Threading.Tasks;

namespace fame.Tests
{
    public class TestCommandOperator :
        BaseCommandOperator
    {
        public async override Task<T> Handle<T>(BaseCommand cmd)
        {
            T resp;
            await Task.CompletedTask;

            var args = cmd.Args as TestCommandArgs;
            if (args?.ShouldThrow is true)
                throw new Exception("Manually thrown for testing purposes.");

            resp = new TestResponse() as T;
            if (resp is null) throw new InvalidCastException(
                string.Format(
                    "Could not cast result of command {0} of type {1} to specified type {2}",
                    cmd.RefId,
                    cmd.GetType().FullName,
                    typeof(T).FullName)
                );
            return resp.Success(new TestResponseArgs(), cmd);
        }
    }
}
