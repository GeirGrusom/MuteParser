using Parser.Expressions;
using System;

namespace Parser.SyntaxTrivia
{
    public abstract class VariableTrivia : Trivia
    {
        public Variable Variable { get; }

        protected VariableTrivia(Variable variable)
        {
            Variable = variable;
        }

        protected bool Equals(VariableTrivia other)
        {
            return other.Variable.GetVariableShadow() == Variable.GetVariableShadow();
        }

        public override int GetHashCode()
        {
            return GetType().GetHashCode() ^ Variable.GetVariableShadow().GetHashCode() * 8191;
        }
    }
}
