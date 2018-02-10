using Parser.Expressions;
using Parser.SyntaxNodes;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Parser.Components
{
    public sealed class MemberParser : ParserComponent<Member>
    {
        public MemberParser(Parser parser) : base(parser)
        {
        }

        public override Expression Parse()
        {
            Parser.Push();
            if (Parser.TryReadIdentifier(out var id))
            {
                var lhs = Parser.FindVariable(id.Value) ?? (Expression)new UnresolvedIdentifier(id.Value);
                var res = Parse(lhs);
                if (res == null)
                {
                    Parser.Merge();
                    return lhs;
                }
                Parser.Merge();
                return res;
            }
            else
            {
                Parser.Pop();
                return null;
            }
        }

        private Expression Parse(Expression lhs)
        {
            if (Parser.TryReadVerbatim(Kind.MemberSeparator, out var memberSeparatorNode, '.'))
            {
                if (!Parser.TryReadIdentifier(out var id))
                {
                    Parser.SyntaxError("Expected identifier");
                    return null;
                }
                MemberInfo memberInfo = null;
                if (lhs is Variable)
                {
                    var varType = Types.GetClrTypeFromShim(lhs.Type);
                    if (varType != null)
                    {
                        memberInfo = GetMemberInfo(varType, id.Value);
                    }
                }
                else if (lhs is Member mem)
                {
                    memberInfo = GetMemberInfo(Types.GetClrTypeFromShim(mem.Type), id.Value);
                }

                var res = new Member(lhs, id.Value, memberInfo, memberInfo.CreateTypeShim() ?? new UnresolvedTypeShim("", true));
                return Parse(res);
            }
            else
            {
                return lhs;
            }
        }

        private static MemberInfo GetMemberInfo(Type clrType, string name)
        {
            var members = clrType.GetMember(name);
            if (members.Length != 1)
            {
                return null;
            }
            return members[0];
        }
    }
}
