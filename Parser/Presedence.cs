using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser
{
    using Expressions;
    public static class Presedence
    {
        public static int Precedence<T>() where T : Expression
        {
            if(typeof(T) == typeof(Equal))
            {
                return 0;
            }
            if(typeof(T) == typeof(NotEqual))
            {
                return 0;
            }
            if(typeof(T) == typeof(Add))
            {
                return 10;
            }
            if (typeof(T) == typeof(Subtract))
            {
                return 10;
            }
            if (typeof(T) == typeof(Multiply))
            {
                return 20;
            }
            if (typeof(T) == typeof(Divide))
            {
                return 20;
            }
            if (typeof(T) == typeof(Remainder))
            {
                return 20;
            }
            throw new NotImplementedException();
        }
    }
}
