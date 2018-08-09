using System;
using System.Collections.Generic;
using System.Text;

namespace Parser.SyntaxNodes
{

    public enum Kind
    {
        WhiteSpace,
        Comment,
        Identifier,
        Keyword,
        Operator,
        Literal,
        Punctuation,
        StartBrace,
        EndBrace,
        StartArray,
        ArraySeparator,
        EndArray,
        TypeNullable,
        ArrayNullable,
        TupleStart,
        TupleSeparator,
        TupleEnd,
        TupleNullable,
        UnionStart,
        UnionSeparator,
        UnionEnd,
        UnionNullable,
        TypeResult,
        MemberSeparator,
        MethodArgumentsStart,
        MethodArgumentsSeparator,
        MethodArgumentsEnd,
        VariableType,
        CallStart,
        CallSeparator,
        CallEnd,
        StringContents,
        StringEnd,
        StringStart,
    }

    public class SyntaxNode
    {
        public SyntaxNode(ReadOnlyMemory<char> value, Kind kind, Position position)
        {
            Value = value;
            Position = position;
            Kind = kind;
        }

        public ReadOnlyMemory<char> Value { get; }

        public Position Position { get; }

        public Kind Kind { get; }

        public override string ToString() => Value.ToString();

        public override int GetHashCode()
        {
            return Position.GetHashCode();
        }
    }
}
