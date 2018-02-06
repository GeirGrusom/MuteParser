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
    }
}
