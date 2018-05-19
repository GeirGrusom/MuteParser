using System.Collections.Generic;

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
