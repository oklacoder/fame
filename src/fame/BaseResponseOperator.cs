using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace fame
{
    public abstract class BaseResponseOperator :
        IOperator
    {
        private readonly ILogger<BaseResponseOperator> _logger;

        public event EventHandler<IMessage> HandleStarted;
        public event EventHandler<IMessage> HandleInvalid;
        public event EventHandler<IMessage> HandleSucceeded;
        public event EventHandler<IMessage> HandleFailed;
        public event EventHandler<IMessage> HandleFinished;

        public BaseResponseOperator(
            ILogger<BaseResponseOperator> logger = null)
        {
            this._logger = logger;
        }

        async Task<T> IOperator.Handle<T>(IMessage msg)
        {
            var resp = msg as BaseResponse;
            if (resp is null)
                throw new ArgumentException(
                    string.Format(
                        "Argument {0} for {1}.Handle could not be coerced to a type compatible with base type {2} as required",
                        nameof(msg),
                        typeof(IOperator).FullName,
                        typeof(BaseResponse).FullName));
            return await Handle<T>(resp);
        }
        async Task<T> IOperator.SafeHandle<T>(IMessage msg)
        {
            var resp = msg as BaseResponse;
            if (resp is null)
                throw new ArgumentException(
                    string.Format(
                        "Argument {0} for {1}.Handle could not be coerced to a type compatible with base type {2} as required",
                        nameof(msg),
                        typeof(IOperator).FullName,
                        typeof(BaseResponse).FullName));
            return await SafeHandle<T>(resp);
        }


        public abstract Task<T> Handle<T>(BaseResponse res)
            where T : BaseResponse;

        public async Task<T> SafeHandle<T>(BaseResponse res)
            where T : BaseResponse, new()
        {
            T resp = default;
            try
            {
                _logger?.LogDebug("{0} {1} beginning...", res.GetType().FullName, res.RefId);
                HandleStarted?.Invoke(this, res);

                _logger?.LogDebug("{0} {1} validated.  Now executing...", res.GetType().FullName, res.RefId);
                resp = await Handle<T>(res);
                _logger?.LogDebug("{0} {1} execution complete.", res.GetType().FullName, res.RefId);

                HandleSucceeded?.Invoke(this, res);
                _logger?.LogDebug("{0} {1} complete.", res.GetType().FullName, res.RefId);
            }
            catch (Exception ex)
            {
                HandleFailed?.Invoke(this, res);
                List<string> messages = new List<string>();
                messages.Add($"Error processing {res.GetType().FullName} {res.RefId}");
                messages.Add(ex.Message);
                messages.Add(ex.StackTrace);
                if (ex.InnerException is not null)
                {
                    messages.Add(ex?.InnerException.Message);
                    messages.Add(ex?.InnerException.StackTrace);
                }
                messages.ForEach(x => _logger?.LogError(x));
                return new T().Error(messages, res);
            }
            finally
            {
                HandleFinished?.Invoke(this, res);
            }
            return resp;
        }
    }
}
