namespace fame.seaq
{
    public class SeaqPluginConfig
    {
        public const string SeaqPluginConfig_Key = "Seaq";

        public string ClusterScope { get; set; }
        public string ClusterUrl { get; set; } = "http://localhost:9200";
        public string ClusterUser { get; set; } = "elastic";
        public string ClusterPass { get; set; } = "elastic";
        public bool? ClusterBypassCertificateValidation { get; set; } = false;
        public bool? WaitForClusterRefreshOnIndex { get; set; } = false;

        public SeaqPluginConfig()
        {
            
        }
    }
}
