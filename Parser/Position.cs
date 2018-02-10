using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser
{
    public struct Position
    {
        public int Value;
        public int Column;
        public int Row;
        public bool EndOfFile;

        public override string ToString() => $"{Row}, {Column} ({Value})";

        public override int GetHashCode() => Value.GetHashCode();
    }

    public sealed class PositionComparer : IComparer<Position>
    {
        public static readonly PositionComparer Instance = new PositionComparer();
        public int Compare(Position x, Position y)
        {
            return x.Value.CompareTo(y.Value);
        }
    }
}
