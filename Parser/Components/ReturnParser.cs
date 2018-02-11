using System;
using System.Collections.Generic;
using System.Text;
using Parser.Expressions;

namespace Parser.Components
{
    public sealed class ReturnParser : ParserComponent<Return>
    {
        public ReturnParser(Parser parser) : base(parser)
        {
        }

        public override Expression Parse()
        {
            if (Parser.TryReadVerbatim(SyntaxNodes.Kind.Keyword, out var returnNode, "return"))
            {
                var op = Parser.Parse<Binary>();

                return new Return(op);
            }
            return null;
        }
    }
}
