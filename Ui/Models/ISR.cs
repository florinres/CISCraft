using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ui.Models
{
    public class ISR
    {
        public string? Name { get; set; }
        public ushort IVTAddress { get; set; }
        public ushort ISRAddress { get; set; }
        public required string TextCode { get; set; }
    }
}
