using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using seaq;
using System;
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

        private ILogger<SeaqPlugin> _logger;
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
                await IndexMessage(msg);
            };
            target.HandleValidationStarted += async (object target, IMessage msg) =>
            {
                await IndexMessage(msg);
            };
            target.HandleValidationSucceeded += async (object target, IMessage msg) =>
            {
                await IndexMessage(msg);
            };
            target.HandleValidationFailed += async (object target, IMessage msg) =>
            {
                await IndexMessage(msg);
            };
            target.HandleExecutionStarted += async (object target, IMessage msg) =>
            {
                await IndexMessage(msg);
            };
            target.HandleExecutionSucceeded += async (object target, IMessage msg) =>
            {
                await IndexMessage(msg);
            };
            target.HandleSucceeded += async (object target, IMessage msg) =>
            {
                await IndexMessage(msg);
            };
            target.HandleFailed += async (object target, IMessage msg) =>
            {
                await IndexMessage(msg);
            };
            target.HandleFinished += async (object target, IMessage msg) =>
            {
                await IndexMessage(msg);
            };
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

            _cluster.Commit(w);
        }
    }


}
