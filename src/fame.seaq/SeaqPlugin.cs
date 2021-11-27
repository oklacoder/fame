using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using seaq;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace fame.seaq
{
    public partial class SeaqPlugin :
        IFamePlugin
    {
        private Cluster _cluster = null;

        public static System.Text.Json.JsonSerializerOptions SerializerOptions => new System.Text.Json.JsonSerializerOptions
        {
            AllowTrailingCommas = true,
            PropertyNameCaseInsensitive = true
        };

        public bool? IsConfigured => _cluster is not null;
        public bool? CanPing => _cluster?.CanPing();

        public bool? IsProcessing => messageQueueIsProcessing;
        public int? QueuedMessages => _messageQueue?.Count ?? 0;

        private ILogger<SeaqPlugin> _logger;

        ConcurrentQueue<IMessage> _messageQueue = new ConcurrentQueue<IMessage>();
        bool messageQueueIsProcessing = false;

        private SeaqPluginConfig _config;

        public void Configure(
            IConfiguration config,
            ILoggerFactory logger = null)
        {
            _logger = logger?.CreateLogger<SeaqPlugin>();

            _config = new SeaqPluginConfig();
            config.GetSection(SeaqPluginConfig.SeaqPluginConfig_Key).Bind(_config);

            

            var args = new ClusterArgs(
                _config?.ClusterScope,
                _config?.ClusterUrl,
                _config?.ClusterUser,
                _config?.ClusterPass,
                _config?.ClusterBypassCertificateValidation == true);
            _cluster = Cluster.Create(args);
        }

        public void Enroll(IOperator target)
        {
            if (IsConfigured is not true)
                throw new InvalidOperationException($"Cannot enroll an operator in a plugin ({nameof(SeaqPlugin)}) that has not been configured.");

            target.HandleStarted += async (object target, IMessage msg) =>
            {
                await QueueMessage(msg);
            };
            target.HandleValidationStarted += async (object target, IMessage msg) =>
            {
                //await QueueMessage(msg);
            };
            target.HandleValidationSucceeded += async (object target, IMessage msg) =>
            {
                //await QueueMessage(msg);
            };
            target.HandleValidationFailed += async (object target, IMessage msg) =>
            {
                //await QueueMessage(msg);
            };
            target.HandleExecutionStarted += async (object target, IMessage msg) =>
            {
                //await QueueMessage(msg);
            };
            target.HandleExecutionSucceeded += async (object target, IMessage msg) =>
            {
                //await QueueMessage(msg);
            };
            target.HandleSucceeded += async (object target, IMessage msg) =>
            {
                //await QueueMessage(msg);
            };
            target.HandleFailed += async (object target, IMessage msg) =>
            {
                //await QueueMessage(msg);
            };
            target.HandleFinished += async (object target, IMessage msg) =>
            {
                await QueueMessage(msg);
            };
        }

        private async Task QueueMessage(IMessage msg)
        {
            _messageQueue.Enqueue(msg);
            if (messageQueueIsProcessing is not true)
                await ProcessMessageQueue();
        }
        private async Task<int> ProcessMessageQueue()
        {
            messageQueueIsProcessing = true;
            while (_messageQueue.TryDequeue(out var cmd))
            {
                await IndexMessage(cmd);
            }
            messageQueueIsProcessing = false;
            return 0;
        }

        private async Task IndexMessage(IMessage msg)
        {

            var w = new MessageWrapper(msg);
            var idx = _cluster.Indices.FirstOrDefault(x => x.Name.EndsWith(w.MessageType, StringComparison.OrdinalIgnoreCase));
            if (idx is null)
            {
                var idx_conf = new IndexConfig(
                    w.IndexName,
                    w.Type);
                await _cluster.CreateIndexAsync(idx_conf);
                idx = _cluster.Indices.FirstOrDefault(x => x.Name.EndsWith(w.MessageType, StringComparison.OrdinalIgnoreCase));
            }

            await _cluster.CommitAsync(w);
        }
    }


}
