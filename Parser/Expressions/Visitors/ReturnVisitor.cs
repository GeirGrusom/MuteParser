using System;
using System.Collections.Generic;
using System.Text;

namespace Parser.Expressions.Visitors
{
    public class ReturnVisitor : Visitor
    {
        public ReturnVisitor()
        {
            ReturnStatements = new List<Return>();
        }        

        public List<Return> ReturnStatements { get; }

        protected override Expression OnVisit(Return expression)
        {
            ReturnStatements.Add(expression);
            return base.OnVisit(expression);
        }

    }
}
