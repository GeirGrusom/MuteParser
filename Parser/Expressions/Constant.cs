using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser.Expressions
{
    public class Constant : Expression
    {
        public Constant(object value, TypeShim type)
            : base(type)
        {
            Value = value;
        }

        public Constant(object value)
            : base(Types.GetTypeShim(value.GetType(), false))
        {
            Value = value;
        }

        public object Value { get; }

        public static readonly Constant Null = new Null();
        public static readonly Constant True = new True();
        public static readonly Constant False = new Constant(false, Types.Bool);

        public override string ToString()
        {
            if (Value == null)
            {
                return "null";
            }
            else
            {
                return Value.ToString();
            }
        }
    }

    public sealed class Null : Constant
    {
        internal Null()
            : base(null, Types.Null)
        {
        }

        public override string ToString()
        {
            return "null";
        }
    }

    public sealed class True : Constant
    {
        internal True()
            : base(true, Types.Bool)
        {
        }

        public override string ToString()
        {
            return "true";
        }
    }

    public sealed class False : Constant
    {
        internal False()
            : base(false, Types.Bool)
        {
        }

        public override string ToString()
        {
            return "false";
        }
    }
}
