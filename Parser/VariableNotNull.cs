using Parser.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser
{
    public abstract class VariableTrivia
    {
        public Variable Variable { get; }

        protected VariableTrivia(Variable variable)
        {
            Variable = variable;
        }
    }

    public class VariableIsTypeTrivia : VariableTrivia
    {
        public TypeShim Type { get; }
        public VariableIsTypeTrivia(Variable variable, TypeShim type) : base(variable)
        {
            Type = type;
        }
    }

    public class VariableIsNullTrivia : VariableTrivia
    {
        public VariableIsNullTrivia(Variable variable) : base(variable)
        {
        }
    }

    public class VariableIsNotNullTrivia : VariableTrivia
    {
        public VariableIsNotNullTrivia(Variable variable)
            : base(variable)
        {
        }
    }

    public class VariableAssignedTrivia : VariableTrivia
    {
        public VariableAssignedTrivia(Variable variable) : base(variable)
        {
        }
    }
}
