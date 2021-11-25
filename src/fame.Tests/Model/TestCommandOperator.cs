using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace fame.Tests
{
    public class TestCommandOperator :
        BaseCommandOperator
    {
        public TestCommandOperator(
            IConfiguration config = null,
            ILoggerFactory logger = null,
            IEnumerable<IFamePlugin> plugins = null) :
            base(config, logger, plugins)
        {

        }

        public async override Task<T> Handle<T>(BaseCommand cmd)
        {
            T resp;
            await Task.CompletedTask;

            var args = cmd.Args as TestCommandArgs;
            if (args?.ShouldThrow is true)
                throw new InvalidOperationException("Manually thrown for testing purposes.");

            await Task.Delay(5);

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
