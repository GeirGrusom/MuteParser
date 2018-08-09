using Parser.Expressions;
using Parser.SyntaxNodes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parser.Components
{
    public sealed class DataTypeParser : ParserComponent<DataType>
    {
        public DataTypeParser(Parser parser) : base(parser)
        {
        }

        public override Expression Parse()
        {
            using (var stack = Parser.Push())
            {
                if (Parser.TryReadIdentifier(out var id))
                {
                    bool isNullable = Parser.TryReadVerbatim(Kind.TypeNullable, out var nullableTypeNode, '?');

                    bool isArray;

                    List<int> arrayArity = new List<int>();
                    if (isArray = Parser.TryReadVerbatim(Kind.StartArray, out var arrayStartNode, '['))
                    {
                        while (true)
                        {
                            int arity = -1;
                            if (Parser.TryReadWhile(c => Char.IsDigit((char)c), Kind.Literal, out var value))
                            {
                                arity = int.Parse(value.Value.Span);
                            }
                            arrayArity.Add(arity);
                            if (Parser.TryReadVerbatim(Kind.EndArray, out var arrayEndNode, ']'))
                            {
                                break;
                            }
                            if (Parser.TryReadVerbatim(Kind.ArraySeparator, out var arrayAritySeparatorNode, ','))
                            {
                                continue;
                            }
                            Parser.SyntaxError("Expected integer, ',' or ']'");
                            return null;
                        }
                    }

                    TypeShim typeResult;

                    ReadOnlySpan<char> nullSpan = stackalloc char[] { 'n', 'u', 'l', 'l' };
                    ReadOnlySpan<char> voidSpan = stackalloc char[] { 'v', 'o', 'i', 'd' };
                    ReadOnlySpan<char> neverSpan = stackalloc char[] { 'n', 'e', 'v', 'e', 'r' };
                    ReadOnlySpan<char> stringSpan = stackalloc char[] { 'n', 'e', 'v', 'e', 'r' };
                    ReadOnlySpan<char> charSpan = stackalloc char[] { 'c', 'h', 'a', 'r' };

                    ReadOnlySpan<char> i8Span = stackalloc char[] { 'i', '8' };
                    ReadOnlySpan<char> i16Span = stackalloc char[] { 'i', '1', '6' };
                    ReadOnlySpan<char> i32Span = stackalloc char[] { 'i', '3', '2' };
                    ReadOnlySpan<char> i64Span = stackalloc char[] { 'i', '6', '4' };

                    ReadOnlySpan<char> f32Span = stackalloc char[] { 'f', '3', '2' };
                    ReadOnlySpan<char> f64Span = stackalloc char[] { 'f', '6', '4' };


                    ReadOnlySpan<char> u16Span = stackalloc char[] { 'u', '1', '6' };
                    ReadOnlySpan<char> u32Span = stackalloc char[] { 'u', '3', '2' };
                    ReadOnlySpan<char> u64Span = stackalloc char[] { 'u', '6', '4' };

                    ReadOnlySpan<char> boolSpan = stackalloc char[] { 'b', 'o', 'o', 'l' };
                    ReadOnlySpan<char> thisSpan = stackalloc char[] { 't', 'h', 'i', 's' };

                    if (id.Value.Span.Equals(nullSpan, StringComparison.Ordinal))
                    {
                        if (isNullable)
                        {
                            Parser.SyntaxError("Cannot make null values nullable", id.Position);
                        }
                        typeResult = Types.Null;
                    }
                    else if (id.Value.Span.Equals(voidSpan, StringComparison.Ordinal))
                    {
                        if (isArray)
                        {
                            Parser.SyntaxError("Cannot make void arrays", id.Position);
                        }
                        if (isNullable)
                        {
                            Parser.SyntaxError("Void cannot be nullable", id.Position);
                        }
                        typeResult = Types.Void;
                    }
                    else if (id.Value.Span.Equals(neverSpan, StringComparison.Ordinal))
                    {
                        if (isArray)
                        {
                            Parser.SyntaxError("Cannot make arrays of never return types");
                        }
                        if (isNullable)
                        {
                            Parser.SyntaxError("Never cannot be nullable - it has not value");
                        }
                        typeResult = Types.Void;
                    }
                    else if (id.Value.Span.Equals(stringSpan, StringComparison.Ordinal))
                    {
                        typeResult = Types.GetTypeShim(typeof(string), isNullable);
                    }
                    else if (id.Value.Span.Equals(charSpan, StringComparison.Ordinal))
                    {
                        typeResult = Types.GetTypeShim(typeof(char), isNullable);
                    }
                    else if (id.Value.Span.Equals(i8Span, StringComparison.Ordinal))
                    {
                        typeResult = Types.GetTypeShim(typeof(byte), isNullable);
                    }
                    else if (id.Value.Span.Equals(i16Span, StringComparison.Ordinal))
                    {
                        typeResult = Types.GetTypeShim(typeof(short), isNullable);
                    }
                    else if (id.Value.Span.Equals(i32Span, StringComparison.Ordinal))
                    {
                        typeResult = Types.GetTypeShim(typeof(int), isNullable);
                    }
                    else if (id.Value.Span.Equals(i64Span, StringComparison.Ordinal))
                    {
                        typeResult = Types.GetTypeShim(typeof(long), isNullable);
                    }
                    else if (id.Value.Span.Equals(f32Span, StringComparison.Ordinal))
                    {
                        typeResult = Types.GetTypeShim(typeof(float), isNullable);
                    }
                    else if (id.Value.Span.Equals(f64Span, StringComparison.Ordinal))
                    {
                        typeResult = Types.GetTypeShim(typeof(double), isNullable);
                    }
                    else if (id.Value.Span.Equals(boolSpan, StringComparison.Ordinal))
                    {
                        typeResult = Types.GetTypeShim(typeof(bool), isNullable);
                    }
                    else if (id.Value.Span.Equals(thisSpan, StringComparison.Ordinal))
                    {
                        if (isNullable)
                        {
                            typeResult = Types.ThisNull;
                        }
                        else
                        {
                            typeResult = Types.This;
                        }
                    }
                    else
                    {
                        typeResult = Parser.GetUnresolvedType(id.Value.ToString(), isNullable);
                    }

                    if (!isArray)
                    {
                        stack.Merge();
                        return new DataType(typeResult);
                    }

                    var arrayIsNullable = Parser.TryReadVerbatim(Kind.ArrayNullable, out var arrayisNullableNode, '?');
                    stack.Merge();
                    return new DataType(new ArrayTypeShim(arrayArity.ToArray(), typeResult, arrayIsNullable));
                }
                else if (Parser.TryReadVerbatim(Kind.UnionStart, out var unionTypeStartNode, '<'))
                {
                    List<TypeShim> values = new List<TypeShim>();
                    do
                    {
                        var type = (DataType)Parser.Parse<DataType>();
                        if (type.Type == Types.Void)
                        {
                            Parser.SyntaxError("Void cannot be a member of a union.");
                        }
                        if (type == null)
                        {
                            break;
                        }
                        values.Add(type.Type);
                    } while (Parser.TryReadVerbatim(Kind.UnionSeparator, out var unionSepNode, '|'));
                    if (Parser.TryReadVerbatim(Kind.UnionEnd, out var unionEndNode, '>'))
                    {
                        bool isNullable = Parser.TryReadVerbatim(Kind.UnionNullable, out var unionNullableNode, '?');
                        Parser.Merge();
                        return new DataType(new UnionTypeShim(values.ToArray(), isNullable));
                    }
                    else
                    {
                        Parser.SyntaxError("Expected '>' or '|'");
                        return null;
                    }
                }
                else if (Parser.TryReadVerbatim(Kind.TupleStart, out var tupleStartNode, '('))
                {
                    var varDecl = new List<Variable>();
                    while (true)
                    {
                        var start = Parser.CurrentPosition;
                        var par = (Parameter)Parser.Parse<Parameter>();

                        if (par == null)
                        {
                            if (Parser.TryReadVerbatim(Kind.TupleEnd, out var tupleEndNode, ')'))
                            {
                                break;
                            }
                            Parser.CurrentPosition = start;
                            Parser.SyntaxError("Expected parameter");
                            break;
                        }
                        varDecl.Add(par);
                        if (Parser.TryReadVerbatim(Kind.TupleSeparator, out var tupleSepNode, ','))
                        {
                            continue;
                        }
                        else if (Parser.TryReadVerbatim(Kind.TupleEnd, out var tupleEndNode, ')'))
                        {
                            break;
                        }
                    }

                    bool isNullable = Parser.TryReadVerbatim(Kind.TypeNullable, out var nullableNode, '?');

                    if (Parser.TryReadVerbatim(Kind.TypeResult, out var resultTypeNode, "=>"))
                    {
                        var returnType = Parser.Parse<DataType>();

                        if (returnType == null)
                        {
                            Parser.SyntaxError("Expected type");
                            return null;
                        }
                        stack.Merge();
                        return new DataType(new FunctionTypeShim(returnType.Type, isNullable, varDecl.ToArray()));
                    }
                    else
                    {
                        stack.Merge();
                        return new DataType(new TupleTypeShim(isNullable, varDecl.ToArray()));

                    }
                }
                throw new NotImplementedException();
            }
        }
    }
}
