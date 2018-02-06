using Parser.Expressions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser
{
    public abstract class TypeShim : IEquatable<TypeShim>
    {
        protected TypeShim(object wrappedType, bool nullable)
        {
            WrappedType = wrappedType;
            Nullable = nullable;
        }

        public object WrappedType { get; }
        public bool Nullable { get; }

        public override bool Equals(object obj)
        {
            return Equals(obj as TypeShim);
        }

        public bool Equals(TypeShim other)
        {
            return other != null &&
                   EqualityComparer<object>.Default.Equals(WrappedType, other.WrappedType) &&
                   Nullable == other.Nullable;
        }

        public override int GetHashCode()
        {
            var hashCode = 1538852026;
            hashCode = hashCode * -1521134295 + EqualityComparer<object>.Default.GetHashCode(WrappedType);
            hashCode = hashCode * -1521134295 + Nullable.GetHashCode();
            return hashCode;
        }
    }

    public sealed class NullTypeShim : TypeShim
    {
        public NullTypeShim() : base(null, false)
        {
        }

        public override string ToString()
        {
            return "null";
        }
    }

    public sealed class ThisTypeShim : TypeShim
    {
        public static readonly ThisTypeShim ThisNull = new ThisTypeShim(true);
        public static readonly ThisTypeShim This = new ThisTypeShim(false);

        private ThisTypeShim(bool nullable)
            : base(null, nullable)
        {
        }

        private static readonly int hashCodeNull = "this?".GetHashCode();
        private static readonly int hashCode = "this".GetHashCode();

        public override int GetHashCode() => Nullable ? hashCodeNull : hashCode;

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj);
        }


    }

    public sealed class UnionTypeShim : TypeShim, IEquatable<UnionTypeShim>
    {
        public UnionTypeShim(TypeShim[] types, bool nullable)
            : base(types, nullable)
        {
            Types = types;
        }

        public TypeShim[] Types { get; }

        public override bool Equals(object obj)
        {
            return Equals(obj as UnionTypeShim);
        }

        public bool Equals(UnionTypeShim other)
        {
            return other != null &&
                   base.Equals(other) &&
                   EqualityComparer<TypeShim[]>.Default.Equals(Types, other.Types);
        }

        public override int GetHashCode()
        {
            var hashCode = -1474912454;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<TypeShim[]>.Default.GetHashCode(Types);
            return hashCode;
        }

        public override string ToString()
        {
            return $"<{string.Join(" | ", Types.AsEnumerable())}>";
        }
    }

    public sealed class ArrayTypeShim : TypeShim
    {
        public ArrayTypeShim(int[] dimensions, TypeShim arrayType, bool nullable)
            : base(arrayType, nullable)
        {
            Dimensions = dimensions;
            ArrayType = arrayType;
        }

        public int[] Dimensions { get; }

        public TypeShim ArrayType { get; }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append(ArrayType);
            builder.Append('[');
            for (int i = 0; i < Dimensions.Length; ++i)
            {
                if (Dimensions[i] > -1)
                {
                    builder.Append(Dimensions[i].ToString(CultureInfo.InvariantCulture));
                }
                if (i < Dimensions.Length - 1)
                {
                    builder.Append(", ");
                }
            }
            builder.Append(']');
            if (Nullable)
            {
                builder.Append('?');
            }
            return builder.ToString();
        }
    }

    public sealed class ClrTypeShim : TypeShim, IEquatable<ClrTypeShim>
    {
        public ClrTypeShim(Type clrType, bool nullable)
            : base(clrType, nullable)
        {
            ClrType = clrType;
        }

        public Type ClrType { get; }

        public override bool Equals(object obj)
        {
            return Equals(obj as ClrTypeShim);
        }

        public bool Equals(ClrTypeShim other)
        {
            return other != null &&
                   base.Equals(other) &&
                   EqualityComparer<Type>.Default.Equals(ClrType, other.ClrType);
        }

        public override int GetHashCode()
        {
            var hashCode = -227472592;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<Type>.Default.GetHashCode(ClrType);
            return hashCode;
        }

        private readonly Dictionary<Type, string> ClrTypeAlias = new Dictionary<Type, string>
        {
            [typeof(byte)] = "u8",
            [typeof(ushort)] = "u16",
            [typeof(uint)] = "u32",
            [typeof(ulong)] = "u64",
            [typeof(sbyte)] = "i8",
            [typeof(short)] = "i16",
            [typeof(Guid)] = "guid",
            [typeof(int)] = "i32",
            [typeof(long)] = "i64",
            [typeof(string)] = "string",
            [typeof(float)] = "f32",
            [typeof(double)] = "f64",
            [typeof(bool)] = "bool",
            [typeof(void)] = "void"
        };

        private string GetClrName()
        {
            if(ClrTypeAlias.TryGetValue(ClrType, out var alias))
            {
                return alias;
            }
            return ClrType.Name;
        }

        public override string ToString()
        {
            if (Nullable)
            {
                return GetClrName() + "?";
            }
            else
            {
                return GetClrName();
            }
        }
    }

    public sealed class UnresolvedTypeShim : TypeShim, IEquatable<UnresolvedTypeShim>
    {
        public UnresolvedTypeShim(string type, bool nullable)
            : base(type, nullable)
        {
            UnresolvedType = type;
        }

        public string UnresolvedType { get; }

        public TypeShim ResvoledType { get; private set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as UnresolvedTypeShim);
        }

        public bool Equals(UnresolvedTypeShim other)
        {
            return other != null &&
                   base.Equals(other) &&
                   UnresolvedType == other.UnresolvedType;
        }

        public override int GetHashCode()
        {
            var hashCode = -1710142870;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(UnresolvedType);
            return hashCode;
        }

        public void Resolve(TypeShim resolvedType)
        {
            ResvoledType = resolvedType;
        }
    }

    public sealed class TupleTypeShim : TypeShim
    {
        public TupleTypeShim(bool nullable, params Variable[] values)
            : base(values, nullable)
        {
            Parameters = values;
        }

        public Variable[] Parameters { get; }

        public override string ToString()
        {
            return $"({String.Join(", ", Parameters.AsEnumerable())})";
        }
    }

    public sealed class FunctionTypeShim : TypeShim, IEquatable<FunctionTypeShim>
    {
        public FunctionTypeShim(TypeShim returnType, bool nullable, params Variable[] parameters)
            : base((parameters, returnType), nullable)
        {
            Parameters = parameters;
            ReturnType = returnType;
        }

        public TypeShim ReturnType { get; }
        public Variable[] Parameters { get; }

        public override bool Equals(object obj)
        {
            return Equals(obj as FunctionTypeShim);
        }

        public bool Equals(FunctionTypeShim other)
        {
            return other != null &&
                   base.Equals(other) &&
                   EqualityComparer<TypeShim>.Default.Equals(ReturnType, other.ReturnType) &&
                   EqualityComparer<Variable[]>.Default.Equals(Parameters, other.Parameters);
        }

        public override int GetHashCode()
        {
            var hashCode = -486575272;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<TypeShim>.Default.GetHashCode(ReturnType);
            hashCode = hashCode * -1521134295 + EqualityComparer<Variable[]>.Default.GetHashCode(Parameters);
            return hashCode;
        }

        public override string ToString()
        {
            return $"({String.Join(", ", Parameters.AsEnumerable())}) => {ReturnType}";
        }
    }



    public static class Types
    {
        /// <summary>
        /// Denotes the return type of a function that never returns.
        /// </summary>
        public static readonly TypeShim Never = new ClrTypeShim(typeof(void), false);

        /// <summary>
        /// Denotes the type of the null value
        /// </summary>
        public static readonly TypeShim Null = new NullTypeShim();

        public static readonly TypeShim Void = new ClrTypeShim(typeof(void), false);
        public static readonly TypeShim String = new ClrTypeShim(typeof(string), false);
        public static readonly TypeShim Int = new ClrTypeShim(typeof(int), false);
        public static readonly TypeShim Long = new ClrTypeShim(typeof(long), false);
        public static readonly TypeShim Bool = new ClrTypeShim(typeof(bool), false);
        public static readonly TypeShim Float = new ClrTypeShim(typeof(float), false);
        public static readonly TypeShim Double = new ClrTypeShim(typeof(double), false);

        public static readonly TypeShim This = ThisTypeShim.This;
        public static readonly TypeShim ThisNull = ThisTypeShim.ThisNull;

        private static readonly Dictionary<(Type type, bool nullable), TypeShim> typeCache = new Dictionary<(Type type, bool nullable), TypeShim>
        {
            [(typeof(void), false)] = Void,
            [(typeof(string), false)] = String,
            [(typeof(int), false)] = Int,
            [(typeof(long), false)] = Long,
            [(typeof(bool), false)] = Bool,
            [(typeof(float), false)] = Float,
            [(typeof(double), false)] = Double,
        };

        public static TypeShim MakeNonNull(TypeShim shim)
        {
            if(shim is ClrTypeShim clrType)
            {
                return GetTypeShim(clrType.ClrType, false);
            }
            if(shim is ArrayTypeShim arrayType)
            {
                return new ArrayTypeShim(arrayType.Dimensions, arrayType.ArrayType, false);
            }
            if(shim is ThisTypeShim)
            {
                return This;
            }
            if(shim is TupleTypeShim tupleType)
            {
                return new TupleTypeShim(false, tupleType.Parameters);
            }
            if(shim is FunctionTypeShim functionType)
            {
                return new FunctionTypeShim(functionType.ReturnType, false, functionType.Parameters);
            }
            if(shim is UnionTypeShim unionType)
            {
                return new UnionTypeShim(unionType.Types, false);
            }
            if(shim is UnresolvedTypeShim unresolvedType)
            {
                return new UnresolvedTypeShim(unresolvedType.UnresolvedType, false);
            }
            if(shim is NullTypeShim)
            {
                return shim;
            }
            throw new NotImplementedException();
        }

        public static TypeShim GetTypeShim(Type clrType, bool nullable)
        {
            if(clrType == null)
            {
                return Null;
            }
            if (typeCache.TryGetValue((clrType, nullable), out var result))
            {
                return result;
            }
            result = new ClrTypeShim(clrType, nullable);
            typeCache.Add((clrType, nullable), result);
            return result;
        }



        public static bool IsAssignable(TypeShim target, TypeShim source)
        {
            if(target == source)
            {
                return true;
            }
            if(!target.Nullable && source.Nullable)
            {
                return false;
            }
            if(target is ArrayTypeShim targetArray && source is ArrayTypeShim sourceArray)
            {
                return targetArray.Dimensions.SequenceEqual(targetArray.Dimensions) && IsAssignable(targetArray.ArrayType, sourceArray.ArrayType);
            }
            if(target is ClrTypeShim targetClr && source is ClrTypeShim sourceClr)
            {
                return targetClr.ClrType == sourceClr.ClrType;
            }
            if(target is FunctionTypeShim targetFunction && source is FunctionTypeShim sourceFunction)
            {
                return IsAssignable(targetFunction.ReturnType, sourceFunction.ReturnType) && targetFunction.Parameters.Select(p => p.Type).Zip(sourceFunction.Parameters.Select(p => p.Type), IsAssignable).All(x => x);
            }
            if(target is UnionTypeShim targetUnion && source is UnionTypeShim sourceUnion)
            {
                return targetUnion.Types.Zip(sourceUnion.Types, IsAssignable).All(x => x);
            }
            if(target is UnresolvedTypeShim && source is UnresolvedTypeShim)
            {
                return true;
            }
            if(source is NullTypeShim && target.Nullable)
            {
                return true;
            }
            return false;
            
        }
    }
}
