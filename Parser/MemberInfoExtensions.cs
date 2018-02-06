namespace Parser
{
    using System;
    using System.Linq;
    using System.Reflection;

    public static class MemberInfoExtensions
    {
        public static Type GetMemberInfoResultType(this MemberInfo memberInfo)
        {
            switch (memberInfo)
            {
                case MethodInfo meth:
                    return meth.ReturnType;
                case PropertyInfo prop:
                    return prop.PropertyType;
                case FieldInfo field:
                    return field.FieldType;
            }
            return null;
        }

        public static TypeShim CreateTypeShim(this MemberInfo memberInfo)
        {
            if (memberInfo == null)
            {
                return null;
            }
            switch (memberInfo)
            {
                case MethodInfo meth:
                    return Types.GetTypeShim(meth.ReturnType, IsNullable(meth));
                case PropertyInfo prop:
                    return Types.GetTypeShim(prop.PropertyType, IsNullable(prop));
                case FieldInfo field:
                    return Types.GetTypeShim(field.FieldType, IsNullable(field));
                case ConstructorInfo ctor:
                    return Types.GetTypeShim(ctor.DeclaringType, false);
                case EventInfo ev:
                    return Types.GetTypeShim(ev.EventHandlerType, true);
            }
            return null;
        }

        private static bool IsNullable(PropertyInfo prop)
        {
            var notNull = prop.GetCustomAttributes().Any(x => x.GetType().Name == "NotNull") || prop.GetMethod.ReturnTypeCustomAttributes.GetCustomAttributes(false).Any(x => x.GetType().Name == "NotNull");
            return !notNull;
        }

        private static bool IsNullable(FieldInfo prop)
        {
            var notNull = prop.GetCustomAttributes().Any(x => x.GetType().Name == "NotNull");
            return !notNull;
        }

        private static bool IsNullable(MethodInfo prop)
        {
            var notNull = prop.GetCustomAttributes().Any(x => x.GetType().Name == "NotNull") || prop.ReturnTypeCustomAttributes.GetCustomAttributes(false).Any(x => x.GetType().Name == "NotNull");
            return !notNull;
        }

    }
}
