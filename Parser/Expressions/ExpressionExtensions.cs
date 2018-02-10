using System.Reflection;

namespace Parser.Expressions
{
    public static class ExpressionExtensions
    {
        public static bool IsMutable(this Expression exp, bool inConstructor = false)
        {
            if (exp is Variable var)
            {
                return var.Mutable;
            }
            if (exp is Member mem)
            {
                switch (mem.MemberInfo)
                {
                    case PropertyInfo prop:
                        return prop.CanWrite;
                    case FieldInfo field:
                        return inConstructor && field.IsInitOnly;
                    case MethodInfo meth:
                        return meth.ReturnType.IsByRef;
                    case EventInfo ev:
                        return ev.AddMethod != null;
                    case ConstructorInfo ctor:
                        return false;
                }
            }
            return false;
        }
    }
}
