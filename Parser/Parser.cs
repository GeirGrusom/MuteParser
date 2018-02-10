

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
        private readonly List<int> uncomittedBuffer;
        private readonly Stream source;
        private readonly StreamReader reader;

        private readonly Dictionary<Type, Components.ParserComponent> parsers;

        private RefStack<Position> positionStack;
        private Position currentPosition;

        private List<Scope> scopeStack;

        private readonly Dictionary<(string, bool), UnresolvedTypeShim> unresolvedTypes;

        public List<SyntaxError> SyntaxErrors { get; }

        public Position CurrentPosition { get => currentPosition; set => currentPosition = value; }

        public Parser(string source)
            : this(new References(new Assembly[0]), new MemoryStream(Encoding.UTF8.GetBytes(source)))
        {
        }

        private List<SyntaxNodes.SyntaxNode> syntaxNodes;

        public int ScopeStackCount => scopeStack.Count;

        public int StackCount => positionStack.Count;
        public Parser(References externalReferences, Stream source)
        {
            syntaxNodes = new List<SyntaxNodes.SyntaxNode>();
            this.parsers = new Dictionary<Type, Components.ParserComponent>();
            SetupParsers();
            this.source = source;
            this.uncomittedBuffer = new List<int>();
            SyntaxErrors = new List<SyntaxError>();
            positionStack = new RefStack<Position>();
            reader = new StreamReader(source);
            scopeStack = new List<Scope>();
            unresolvedTypes = new Dictionary<(string, bool), UnresolvedTypeShim>();
        }

        private void AddSyntaxNode(SyntaxNodes.SyntaxNode node)
        {
            syntaxNodes.Add(node);
        }

        private void SetupParsers()
        {
            AddParser<If>(new Components.IfParser(this));
            AddParser<Block>(new Components.BlockParser(this));
            AddParser<Binary>(new Components.BinaryParser(this));
            AddParser<Add>(new Components.AddParser(this));
            AddParser<Assign>(new Components.AssignParser(this));
            AddParser<Multiply>(new Components.MultiplyParser(this));
            AddParser<Equal>(new Components.EqualParser(this));
            AddParser<Constant>(new Components.ConstantParser(this));
            AddParser<Method>(new Components.MethodParser(this));
            AddParser<VariableDeclaration>(new Components.VariableDeclarationParser(this));
            AddParser<Using>(new Components.UsingParser(this));
            AddParser<Member>(new Components.MemberParser(this));
            AddParser<CompilationUnit>(new Components.CompilationUnitParser(this));
            AddParser<Parameter>(new Components.ParameterParser(this));
            AddParser<DataType>(new Components.DataTypeParser(this));
        }

        private void AddParser<TType>(Components.ParserComponent component)
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
            var realVariable = Variable.GetShadowedVariable(variable);
            for (int i = scopeStack.Count - 1; i >= 0; --i)
            {
                for (int j = scopeStack[i].Trivia.Count - 1; j >= 0; --j)
                {
                    if (scopeStack[i].Trivia[j] is VariableTrivia varTrivia && Variable.GetShadowedVariable(varTrivia.Variable) == realVariable)
                    {
                        yield return varTrivia;
                    }
                }
            }
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

        public bool TryReadWhile(Func<int, bool> condition, Kind kind, out SyntaxNode value)
        {
            SkipWhiteSpaceAndComments();
            var start = currentPosition;
            var pos = currentPosition;
            Push();

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
                        Pop();
                        value = null;
                        return false;
                    }
                    StringBuilder result = new StringBuilder();
                    for (int i = start.Value; i < pos.Value; ++i)
                    {
                        result.Append(Encoding.UTF32.GetChars(BitConverter.GetBytes(uncomittedBuffer[i])));
                    }
                    value = new SyntaxNode(result.ToString(), kind, start);
                    AddSyntaxNode(value);
                    Merge();
                    currentPosition = pos;
                    return true;
                }
            } while (true);
        }

        public bool TryReadIdentifier(out SyntaxNode identifier)
        {
            SkipWhiteSpaceAndComments();
            var start = currentPosition;
            var pos = currentPosition;
            Push();

            var c = ReadChar();
            if (!char.IsLetter((char)c) && c != '_')
            {
                Pop();
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
                    StringBuilder result = new StringBuilder();
                    for (int i = start.Value; i < pos.Value; ++i)
                    {
                        result.Append(Encoding.UTF32.GetChars(BitConverter.GetBytes(uncomittedBuffer[i])));
                    }
                    identifier = new SyntaxNode(result.ToString(), Kind.Identifier, start);
                    AddSyntaxNode(identifier);
                    Merge();
                    currentPosition = pos;
                    return true;
                }
            } while (true);
        }

        public IEnumerable<SyntaxNode> GetSyntaxNodes()
        {
            return syntaxNodes;
        }

        private void SkipWhiteSpaceAndComments()
        {
            Position pos = CurrentPosition;
            StringBuilder builder = new StringBuilder();
            Position startPos = pos;

            void Emit(Kind kind)
            {
                if(builder.Length == 0)
                {
                    return;
                }
                AddSyntaxNode(new SyntaxNode(builder.ToString(), kind, pos));
                builder.Clear();
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
                            builder.Append("/*");                            
                            continue;
                        }
                        else if (ch == '/')
                        {
                            while (ch != '\n')
                            {
                                ch = ReadChar();
                            }
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
                        builder.Append((char)ch);
                        continue;
                    }
                    else
                    {
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
                            builder.Append("*/");
                            Emit(Kind.Comment);
                            lastPos = CurrentPosition;
                            
                            continue;
                        }
                    }
                    else
                    {
                        builder.Append((char)ch);
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
                node = new SyntaxNode(((char)c).ToString(), kind, pos);
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
            Position pos = CurrentPosition;
            Push();
            int count = 0;
            do
            {
                var c = ReadChar();
                if (c != value[count])
                {
                    Pop();
                    node = null;
                    return false;
                }
                ++count;
            } while (count < value.Length);
            Merge();
            node = new SyntaxNode(value.ToString(), kind, pos);
            AddSyntaxNode(node);
            return true;
        }

        public bool TryReadVerbatim(Kind kind, out SyntaxNode node, params char[] values)
        {
            SkipWhiteSpaceAndComments();
            var pos = currentPosition;
            var c = (char)ReadChar();
            if (values.Contains(c))
            {
                node = new SyntaxNode(((char)c).ToString(), kind, pos);
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


        public void Push()
        {
            positionStack.Push(currentPosition);
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

        private int ReadChar()
        {
            if (currentPosition.Value >= uncomittedBuffer.Count)
            {
                if (uncomittedBuffer.Count > 0 && uncomittedBuffer[uncomittedBuffer.Count - 1] == -1)
                {
                    return -1;
                }
                uncomittedBuffer.Add(reader.Read());
            }

            var result = uncomittedBuffer[currentPosition.Value];

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
