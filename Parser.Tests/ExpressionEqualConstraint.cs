using NUnit.Framework.Constraints;
using Parser.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parser.Tests
{
    public class ExpressionEqualConstraint : Constraint
    {
        private readonly ExpressionEqualityVisitor equalityVisitor;
        private readonly Expression expectedValue;
        public ExpressionEqualConstraint(Expression expectedValue)
        {
            equalityVisitor = new ExpressionEqualityVisitor();
            this.expectedValue = expectedValue;
        }

        public override string Description
        {
            get => expectedValue.ToString();
        }

        public override ConstraintResult ApplyTo<TActual>(TActual actual)
        {
            if (actual is Expression actualExpression)
            {
                if (equalityVisitor.Visit(expectedValue, (Expression)(object)actual) == Constant.True)
                {
                    return new ConstraintResult(this, actual, ConstraintStatus.Success);
                }
                return new ConstraintResult(this, actual, ConstraintStatus.Failure);
            }
            else
            {
                return new ConstraintResult(this, actual, ConstraintStatus.Failure);
            }
        }
    }
}
