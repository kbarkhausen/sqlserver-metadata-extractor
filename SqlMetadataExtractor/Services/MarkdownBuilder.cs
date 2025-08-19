using SqlMetadataExtractor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlMetadataExtractor.Services
{
    public class MarkdownBuilder
    {
        private readonly StringBuilder _sb = new();

        public void AppendHeader(string table) => _sb.AppendLine($"## Table: {table}");

        public void AppendDescription(string? desc)
        {
            if (!string.IsNullOrWhiteSpace(desc))
                _sb.AppendLine($"- **Description**: {desc}");
        }

        public void AppendColumns(IEnumerable<ColumnMetadata> columns)
        {
            _sb.AppendLine("- **Columns:**");
            foreach (var col in columns)
            {
                var def = string.IsNullOrWhiteSpace(col.DefaultValue) ? "" : $", DEFAULT {col.DefaultValue}";
                var comment = string.IsNullOrWhiteSpace(col.Comment) ? "" : $" -- {col.Comment}";
                _sb.AppendLine($"  - `{col.Name}` ({col.DataType}, {(col.IsNullable ? "NULL" : "NOT NULL")}{(col.IsIdentity ? ", IDENTITY" : "")}{def}{comment})");
            }
        }

        public void AppendPrimaryKeys(IEnumerable<string> keys)
        {
            if (!keys.Any()) return; // Skip if no primary keys
            _sb.AppendLine("- **Primary Keys:**");
            foreach (var key in keys)
                _sb.AppendLine($"  - `{key}`");
        }

        public void AppendForeignKeys(IEnumerable<ForeignKeyMetadata> fks)
        {
            if (!fks.Any()) return; // Skip if no foreign keys
            _sb.AppendLine("- **Foreign Keys:**");
            foreach (var fk in fks)
                _sb.AppendLine($"  - `{fk.Name}`: `{fk.ColumnName}` → `{fk.ReferencedTable}.{fk.ReferencedColumn}`");
        }

        public void AppendIndexes(IEnumerable<IndexMetadata> idxs)
        {
            if (!idxs.Any()) return; // Skip if no indexes
            _sb.AppendLine("- **Indexes:**");
            foreach (var idx in idxs)
            {
                var type = idx.IsPrimaryKey ? "PRIMARY KEY" : (idx.IsUnique ? "UNIQUE" : "NON‑UNIQUE");
                var cols = string.Join(", ", idx.Columns);
                _sb.AppendLine($"  - `{idx.Name}`: [{type}] {cols}");
            }
        }

        public void AppendLine() => _sb.AppendLine();

        public string Build() => _sb.ToString();
    }
}
