using System;
using System.Threading.Tasks;

namespace fame
{
    public interface IOperator
    {
        public event EventHandler<IMessage> HandleStarted;
        public event EventHandler<IMessage> HandleValidationStarted;
        public event EventHandler<IMessage> HandleValidationSucceeded;
        public event EventHandler<IMessage> HandleValidationFailed;
        public event EventHandler<IMessage> HandleExecutionStarted;
        public event EventHandler<IMessage> HandleExecutionSucceeded;
        public event EventHandler<IMessage> HandleSucceeded;
        public event EventHandler<IMessage> HandleFailed;
        public event EventHandler<IMessage> HandleFinished;

        abstract Task<T> Handle<T>(IMessage cmd)
            where T : BaseResponse, new();
        public Task<T> SafeHandle<T>(IMessage cmd)
            where T : BaseResponse, new();
    }
}
