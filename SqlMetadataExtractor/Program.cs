using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SqlMetadataExtractor.Models;
using SqlMetadataExtractor.Services;

namespace SqlMetadataExtractor
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var opts = config.GetSection("MetadataOptions").Get<MetadataOptions>();

            try
            {
                var connectionString = config.GetConnectionString("Default");

                using var conn = new SqlConnection(connectionString);

                conn.Open();

                var service = new MetadataService(conn);
                var builder = new MarkdownBuilder();

                var tables = service.GetTables(opts.IncludePattern, opts.ExcludePatterns).OrderBy(x => x).ToList();

                foreach (var table in tables)
                {
                    Console.WriteLine($"Fetching metadata for: {table}");

                    builder.AppendHeader(table);
                    builder.AppendDescription(service.GetDescription(table));
                    builder.AppendColumns(service.GetColumns(table));
                    builder.AppendPrimaryKeys(service.GetPrimaryKeys(table));
                    builder.AppendForeignKeys(service.GetForeignKeys(table));
                    builder.AppendIndexes(service.GetIndexes(table));
                    builder.AppendLine();
                }

                Console.WriteLine($"Fetching metadata completed.");

                System.IO.File.WriteAllText(opts.OutputPath, builder.Build());
                Console.WriteLine($"Metadata written to {opts.OutputPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }
    }
}
