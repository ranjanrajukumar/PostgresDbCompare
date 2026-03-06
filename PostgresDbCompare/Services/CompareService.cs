using PostgresDbCompare.Models;

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

            // 1️⃣ Compare Schemas
            var sourceSchemas = await _databaseService.GetSchemas(source);
            var targetSchemas = await _databaseService.GetSchemas(target);

            result.MissingSchemas = sourceSchemas
                .Except(targetSchemas)
                .ToList();

            // 2️⃣ Compare Tables
            var sourceTables = await _databaseService.GetTables(source);
            var targetTables = await _databaseService.GetTables(target);

            result.MissingTables = sourceTables
                .Except(targetTables)
                .ToList();

            // 3️⃣ Compare Columns
            var sourceColumns = await _databaseService.GetColumns(source);
            var targetColumns = await _databaseService.GetColumns(target);

            result.MissingColumns = sourceColumns
                .Except(targetColumns)
                .ToList();

            // 4️⃣ Compare Indexes
            var sourceIndexes = await _databaseService.GetIndexes(source);
            var targetIndexes = await _databaseService.GetIndexes(target);

            result.MissingIndexes = sourceIndexes
                .Except(targetIndexes)
                .ToList();

            // 5️⃣ Compare Constraints
            var sourceConstraints = await _databaseService.GetConstraints(source);
            var targetConstraints = await _databaseService.GetConstraints(target);

            result.MissingConstraints = sourceConstraints
                .Except(targetConstraints)
                .ToList();

            // 6️⃣ Compare Views
            var sourceViews = await _databaseService.GetViews(source);
            var targetViews = await _databaseService.GetViews(target);

            result.MissingViews = sourceViews
                .Except(targetViews)
                .ToList();

            // 7️⃣ Compare Functions
            var sourceFunctions = await _databaseService.GetFunctions(source);
            var targetFunctions = await _databaseService.GetFunctions(target);

            result.MissingFunctions = sourceFunctions
                .Except(targetFunctions)
                .ToList();

            // Generate Migration Script
            result.GeneratedScript = GenerateMigrationScript(result);

            return result;
        }


        private string GenerateMigrationScript(CompareResultModel result)
        {
            var script = "";

            // Create Schemas first
            foreach (var schema in result.MissingSchemas)
            {
                script += $"CREATE SCHEMA IF NOT EXISTS {schema};\n\n";
            }

            // Create Tables
            foreach (var table in result.MissingTables)
            {
                script += $"-- Create Table\n";
                script += $"CREATE TABLE {table} (...);\n\n";
            }

            // Create Views
            foreach (var view in result.MissingViews)
            {
                script += $"-- Create View\n";
                script += $"CREATE VIEW {view} AS ...;\n\n";
            }

            // Create Functions
            foreach (var fn in result.MissingFunctions)
            {
                script += $"-- Create Function\n";
                script += $"CREATE FUNCTION {fn}();\n\n";
            }

            return script;
        }
    }
}