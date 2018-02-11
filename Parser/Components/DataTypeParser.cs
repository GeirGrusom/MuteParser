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
            Parser.Push();
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
                            arity = int.Parse(value.Value);
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
                        Parser.Pop();
                        return null;
                    }
                }

                TypeShim typeResult;

                switch (id.Value)
                {
                    case "null":
                        {
                            if(isNullable)
                            {
                                Parser.SyntaxError("Cannot make null values nullable", id.Position);
                            }
                            typeResult = Types.Null;
                            break;
                        }
                    case "void":
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
                            break;
                        }
                    case "never":
                        {
                            if(isArray)
                            {
                                Parser.SyntaxError("Cannot make arrays of never return types");
                            }
                            if(isNullable)
                            {
                                Parser.SyntaxError("Never cannot be nullable - it has not value");
                            }
                            typeResult = Types.Void;
                            break;
                        }
                    case "string":
                        {
                            typeResult = Types.GetTypeShim(typeof(string), isNullable);
                            break;
                        }
                    case "char":
                        {
                            typeResult = Types.GetTypeShim(typeof(char), isNullable);
                            break;
                        }
                    case "i8":
                        {
                            typeResult = Types.GetTypeShim(typeof(byte), isNullable);
                            break;
                        }
                    case "i16":
                        {
                            typeResult = Types.GetTypeShim(typeof(short), isNullable);
                            break;
                        }
                    case "i32":
                        {
                            typeResult = Types.GetTypeShim(typeof(int), isNullable);
                            break;
                        }
                    case "i64":
                        {
                            typeResult = Types.GetTypeShim(typeof(long), isNullable);
                            break;
                        }
                    case "f32":
                        {
                            typeResult = Types.GetTypeShim(typeof(float), isNullable);
                            break;
                        }
                    case "f64":
                        {
                            typeResult = Types.GetTypeShim(typeof(double), isNullable);
                            break;
                        }
                    case "bool":
                        {
                            typeResult = Types.GetTypeShim(typeof(bool), isNullable);
                            break;
                        }
                    case "this":
                        {
                            if (isNullable)
                            {
                                typeResult = Types.ThisNull;
                            }
                            else
                            {
                                typeResult = Types.This;
                            }
                            break;
                        }
                    default:
                        {
                            typeResult = Parser.GetUnresolvedType(id.Value, isNullable);
                            break;
                        }
                }

                if (!isArray)
                {
                    Parser.Merge();
                    return new DataType(typeResult);
                }

                var arrayIsNullable = Parser.TryReadVerbatim(Kind.ArrayNullable, out var arrayisNullableNode, '?');
                Parser.Merge();
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
                    Parser.Pop();
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
                        Parser.Pop();
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
                        Parser.Pop();
                        return null;
                    }
                    Parser.Merge();
                    return new DataType(new FunctionTypeShim(returnType.Type, isNullable, varDecl.ToArray()));
                }
                else
                {
                    Parser.Merge();
                    return new DataType(new TupleTypeShim(isNullable, varDecl.ToArray()));

                }
            }
            throw new NotImplementedException();
        }
    }
}
