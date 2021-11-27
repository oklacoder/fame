using Microsoft.Extensions.Configuration;
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
        public event EventHandler<IMessage> HandleValidationStarted;
        public event EventHandler<IMessage> HandleValidationSucceeded;
        public event EventHandler<IMessage> HandleValidationFailed;
        public event EventHandler<IMessage> HandleExecutionStarted;
        public event EventHandler<IMessage> HandleExecutionSucceeded;
        public event EventHandler<IMessage> HandleSucceeded;
        public event EventHandler<IMessage> HandleFailed;
        public event EventHandler<IMessage> HandleFinished;

        
        public BaseCommandOperator(
            IConfiguration config = null,
            ILoggerFactory logger = null,
            IEnumerable<IFamePlugin> plugins = null)
        {
            this._logger = logger?.CreateLogger<BaseCommandOperator>();

            List<IFamePlugin> pluginTemp = new List<IFamePlugin>();

            if (plugins?.Any() is true)
            {
                _logger?.LogDebug("Configuring plugins for {0}", GetType().FullName);
                foreach (var p in plugins)
                {
                    _logger?.LogDebug("Configuring plugin: {0}", p.GetType().FullName);
                    p.Configure(config, logger);
                    p.Enroll(this);
                    pluginTemp.Add(p);
                    
                }
            }

            _plugins = pluginTemp;
        }

        private IEnumerable<IFamePlugin> _plugins { get; set; }
        public IEnumerable<string> Plugins => _plugins.Select(x => x.GetType().FullName);
        public bool? AnyPluginsProcessing => _plugins?.Any(x => x.IsProcessing is true);
        public int? PluginQueuedMessages => _plugins.Select(x => x.QueuedMessages)?.Sum();


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
                HandleValidationStarted?.Invoke(this, cmd);
                if (!cmd.Validate(out var messages))
                {
                    cmd.ValidationFailedDateUtc = DateTime.UtcNow;
                    HandleValidationFailed?.Invoke(this, cmd);
                    _logger?.LogDebug("{0} {1} failed validation", cmd.GetType().FullName, cmd.RefId);
                    messages?.ToList().ForEach(x => _logger?.LogDebug(x));
                    return new T().Invalid(messages, cmd);
                }
                HandleValidationSucceeded?.Invoke(this, cmd);

                _logger?.LogDebug("{0} {1} validated.  Now executing...", cmd.GetType().FullName, cmd.RefId);
                HandleExecutionStarted?.Invoke(this, cmd);
                resp = await Handle<T>(cmd);
                cmd.CompletedDateUtc = DateTime.UtcNow;
                HandleExecutionSucceeded?.Invoke(this, cmd);
                _logger?.LogDebug("{0} {1} execution complete.", cmd.GetType().FullName, cmd.RefId);

                HandleSucceeded?.Invoke(this, cmd);
                _logger?.LogDebug("{0} {1} complete.", cmd.GetType().FullName, cmd.RefId);
            }
            catch (Exception ex)
            {
                cmd.ErrorDateUtc = DateTime.UtcNow;
                cmd.ErrorMessage = ex.Message;
                cmd.ErrorStackTrace = ex.StackTrace;
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
                resp = new T().Error(messages, cmd);
            }
            finally
            {
                HandleFinished?.Invoke(this, cmd);
            }
            return resp;
        }
    }
}
