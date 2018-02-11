using Parser.SyntaxTrivia;
using System.Collections.Generic;
using System.Reflection;

namespace Parser.Expressions
{
    public static class ExpressionExtensions
    {
        public static bool IsMutable(this Expression exp, bool inConstructor = false)
        {
            if (exp is Variable var)
            {
                return var.Mutable;
            }
            if (exp is Member mem)
            {
                switch (mem.MemberInfo)
                {
                    case PropertyInfo prop:
                        return prop.CanWrite;
                    case FieldInfo field:
                        return inConstructor && field.IsInitOnly;
                    case MethodInfo meth:
                        return meth.ReturnType.IsByRef;
                    case EventInfo ev:
                        return ev.AddMethod != null;
                    case ConstructorInfo ctor:
                        return false;
                }
            }
            return false;
        }

        public static TExpression WithTrivia<TExpression>(this TExpression expression, Expression source)
            where TExpression : Expression
        {
            foreach (var trivia in source.Trivia)
            {
                expression.Trivia.Add(trivia);
            }
            return expression;
        }

        public static TExpression WithTrivia<TExpression>(this TExpression expression, Scope source)
            where TExpression : Expression
        {
            foreach (var trivia in source.Trivia)
            {
                expression.Trivia.Add(trivia);
            }
            return expression;
        }

        public static TExpression WithTrivia<TExpression>(this TExpression expression, IEnumerable<Trivia> source)
            where TExpression : Expression
        {
            foreach (var trivia in source)
            {
                expression.Trivia.Add(trivia);
            }
            return expression;
        }
    }
}
