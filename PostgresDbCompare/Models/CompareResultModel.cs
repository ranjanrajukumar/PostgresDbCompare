namespace PostgresDbCompare.Models
{
    public class CompareResultModel
    {
        public List<string> MissingSchemas { get; set; } = new();
        public List<string> MissingTables { get; set; } = new();
        public List<string> MissingColumns { get; set; } = new();
        public List<string> MissingIndexes { get; set; } = new();
        public List<string> MissingConstraints { get; set; } = new();
        public List<string> MissingFunctions { get; set; } = new();
        public List<string> MissingViews { get; set; } = new();

        public string GeneratedScript { get; set; } = "";
    }
}