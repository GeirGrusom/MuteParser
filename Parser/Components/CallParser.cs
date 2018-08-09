using Parser.Expressions;
using Parser.SyntaxNodes;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Parser.Components
{
    public sealed class CallParser : ParserComponent<Call>
    {
        public CallParser(Parser parser) : base(parser)
        {
        }

        private TypeShim GetReturnType(Expression lhs, List<Expression> arguments)
        {
            TypeShim returnType;
            if (lhs is Member mem)
            {
                if (mem.MemberInfo is MethodInfo methInfo)
                {
                    returnType = methInfo.CreateTypeShim();
                }
                else
                {
                    methInfo = FindMethodInfo(mem, arguments);
                    mem.MemberInfo = methInfo;
                    if (mem.MemberInfo == null)
                    {
                        return new UnresolvedTypeShim("", true);
                    }
                    else
                    {
                        return methInfo.CreateTypeShim();
                    }
                }
                return new UnresolvedTypeShim("", true);
            }
            else
            {
                return new UnresolvedTypeShim("", true);
            }
        }

        private TypeShim GetReturnType(Expression lhs, Expression argument)
        {
            TypeShim returnType;
            if (lhs is Member mem)
            {
                if (mem.MemberInfo is MethodInfo methInfo)
                {
                    returnType = methInfo.CreateTypeShim();
                }
                else
                {
                    if (argument is Tuple tuple)
                    {
                        methInfo = FindMethodInfo(mem, tuple.Members.ToList());
                    }
                    else
                    {
                        methInfo = FindMethodInfo(mem, new List<Expression> { argument });
                    }
                    mem.MemberInfo = methInfo;
                    if (mem.MemberInfo == null)
                    {
                        return new UnresolvedTypeShim("", true);
                    }
                    else
                    {
                        return methInfo.CreateTypeShim();
                    }
                }
                return new UnresolvedTypeShim("", true);
            }
            else
            {
                return new UnresolvedTypeShim("", true);
            }
        }

        public override Expression Parse()
        {
            var lhs = Parser.Parse<Member>();
            using (var stack = Parser.Push())
            {
                if (Parser.TryReadVerbatim(Kind.CallStart, out var startNode, '('))
                {
                    var arguments = new List<Expression>();
                    Expression argument;
                    while (null != (argument = Parser.Parse<Binary>()))
                    {
                        arguments.Add(argument);
                        if (!Parser.TryReadVerbatim(Kind.CallSeparator, out var sepNode, ','))
                        {
                            break;
                        }
                    }

                    if (!Parser.TryReadVerbatim(Kind.CallEnd, out var endNode, ')'))
                    {
                        Parser.SyntaxError("Expected ')'");
                        return lhs;
                    }

                    var returnType = GetReturnType(lhs, arguments);
                    
                    stack.Merge();
                    return new Call(lhs, returnType, arguments.ToArray());
                }
                else
                {
                    return lhs;
                }
            }
        }

        private static MethodInfo FindMethodInfo(Member member, List<Expression> arguments)
        {
            if(member.Left == null)
            {
                return null;
            }
            var clrType = Types.GetClrTypeFromShim(member.Left.Type);
            return clrType.GetMethod(member.Name, arguments.Select(t => Types.GetClrTypeFromShim(t.Type)).ToArray());
        }
    }
}
