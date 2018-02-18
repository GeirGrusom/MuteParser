using Parser.Expressions;
using Parser.SyntaxTrivia;
using System.Collections.Generic;

namespace Parser.Components
{
    public sealed class OrElseParser : ParserComponent<OrElse>
    {
        public OrElseParser(Parser parser) : base(parser)
        {
        }

        public override Expression Parse()
        {
            var lhs = Parser.Parse<AndAlso>();

            if (Parser.TryReadVerbatim(SyntaxNodes.Kind.Operator, out var op, "|"))
            {
                var rhs = Parser.Parse<OrElse>();

                var trivia = new HashSet<Trivia>(lhs.Trivia);
                trivia.IntersectWith(rhs.Trivia);

                return new OrElse(lhs, rhs).WithTrivia(trivia);
            }
            else
            {
                return lhs;
            }
        }
    }
}
