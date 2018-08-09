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
            using (var stack = Parser.Push())
            {
                if (Parser.TryReadIdentifier(out var id))
                {
                    string sid = id.Value.ToString();
                    var lhs = Parser.FindVariable(sid) ?? Parser.FindMethod(sid)  ?? (Expression)new UnresolvedIdentifier(sid);
                    var res = Parse(lhs);
                    if (res == null)
                    {
                        stack.Merge();
                        return lhs;
                    }
                    stack.Merge();
                    return res;
                }
                else
                {
                    return null;
                }
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

                string sid = id.Value.ToString();
                MemberInfo memberInfo = null;
                if (lhs is Variable)
                {
                    var varType = Types.GetClrTypeFromShim(lhs.Type);
                    if (varType != null)
                    {
                        memberInfo = GetMemberInfo(varType, sid);
                    }
                }
                else if (lhs is Member mem)
                {
                    memberInfo = GetMemberInfo(Types.GetClrTypeFromShim(mem.Type), sid);
                }

                var res = new Member(lhs, sid, memberInfo, memberInfo.CreateTypeShim() ?? new UnresolvedTypeShim("", true));
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
