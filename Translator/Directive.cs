using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Translator
{
    class Directive
    {
        public string name { get; set; } = string.Empty;

        [JsonPropertyName("microcod")]
        public List<string> commands { get; set; } = new List<string>();
    }
}
