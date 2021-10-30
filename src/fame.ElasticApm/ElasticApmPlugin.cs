using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace fame.ElasticApm
{
    public static class ElasticApmPlugin 
    {
        public const string validation_key = "validation";
        public const string execution_key = "execution";

        static Func<Guid, string> GetValidationKey = (Guid id) =>
        {
            return $"{id}|{validation_key}";
        };
        static Func<Guid, string> GetExecutionKey = (Guid id) =>
        {
            return $"{id}|{execution_key}";
        };

        public static void Configure(this IConfiguration config)
        {
            var apmConfig = new ApmConfigReader();
            config.GetSection(ApmConfigReader.ApmConfigSection_Key).Bind(apmConfig);
            Elastic.Apm.Agent.Setup(
                new Elastic.Apm.AgentComponents(
                    configurationReader: apmConfig
                    )
                );
            TransactionCache.Configure();
        }

        public static void Enroll(this IOperator target)
        {


            target.HandleStarted += (object target, IMessage msg) =>
            {
                AddOrGetTransaction(target, msg);
            };
            target.HandleValidationStarted += (object target, IMessage msg) =>
            {
                AddOrGetValidationSpan(msg);
            };
            target.HandleValidationSucceeded += (object target, IMessage msg) =>
            {
                var span = GetValidationSpan(msg);
                if (span != default)
                {
                    span.Outcome = Elastic.Apm.Api.Outcome.Success;
                    span.End();
                    RemoveValidationSpan(msg);
                }
            };
            target.HandleValidationFailed += (object target, IMessage msg) =>
            {
                var tran = GetTransaction(msg);
                if (tran != default)
                {
                    tran.Outcome = Elastic.Apm.Api.Outcome.Failure;
                    tran.End();
                }
                var span = GetValidationSpan(msg);
                if (span != default)
                {
                    span.Outcome = Elastic.Apm.Api.Outcome.Success;
                    span.End();
                }
                CleanupTransaction(msg);
            };
            target.HandleExecutionStarted += (object target, IMessage msg) =>
            {
                AddOrGetExecutionSpan(msg);
            };
            target.HandleExecutionSucceeded += (object target, IMessage msg) =>
            {
                var span = GetExecutionSpan(msg);
                if (span != default)
                {
                    span.Outcome = Elastic.Apm.Api.Outcome.Success;
                    span.End();
                    RemoveExecutionSpan(msg);
                }
            };
            target.HandleSucceeded += (object target, IMessage msg) =>
            {
                var tran = GetTransaction(msg);
                if (tran != default)
                {
                    tran.Outcome = Elastic.Apm.Api.Outcome.Success;
                }
            };
            target.HandleFailed += (object target, IMessage msg) =>
            {
                var span = GetExecutionSpan(msg);
                if (span != default)
                {
                    span.Outcome = Elastic.Apm.Api.Outcome.Failure;
                    span.End();
                    RemoveExecutionSpan(msg);
                }
                var tran = GetTransaction(msg);
                if (tran != default)
                {
                    tran.Outcome = Elastic.Apm.Api.Outcome.Failure;
                }
            };
            target.HandleFinished += (object target, IMessage msg) =>
            {
                var tran = GetTransaction(msg);
                if (tran != default)
                {
                    tran.End();
                    CleanupTransaction(msg);
                }
            };
        }

        private static Elastic.Apm.Api.ITransaction AddOrGetTransaction(object target, IMessage msg)
        {
            return TransactionCache._transactions.GetOrAdd(
                    msg.RefId,
                    Elastic.Apm.Agent.Tracer.StartTransaction(
                        msg.RefId.ToString(),
                        msg.GetType().FullName));
        }
        private static Elastic.Apm.Api.ITransaction GetTransaction(IMessage msg)
        {
            return TransactionCache._transactions.GetValueOrDefault(msg.RefId);
        }
        private static bool RemoveTransaction(IMessage msg)
        {
            return TransactionCache._transactions.Remove(msg.RefId, out _);
        }
        private static Elastic.Apm.Api.ISpan AddOrGetValidationSpan(IMessage msg)
        {
            var tran = GetTransaction(msg);
            if (tran == default)
                return default;
            return TransactionCache._spans.GetOrAdd(
                        GetValidationKey(msg.RefId),
                        tran.StartSpan(
                            validation_key,
                            tran.Type));
        }
        private static Elastic.Apm.Api.ISpan GetValidationSpan(IMessage msg)
        {
            return TransactionCache._spans.GetValueOrDefault(GetValidationKey(msg.RefId));
        }
        private static bool RemoveValidationSpan(IMessage msg)
        {
            return TransactionCache._spans.Remove(GetValidationKey(msg.RefId), out _);
        }
        private static Elastic.Apm.Api.ISpan AddOrGetExecutionSpan(IMessage msg)
        {
            var tran = GetTransaction(msg);
            if (tran == default)
                return default;
            return TransactionCache._spans.GetOrAdd(
                GetExecutionKey(msg.RefId),
                tran.StartSpan(
                    execution_key,
                    tran.Type));
        }
        private static Elastic.Apm.Api.ISpan GetExecutionSpan(IMessage msg)
        {
            return TransactionCache._spans.GetValueOrDefault(GetExecutionKey(msg.RefId));
        }
        private static bool RemoveExecutionSpan(IMessage msg)
        {
            return TransactionCache._spans.Remove(GetExecutionKey(msg.RefId), out _);
        }

        private static void CleanupTransaction(IMessage msg)
        {
            RemoveTransaction(msg);
            RemoveValidationSpan(msg);
            RemoveExecutionSpan(msg);
        }
    }   
}
