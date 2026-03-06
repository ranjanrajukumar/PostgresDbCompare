namespace PostgresDbCompare.Models
{
    public class DbConnectionModel
    {
        // Source DB
        public string SourceHost { get; set; }
        public int SourcePort { get; set; }
        public string SourceDatabase { get; set; }
        public string SourceUsername { get; set; }
        public string SourcePassword { get; set; }

        // Target DB
        public string TargetHost { get; set; }
        public int TargetPort { get; set; }
        public string TargetDatabase { get; set; }
        public string TargetUsername { get; set; }
        public string TargetPassword { get; set; }
    }
}