

namespace Parser
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Reflection;

    using Expressions;
    using SyntaxTrivia;
    using SyntaxNodes;

    public class Parser
    {
        private readonly string source;

        private readonly Dictionary<Type, Components.ParserComponent> parsers;

        private RefStack<Position> positionStack;
        private Position currentPosition;

        private List<Scope> scopeStack;

        private readonly Dictionary<(string, bool), UnresolvedTypeShim> unresolvedTypes;

        public List<SyntaxError> SyntaxErrors { get; }

        public string Source => source;

        public Position CurrentPosition { get => currentPosition; set => currentPosition = value; }

        public Parser(string source)
            : this(new References(new Assembly[0]), source)
        {
        }

        private List<SyntaxNodes.SyntaxNode> syntaxNodes;

        public int ScopeStackCount => scopeStack.Count;

        public int StackCount => positionStack.Count;
        public Parser(References externalReferences, string source)
        {
            syntaxNodes = new List<SyntaxNodes.SyntaxNode>();
            this.parsers = new Dictionary<Type, Components.ParserComponent>();
            SetupParsers();
            this.source = source;
            SyntaxErrors = new List<SyntaxError>();
            positionStack = new RefStack<Position>();
            scopeStack = new List<Scope>();
            unresolvedTypes = new Dictionary<(string, bool), UnresolvedTypeShim>();
        }

        public void AddSyntaxNode(SyntaxNode node)
        {
            syntaxNodes.Add(node);
        }

        private void SetupParsers()
        {
            AddParser(new Components.IfParser(this));
            AddParser(new Components.BlockParser(this));
            AddParser(new Components.BinaryParser(this));
            AddParser(new Components.AddParser(this));
            AddParser(new Components.AssignParser(this));
            AddParser(new Components.MultiplyParser(this));
            AddParser(new Components.EqualParser(this));
            AddParser(new Components.ConstantParser(this));
            AddParser(new Components.MethodParser(this));
            AddParser(new Components.VariableDeclarationParser(this));
            AddParser(new Components.UsingParser(this));
            AddParser(new Components.MemberParser(this));
            AddParser(new Components.CompilationUnitParser(this));
            AddParser(new Components.ParameterParser(this));
            AddParser(new Components.DataTypeParser(this));
            AddParser(new Components.CallParser(this));
            AddParser(new Components.UnaryParser(this));
            AddParser(new Components.ReturnParser(this));
            AddParser(new Components.LogicalParser(this));
            AddParser(new Components.OrElseParser(this));
            AddParser(new Components.AndAlsoParser(this));
        }

        private void AddParser<TType>(Components.ParserComponent<TType> component)
            where TType : Expression
        {
            parsers.Add(typeof(TType), component);
        }

        public TypeShim GetUnresolvedType(string name, bool nullable)
        {
            if (unresolvedTypes.TryGetValue((name, nullable), out var result))
            {
                return result;
            }

            result = new UnresolvedTypeShim(name, nullable);
            unresolvedTypes.Add((name, nullable), result);
            return result;
        }

        [System.Diagnostics.DebuggerHidden]
        [System.Diagnostics.DebuggerStepThrough]
        public Expression Parse<T>() where T : Expression
        {
            if (!parsers.TryGetValue(typeof(T), out var parser))
            {
                throw new NotImplementedException($"There is no parser defined for {typeof(T)}");
            }
            return parser.Parse();
        }

        [System.Diagnostics.DebuggerHidden]
        [System.Diagnostics.DebuggerStepThrough]
        public Expression Parse<T>(Scope initialScope) where T : Expression
        {
            if (!parsers.TryGetValue(typeof(T), out var parser))
            {
                throw new NotImplementedException($"There is no parser defined for {typeof(T)}");
            }
            return parser.Parse(initialScope);
        }

        public Variable Shadow(Variable variable, TypeShim type)
        {
            var result = new ShadowingVariable(variable, type);
            CurrentScope.Variables.Add(result);
            return result;
        }

        public Expression Parse()
        {
            positionStack.Clear();
            currentPosition = new Position();
            currentPosition.Row = 1;
            currentPosition.Column = 1;
            return Parse<CompilationUnit>();
        }

        public void SyntaxError(string message)
        {
            SyntaxErrors.Add(new SyntaxError(currentPosition, message));
        }

        public void SyntaxError(string message, Position position)
        {
            SyntaxErrors.Add(new SyntaxError(position, message));
        }

        public void PushScopeStack(Scope scope = null)
        {
            scopeStack.Add(scope ?? new Scope());
        }

        public Scope PopScopeStack()
        {
            var result = scopeStack[scopeStack.Count - 1];
            scopeStack.Remove(result);
            return result;
        }

        public IEnumerable<VariableTrivia> GetVariableTrivia(Variable variable)
        {
            var realVariable = variable.GetVariableShadow();
            for (int i = scopeStack.Count - 1; i >= 0; --i)
            {
                for (int j = scopeStack[i].Trivia.Count - 1; j >= 0; --j)
                {
                    if (scopeStack[i].Trivia[j] is VariableTrivia varTrivia && varTrivia.Variable.GetVariableShadow() == realVariable)
                    {
                        yield return varTrivia;
                    }
                }
            }
        }

        public Method FindMethod(string name)
        {
            for (int i = scopeStack.Count - 1; i >= 0; --i)
            {
                var scope = scopeStack[i];
                for (int j = scope.Methods.Count - 1; j >= 0; --j)
                {
                    if (string.Equals(scope.Methods[j].Name, name, StringComparison.Ordinal))
                    {
                        return scope.Methods[j];
                    }
                }
            }
            return null;
        }

        public Variable FindVariable(string name)
        {
            for (int i = scopeStack.Count - 1; i >= 0; --i)
            {
                var scope = scopeStack[i];
                for (int j = scope.Variables.Count - 1; j >= 0; --j)
                {
                    if (string.Equals(scope.Variables[j].Name, name, StringComparison.Ordinal))
                    {
                        return scope.Variables[j];
                    }
                }
            }
            return null;
        }

        public Scope CurrentScope => scopeStack[scopeStack.Count - 1];

        public bool TryReadWhile(Func<int, bool> condition, Kind kind, out SyntaxNode value, bool skipWhitespace = true)
        {
            SkipWhiteSpaceAndComments();
            var start = currentPosition;
            var pos = currentPosition;
            using (var stack = Push())
            {

                do
                {
                    pos = currentPosition;
                    var c = ReadChar();
                    if (condition(c))
                    {
                        continue;
                    }
                    else
                    {
                        if (pos.Value == start.Value)
                        {
                            value = null;
                            return false;
                        }
                        
                        value = new SyntaxNode(source.AsMemory(start.Value, pos.Value - start.Value), kind, start);
                        AddSyntaxNode(value);
                        stack.Merge();
                        currentPosition = pos;
                        return true;
                    }
                } while (true);
            }
        }

        public bool TryReadInteger(out SyntaxNode value)
        {
            return TryReadWhile(c => Char.IsDigit((char)c), Kind.Literal, out value);
        }

        public bool TryReadHexadecimal(out SyntaxNode value)
        {
            SkipWhiteSpaceAndComments();
            var start = currentPosition;
            var c = ReadChar();
            if (c != '0' || ReadChar() != 'x')
            {
                currentPosition = start;
                value = null;
                return false;
            }

            var pos = currentPosition;
            using (var stack = Push())
            {
                do
                {
                    pos = currentPosition;
                    c = ReadChar();
                    if (Char.IsDigit((char)c) || c >= 'a' && c <= 'f' || c >= 'A' && c <= 'F')
                    {
                        continue;
                    }
                    else
                    {
                        if (pos.Value == start.Value)
                        {
                            value = null;
                            return false;
                        }

                        value = new SyntaxNode(source.AsMemory(start.Value, pos.Value - start.Value), Kind.Literal, start);
                        AddSyntaxNode(value);
                        stack.Merge();
                        currentPosition = pos;
                        return true;
                    }
                } while (true);
            }
        }

        public bool TryReadIdentifier(out SyntaxNode identifier)
        {
            SkipWhiteSpaceAndComments();
            var start = currentPosition;
            var pos = currentPosition;
            using (var stack = Push())
            {

                var c = ReadChar();
                if (!char.IsLetter((char)c) && c != '_')
                {
                    identifier = null;
                    return false;
                }
                do
                {
                    pos = currentPosition;
                    c = ReadChar();
                    if (char.IsLetterOrDigit((char)c) || c == '_')
                    {
                        continue;
                    }
                    else
                    {
                        identifier = new SyntaxNode(source.AsMemory(start.Value, pos.Value - start.Value), Kind.Identifier, start);
                        AddSyntaxNode(identifier);
                        stack.Merge();
                        currentPosition = pos;
                        return true;
                    }
                } while (true);
            }
        }

        public IEnumerable<SyntaxNode> GetSyntaxNodes()
        {
            return syntaxNodes;
        }

        private void SkipWhiteSpaceAndComments()
        {
            Position pos = currentPosition;
            Position startPos = currentPosition;

            void Emit(Kind kind)
            {
                if(startPos.Value == pos.Value)
                {
                    return;
                }

                AddSyntaxNode(new SyntaxNode(source.AsMemory(startPos.Value, pos.Value - startPos.Value), kind, pos));
            }

            bool isInComment = false;
            while (true)
            {
                var lastPos = currentPosition;
                var ch = ReadChar();
                if (!isInComment)
                {
                    if (ch == '/')
                    {
                        ch = ReadChar();
                        if (ch == '*')
                        {
                            isInComment = true;
                            Emit(Kind.WhiteSpace);
                            startPos = lastPos;
                            pos = currentPosition;
                            continue;
                        }
                        else if (ch == '/')
                        {
                            do
                            {
                                ch = ReadChar();
                            } while (ch != '\n');

                            pos = currentPosition;
                            Emit(Kind.WhiteSpace);
                            return;
                        }
                        else
                        {
                            Emit(Kind.WhiteSpace);
                            currentPosition = lastPos;
                            return;
                        }
                    }
                    else if (char.IsWhiteSpace((char)ch))
                    {
                        continue;
                    }
                    else
                    {
                        pos = lastPos;
                        Emit(Kind.WhiteSpace);
                        currentPosition = lastPos;
                        return;
                    }
                }
                else
                {
                    if (ch == '*')
                    {
                        ch = ReadChar();
                        if (ch == '/')
                        {
                            isInComment = false;
                            pos = currentPosition;
                            Emit(Kind.Comment);
                            lastPos = CurrentPosition;
                            
                            continue;
                        }
                    }
                }
            }
        }

        public bool TryReadVerbatim(Kind kind, out SyntaxNode node, char value)
        {
            SkipWhiteSpaceAndComments();
            var pos = currentPosition;
            var c = ReadChar();
            if (value == c)
            {
                node = new SyntaxNode(source.AsMemory(pos.Value, 1), kind, pos);
                AddSyntaxNode(node);
                return true;
            }
            currentPosition = pos;
            node = null;
            return false;
        }

        public bool TryReadVerbatim(Kind kind, out SyntaxNode node, char v1, char v2)
        {
            SkipWhiteSpaceAndComments();
            var pos = currentPosition;
            var c = ReadChar();
            if (v1 == c || v2 == c)
            {
                node = new SyntaxNode(source.AsMemory(pos.Value, 1), kind, pos);
                AddSyntaxNode(node);
                return true;
            }
            currentPosition = pos;
            node = null;
            return false;
        }

        public bool TryReadVerbatim(Kind kind, out SyntaxNode node, char v1, char v2, char v3)
        {
            SkipWhiteSpaceAndComments();
            var pos = currentPosition;
            var c = ReadChar();
            if (v1 == c || v2 == c || v3 == c)
            {
                node = new SyntaxNode(source.AsMemory(pos.Value, 1), kind, pos);
                AddSyntaxNode(node);
                return true;
            }
            currentPosition = pos;
            node = null;
            return false;
        }

        public bool TryReadVerbatim(Kind kind, out SyntaxNode node, char v1, char v2, char v3, char v4)
        {
            SkipWhiteSpaceAndComments();
            var pos = currentPosition;
            var c = ReadChar();
            if (v1 == c || v2 == c || v3 == c || v4 == c)
            {
                node = new SyntaxNode(source.AsMemory(pos.Value, 1), kind, pos);
                AddSyntaxNode(node);
                return true;
            }
            currentPosition = pos;
            node = null;
            return false;
        }

        public bool TryReadVerbatim(Kind kind, out SyntaxNode node, string value)
        {
            SkipWhiteSpaceAndComments();
            var start = CurrentPosition;
            Position pos = CurrentPosition;
            using (var stack = Push())
            {
                int count = 0;
                do
                {
                    var c = ReadChar();
                    if (c != value[count])
                    {
                        node = null;
                        return false;
                    }
                    ++count;
                } while (count < value.Length);
                stack.Merge();
                node = new SyntaxNode(source.AsMemory(start.Value, value.Length), kind, pos);
                AddSyntaxNode(node);
                return true;
            }
        }

        public bool TryReadVerbatim(Kind kind, out SyntaxNode node, params char[] values)
        {
            SkipWhiteSpaceAndComments();
            var pos = currentPosition;
            var c = (char)ReadChar();
            if (values.Contains(c))
            {
                node = new SyntaxNode(source.AsMemory(pos.Value, 1), kind, pos);
                AddSyntaxNode(node);
                return true;
            }
            currentPosition = pos;
            node = null;
            return false;
        }

        public bool TryReadVerbatim(Kind kind, out SyntaxNode node, params string[] value)
        {
            for (int i = 0; i < value.Length; ++i)
            {
                if (TryReadVerbatim(kind, out node, value[i]))
                {
                    return true;
                }
            }
            node = null;
            return false;
        }

        public struct DisposableScope : IDisposable
        {
            private readonly Parser parser;
            private bool merged;

            public DisposableScope(Parser parser)
            {
                this.parser = parser;
                this.merged = false;
            }


            public void Merge()
            {
                if(this.merged)
                {
                    throw new InvalidOperationException("This scope has already been merged.");
                }
                this.parser.Merge();
                this.merged = true;
            }
            public void Dispose()
            {
                if (this.merged)
                {
                    return;
                }
                this.parser.Pop();
                this.merged = true;
            }
        }

        public DisposableScope Push()
        {
            positionStack.Push(currentPosition);
            return new DisposableScope(this);
        }

        public void Merge()
        {
            positionStack.Pop();
        }

        public void Pop()
        {
            currentPosition = positionStack.Pop();
            for (int i = syntaxNodes.Count - 1; i >= 0; --i)
            {
                if (syntaxNodes[i].Position.Value >= currentPosition.Value)
                {
                    syntaxNodes.RemoveAt(i);
                }
                else
                {
                    break;
                }
            }
        }

        public void Rewind()
        {
            currentPosition = positionStack.Peek();
            for (int i = syntaxNodes.Count - 1; i >= 0; ++i)
            {
                if (syntaxNodes[i].Position.Value >= currentPosition.Value)
                {
                    syntaxNodes.RemoveAt(i);
                }
                else
                {
                    break;
                }
            }
        }

        public int ReadChar()
        {
            if (currentPosition.Value >= source.Length)
            {
                return -1;
            }

            var result = source[currentPosition.Value];

            if (result == '\n')
            {
                ++currentPosition.Row;
                currentPosition.Column = 1;
            }
            else
            {
                ++currentPosition.Column;
            }

            ++currentPosition.Value;

            if (result == -1)
            {
                currentPosition.EndOfFile = true;
                return -1;
            }

            return result;
        }
    }
}
