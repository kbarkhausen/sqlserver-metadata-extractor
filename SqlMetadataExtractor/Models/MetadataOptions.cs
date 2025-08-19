using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlMetadataExtractor.Models
{
    public class MetadataOptions
    {
        public string OutputPath { get; set; } = "metadata.md";
        public string IncludePattern { get; set; } = null!;
        public List<string> ExcludePatterns { get; set; } = new List<string>();
    }
}
