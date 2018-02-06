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
                default:
                    throw new NotImplementedException();
            }

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

        protected virtual Expression OnVisit(Add expression)
        {
            var left = OnVisit(expression.Left);
            var right = OnVisit(expression.Right);
            if (left != expression.Left || right != expression.Right)
            {
                return new Add(left, right, expression.Type);
            }
            return expression;
        }

        protected virtual Expression OnVisit(Subtract expression)
        {
            var left = OnVisit(expression.Left);
            var right = OnVisit(expression.Right);
            if (left != expression.Left || right != expression.Right)
            {
                return new Subtract(left, right, expression.Type);
            }
            return expression;
        }

        protected virtual Expression OnVisit(Multiply expression)
        {
            var left = OnVisit(expression.Left);
            var right = OnVisit(expression.Right);
            if (left != expression.Left || right != expression.Right)
            {
                return new Multiply(left, right, expression.Type);
            }
            return expression;
        }
        protected virtual Expression OnVisit(Divide expression)
        {
            var left = OnVisit(expression.Left);
            var right = OnVisit(expression.Right);
            if (left != expression.Left || right != expression.Right)
            {
                return new Divide(left, right, expression.Type);
            }
            return expression;
        }
        protected virtual Expression OnVisit(Remainder expression)
        {
            var left = OnVisit(expression.Left);
            var right = OnVisit(expression.Right);
            if (left != expression.Left || right != expression.Right)
            {
                return new Remainder(left, right, expression.Type);
            }
            return expression;
        }
        protected virtual Expression OnVisit(Equal expression)
        {
            var left = OnVisit(expression.Left);
            var right = OnVisit(expression.Right);
            if (left != expression.Left || right != expression.Right)
            {
                return new Equal(left, right);
            }
            return expression;
        }

        protected virtual Expression OnVisit(NotEqual expression)
        {
            var left = OnVisit(expression.Left);
            var right = OnVisit(expression.Right);
            if (left != expression.Left || right != expression.Right)
            {
                return new NotEqual(left, right);
            }
            return expression;
        }

        protected virtual Expression OnVisit(Block expression)
        {
            var body = expression.Statements.Select(OnVisit).Where(x => x != null).ToArray();

            if (!body.SequenceEqual(expression.Statements))
            {
                return new Block(expression.Scope, body);
            }
            return expression;
        }

        protected virtual Expression OnVisit(Method expression)
        {
            var body = OnVisit(expression.Body);
            var parameters = expression.Parameters.Select(OnVisit).Where(p => p != null).Cast<Variable>().ToArray();
            if (body != expression.Body || !parameters.SequenceEqual(expression.Parameters))
            {
                return new Method(expression.ReturnType, expression.Name, body, expression.Scope, parameters);
            }
            return expression;
        }

        protected virtual Expression OnVisit(CompilationUnit expression)
        {
            var body = expression.Body.Select(OnVisit).Where(x => x != null).ToArray();

            if (!body.SequenceEqual(expression.Body))
            {
                return new CompilationUnit(expression.Module, expression.Scope, body);
            }

            return expression;
        }
    }
}
