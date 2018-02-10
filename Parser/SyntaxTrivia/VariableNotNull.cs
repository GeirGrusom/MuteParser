using Parser.Expressions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser.SyntaxTrivia
{
    public class VariableIsNotNullTrivia : VariableTrivia
    {
        public VariableIsNotNullTrivia(Variable variable)
            : base(variable)
        {
        }
    }
}
