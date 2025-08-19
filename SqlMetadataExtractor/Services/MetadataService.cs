using Microsoft.Data.SqlClient;
using SqlMetadataExtractor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlMetadataExtractor.Services
{
    public class MetadataService
    {
        private readonly SqlConnection _conn;
        public MetadataService(SqlConnection conn) => _conn = conn;

        public IEnumerable<string> GetTables(string includePattern, List<string> excludes)
        {
            var sql = "SELECT name FROM sys.tables WHERE name LIKE @include";
            for (int i = 0; i < excludes.Count; i++)
                sql += $" AND name NOT LIKE @ex{i}";

            using var cmd = new SqlCommand(sql, _conn);
            cmd.Parameters.AddWithValue("@include", includePattern);
            for (int i = 0; i < excludes.Count; i++)
                cmd.Parameters.AddWithValue($"@ex{i}", excludes[i]);

            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
                yield return rdr.GetString(0);
        }

        public string? GetDescription(string table)
        {
            using var cmd = new SqlCommand(
                @"SELECT CAST(value AS NVARCHAR(MAX)) 
              FROM sys.extended_properties 
              WHERE major_id = OBJECT_ID(@tbl) AND minor_id = 0 AND name = 'MS_Description'", _conn);
            cmd.Parameters.AddWithValue("@tbl", table);
            return cmd.ExecuteScalar() as string;
        }

        public List<ColumnMetadata> GetColumns(string table)
        {
            var list = new List<ColumnMetadata>();
            using var cmd = _conn.CreateCommand();
            cmd.CommandText = @"
SELECT c.name, ty.name, c.max_length, c.precision, c.scale, c.is_nullable, c.is_identity,
       dc.definition, ep.value
FROM sys.columns c
JOIN sys.types ty ON c.user_type_id = ty.user_type_id
LEFT JOIN sys.default_constraints dc ON c.default_object_id = dc.object_id
LEFT JOIN sys.extended_properties ep
  ON ep.major_id = c.object_id AND ep.minor_id = c.column_id AND ep.name = 'MS_Description'
WHERE c.object_id = OBJECT_ID(@tbl)
ORDER BY c.name";
            cmd.Parameters.AddWithValue("@tbl", table);

            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                var dtName = rdr.GetString(1);
                var dataType = dtName switch
                {
                    "varchar" or "nvarchar" or "char" or "nchar" => $"{dtName}({(rdr.GetInt16(2) == -1 ? "MAX" : rdr.GetInt16(2).ToString())})",
                    "decimal" or "numeric" => $"{dtName}({rdr.GetByte(3)},{rdr.GetByte(4)})",
                    _ => dtName
                };

                list.Add(new ColumnMetadata
                {
                    Name = rdr.GetString(0),
                    DataType = dataType,
                    IsNullable = rdr.GetBoolean(5),
                    IsIdentity = rdr.GetBoolean(6),
                    DefaultValue = rdr.IsDBNull(7) ? null : rdr.GetString(7),
                    Comment = rdr.IsDBNull(8) ? null : rdr.GetString(8)
                });
            }
            return list;
        }

        public List<string> GetPrimaryKeys(string table)
        {
            var keys = new List<string>();
            using var cmd = _conn.CreateCommand();
            cmd.CommandText = @"
SELECT c.name
FROM sys.indexes i
JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
WHERE i.object_id = OBJECT_ID(@tbl) AND i.is_primary_key = 1
ORDER BY ic.key_ordinal";
            cmd.Parameters.AddWithValue("@tbl", table);
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read()) keys.Add(rdr.GetString(0));
            return keys;
        }

        public List<ForeignKeyMetadata> GetForeignKeys(string table)
        {
            var list = new List<ForeignKeyMetadata>();
            using var cmd = _conn.CreateCommand();
            cmd.CommandText = @"
SELECT fk.name, cp.name, rt.name, cr.name
FROM sys.foreign_keys fk
JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
JOIN sys.columns cp ON fkc.parent_object_id = cp.object_id AND fkc.parent_column_id = cp.column_id
JOIN sys.columns cr ON fkc.referenced_object_id = cr.object_id AND fkc.referenced_column_id = cr.column_id
JOIN sys.tables rt ON fkc.referenced_object_id = rt.object_id
WHERE fk.parent_object_id = OBJECT_ID(@tbl)";

            cmd.Parameters.AddWithValue("@tbl", table);
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                list.Add(new ForeignKeyMetadata
                {
                    Name = rdr.GetString(0),
                    ColumnName = rdr.GetString(1),
                    ReferencedTable = rdr.GetString(2),
                    ReferencedColumn = rdr.GetString(3)
                });
            }
            return list;
        }

        public List<IndexMetadata> GetIndexes(string table)
        {
            var list = new List<IndexMetadata>();
            using var cmd = _conn.CreateCommand();
            cmd.CommandText = @"
SELECT i.name, i.is_unique, i.is_primary_key,
       STRING_AGG(c.name, ', ') WITHIN GROUP (ORDER BY ic.key_ordinal) AS cols
FROM sys.indexes i
JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
WHERE i.object_id = OBJECT_ID(@tbl)
GROUP BY i.name, i.is_unique, i.is_primary_key";

            cmd.Parameters.AddWithValue("@tbl", table);
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                list.Add(new IndexMetadata
                {
                    Name = rdr.GetString(0),
                    IsUnique = rdr.GetBoolean(1),
                    IsPrimaryKey = rdr.GetBoolean(2),
                    Columns = rdr.GetString(3).Split(',').Select(s => s.Trim()).ToList()
                });
            }
            return list;
        }
    }
}
