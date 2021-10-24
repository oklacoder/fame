using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace fame
{
    public abstract class BaseCommandOperator :
        IOperator
    {
        private readonly ILogger<BaseCommandOperator> _logger;

        public event EventHandler<IMessage> HandleStarted;
        public event EventHandler<IMessage> HandleInvalid;
        public event EventHandler<IMessage> HandleSucceeded;
        public event EventHandler<IMessage> HandleFailed;
        public event EventHandler<IMessage> HandleFinished;

        
        public BaseCommandOperator(
            ILogger<BaseCommandOperator> logger = null)
        {
            this._logger = logger;
        }

        async Task<T> IOperator.Handle<T>(IMessage msg)
        {
            var cmd = msg as BaseCommand;
            if (cmd is null)
                throw new ArgumentException(
                    string.Format(
                        "Argument {0} for {1}.Handle could not be coerced to a type compatible with base type {2} as required", 
                        nameof(msg), 
                        typeof(IOperator).FullName, 
                        typeof(BaseCommand).FullName));
            return await Handle<T>(cmd);
        }
        async Task<T> IOperator.SafeHandle<T>(IMessage msg)
        {
            var cmd = msg as BaseCommand;
            if (cmd is null)
                throw new ArgumentException(
                    string.Format(
                        "Argument {0} for {1}.Handle could not be coerced to a type compatible with base type {2} as required",
                        nameof(msg),
                        typeof(IOperator).FullName,
                        typeof(BaseCommand).FullName));
            return await SafeHandle<T>(cmd);
        }


        public abstract Task<T> Handle<T>(BaseCommand cmd)
            where T : BaseResponse;

        public async Task<T> SafeHandle<T>(BaseCommand cmd)
            where T : BaseResponse, new()
        {
            T resp = default;
            try
            {
                _logger?.LogDebug("{0} {1} beginning...", cmd.GetType().FullName, cmd.RefId);
                HandleStarted?.Invoke(this, cmd);

                _logger?.LogDebug("{0} {1} validating...", cmd.GetType().FullName, cmd.RefId);
                if (!cmd.Validate(out var messages))
                {
                    _logger?.LogDebug("{0} {1} failed validation", cmd.GetType().FullName, cmd.RefId);
                    messages?.ToList().ForEach(x => _logger?.LogDebug(x));
                    HandleInvalid?.Invoke(this, cmd);                    
                    return new T().Invalid(messages, cmd);
                }

                _logger?.LogDebug("{0} {1} validated.  Now executing...", cmd.GetType().FullName, cmd.RefId);
                resp = await Handle<T>(cmd);
                _logger?.LogDebug("{0} {1} execution complete.", cmd.GetType().FullName, cmd.RefId);

                HandleSucceeded?.Invoke(this, cmd);
                _logger?.LogDebug("{0} {1} complete.", cmd.GetType().FullName, cmd.RefId);
            }
            catch (Exception ex)
            {
                HandleFailed?.Invoke(this, cmd);
                List<string> messages = new List<string>();
                messages.Add($"Error processing {cmd.GetType().FullName} {cmd.RefId}");
                messages.Add(ex.Message);
                messages.Add(ex.StackTrace);
                if (ex.InnerException is not null)
                {
                    messages.Add(ex?.InnerException.Message);
                    messages.Add(ex?.InnerException.StackTrace);
                }
                messages.ForEach(x => _logger?.LogError(x));
                return new T().Error(messages, cmd);
            }
            finally
            {
                HandleFinished?.Invoke(this, cmd);
            }
            return resp;
        }
    }
}
