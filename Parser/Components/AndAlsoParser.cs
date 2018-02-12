using System;
using System.Collections.Generic;
using System.Text;
using Parser.Expressions;

namespace Parser.Components
{
    public sealed class AndAlsoParser : ParserComponent<AndAlso>
    {
        public AndAlsoParser(Parser parser) : base(parser)
        {
        }

        public override Expression Parse()
        {
            throw new NotImplementedException();
        }
    }
}
