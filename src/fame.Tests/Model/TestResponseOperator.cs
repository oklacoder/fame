using System;
using System.Threading.Tasks;

namespace fame.Tests
{
    public class TestResponseOperator :
        BaseResponseOperator
    {
        public async override Task<T> Handle<T>(BaseResponse res)
        {
            T resp;
            await Task.CompletedTask;

            var args = res.Args as TestResponseArgs;
            if (args?.ShouldThrow is true)
                throw new Exception("Manually thrown for testing purposes.");

            resp = new TestResponse() as T;
            if (resp is null) throw new InvalidCastException(
                string.Format(
                    "Could not cast result of command {0} of type {1} to specified type {2}",
                    res.RefId,
                    res.GetType().FullName,
                    typeof(T).FullName)
                );
            return resp.Success(new TestResponseArgs(), res);
        }
    }
}
