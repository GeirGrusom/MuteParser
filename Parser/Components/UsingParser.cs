using System;
using System.Collections.Generic;
using System.Text;
using Parser.Expressions;
using Parser.SyntaxNodes;

namespace Parser.Components
{
    public sealed class UsingParser : ParserComponent<Using>
    {
        public UsingParser(Parser parser) : base(parser)
        {
        }

        public override Expression Parse()
        {
            using (var stack = Parser.Push())
            {
                if (Parser.TryReadVerbatim(Kind.Keyword, out var usingNode, "using"))
                {
                    var resultNamespace = new List<ReadOnlyMemory<char>>();

                    while (Parser.TryReadIdentifier(out var nsId))
                    {
                        resultNamespace.Add(nsId.Value);

                        if (!Parser.TryReadVerbatim(Kind.MemberSeparator, out var memberSepNode, '.'))
                        {
                            break;
                        }
                    }
                    stack.Merge();
                    return new Using(resultNamespace);
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
