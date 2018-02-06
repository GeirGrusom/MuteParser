using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser.Expressions
{
    public class Variable : Expression
    {
        public Variable(string name, bool mutable, TypeShim type)
            : base(type)
        {
            Name = name;
            Mutable = mutable;
        }

        public string Name { get; }

        public bool Mutable { get; }

        public override string ToString()
        {
            return Name;
        }
    }
}
