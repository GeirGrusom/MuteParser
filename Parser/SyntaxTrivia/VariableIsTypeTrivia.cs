using Parser.Expressions;
using System;

namespace Parser.SyntaxTrivia
{
    public class VariableIsTypeTrivia : VariableTrivia, IEquatable<VariableIsTypeTrivia>
    {
        public TypeShim Type { get; }
        public VariableIsTypeTrivia(Variable variable, TypeShim type) : base(variable)
        {
            Type = type;
        }

        public bool Equals(VariableIsTypeTrivia other)
        {
            return Variable.GetShadowedVariable(other.Variable) == Variable.GetShadowedVariable(Variable) && Type.Equals(other.Type);
        }
    }
}
