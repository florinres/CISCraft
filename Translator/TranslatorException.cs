using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Translator
{
    class TranslatorException :Exception
    {
        public TranslatorException()
        {

        }
        public TranslatorException(string message) : base(message)
        {
            
        }
        public TranslatorException(string message, Exception inner) : base(message, inner)
        {

        }
    }
}
