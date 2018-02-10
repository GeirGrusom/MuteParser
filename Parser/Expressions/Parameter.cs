using System;
using System.Collections.Generic;
using System.Text;

namespace Parser.Expressions
{
    public sealed class Parameter : Variable
    {
        public Parameter(string name, bool mutable, TypeShim type) : base(name, mutable, type)
        {
        }
    }
}
