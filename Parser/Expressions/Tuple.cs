using System.Linq;

namespace Parser.Expressions
{
    public class Tuple : Expression
    {
        public Expression[] Members { get; }
        public Tuple(params Expression[] members)
            : base(Types.GetTupleTypeShim(members.Select(x => x.Type).ToArray()))
        {
            this.Members = members;
        }

        public override string ToString()
        {
            return '(' + string.Join(", ", Members.Select(x => x.ToString())) + ')';
        }
    }
}
