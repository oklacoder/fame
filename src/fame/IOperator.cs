using System;
using System.Threading.Tasks;

namespace fame
{
    public interface IOperator
    {
        public event EventHandler<IMessage> HandleStarted;
        public event EventHandler<IMessage> HandleInvalid;
        public event EventHandler<IMessage> HandleSucceeded;
        public event EventHandler<IMessage> HandleFailed;
        public event EventHandler<IMessage> HandleFinished;

        abstract Task<T> Handle<T>(IMessage cmd)
            where T : BaseResponse, new();
        public Task<T> SafeHandle<T>(IMessage cmd)
            where T : BaseResponse, new();
    }
}
