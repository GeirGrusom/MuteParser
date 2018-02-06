using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser.Expressions
{
    public abstract class Binary : Expression
    {
        public Expression Left { get; }

        public Expression Right { get; }

        public Binary(Expression left, Expression right, TypeShim result)
            : base(result)
        {
            Left = left;
            Right = right;
        }
    }

    public sealed class Add : Binary
    {
        public Add(Expression left, Expression right, TypeShim result) : base(left, right, result)
        {
        }

        public override string ToString()
        {
            return $"({Left}) + ({Right})";
        }
    }

    public sealed class Subtract : Binary
    {
        public Subtract(Expression left, Expression right, TypeShim result) : base(left, right, result)
        {
        }

        public override string ToString()
        {
            return $"({Left}) - ({Right})";
        }
    }

    public sealed class Multiply : Binary
    {
        public Multiply(Expression left, Expression right, TypeShim result) : base(left, right, result)
        {
        }

        public override string ToString()
        {
            return $"({Left}) * ({Right})";
        }
    }

    public sealed class Divide : Binary
    {
        public Divide(Expression left, Expression right, TypeShim result) : base(left, right, result)
        {
        }

        public override string ToString()
        {
            return $"({Left}) / ({Right})";
        }
    }

    public sealed class Remainder : Binary
    {
        public Remainder(Expression left, Expression right, TypeShim result) : base(left, right, result)
        {
        }

        public override string ToString()
        {
            return $"({Left}) % ({Right})";
        }
    }

    public sealed class Equal : Binary
    {
        public Equal(Expression left, Expression right)
            : base(left, right, Types.Bool)
        {
        }

        public override string ToString()
        {
            return $"({Left} = {Right})";
        }
    }

    public sealed class NotEqual : Binary
    {
        public NotEqual(Expression left, Expression right)
            : base(left, right, Types.Bool)
        {
        }

        public override string ToString()
        {
            return $"({Left} != {Right})";
        }
    }
}
