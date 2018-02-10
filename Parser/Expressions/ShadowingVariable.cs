using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser.Expressions
{
    public class ShadowingVariable : Variable
    {
        public Variable Shadow { get; }
        public ShadowingVariable(Variable shadow, TypeShim type) : base(shadow.Name, shadow.Mutable, type)
        {
            this.Shadow = shadow;
            while(this.Shadow is ShadowingVariable shadowing)
            {
                this.Shadow = shadowing.Shadow;
            }
        }
    }
}
