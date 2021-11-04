﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nest;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace fame.Persist.Elastic
{

    public class ElasticPlugin :
        IFamePlugin
    {
        private ElasticPluginConfig _config = null;
        private ElasticClient _client = null;
        public bool? IsConfigured => _config is not null;
        public bool? CanPing => _client?.Ping()?.IsValid;

        private ILogger<ElasticPlugin> _logger;

        public void Configure(
            IConfiguration config,
            ILoggerFactory logger = null)
        {
            _logger = logger?.CreateLogger<ElasticPlugin>();

            _config = new ElasticPluginConfig();
            config.GetSection(ElasticPluginConfig.ElasticPluginConfig_Key).Bind(_config);

            var conn = new ConnectionSettings(new Uri(_config.ElasticUrl));
            if (_config?.ElasticUser is not null && _config?.ElasticPass is not null)
                conn.BasicAuthentication(_config?.ElasticUser, _config?.ElasticPass);
            _client = new ElasticClient(conn);
        }
        public void Enroll(IOperator target)
        {
            if (IsConfigured is not true)
                throw new InvalidOperationException($"Cannot enroll an operator in a plugin ({nameof(ElasticPlugin)}) that has not been configured.");

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

        public static string GetIndexNameFromObject(
            object obj, 
            string indexPrefix = null)
        {
            return string.IsNullOrWhiteSpace(indexPrefix) ?
                obj?.GetType()?.FullName.ToLowerInvariant() :
                $"{indexPrefix}_{GetObjectTypeAsIndexName(obj)}";
        }

        private string GetIndexNameFromObject(object obj)
        {
            return GetIndexNameFromObject(obj, _config?.IndexPrefix);
        }

        private static string GetObjectTypeAsIndexName(object obj)
        {
            return obj?.GetType()?.FullName.ToLowerInvariant();
        }
            

        private async Task IndexMessage(IMessage msg)
        {
            if (msg is not null)
            {
                var t = msg.GetType();

                var resp = await _client?.IndexAsync(
                    Convert.ChangeType(msg, t), 
                    x => x
                        .Index(GetIndexNameFromObject(msg))
                        .Id(msg.RefId)
                        .Refresh(
                            _config?.WaitForRefresh == true ? 
                                Elasticsearch.Net.Refresh.True : 
                                Elasticsearch.Net.Refresh.False));
                if (!resp.IsValid)
                {
                    _logger.LogWarning("Couldn't persist message {0} using plugin {1} - {2}", msg.RefId, GetType().FullName, resp?.ServerError?.Error?.Reason);
                }
            }
        }
    }
}
