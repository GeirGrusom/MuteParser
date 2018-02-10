using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parser.Expressions
{
    public sealed class ExpressionEqualityVisitor : Visitor
    {
        private readonly Stack<Expression> otherExpressionStack;

        private Expression Other => otherExpressionStack.Peek();

        public ExpressionEqualityVisitor()
        {
            otherExpressionStack = new Stack<Expression>();
        }

        public Expression Visit(Expression left, Expression right)
        {
            otherExpressionStack.Clear();
            otherExpressionStack.Push(right);
            return OnVisit(left);
        }

        private Expression Test(Expression other, Expression @this)
        {
            otherExpressionStack.Push(other);
            var result = OnVisit(@this);
            otherExpressionStack.Pop();
            return result;
        }

        protected override Expression OnVisit(Constant expression)
        {
            if(Other is Constant otherConst)
            {
                if(otherConst.Type != expression.Type)
                {
                    return Constant.False;
                }
                if(otherConst.Value == expression.Value)
                {
                    return Constant.True;
                }
                if(otherConst.Value == null || expression.Value == null)
                {
                    return Constant.False;
                }
                if(otherConst.Value.Equals(expression.Value))
                {
                    return Constant.True;
                }
            }
            return Constant.False;
        }

        protected override Expression OnVisit(Binary expression)
        {
            if (Other is Binary otherBinary)
            {
                if (expression.Left?.GetType() != expression.Right?.GetType())
                {
                    return Constant.False;
                }
                if (Test(otherBinary.Left, expression.Left) == Constant.False || Test(otherBinary.Right, expression.Right) == Constant.False)
                {
                    return Constant.False;
                }
                return Constant.True;
            }
            else
            {
                return Constant.False;
            }
        }

        

        protected override Expression OnVisit(CompilationUnit expression)
        {
            if (Other is CompilationUnit otherComp)
            {
                if(otherComp.Body.Count != expression.Body.Count)
                {
                    return Constant.False;
                }
                for(int i = 0; i < expression.Body.Count; ++i)
                {
                    if(Test(otherComp.Body[i], expression.Body[i]) == Constant.False)
                    {
                        return Constant.False;
                    }
                }
                return Constant.True;
            }
            return Constant.False;
        }

        protected override Expression OnVisit(Block expression)
        {
            if (Other is Block otherComp)
            {
                if (otherComp.Statements.Length != expression.Statements.Length)
                {
                    return Constant.False;
                }
                for (int i = 0; i < expression.Statements.Length; ++i)
                {
                    if (Test(otherComp.Statements[i], expression.Statements[i]) == Constant.False)
                    {
                        return Constant.False;
                    }
                }
                return Constant.True;
            }
            return Constant.False;
        }
        

    }
}
