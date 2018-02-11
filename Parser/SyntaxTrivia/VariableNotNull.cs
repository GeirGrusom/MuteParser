using Parser.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser.SyntaxTrivia
{
    public class VariableIsNotNullTrivia : VariableTrivia, IEquatable<VariableIsNotNullTrivia>
    {
        public VariableIsNotNullTrivia(Variable variable)
            : base(variable)
        {
        }

        public bool Equals(VariableIsNotNullTrivia other)
        {
            return Equals((VariableTrivia)other);
        }

        public override bool Equals(object obj)
        {
            return obj is VariableIsNotNullTrivia trivia && Equals(trivia);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
