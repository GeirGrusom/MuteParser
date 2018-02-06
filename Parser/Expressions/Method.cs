using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser.Expressions
{
    public class Method : Expression, IScopedExpression
    {
        public Method(TypeShim returnType, string name, Expression body, Scope scope, params Variable[] parameters)
        {
            Name = name;
            ReturnType = returnType;
            Parameters = parameters;
            Body = body;
            Scope = scope ?? new Scope();
        }

        public TypeShim ReturnType { get; }

        public string Name { get; }

        public Variable[] Parameters { get; }

        public Expression Body { get; }

        public Scope Scope { get; }

        public override string ToString()
        {
            return $"{Name}({string.Join(", ", Parameters.Select(x => x.Name + ": " + x.Type))}) => {ReturnType} {Body}";
        }
    }
}
