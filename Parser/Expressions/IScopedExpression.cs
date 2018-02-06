using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser.Expressions
{
    public interface IScopedExpression
    {
        Scope Scope { get; }
    }
}
