using System;
using System.Collections.Generic;
using System.Text;

namespace Parser.Expressions
{
    public static class ShadowingVariableExtensions
    {
        public static Variable GetVariableShadow(this Variable variable)
        {
            while(variable is ShadowingVariable shadowing)
            {
                variable = shadowing.Shadow;
            }
            return variable;
        }
    }
}
