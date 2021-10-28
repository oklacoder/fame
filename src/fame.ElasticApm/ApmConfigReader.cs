using Elastic.Apm.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace fame.ElasticApm
{
    public class ApmConfigReader
        : Elastic.Apm.Config.IConfigurationReader
    {
        public const string ApmConfigSection_Key = "ElasticApm";

        public ApmConfigReader()
        {

        }

        public string ApiKey { get; set; }

        public IEnumerable<string> CustomApplicationNamespaces { get; set; } = Elastic.Apm.Config.ConfigConsts.DefaultValues.DefaultApplicationNamespaces;
        public IReadOnlyCollection<string> ApplicationNamespaces => CustomApplicationNamespaces?.ToList().AsReadOnly();

        public string CaptureBody { get; set; } = Elastic.Apm.Config.ConfigConsts.DefaultValues.CaptureBody;

        public List<string> CaptureBodyContentTypes { get; set; } = Elastic.Apm.Config.ConfigConsts.DefaultValues.CaptureBodyContentTypes.Split(",").ToList();

        public bool CaptureHeaders { get; set; } = Elastic.Apm.Config.ConfigConsts.DefaultValues.CaptureHeaders;

        public bool CentralConfig { get; set; } = Elastic.Apm.Config.ConfigConsts.DefaultValues.CentralConfig;

        public string CloudProvider { get; set; } = Elastic.Apm.Config.ConfigConsts.DefaultValues.CloudProvider;

        public IEnumerable<WildcardMatcher> CustomDisableMetrics { get; set; } = Elastic.Apm.Config.ConfigConsts.DefaultValues.DisableMetrics;
        public IReadOnlyList<WildcardMatcher> DisableMetrics => CustomDisableMetrics?.ToList().AsReadOnly();

        public bool Enabled { get; set; } = true;

        public string Environment { get; set; }

        public IEnumerable<string> CustomExcludeNamespaces { get; set; } = Elastic.Apm.Config.ConfigConsts.DefaultValues.DefaultExcludedNamespaces;
        public IReadOnlyCollection<string> ExcludedNamespaces => CustomExcludeNamespaces?.ToList().AsReadOnly();

        public TimeSpan FlushInterval { get; set; } = TimeSpan.FromMilliseconds(Elastic.Apm.Config.ConfigConsts.DefaultValues.FlushIntervalInMilliseconds);

        public Dictionary<string, string> CustomGlobalLabels { get; set; } = new Dictionary<string, string>();
        public IReadOnlyDictionary<string, string> GlobalLabels => new ReadOnlyDictionary<string, string>(CustomGlobalLabels);

        public string HostName { get; set; }

        public IEnumerable<WildcardMatcher> CustomIgnoreMessageQueues { get; set; } = Elastic.Apm.Config.ConfigConsts.DefaultValues.IgnoreMessageQueues;
        public IReadOnlyList<WildcardMatcher> IgnoreMessageQueues => CustomIgnoreMessageQueues?.ToList().AsReadOnly();

        public Elastic.Apm.Logging.LogLevel LogLevel { get; set; } = Elastic.Apm.Config.ConfigConsts.DefaultValues.LogLevel;

        public int MaxBatchEventCount { get; set; } = Elastic.Apm.Config.ConfigConsts.DefaultValues.MaxBatchEventCount;

        public int MaxQueueEventCount { get; set; } = Elastic.Apm.Config.ConfigConsts.DefaultValues.MaxQueueEventCount;

        public double MetricsIntervalInMilliseconds { get; set; } = Elastic.Apm.Config.ConfigConsts.DefaultValues.MetricsIntervalInMilliseconds;

        public bool Recording { get; set; } = true;

        public IEnumerable<WildcardMatcher> CustomSanitizeFieldNames { get; set; } = Elastic.Apm.Config.ConfigConsts.DefaultValues.SanitizeFieldNames;
        public IReadOnlyList<WildcardMatcher> SanitizeFieldNames => CustomSanitizeFieldNames?.ToList().AsReadOnly();

        public string SecretToken { get; set; }

        public string ServerCert { get; set; }

        public Uri ServerUrl { get; set; } = Elastic.Apm.Config.ConfigConsts.DefaultValues.ServerUri;

        public IEnumerable<Uri> CustomServerUris { get; set; } = new[] { Elastic.Apm.Config.ConfigConsts.DefaultValues.ServerUri };
        public IReadOnlyList<Uri> ServerUrls => CustomServerUris?.ToList().AsReadOnly();

        public string ServiceName { get; set; }

        public string ServiceNodeName { get; set; }

        public string ServiceVersion { get; set; }

        public double SpanFramesMinDurationInMilliseconds { get; set; } = Elastic.Apm.Config.ConfigConsts.DefaultValues.SpanFramesMinDurationInMilliseconds;

        public int StackTraceLimit { get; set; } = Elastic.Apm.Config.ConfigConsts.DefaultValues.StackTraceLimit;

        public bool TraceContextIgnoreSampledFalse { get; set; } = Elastic.Apm.Config.ConfigConsts.DefaultValues.TraceContextIgnoreSampledFalse;

        public IEnumerable<WildcardMatcher> CustomTransactionIgnoreUrls { get; set; } = Elastic.Apm.Config.ConfigConsts.DefaultValues.TransactionIgnoreUrls;
        public IReadOnlyList<WildcardMatcher> TransactionIgnoreUrls => CustomTransactionIgnoreUrls?.ToList().AsReadOnly();

        public int TransactionMaxSpans { get; set; } = Elastic.Apm.Config.ConfigConsts.DefaultValues.TransactionMaxSpans;

        public double TransactionSampleRate { get; set; } = Elastic.Apm.Config.ConfigConsts.DefaultValues.TransactionSampleRate;

        public bool UseElasticTraceparentHeader { get; set; } = Elastic.Apm.Config.ConfigConsts.DefaultValues.UseElasticTraceparentHeader;

        public bool VerifyServerCert { get; set; } = Elastic.Apm.Config.ConfigConsts.DefaultValues.VerifyServerCert;
    }
}
