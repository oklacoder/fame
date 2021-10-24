using System;
using System.Threading.Tasks;

namespace fame.Tests
{
    public class TestQueryOperator :
        BaseQueryOperator
    {
        public async override Task<T> Handle<T>(BaseQuery query)
        {
            T resp;
            await Task.CompletedTask;

            var args = query.Args as TestQueryArgs;
            if (args?.ShouldThrow is true)
                throw new Exception("Manually thrown for testing purposes.");

            resp = new TestResponse() as T;
            if (resp is null) throw new InvalidCastException(
                string.Format(
                    "Could not cast result of command {0} of type {1} to specified type {2}",
                    query.RefId,
                    query.GetType().FullName,
                    typeof(T).FullName)
                );
            return resp.Success(new TestResponseArgs(), query);
        }
    }
}
