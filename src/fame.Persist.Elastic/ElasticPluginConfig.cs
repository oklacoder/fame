namespace fame.Persist.Elastic
{
    public class ElasticPluginConfig
    {
        public const string ElasticPluginConfig_Key = "ElasticServer";

        public string ElasticUrl { get; set; } = "http://localhost:9200";
        public string ElasticUser { get; set; } = "elastic";
        public string ElasticPass { get; set; } = "elastic";
        public string IndexPrefix { get; set; }
        public bool WaitForRefresh { get; set; } = false;
    }
}
