using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser
{
    public sealed class SyntaxError
    {
        public SyntaxError(Position position, string message)
        {
            Position = position;
            Message = message;
        }

        public Position Position { get; }

        public string Message { get; }

        public override string ToString()
        {
            return $"{Position.Row}, {Position.Column}: {Message}";
        }
    }
}
