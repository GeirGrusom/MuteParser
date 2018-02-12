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
            var pos = Parser.CurrentPosition;
            var result = Parser.Parse<Logical>();

            if(result is Binary bin && (bin.Left is null || bin.Right is null))
            {
                Parser.SyntaxError("Expected expression", pos);
            }

            return result;
        }
    }
}
