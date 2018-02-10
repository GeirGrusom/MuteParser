using Parser.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parser.Components
{
    public sealed class CompilationUnitParser : ParserComponent<CompilationUnit>
    {
        public CompilationUnitParser(Parser parser) : base(parser)
        {
        }

        public override Expression Parse()
        {
            Parser.Push();
            Parser.PushScopeStack();

            List<Expression> statements = new List<Expression>();
            while (true)
            {
                var nextStatement = ParseStatement();
                if (nextStatement == null)
                {
                    break;
                }
                statements.Add(nextStatement);
            }

            Parser.Merge();

            return new CompilationUnit(Parser.PopScopeStack(), statements);
        }

        private Expression ParseStatement()
        {
            return Parser.Parse<VariableDeclaration>() ?? Parser.Parse<Assign>() ?? Parser.Parse<If>() ?? Parser.Parse<Method>() ?? Parser.Parse<Using>();
        }

    }
}
