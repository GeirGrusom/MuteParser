using Parser.Expressions;
using System;

namespace Parser.SyntaxTrivia
{
    public class VariableAssignedTrivia : VariableTrivia, IEquatable<VariableAssignedTrivia>
    {
        public VariableAssignedTrivia(Variable variable) : base(variable)
        {
        }

        public bool Equals(VariableAssignedTrivia other)
        {
            return Variable.GetVariableShadow() == other.Variable.GetVariableShadow();
        }

        public override bool Equals(object obj)
        {
            if(obj is VariableAssignedTrivia trivia)
            {
                return Equals(trivia);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Variable.GetHashCode();
        }
    }
}
