using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser.Expressions
{
    public class Block : Expression, IScopedExpression
    {
        public Block(Scope scope, TypeShim returnType, params Expression[] statements)
            : base(returnType)
        {
            Statements = statements;
            Scope = scope ?? new Scope();
        }

        public Block(Scope scope, params Expression[] statements)
            : base(statements.Length > 0 ? statements[statements.Length - 1].Type : Types.Void)
        {
            Statements = statements;
            Scope = scope ?? new Scope();
        }

        public Expression[] Statements { get; }

        public Scope Scope { get; }

        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("{");
            foreach (var st in Statements)
            {
                builder.AppendLine(st.ToString());
            }
            builder.AppendLine("}");
            return builder.ToString();

        }
    }
}
