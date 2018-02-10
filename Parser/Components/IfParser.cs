﻿namespace Parser.Components
{
    using System.Collections.Generic;

    using Expressions;
    using global::Parser.SyntaxNodes;
    using SyntaxTrivia;

    public class IfParser : ParserComponent<If>
    {
        public IfParser(Parser parser)
            : base(parser)
        {
        }

        public override Expression Parse()
        {
            Parser.Push();
            Parser.PushScopeStack();
            if (!Parser.TryReadVerbatim(Kind.Keyword, out var ifNode, "if"))
            {
                Parser.PopScopeStack();
                Parser.Pop();
                return null;
            }

            var condition = Parser.Parse<Binary>();

            if (condition == null)
            {
                Parser.SyntaxError("Expected expression");
                Parser.PopScopeStack();
                Parser.Pop();
                
                return null;
            }

            if (condition.Type != Types.Bool && !(condition.Type is UnresolvedTypeShim))
            {
                Parser.SyntaxError("If condition must be a boolean expression");
            }

            List<Variable> triviaShadowing = new List<Variable>();
            List<Variable> falseTriviaShadowing = new List<Variable>();
            foreach (var trivia in condition.Trivia)
            {
                if (trivia is VariableIsNotNullTrivia notNull)
                {
                    triviaShadowing.Add(new ShadowingVariable(notNull.Variable, Types.MakeNonNull(notNull.Variable.Type)));
                    falseTriviaShadowing.Add(new ShadowingVariable(notNull.Variable, Types.Null));
                }
                else if (trivia is VariableIsNullTrivia isNull)
                {
                    triviaShadowing.Add(new ShadowingVariable(isNull.Variable, Types.Null));
                    falseTriviaShadowing.Add(new ShadowingVariable(isNull.Variable, Types.MakeNonNull(isNull.Variable.Type)));
                }
                else if (trivia is VariableIsTypeTrivia isTypeTrivia)
                {
                    triviaShadowing.Add(new ShadowingVariable(isTypeTrivia.Variable, isTypeTrivia.Type));
                }
            }

            var @true = Parser.Parse<Block>(new Scope(triviaShadowing));

            if (@true == null)
            {
                Parser.PopScopeStack();
                Parser.Pop();
                Parser.SyntaxError("Expected block expression");
                return null;
            }

            Expression @false = null;
            if (Parser.TryReadVerbatim(Kind.Keyword, out var elseNode, "else"))
            {
                @false = Parser.Parse<Block>(new Scope(falseTriviaShadowing));
            }

            if (@false != null)
            {
                foreach (var trueTrivia in @true.Trivia)
                {
                    foreach (var falseTrivia in @false.Trivia)
                    {
                        if (trueTrivia.Equals(falseTrivia))
                        {
                            Parser.CurrentScope.Trivia.Add(trueTrivia);
                        }
                    }
                }
            }
            else
            {
                Parser.CurrentScope.Trivia.AddRange(@true.Trivia);
            }

            Parser.Merge();
            return new If(condition, @true, @false, Parser.PopScopeStack());
        }
    }
}