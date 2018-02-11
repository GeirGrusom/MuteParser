using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Parser
{
    using Expressions;
    public class Visitor
    {
        public Expression Visit(Expression expression)
        {
            return OnVisit(expression);
        }

        protected virtual Expression OnVisit(Expression expression)
        {
            switch (expression)
            {
                case Block block:
                    return OnVisit(block);
                case Method method:
                    return OnVisit(method);
                case CompilationUnit compilationUnit:
                    return OnVisit(compilationUnit);
                case Constant constant:
                    return OnVisit(constant);
                case Binary bin:
                    return OnVisit(bin);
                case Unary unary:
                    return OnVisit(unary);
                default:
                    throw new NotImplementedException();
            }

        }

        protected virtual Expression OnVisit(Unary expression)
        {
            switch(expression)
            {
                case Negate negate:
                    return OnVisit(negate);
                case Positive positive:
                    return OnVisit(positive);
                case Not not:
                    return OnVisit(not);
                case Return ret:
                    return OnVisit(ret);
                case NotNull notNull:
                    return OnVisit(notNull);
                default:
                    throw new NotImplementedException();
            }

        }

        protected virtual Expression OnVisit(NotNull expression)
        {
            var operand = OnVisit(expression.Operand);
            if (operand != expression.Operand)
            {
                return CopyTrivia(new NotNull(operand), expression);
            }
            return expression;
        }

        protected virtual Expression OnVisit(Negate expression)
        {
            var operand = OnVisit(expression.Operand);
            if(operand != expression.Operand)
            {
                return CopyTrivia(new Negate(operand), expression);
            }
            return expression;
        }

        protected virtual Expression OnVisit(Positive expression)
        {
            var operand = OnVisit(expression.Operand);
            if (operand != expression.Operand)
            {
                return CopyTrivia(new Positive(operand), expression);
            }
            return expression;
        }

        protected virtual Expression OnVisit(Not expression)
        {
            var operand = OnVisit(expression.Operand);
            if (operand != expression.Operand)
            {
                return CopyTrivia(new Not(operand), expression);
            }
            return expression;
        }

        protected virtual Expression OnVisit(Binary expression)
        {
            switch (expression)
            {
                case Add add:
                    return OnVisit(add);
                case Subtract subtract:
                    return OnVisit(subtract);
                case Multiply multiply:
                    return OnVisit(multiply);
                case Divide divide:
                    return OnVisit(divide);
                case Remainder remainder:
                    return OnVisit(remainder);
                case Equal equal:
                    return OnVisit(equal);
                case NotEqual notEqual:
                    return OnVisit(notEqual);
            }
            throw new NotImplementedException();
        }

        protected Expression CopyTrivia(Expression target, Expression source)
        {
            return target.WithTrivia(source);
        }

        protected virtual Expression OnVisit(Return expression)
        {
            var operand = OnVisit(expression);
            if(operand != expression.Operand)
            {
                return CopyTrivia(new Return(operand), expression);
            }
            return expression;
        }

        protected virtual Expression OnVisit(Constant expression)
        {
            return expression;
        }

        protected virtual Expression OnVisit(Add expression)
        {
            var left = OnVisit(expression.Left);
            var right = OnVisit(expression.Right);
            if (left != expression.Left || right != expression.Right)
            {
                return CopyTrivia(new Add(left, right, expression.Type), expression);
            }
            return expression;
        }

        protected virtual Expression OnVisit(Subtract expression)
        {
            var left = OnVisit(expression.Left);
            var right = OnVisit(expression.Right);
            if (left != expression.Left || right != expression.Right)
            {
                return CopyTrivia(new Subtract(left, right, expression.Type), expression);
            }
            return expression;
        }

        protected virtual Expression OnVisit(Multiply expression)
        {
            var left = OnVisit(expression.Left);
            var right = OnVisit(expression.Right);
            if (left != expression.Left || right != expression.Right)
            {
                return CopyTrivia(new Multiply(left, right, expression.Type), expression);
            }
            return expression;
        }
        protected virtual Expression OnVisit(Divide expression)
        {
            var left = OnVisit(expression.Left);
            var right = OnVisit(expression.Right);
            if (left != expression.Left || right != expression.Right)
            {
                return CopyTrivia(new Divide(left, right, expression.Type), expression);
            }
            return expression;
        }
        protected virtual Expression OnVisit(Remainder expression)
        {
            var left = OnVisit(expression.Left);
            var right = OnVisit(expression.Right);
            if (left != expression.Left || right != expression.Right)
            {
                return CopyTrivia(new Remainder(left, right, expression.Type), expression);
            }
            return expression;
        }
        protected virtual Expression OnVisit(Equal expression)
        {
            var left = OnVisit(expression.Left);
            var right = OnVisit(expression.Right);
            if (left != expression.Left || right != expression.Right)
            {
                return CopyTrivia(new Equal(left, right), expression);
            }
            return expression;
        }

        protected virtual Expression OnVisit(NotEqual expression)
        {
            var left = OnVisit(expression.Left);
            var right = OnVisit(expression.Right);
            if (left != expression.Left || right != expression.Right)
            {
                return CopyTrivia(new NotEqual(left, right), expression);
            }
            return expression;
        }

        protected virtual Expression OnVisit(Block expression)
        {
            var body = expression.Statements.Select(OnVisit).Where(x => x != null).ToArray();

            if (!body.SequenceEqual(expression.Statements))
            {
                return CopyTrivia(new Block(expression.Scope, body), expression);
            }
            return expression;
        }

        protected virtual Expression OnVisit(Method expression)
        {
            var body = OnVisit(expression.Body);
            var parameters = expression.Parameters.Select(OnVisit).Where(p => p != null).Cast<Variable>().ToArray();
            if (body != expression.Body || !parameters.SequenceEqual(expression.Parameters))
            {
                return CopyTrivia(new Method(expression.ReturnType, expression.Name, body, expression.Scope, parameters), expression);
            }
            return expression;
        }

        protected virtual Expression OnVisit(CompilationUnit expression)
        {
            var body = expression.Body.Select(OnVisit).Where(x => x != null).ToArray();

            if (!body.SequenceEqual(expression.Body))
            {
                return CopyTrivia(new CompilationUnit(expression.Scope, body), expression);
            }

            return expression;
        }
    }
}
