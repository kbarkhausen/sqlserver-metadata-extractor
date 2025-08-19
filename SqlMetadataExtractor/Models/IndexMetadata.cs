using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlMetadataExtractor.Models
{
    public class IndexMetadata
    {
        public string Name { get; init; } = null!;
        public bool IsUnique { get; init; }
        public bool IsPrimaryKey { get; init; }
        public List<string> Columns { get; init; } = new();
    }
}
