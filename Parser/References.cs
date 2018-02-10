using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
namespace Parser
{
    public class References
    {
        private readonly Assembly[] assemblies;

        private readonly Namespace globalNamespace;

        public References(IEnumerable<Assembly> assemblies)
        {
            this.assemblies = assemblies.ToArray();
            globalNamespace = new Namespace("global");
            foreach(var assembly in assemblies)
            {
                var exportedTypes = assembly.GetExportedTypes().GroupBy(x => x.Namespace);

                foreach(var ns in exportedTypes)
                {
                    var @namespace = GetOrCreateNamespace(ns.Key);

                    foreach(var type in ns)
                    {
                        if(type.DeclaringType != null)
                        {
                            continue;
                        }
                        @namespace.Types.Add(type.Name, type);
                    }
                }
            }
        }

        private Namespace GetOrCreateNamespace(string @namespace)
        {
            var parts = @namespace.Split('.');
            Namespace current = globalNamespace;
            foreach(var part in parts)
            {
                var nextNamespace = current.TryGetNamespace(part);

                if(nextNamespace == null)
                {
                    nextNamespace = new Namespace(part);
                    current.Namespaces.Add(part, nextNamespace);
                    current = nextNamespace;
                }
            }
            return current;
        }

        private Namespace TryGetNamespace(IEnumerable<string> @namespace)
        {
            Namespace current = globalNamespace;

            foreach(var part in @namespace)
            {
                current = current.TryGetNamespace(part);
                if(current == null)
                {
                    return null;
                }
            }
            return current;
        }

        public Type TryFindType(IEnumerable<string> @namespace, string typeName)
        {
            return TryGetNamespace(@namespace)?.TryGetType(typeName);
        }

        private class Namespace
        {
            public Namespace(string name)
            {
                Name = name;
                Types = new Dictionary<string, Type>();
                Namespaces = new Dictionary<string, Namespace>();
            }

            public string Name { get; }

            internal Dictionary<string, Type> Types { get; }

            internal Dictionary<string, Namespace> Namespaces { get; }

            public Namespace TryGetNamespace(string @namespace)
            {
                Namespaces.TryGetValue(@namespace, out var result);
                return result;
            }

            public Type TryGetType(string name)
            {
                Types.TryGetValue(name, out var result);
                return result;
            }

        }
    }
}
