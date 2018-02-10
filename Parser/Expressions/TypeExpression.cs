using System;
using System.Collections.Generic;
using System.Text;

namespace Parser.Expressions
{
    public sealed class DataType : Expression
    {
        public DataType(TypeShim type)
            : base(type)
        {
        }
    }
}
