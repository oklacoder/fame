using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace fame.Tests
{
    public class TestEventOperator :
        BaseEventOperator
    {
        public TestEventOperator(
            IConfiguration config = null,
            ILoggerFactory logger = null,
            IEnumerable<IFamePlugin> plugins = null) :
            base(config, logger, plugins)
        {

        }
        public async override Task<T> Handle<T>(BaseEvent evt)
        {
            T resp;
            await Task.CompletedTask;

            var args = evt.Args as TestEventArgs;
            if (args?.ShouldThrow is true)
                throw new InvalidOperationException("Manually thrown for testing purposes.");

            resp = new TestResponse() as T;
            if (resp is null) throw new InvalidCastException(
                string.Format(
                    "Could not cast result of command {0} of type {1} to specified type {2}",
                    evt.RefId,
                    evt.GetType().FullName,
                    typeof(T).FullName)
                );
            return resp.Success(new TestResponseArgs(), evt);
        }
    }
}
