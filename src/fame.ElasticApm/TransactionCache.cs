using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace fame.ElasticApm
{
    public static class TransactionCache
    {
        internal static ConcurrentDictionary<Guid, Elastic.Apm.Api.ITransaction> _transactions;
        internal static ConcurrentDictionary<string, Elastic.Apm.Api.ISpan> _spans;

        public static IEnumerable<Elastic.Apm.Api.ITransaction> Transactions => _transactions?.Values;
        public static IEnumerable<Elastic.Apm.Api.ISpan> Spans => _spans?.Values;

        internal static bool IsConfigured;

        internal static void Configure()
        {
            if (IsConfigured)
            {
                return;
            }
            RebuildTransactionCache();
            RebuildSpanCache();
            IsConfigured = true;
        }

        internal static void RebuildTransactionCache()
        {
            _transactions = new ConcurrentDictionary<Guid, Elastic.Apm.Api.ITransaction>();
        }
        internal static void RebuildSpanCache()
        {
            _spans = new ConcurrentDictionary<string, Elastic.Apm.Api.ISpan>();
        }
    }
}
