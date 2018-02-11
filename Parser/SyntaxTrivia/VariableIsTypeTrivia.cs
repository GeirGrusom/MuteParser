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
            return other.Variable.GetVariableShadow() == Variable.GetVariableShadow() && Type.Equals(other.Type);
        }

        public override bool Equals(object obj)
        {
            return obj is VariableIsTypeTrivia trivia && Equals(trivia);
        }

        public override int GetHashCode()
        {
            return Type.GetHashCode() ^ Variable.GetHashCode() * 8191;
        }
    }
}
