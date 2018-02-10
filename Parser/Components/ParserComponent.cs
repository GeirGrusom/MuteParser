using Parser.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parser.Components
{
    public abstract class ParserComponent
    {
        protected ParserComponent(Parser parser)
        {
            Parser = parser;
        }

        public abstract Type Result { get; }

        protected Parser Parser { get; }

        public abstract Expression Parse();

        public Expression Parse(Scope initialScope)
        {
            Parser.PushScopeStack(initialScope);
            try
            {
                return Parse();
            }
            finally
            {
                Parser.PopScopeStack();
            }
        }
    }
    public abstract class ParserComponent<T> : ParserComponent
    {
        protected ParserComponent(Parser parser) : base(parser)
        {
        }

        public override Type Result => typeof(T);
    }
}
