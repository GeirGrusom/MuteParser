using Parser.Expressions;

namespace Parser.SyntaxTrivia
{
    public abstract class VariableTrivia : Trivia
    {
        public Variable Variable { get; }

        protected VariableTrivia(Variable variable)
        {
            Variable = variable;
        }
    }
}
