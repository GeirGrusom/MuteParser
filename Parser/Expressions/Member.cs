using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Parser.Expressions
{
    public class Member : Expression
    {
        public Member(Expression left, string name, MemberInfo memberInfo, TypeShim type)
            : base(type)
        {
            Left = left;
            Name = name;
            MemberInfo = memberInfo;
        }

        public MemberInfo MemberInfo { get; internal set; }

        public Expression Left { get; }

        public string Name { get; }

        public override string ToString()
        {
            return $"{Left}.{Name}";
        }
    }
}
