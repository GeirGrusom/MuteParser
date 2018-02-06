using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser.Expressions
{
    public class If : Expression, IScopedExpression
    {
        private static TypeShim GetExpressionType(Expression @true, Expression @false)
        {
            if (@false == null || @true.Type != @false.Type)
            {
                return Types.Void;
            }
            return @true.Type;
        }

        public If(Expression condition, Expression @true, Expression @false, Scope scope)
            : base(GetExpressionType(@true, @false))
        {
            Condition = condition;
            True = @true;
            False = @false;
            Scope = scope ?? new Scope();
        }

        public Expression Condition { get; }

        public Expression True { get; }

        public Expression False { get; }

        public Scope Scope { get; }


        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine($"if({Condition})");
            builder.AppendLine(True.ToString());

            if (False != null)
            {
                builder.Append("else");
                if (False is If)
                {
                    builder.Append(False);
                }
                else
                {
                    builder.AppendLine();
                    builder.Append(False);
                }
            }

            return builder.ToString();
        }
    }
}
