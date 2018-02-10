using System;

namespace Parser.Components
{
    using Expressions;
    public sealed class BinaryParser : ParserComponent<Binary>
    {
        public BinaryParser(Parser parser) : base(parser)
        {
        }

        public override Expression Parse()
        {
            return Parser.Parse<Equal>();
        }
    }
}
