using System;
using System.Collections.Generic;
using System.Text;

namespace Parser.Expressions
{
    public class Using : Expression 
    {
        public List<string> Namespace { get; }

        public Using(List<string> ns)
            : base(Types.Void)
        {
        }
    }
}
