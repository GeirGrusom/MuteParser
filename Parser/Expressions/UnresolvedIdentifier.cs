using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser.Expressions
{
    public class UnresolvedIdentifier : Expression
    {
        public UnresolvedIdentifier(string identifier)
            : base(new UnresolvedTypeShim("", false))
        {
            Identifier = identifier;
        }

        public string Identifier { get; }
    }
}
