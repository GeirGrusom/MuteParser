using Parser.Components;
using Parser.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parser.Tests.Components
{
    public static class ComponentHelper
    {
        public static T CreateParser<T>(string code)
            where T : ParserComponent
        {
            var parser = new Parser(code);
            parser.PushScopeStack();
            var component = (T)Activator.CreateInstance(typeof(T), parser);
            return component;
        }

        public static T CreateParser<T>(string code, params Variable[] scopeVariables)
            where T : ParserComponent
        {
            var parser = new Parser(code);
            parser.PushScopeStack();
            foreach (var variable in scopeVariables)
            {
                parser.CurrentScope.Add(variable);
            }
            var component = (T)Activator.CreateInstance(typeof(T), parser);
            return component;
        }

    }
}
