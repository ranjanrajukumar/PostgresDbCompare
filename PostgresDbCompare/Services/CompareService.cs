using PostgresDbCompare.Models;
using Npgsql;
using System.Text;
using System.Text.RegularExpressions;

namespace PostgresDbCompare.Services
{
    public class CompareService
    {
        private readonly DatabaseService _databaseService;

        public CompareService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task<CompareResultModel> Compare(string source, string target)
        {
            var result = new CompareResultModel();

            // Schemas
            var sourceSchemas = await _databaseService.GetSchemas(source);
            var targetSchemas = await _databaseService.GetSchemas(target);
            result.MissingSchemas = sourceSchemas.Except(targetSchemas).ToList();

            // Tables
            var sourceTables = await _databaseService.GetTables(source);
            var targetTables = await _databaseService.GetTables(target);
            result.MissingTables = sourceTables.Except(targetTables).ToList();

            // Columns
            var sourceColumns = await _databaseService.GetColumns(source);
            var targetColumns = await _databaseService.GetColumns(target);
            result.MissingColumns = sourceColumns.Except(targetColumns).ToList();

            // Indexes
            var sourceIndexes = await _databaseService.GetIndexes(source);
            var targetIndexes = await _databaseService.GetIndexes(target);
            result.MissingIndexes = sourceIndexes.Except(targetIndexes).ToList();

            // Constraints
            var sourceConstraints = await _databaseService.GetConstraints(source);
            var targetConstraints = await _databaseService.GetConstraints(target);
            result.MissingConstraints = sourceConstraints.Except(targetConstraints).ToList();

            // Views
            var sourceViews = await _databaseService.GetViews(source);
            var targetViews = await _databaseService.GetViews(target);
            result.MissingViews = sourceViews.Except(targetViews).ToList();

            // Functions
            var sourceFunctions = await _databaseService.GetFunctions(source);
            var targetFunctions = await _databaseService.GetFunctions(target);
            result.MissingFunctions = sourceFunctions.Except(targetFunctions).ToList();

            result.GeneratedScript = await GenerateMigrationScript(result, source);

            return result;
        }


        private async Task<string> GenerateMigrationScript(
            CompareResultModel result,
            string sourceConnection)
        {
            var script = new StringBuilder();

            // SCHEMAS
            foreach (var schema in result.MissingSchemas)
            {
                script.AppendLine($"CREATE SCHEMA IF NOT EXISTS {schema};");
                script.AppendLine();
            }

            // TABLES
            foreach (var table in result.MissingTables)
            {
                var parts = table.Split('.');

                if (parts.Length != 2)
                    continue;

                var schema = parts[0];
                var tableName = parts[1];

                script.AppendLine($"-- Create Table {table}");

                var tableScript = await _databaseService.GetTableScript(
                    sourceConnection,
                    schema,
                    tableName);

                if (!string.IsNullOrEmpty(tableScript))
                {
                    // Remove problematic nextval sequence defaults
                    tableScript = Regex.Replace(
                        tableScript,
                        @"DEFAULT nextval\('.*?'\:\:regclass\)",
                        "",
                        RegexOptions.IgnoreCase);

                    script.AppendLine(tableScript);
                }

                script.AppendLine();
            }

            // VIEWS
            foreach (var view in result.MissingViews)
            {
                script.AppendLine($"-- Create View {view}");
                script.AppendLine($"CREATE VIEW {view} AS SELECT * FROM ...;");
                script.AppendLine();
            }

            // FUNCTIONS
            foreach (var fn in result.MissingFunctions)
            {
                script.AppendLine($"-- Create Function {fn}");
                script.AppendLine($"CREATE FUNCTION {fn}() RETURNS void AS $$ BEGIN END; $$ LANGUAGE plpgsql;");
                script.AppendLine();
            }

            return script.ToString();
        }


        public async Task ExecuteScript(string script, string connection)
        {
            using var conn = new NpgsqlConnection(connection);
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand(script, conn);
            await cmd.ExecuteNonQueryAsync();
        }
    }
}