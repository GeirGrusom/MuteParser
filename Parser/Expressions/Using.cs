using System;
using System.Collections.Generic;

namespace Parser.Expressions
{
    public class Using : Expression 
    {
        public List<ReadOnlyMemory<char>> Namespace { get; }

        public Using(List<ReadOnlyMemory<char>> ns)
            : base(Types.Void)
        {
        }
    }
}
