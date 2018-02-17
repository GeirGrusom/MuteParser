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
            return Parser.Parse<OrElse>();
        }
    }
}
