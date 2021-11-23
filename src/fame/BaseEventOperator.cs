using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace fame
{
    public abstract class BaseEventOperator :
        IOperator
    {
        private readonly ILogger<BaseEventOperator> _logger;

        public event EventHandler<IMessage> HandleStarted;
        public event EventHandler<IMessage> HandleValidationStarted;
        public event EventHandler<IMessage> HandleValidationSucceeded;
        public event EventHandler<IMessage> HandleValidationFailed;
        public event EventHandler<IMessage> HandleExecutionStarted;
        public event EventHandler<IMessage> HandleExecutionSucceeded;
        public event EventHandler<IMessage> HandleSucceeded;
        public event EventHandler<IMessage> HandleFailed;
        public event EventHandler<IMessage> HandleFinished;

        public BaseEventOperator(
            IConfiguration config = null,
            ILoggerFactory logger = null,
            IEnumerable<IFamePlugin> plugins = null)
        {
            this._logger = logger?.CreateLogger<BaseEventOperator>();

            List<string> _plugins = new List<string>();

            if (plugins?.Any() is true)
            {
                _logger.LogDebug("Configuring plugins for {0}", GetType().FullName);
                foreach (var p in plugins)
                {
                    _logger?.LogDebug("Configuring plugin: {0}", p.GetType().FullName);
                    p.Configure(config, logger);
                    p.Enroll(this);
                    _plugins.Add(p.GetType().FullName);
                }
            }

            Plugins = _plugins;
        }
        public IEnumerable<string> Plugins { get; private set; }

        async Task<T> IOperator.Handle<T>(IMessage msg)
        {
            var query = msg as BaseEvent;
            if (query is null)
                throw new ArgumentException(
                    string.Format(
                        "Argument {0} for {1}.Handle could not be coerced to a type compatible with base type {2} as required",
                        nameof(msg),
                        typeof(IOperator).FullName,
                        typeof(BaseEvent).FullName));
            return await Handle<T>(query);
        }
        async Task<T> IOperator.SafeHandle<T>(IMessage msg)
        {
            var query = msg as BaseEvent;
            if (query is null)
                throw new ArgumentException(
                    string.Format(
                        "Argument {0} for {1}.Handle could not be coerced to a type compatible with base type {2} as required",
                        nameof(msg),
                        typeof(IOperator).FullName,
                        typeof(BaseEvent).FullName));
            return await SafeHandle<T>(query);
        }


        public abstract Task<T> Handle<T>(BaseEvent evt)
            where T : BaseResponse;

        public async Task<T> SafeHandle<T>(BaseEvent evt)
            where T : BaseResponse, new()
        {
            T resp = default;
            try
            {
                _logger?.LogDebug("{0} {1} beginning...", evt.GetType().FullName, evt.RefId);
                HandleStarted?.Invoke(this, evt);

                _logger?.LogDebug("{0} {1} validated.  Now executing...", evt.GetType().FullName, evt.RefId);
                HandleExecutionStarted?.Invoke(this, evt);
                resp = await Handle<T>(evt);
                HandleExecutionSucceeded?.Invoke(this, evt);
                _logger?.LogDebug("{0} {1} execution complete.", evt.GetType().FullName, evt.RefId);

                HandleSucceeded?.Invoke(this, evt);
                _logger?.LogDebug("{0} {1} complete.", evt.GetType().FullName, evt.RefId);
            }
            catch (Exception ex)
            {
                HandleFailed?.Invoke(this, evt);
                List<string> messages = new List<string>();
                messages.Add($"Error processing {evt.GetType().FullName} {evt.RefId}");
                messages.Add(ex.Message);
                messages.Add(ex.StackTrace);
                if (ex.InnerException is not null)
                {
                    messages.Add(ex?.InnerException.Message);
                    messages.Add(ex?.InnerException.StackTrace);
                }
                messages.ForEach(x => _logger?.LogError(x));
                return new T().Error(messages, evt);
            }
            finally
            {
                HandleFinished?.Invoke(this, evt);
            }
            return resp;
        }
    }
}
