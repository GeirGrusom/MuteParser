using System.Collections.Generic;
using System.Linq;

namespace Parser
{
    using Expressions;
    using SyntaxTrivia;

    public sealed class Scope
    {
        public List<Variable> Variables { get; }
        public List<Method> Methods { get; }
        public List<Trivia> Trivia { get; }
        public List<Using> Using { get; }

        public IEnumerable<object> FindVariableTrivia(Variable variable)
        {
            return Trivia.OfType<VariableTrivia>().Where(t => t.Variable == variable);
        }

        public Scope(IEnumerable<Trivia> trivia)
        {
            Variables = new List<Variable>();
            Trivia = new List<Trivia>(trivia);
            Using = new List<Using>();
            Methods = new List<Method>();
        }

        public Scope(IEnumerable<Variable> variables)
        {
            Variables = new List<Variable>(variables);
            Trivia = new List<Trivia>();
            Using = new List<Using>();
            Methods = new List<Method>();
        }

        public Scope()
        {
            Variables = new List<Variable>();
            Trivia = new List<Trivia>();
            Using = new List<Using>();
            Methods = new List<Method>();
        }

        public void Add(Variable variable)
        {
            Variables.Add(variable);
        }

        public void Add(Trivia trivia)
        {
            Trivia.Add(trivia);
        }

        public void Add(Method method)
        {
            Methods.Add(method);
        }
    }
}
