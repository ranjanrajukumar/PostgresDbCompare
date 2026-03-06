using Npgsql;

namespace PostgresDbCompare.Services
{
    public class DatabaseService
    {
        // SCHEMAS
        public async Task<List<string>> GetSchemas(string connection)
        {
            var schemas = new List<string>();

            using var conn = new NpgsqlConnection(connection);
            await conn.OpenAsync();

            var cmd = new NpgsqlCommand(@"
                SELECT schema_name
                FROM information_schema.schemata
                WHERE schema_name NOT IN ('pg_catalog','information_schema')
            ", conn);

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
                schemas.Add(reader.GetString(0));

            return schemas;
        }


        // TABLES
        public async Task<List<string>> GetTables(string connection)
        {
            var tables = new List<string>();

            using var conn = new NpgsqlConnection(connection);
            await conn.OpenAsync();

            var cmd = new NpgsqlCommand(@"
                SELECT table_schema || '.' || table_name
                FROM information_schema.tables
                WHERE table_type='BASE TABLE'
                AND table_schema NOT IN ('pg_catalog','information_schema')
            ", conn);

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
                tables.Add(reader.GetString(0));

            return tables;
        }


        // VIEWS
        public async Task<List<string>> GetViews(string connection)
        {
            var views = new List<string>();

            using var conn = new NpgsqlConnection(connection);
            await conn.OpenAsync();

            var cmd = new NpgsqlCommand(@"
                SELECT table_schema || '.' || table_name
                FROM information_schema.views
                WHERE table_schema NOT IN ('pg_catalog','information_schema')
            ", conn);

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
                views.Add(reader.GetString(0));

            return views;
        }


        // FUNCTIONS
        public async Task<List<string>> GetFunctions(string connection)
        {
            var functions = new List<string>();

            using var conn = new NpgsqlConnection(connection);
            await conn.OpenAsync();

            var cmd = new NpgsqlCommand(@"
                SELECT routine_schema || '.' || routine_name
                FROM information_schema.routines
                WHERE routine_schema NOT IN ('pg_catalog','information_schema')
            ", conn);

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
                functions.Add(reader.GetString(0));

            return functions;
        }


        // COLUMNS
        public async Task<List<string>> GetColumns(string connection)
        {
            var columns = new List<string>();

            using var conn = new NpgsqlConnection(connection);
            await conn.OpenAsync();

            var cmd = new NpgsqlCommand(@"
                SELECT table_schema || '.' || table_name || '.' || column_name
                FROM information_schema.columns
                WHERE table_schema NOT IN ('pg_catalog','information_schema')
            ", conn);

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
                columns.Add(reader.GetString(0));

            return columns;
        }


        // INDEXES
        public async Task<List<string>> GetIndexes(string connection)
        {
            var indexes = new List<string>();

            using var conn = new NpgsqlConnection(connection);
            await conn.OpenAsync();

            var cmd = new NpgsqlCommand(@"
                SELECT schemaname || '.' || indexname
                FROM pg_indexes
                WHERE schemaname NOT IN ('pg_catalog','information_schema')
            ", conn);

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
                indexes.Add(reader.GetString(0));

            return indexes;
        }


        // CONSTRAINTS
        public async Task<List<string>> GetConstraints(string connection)
        {
            var constraints = new List<string>();

            using var conn = new NpgsqlConnection(connection);
            await conn.OpenAsync();

            var cmd = new NpgsqlCommand(@"
                SELECT n.nspname || '.' || c.conname
                FROM pg_constraint c
                JOIN pg_namespace n ON n.oid = c.connamespace
                WHERE n.nspname NOT IN ('pg_catalog','information_schema')
            ", conn);

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
                constraints.Add(reader.GetString(0));

            return constraints;
        }


        // GENERATE TABLE SCRIPT
        public async Task<string> GetTableScript(string connection, string schema, string table)
        {
            using var conn = new NpgsqlConnection(connection);
            await conn.OpenAsync();

            var sql = @"
                SELECT 
                    'CREATE TABLE ' || table_schema || '.' || table_name || E' (\n' ||
                    string_agg(
                        column_name || ' ' || data_type ||
                        CASE 
                            WHEN character_maximum_length IS NOT NULL 
                            THEN '(' || character_maximum_length || ')'
                            ELSE ''
                        END ||
                        CASE 
                            WHEN is_nullable = 'NO' 
                            THEN ' NOT NULL'
                            ELSE ''
                        END ||
                        CASE 
                            WHEN column_default IS NOT NULL 
                            THEN ' DEFAULT ' || column_default
                            ELSE ''
                        END,
                        E',\n'
                    ) || E'\n);'
                FROM information_schema.columns
                WHERE table_schema = @schema
                AND table_name = @table
                GROUP BY table_schema, table_name";

            using var cmd = new NpgsqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@schema", schema);
            cmd.Parameters.AddWithValue("@table", table);

            var result = await cmd.ExecuteScalarAsync();

            return result?.ToString() ?? "";
        }
    }
}