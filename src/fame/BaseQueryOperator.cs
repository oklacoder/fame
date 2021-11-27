using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace fame
{
    public abstract class BaseQueryOperator :
        IOperator
    {
        private readonly ILogger<BaseQueryOperator> _logger;

        public event EventHandler<IMessage> HandleStarted;
        public event EventHandler<IMessage> HandleValidationStarted;
        public event EventHandler<IMessage> HandleValidationSucceeded;
        public event EventHandler<IMessage> HandleValidationFailed;
        public event EventHandler<IMessage> HandleExecutionStarted;
        public event EventHandler<IMessage> HandleExecutionSucceeded;
        public event EventHandler<IMessage> HandleSucceeded;
        public event EventHandler<IMessage> HandleFailed;
        public event EventHandler<IMessage> HandleFinished;

        public BaseQueryOperator(
            IConfiguration config = null,
            ILoggerFactory logger = null,
            IEnumerable<IFamePlugin> plugins = null)
        {
            this._logger = logger?.CreateLogger<BaseQueryOperator>();

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
            var query = msg as BaseQuery;
            if (query is null)
                throw new ArgumentException(
                    string.Format(
                        "Argument {0} for {1}.Handle could not be coerced to a type compatible with base type {2} as required",
                        nameof(msg),
                        typeof(IOperator).FullName,
                        typeof(BaseQuery).FullName));
            return await Handle<T>(query);
        }
        async Task<T> IOperator.SafeHandle<T>(IMessage msg)
        {
            var query = msg as BaseQuery;
            if (query is null)
                throw new ArgumentException(
                    string.Format(
                        "Argument {0} for {1}.Handle could not be coerced to a type compatible with base type {2} as required",
                        nameof(msg),
                        typeof(IOperator).FullName,
                        typeof(BaseQuery).FullName));
            return await SafeHandle<T>(query);
        }


        public abstract Task<T> Handle<T>(BaseQuery query)
            where T : BaseResponse;

        public async Task<T> SafeHandle<T>(BaseQuery query)
            where T : BaseResponse, new()
        {
            T resp = default;
            try
            {
                _logger?.LogDebug("{0} {1} beginning...", query.GetType().FullName, query.RefId);
                HandleStarted?.Invoke(this, query);

                _logger?.LogDebug("{0} {1} validated.  Now executing...", query.GetType().FullName, query.RefId);
                HandleExecutionStarted?.Invoke(this, query);
                resp = await Handle<T>(query);
                query.CompletedDateUtc = DateTime.UtcNow;
                HandleExecutionSucceeded?.Invoke(this, query);
                _logger?.LogDebug("{0} {1} execution complete.", query.GetType().FullName, query.RefId);

                HandleSucceeded?.Invoke(this, query);
                _logger?.LogDebug("{0} {1} complete.", query.GetType().FullName, query.RefId);
            }
            catch (Exception ex)
            {
                query.ErrorDateUtc = DateTime.UtcNow;
                query.ErrorMessage = ex.Message;
                query.ErrorStackTrace = ex.StackTrace;
                HandleFailed?.Invoke(this, query);
                List<string> messages = new List<string>();
                messages.Add($"Error processing {query.GetType().FullName} {query.RefId}");
                messages.Add(ex.Message);
                messages.Add(ex.StackTrace);
                if (ex.InnerException is not null)
                {
                    messages.Add(ex?.InnerException.Message);
                    messages.Add(ex?.InnerException.StackTrace);
                }
                messages.ForEach(x => _logger?.LogError(x));
                return new T().Error(messages, query);
            }
            finally
            {
                HandleFinished?.Invoke(this, query);
            }
            return resp;
        }
    }
}
