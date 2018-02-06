using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser.Expressions
{
    public class VariableDeclaration : Expression
    {
        public VariableDeclaration(string name, bool mutable, TypeShim type, Expression assignment = null)
            : base(type)
        {
            Variable = new Variable(name, mutable, type);
            Assignment = assignment;
        }

        public Variable Variable { get; }

        public Expression Assignment { get; }

        public override string ToString()
        {
            return $"{(Variable.Mutable ? "var" : "let")} {Variable.Name}:{Type} {(Assignment != null ? " <- " + Assignment : "")}";
        }
    }
}
