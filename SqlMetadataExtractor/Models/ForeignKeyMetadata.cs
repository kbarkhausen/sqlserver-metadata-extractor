using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlMetadataExtractor.Models
{
    public class ForeignKeyMetadata
    {
        public string Name { get; init; } = null!;
        public string ColumnName { get; init; } = null!;
        public string ReferencedTable { get; init; } = null!;
        public string ReferencedColumn { get; init; } = null!;
    }
}
