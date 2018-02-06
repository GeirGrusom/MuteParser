using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser.Expressions
{
    public sealed class VariableDefinatelyAssignedTrivia
    {
        public Variable Variable { get; }

        public VariableDefinatelyAssignedTrivia(Variable variable)
        {
            Variable = variable;
        }

        public override string ToString()
        {
            return $"{Variable} definately assigned";
        }
    }
}
