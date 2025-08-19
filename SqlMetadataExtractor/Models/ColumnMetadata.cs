using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlMetadataExtractor.Models
{
    public class ColumnMetadata
    {
        public string Name { get; init; } = null!;
        public string DataType { get; init; } = null!;
        public bool IsNullable { get; init; }
        public bool IsIdentity { get; init; }
        public string? DefaultValue { get; init; }
        public string? Comment { get; init; }
    }
}
