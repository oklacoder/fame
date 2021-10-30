using Microsoft.Extensions.Configuration;
using Nest;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace fame.Persist.Elastic
{
    public class ElasticPluginConfig
    {
        public const string ElasticPluginConfig_Key = "ElasticServer";

        public string ElasticUrl { get; set; }
        public string ElasticUser { get; set; }
        public string ElasticPass { get; set; }
        public string IndexPrefix { get; set; }
    }

    public static class ElasticPlugin
    {
        private static ElasticPluginConfig _config = null;
        private static ElasticClient _client = null;
        public static bool IsConfigured => _config is not null;
        public static bool? CanPing => _client?.Ping()?.IsValid;

        public static void Configure(
            IConfiguration config)
        {
            _config = new ElasticPluginConfig();
            config.GetSection(ElasticPluginConfig.ElasticPluginConfig_Key).Bind(_config);

            var conn = new ConnectionSettings(new Uri(_config.ElasticUrl));
            if (_config?.ElasticUser is not null && _config?.ElasticPass is not null)
                conn.BasicAuthentication(_config?.ElasticUser, _config?.ElasticPass);
            _client = new ElasticClient(conn);
        }
        public static void Enroll(this IOperator target)
        {
            if (_client is null)
                throw new InvalidOperationException($"Cannot enroll an operator in a plugin ({nameof(ElasticPlugin)}) that has not been configured.");

            target.HandleStarted += async (object target, IMessage msg) =>
            {
                const int v = 1;
                await IndexMessage(msg, v);
            };
            target.HandleValidationStarted += async (object target, IMessage msg) =>
            {
                const int v = 2;
                await IndexMessage(msg, v);
            };
            target.HandleValidationSucceeded += async (object target, IMessage msg) =>
            {
                const int v = 3;
                await IndexMessage(msg, v);
            };
            target.HandleValidationFailed += async (object target, IMessage msg) =>
            {
                const int v = 4;
                await IndexMessage(msg, v);
            };
            target.HandleExecutionStarted += async (object target, IMessage msg) =>
            {
                const int v = 5;
                await IndexMessage(msg, v);
            };
            target.HandleExecutionSucceeded += async (object target, IMessage msg) =>
            {
                const int v = 6;
                await IndexMessage(msg, v);
            };
            target.HandleSucceeded += async (object target, IMessage msg) =>
            {
                const int v = 7;
                await IndexMessage(msg, v);
            };
            target.HandleFailed += async (object target, IMessage msg) =>
            {
                const int v = 8;
                await IndexMessage(msg, v);
            };
            target.HandleFinished += async (object target, IMessage msg) =>
            {
                const int v = 9;
                await IndexMessage(msg, v);
            };
        }

        private static Func<object, string> GetIndexNameFromObject = (object obj) =>
            string.IsNullOrWhiteSpace(_config.IndexPrefix) ?
                obj?.GetType()?.FullName.ToLowerInvariant() :
                $"{_config.IndexPrefix}_{obj?.GetType()?.FullName.ToLowerInvariant()}";

        private static async Task IndexMessage(IMessage msg, int version)
        {
            //TODO
            ///explicit doc versioning.  real race condition worries wrt eventual consistency.
            ///tests
            
            if (msg is not null)
                await _client.IndexAsync(msg, x => x.Index(GetIndexNameFromObject(msg)).Id(msg.RefId).Version(version).VersionType(Elasticsearch.Net.VersionType.ExternalGte));
        }
    }
}
