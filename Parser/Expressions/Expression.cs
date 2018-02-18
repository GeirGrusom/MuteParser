namespace Parser.Expressions
{
    using System.Collections.Generic;

    using SyntaxTrivia;
    public abstract class Expression
    {
        public HashSet<Trivia> Trivia { get; }

        public TypeShim Type { get; }

        protected Expression(TypeShim type)
        {
            Trivia = new HashSet<Trivia>();
            Type = type;
        }

        protected Expression() : this(Types.Void)
        {
        }
    }
}
