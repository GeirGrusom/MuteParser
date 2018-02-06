using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser
{
    public sealed class TriviaCollection : List<object>
    {
        public bool Contains<T>() => this.OfType<T>().Any();
    }
}
