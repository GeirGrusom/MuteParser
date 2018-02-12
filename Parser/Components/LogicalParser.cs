using System;
using System.Collections.Generic;
using System.Text;
using Parser.Expressions;

namespace Parser.Components
{
    public sealed class LogicalParser : ParserComponent<Logical>
    {
        public LogicalParser(Parser parser) : base(parser)
        {
        }

        public override Expression Parse()
        {
            var lhs = Parser.Parse<Equal>();

            if (Parser.TryReadVerbatim(SyntaxNodes.Kind.Operator, out var opAndAlso, "&&"))
            {
                var rhs = Parser.Parse<Logical>();
                var result = new AndAlso(lhs, rhs);

                foreach(var trivia in lhs.Trivia)
                {
                    result.Trivia.Add(trivia);
                }

                foreach(var trivia in rhs.Trivia)
                {
                    result.Trivia.Add(trivia);
                }
                return result;
            }
            else if (Parser.TryReadVerbatim(SyntaxNodes.Kind.Operator, out var opOrElse, "||"))
            {
                var rhs = Parser.Parse<Logical>();
                var result = new OrElse(lhs, rhs);

                foreach (var leftTrivia in lhs.Trivia)
                {
                    foreach (var rightTrivia in rhs.Trivia)
                    {
                        if (leftTrivia.Equals(rightTrivia))
                        {
                            result.Trivia.Add(leftTrivia);
                        }
                    }
                }
                return result;
            }
            else
            {
                return lhs;
            }
        }
    }
}
