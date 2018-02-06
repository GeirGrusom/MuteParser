using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Parser
{
    using Expressions;
    using System.Reflection;

    public sealed class Scope
    {
        public List<Variable> Variables { get; }
        public List<object> Trivia { get; }

        public IEnumerable<object> FindVariableTrivia(Variable variable)
        {
            return Trivia.OfType<VariableTrivia>().Where(t => t.Variable == variable);
        }

        public Scope(IEnumerable<object> trivia)
        {
            Variables = new List<Variable>();
            Trivia = new List<object>(trivia);
        }

        public Scope(IEnumerable<Variable> variables)
        {
            Variables = new List<Variable>(variables);
            Trivia = new List<object>();
        }

        public Scope()
        {
            Variables = new List<Variable>();
            Trivia = new List<object>();
        }

        public void Add(Variable variable)
        {
            Variables.Add(variable);
        }

        public void Add(object trivia)
        {
            Trivia.Add(trivia);
        }
    }

    public class Parser
    {
        private readonly List<int> uncomittedBuffer;
        private readonly Stream source;
        private readonly StreamReader reader;

        private RefStack<Position> positionStack;
        private Position currentPosition;

        private List<Scope> scopeStack;

        private readonly Dictionary<(string, bool), UnresolvedTypeShim> unresolvedTypes;

        public List<SyntaxError> SyntaxErrors { get; }

        public Parser(Stream source)
        {
            this.source = source;
            this.uncomittedBuffer = new List<int>();
            SyntaxErrors = new List<SyntaxError>();
            positionStack = new RefStack<Position>();
            reader = new StreamReader(source);
            scopeStack = new List<Scope>();
            unresolvedTypes = new Dictionary<(string, bool), UnresolvedTypeShim>();
        }

        private TypeShim GetUnresolvedType(string name, bool nullable)
        {
            if (unresolvedTypes.TryGetValue((name, nullable), out var result))
            {
                return result;
            }

            result = new UnresolvedTypeShim(name, nullable);
            unresolvedTypes.Add((name, nullable), result);
            return result;
        }

        public Expression Parse()
        {
            positionStack.Clear();
            currentPosition = new Position();
            currentPosition.Row = 1;
            currentPosition.Column = 1;
            return ParseCompilationUnit();
        }

        public void SyntaxError(string message)
        {
            SyntaxErrors.Add(new SyntaxError(currentPosition, message));
        }

        public void SyntaxError(string message, Position position)
        {
            SyntaxErrors.Add(new SyntaxError(position, message));
        }

        public bool ReadModuleName(out string result)
        {
            SkipWhiteSpaceAndComments();
            if (!TryReadKeyword("module"))
            {
                SyntaxError("Expected 'module'");
                result = null;
                return false;
            }
            SkipWhiteSpaceAndComments();
            if (!TryReadIdentifier(out var id))
            {
                Pop();
                SyntaxError("Expected identifier");
                result = null;
                return false;
            }

            result = id;
            return true;
        }

        private void PushScopeStack(Scope scope = null)
        {
            scopeStack.Add(scope ?? new Scope());
        }

        private Scope PopScopeStack()
        {
            var result = scopeStack[scopeStack.Count - 1];
            scopeStack.Remove(result);
            return result;
        }

        public CompilationUnit ParseCompilationUnit()
        {
            Push();
            PushScopeStack();
            if (!ReadModuleName(out var moduleName))
            {
                return null;
            }


            List<Expression> statements = new List<Expression>();
            while (true)
            {
                SkipWhiteSpaceAndComments();
                var nextStatement = ParseStatement();
                if (nextStatement == null)
                {
                    break;
                }
                statements.Add(nextStatement);
            }

            Merge();

            return new CompilationUnit(moduleName, PopScopeStack(), statements);
        }

        public Expression ParseStatement()
        {
            return ParseVariableDefinition() ?? ParseAssignment() ?? ParseIfStatement() ?? ParseMethod();
        }

        public Expression ParseDefinition()
        {
            return ParseMethod();
        }

        public Expression ParseIfStatement()
        {
            Push();
            PushScopeStack();
            if (!TryReadKeyword("if"))
            {
                PopScopeStack();
                Pop();
                return null;
            }
            var condition = ParseParensExpression();

            if (condition == null)
            {
                PopScopeStack();
                Pop();
                SyntaxError("Expected expression");
                return null;
            }

            if (condition.Type != Types.Bool && !(condition.Type is UnresolvedTypeShim))
            {
                SyntaxError("If condition must be a boolean expression");
            }

            List<Variable> triviaShadowing = new List<Variable>();
            List<Variable> falseTriviaShadowing = new List<Variable>();
            foreach (var trivia in condition.Trivia)
            {
                if (trivia is VariableIsNotNullTrivia notNull)
                {
                    triviaShadowing.Add(new ShadowingVariable(notNull.Variable, Types.MakeNonNull(notNull.Variable.Type)));
                    falseTriviaShadowing.Add(new ShadowingVariable(notNull.Variable, Types.Null));
                }
                else if (trivia is VariableIsNullTrivia isNull)
                {
                    triviaShadowing.Add(new ShadowingVariable(isNull.Variable, Types.Null));
                    falseTriviaShadowing.Add(new ShadowingVariable(isNull.Variable, Types.MakeNonNull(isNull.Variable.Type)));
                    
                }
                else if(trivia is VariableIsTypeTrivia isTypeTrivia)
                {
                    triviaShadowing.Add(new ShadowingVariable(isTypeTrivia.Variable, isTypeTrivia.Type));
                }
            }

            var @true = ParseBlock(new Scope(triviaShadowing));

            if (@true == null)
            {
                PopScopeStack();
                Pop();
                SyntaxError("Expected block expression");
                return null;
            }
            Expression @false = null;
            if(TryReadKeyword("else"))
            {
                @false = ParseBlock(new Scope(falseTriviaShadowing));
            }

            Merge();
            return new If(condition, @true, @false, PopScopeStack());
        }

        public Expression ParseBlock(Scope initialScope = null)
        {
            Push();
            PushScopeStack(initialScope);

            if (!TryReadKeyword('{'))
            {
                PopScopeStack();
                Pop();
                return null;
            }
            
            List<Expression> statements = new List<Expression>();
            while (true)
            {
                SkipWhiteSpaceAndComments();
                var nextStatement = ParseStatement();
                if (nextStatement == null)
                {
                    break;
                }
                statements.Add(nextStatement);
            }

            if (!TryReadKeyword('}'))
            {
                PopScopeStack();
                SyntaxError("Expected '}'");
                Pop();
                return null;
            }

            Merge();
            var result = new Block(PopScopeStack(), statements.ToArray());
            foreach(var exp in statements)
            {
                result.Trivia.AddRange(exp.Trivia);
            }
            return result;

        }

        public Expression ParseParensExpression()
        {
            Push();
            if (TryReadKeyword('('))
            {
                var result = ParseExpression();

                if (!TryReadKeyword(')'))
                {
                    SyntaxError("Expected ')'");
                    Pop();
                    return null;
                }
                Merge();
                return result;
            }
            else
            {
                Pop();
                return null;
            }
        }

        public TypeShim ParseType()
        {
            SkipWhiteSpaceAndComments();
            Push();
            if (TryReadIdentifier(out var id))
            {
                bool isNullable = TryReadKeyword('?');

                bool isArray;

                List<int> arrayArity = new List<int>();
                if (isArray = TryReadKeyword('['))
                {
                    while (true)
                    {
                        int arity = -1;
                        if (TryReadWhile(c => Char.IsDigit((char)c), out var value))
                        {
                            arity = int.Parse(value);
                        }
                        arrayArity.Add(arity);
                        if (TryReadKeyword(']'))
                        {
                            break;
                        }
                        if (TryReadKeyword(','))
                        {
                            continue;
                        }
                        SyntaxError("Expected integer, ',' or ']'");
                        Pop();
                        return null;
                    }
                }

                TypeShim typeResult;

                switch (id)
                {
                    case "void":
                        {
                            if (isArray)
                            {
                                SyntaxError("Cannot make void arrays");
                            }
                            if (isNullable)
                            {
                                SyntaxError("Void cannot be nullable");
                                return null;
                            }
                            Merge();
                            return Types.Void;
                        }
                    case "string":
                        {
                            typeResult = Types.GetTypeShim(typeof(string), isNullable);
                            break;
                        }
                    case "char":
                        {
                            typeResult = Types.GetTypeShim(typeof(char), isNullable);
                            break;
                        }
                    case "i8":
                        {
                            typeResult = Types.GetTypeShim(typeof(byte), isNullable);
                            break;
                        }
                    case "i16":
                        {
                            typeResult = Types.GetTypeShim(typeof(short), isNullable);
                            break;
                        }
                    case "i32":
                        {
                            typeResult = Types.GetTypeShim(typeof(int), isNullable);
                            break;
                        }
                    case "i64":
                        {
                            typeResult = Types.GetTypeShim(typeof(long), isNullable);
                            break;
                        }
                    case "f32":
                        {
                            typeResult = Types.GetTypeShim(typeof(float), isNullable);
                            break;
                        }
                    case "f64":
                        {
                            typeResult = Types.GetTypeShim(typeof(double), isNullable);
                            break;
                        }
                    case "bool":
                        {
                            typeResult = Types.GetTypeShim(typeof(bool), isNullable);
                            break;
                        }
                    case "this":
                        {
                            if (isNullable)
                            {
                                typeResult = Types.ThisNull;
                            }
                            else
                            {
                                typeResult = Types.This;
                            }
                            break;
                        }
                    default:
                        {
                            typeResult = GetUnresolvedType(id, isNullable);
                            break;
                        }
                }

                if (!isArray)
                {
                    return typeResult;
                }

                var arrayIsNullable = TryReadKeyword('?');

                return new ArrayTypeShim(arrayArity.ToArray(), typeResult, arrayIsNullable);
            }
            else if (TryReadKeyword('<'))
            {
                List<TypeShim> values = new List<TypeShim>();
                do
                {
                    SkipWhiteSpaceAndComments();
                    var type = ParseType();
                    if (type == Types.Void)
                    {
                        SyntaxError("Void cannot be a member of a union.");
                    }
                    if (type == null)
                    {
                        break;
                    }
                    values.Add(type);
                    SkipWhiteSpaceAndComments();
                } while (TryReadKeyword('|'));
                if (TryReadKeyword('>'))
                {
                    SkipWhiteSpaceAndComments();
                    bool isNullable = TryReadKeyword('?');
                    Merge();
                    return new UnionTypeShim(values.ToArray(), isNullable);
                }
                else
                {
                    Pop();
                    SyntaxError("Expected '>' or '|'");
                    return null;
                }
            }
            else if (TryReadKeyword('('))
            {
                var varDecl = new List<Variable>();
                while (true)
                {
                    var start = currentPosition;
                    var par = ParseParameter();
                    SkipWhiteSpaceAndComments();
                    if (par == null)
                    {
                        if (TryReadKeyword(")"))
                        {
                            break;
                        }
                        currentPosition = start;
                        SyntaxError("Expected parameter");
                        Pop();
                        break;
                    }
                    varDecl.Add(par);
                    if (TryReadKeyword(","))
                    {
                        continue;
                    }
                    else if (TryReadKeyword(')'))
                    {
                        break;
                    }
                }

                bool isNullable = TryReadKeyword('?');

                if (TryReadKeyword("=>"))
                {
                    var returnType = ParseType();

                    if (returnType == null)
                    {
                        SyntaxError("Expected type");
                        Pop();
                        return null;
                    }
                    Merge();
                    return new FunctionTypeShim(returnType, isNullable, varDecl.ToArray());
                }
                else
                {
                    Merge();
                    return new TupleTypeShim(isNullable, varDecl.ToArray());

                }
            }
            throw new NotImplementedException();
        }

        private Variable FindVariable(string name)
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


        public Variable ParseParameter()
        {
            SkipWhiteSpaceAndComments();

            if (!TryReadIdentifier(out var name))
            {
                Pop();
                SyntaxError("Let expression: expected identifier");
                return null;
            }

            SkipWhiteSpaceAndComments();

            TypeShim type;

            if (TryReadKeyword(":"))
            {
                type = ParseType();
                if (type == null)
                {
                    Pop();
                    return null;
                }

                return new Variable(name, false, type);
            }
            else
            {
                Pop();
                return null;
            }


        }

        public Expression ParseVariableDefinition()
        {
            SkipWhiteSpaceAndComments();
            var pos = currentPosition;
            Push();

            bool isMutable;

            if (TryReadKeyword("let"))
            {
                isMutable = false;
            }
            else if (TryReadKeyword("var"))
            {
                isMutable = true;
            }
            else
            {
                Pop();
                return null;
            }

            SkipWhiteSpaceAndComments();

            if (!TryReadIdentifier(out var name))
            {
                Pop();
                SyntaxError("Let expression: expected identifier");
                return null;
            }

            SkipWhiteSpaceAndComments();

            TypeShim type;

            if (TryReadKeyword(":"))
            {
                type = ParseType();
            }
            else
            {
                type = null;
            }
            Expression result = null;
            if (TryReadKeyword("<-"))
            {
                SkipWhiteSpaceAndComments();
                result = ParseExpression();
                if (result == null)
                {
                    Pop();
                    SyntaxError("Expected expression");
                    return null;
                }
                if (type == null)
                {
                    type = result.Type;
                }
            }
            else if (type == null)
            {
                SyntaxError("A variable declaration without a type must have an assignment.", pos);
            }

            if (result != null && !Types.IsAssignable(type, result.Type))
            {
                SyntaxError($"Cannot assign expression of type {result.Type} to {type}.", pos);
            }

            SkipWhiteSpaceAndComments();
            Merge();
            var res = new VariableDeclaration(name, isMutable, type, result);

            var shadow = FindVariable(name);
            if (shadow != null)
            {
                SyntaxError($"Cannot shadow variable {name}");
            }
            var assigned = new VariableDefinatelyAssignedTrivia(res.Variable);

            res.Trivia.Add(assigned);
            Scope.Trivia.Add(assigned);

            scopeStack[scopeStack.Count - 1].Add(res.Variable);

            if(result != null && type != result.Type)
            {
                scopeStack[scopeStack.Count - 1].Add(new ShadowingVariable(res.Variable, result.Type));
            }

            return res;
        }

        private Expression ParseExpression()
        {
            SkipWhiteSpaceAndComments();
            var result = ParseBinary();
            SkipWhiteSpaceAndComments();
            return result;
        }

        private Expression ParseBinary()
        {
            return ParseEquality();
        }

        public Expression ParseMultiply()
        {
            var lhs = ParseConstant();

            if (TryReadKeyword(out var op, '*', '/', '%'))
            {
                var rhs = ParseMultiply();
                if (rhs == null)
                {
                    return lhs;
                }
                switch (op)
                {
                    case '*':
                        return new Multiply(lhs, rhs, lhs.Type);
                    case '/':
                        return new Divide(lhs, rhs, lhs.Type);
                    case '%':
                        return new Remainder(lhs, rhs, lhs.Type);
                }
                throw new NotImplementedException();
            }
            else
            {
                return lhs;
            }
        }

        public Expression ParseAdd(Expression lhs = null)
        {
            lhs = lhs ?? ParseMultiply();

            if (TryReadKeyword(out var op, '+', '-'))
            {
                var rhs = ParseAdd();
                switch (op)
                {
                    case '+':
                        return new Add(lhs, rhs, lhs.Type);
                    case '-':
                        return new Subtract(lhs, rhs, lhs.Type);
                }
                throw new NotImplementedException();
            }
            else
            {
                return lhs;
            }
        }

        public Expression ParseEquality(Expression lhs = null)
        {
            lhs = lhs ?? ParseAdd();

            var pos = currentPosition;
            if(TryReadKeyword("="))
            {
                var rhs = ParseEquality();
                var result = new Equal(lhs, rhs);

                void AddVariableIsNullTrivia(Variable variable)
                {
                    if(!variable.Type.Nullable)
                    {
                        SyntaxError("Comparing non-null to null", pos);
                    }
                    result.Trivia.Add(new VariableIsNullTrivia(variable));
                }

                if(rhs == Constant.Null && lhs is Variable lhsVariable)
                {
                    AddVariableIsNullTrivia(lhsVariable);
                }

                if (lhs == Constant.Null && rhs is Variable rhsVariable)
                {
                    AddVariableIsNullTrivia(rhsVariable);
                }

                return result;
            }
            else if(TryReadKeyword("!="))
            {
                var rhs = ParseEquality();

                var result = new NotEqual(lhs, rhs);

                void AddVariableIsNotNullTrivia(Variable variable)
                {
                    if (!variable.Type.Nullable)
                    {
                        SyntaxError("Comparing non-null to null", pos);
                    }
                    result.Trivia.Add(new VariableIsNotNullTrivia(variable));
                }


                if (rhs == Constant.Null && lhs is Variable lhsVariable)
                {
                    AddVariableIsNotNullTrivia(lhsVariable);
                }

                if (lhs == Constant.Null && rhs is Variable rhsVariable)
                {
                    AddVariableIsNotNullTrivia(rhsVariable);
                }
                return result;
            }
            else
            {
                return lhs;
            }
        }

        public Expression ParseMemberChain()
        {
            Push();
            if(TryReadIdentifier(out var id))
            {
                var lhs = FindVariable(id) ?? (Expression)new UnresolvedIdentifier(id);
                var res = ParseMemberChain(lhs);
                if(res == null)
                {
                    return lhs;
                }
                return res;
            }
            else
            {
                Pop();
                return null;
            }
        }

        public Expression ParseMemberChain(Expression lhs)
        {
            if(TryReadKeyword('.'))
            {
                if(!TryReadIdentifier(out var id))
                {
                    SyntaxError("Expected identifier");
                    return null;
                }
                MemberInfo memberInfo = null;
                if(lhs is Variable)
                {
                    var varType = GetClrTypeFromShim(lhs.Type);
                    if (varType != null)
                    {
                        memberInfo = GetMemberInfo(varType, id);
                    }
                }
                else if(lhs is Member mem)
                {
                    memberInfo = GetMemberInfo(GetClrTypeFromShim(mem.Type), id);
                }

                var res = new Member(lhs, id, memberInfo, memberInfo.CreateTypeShim() ?? new UnresolvedTypeShim("", true));
                return ParseMemberChain(res);
            }
            else
            {
                return lhs;
            }
        }

        private static Type GetClrTypeFromShim(TypeShim shim)
        {
            if(shim is ClrTypeShim clrType)
            {
                return clrType.ClrType;
            }
            if(shim is ArrayTypeShim arrayType)
            {
                return GetClrTypeFromShim(arrayType.ArrayType).MakeArrayType(arrayType.Dimensions.Length);
            }
            if(shim is FunctionTypeShim functionType)
            {
                if (functionType.ReturnType == Types.Void)
                {
                    return System.Linq.Expressions.LambdaExpression.GetActionType(functionType.Parameters.Select(x => GetClrTypeFromShim(x.Type)).ToArray());
                }
                else
                {
                    return System.Linq.Expressions.LambdaExpression.GetFuncType(functionType.Parameters.Select(x => GetClrTypeFromShim(x.Type)).Concat(new[] { GetClrTypeFromShim(functionType.ReturnType) }).ToArray());
                }
            }
            return null;
        }

        private static MemberInfo GetMemberInfo(Type clrType, string name)
        {
            var members = clrType.GetMember(name);
            if(members.Length != 1)
            {
                return null;
            }
            return members[0];
        }

        private static bool IsMutable(Expression exp, bool inConstructor = false)
        {
            if(exp is Variable var)
            {
                return var.Mutable;
            }
            if(exp is Member mem)
            {
                switch(mem.MemberInfo)
                {
                    case PropertyInfo prop:
                        return prop.CanWrite;
                    case FieldInfo field:
                        return inConstructor && field.IsInitOnly;
                    case MethodInfo meth:
                        return meth.ReturnType.IsByRef;
                    case EventInfo ev:
                        return ev.AddMethod != null;
                    case ConstructorInfo ctor:
                        return false;
                }
            }
            return false;
        }

        private Scope Scope => scopeStack[scopeStack.Count - 1];

        public Expression ParseAssignment()
        {
            var start = currentPosition;
            Push();
            var lhs = ParseMemberChain();
            if(lhs == null)
            {
                Pop();
                return null;
            }

            if(TryReadKeyword("<-"))
            {
                var pos = currentPosition;
                var rhs = ParseExpression();
                if(rhs != null)
                {
                    Merge();
                    if(!IsMutable(lhs))
                    {
                        SyntaxError($"The left hand expression is not assignable");
                    }


                    var result = new Assign(lhs, rhs);
                    if (lhs is Variable variable)
                    {
                        if (variable is ShadowingVariable shadowing)
                        {
                            if (!Types.IsAssignable(shadowing.Shadow.Type, rhs.Type))
                            {
                                SyntaxError($"Cannot assign expression of type {rhs.Type} to {shadowing.Shadow.Type}.", pos);
                            }
                            Scope.Variables.Add(new ShadowingVariable(shadowing.Shadow, rhs.Type));
                        }
                        else if (!Types.IsAssignable(variable.Type, rhs.Type))
                        {
                            SyntaxError($"Cannot assign expression of type {rhs.Type} to {variable.Type}.", pos);
                        }

                        var assigned = new VariableAssignedTrivia(variable);
                        Scope.Trivia.Add(assigned);
                        result.Trivia.Add(assigned);
                        result.Trivia.AddRange(lhs.Trivia);
                        result.Trivia.AddRange(rhs.Trivia);
                    }

                    return result;                    
                }
                else
                {
                    SyntaxError("Expected expression", pos);
                    Pop();
                    return null;
                }
            }
            else
            {
                Pop();
                return null;
            }            
        }

        private Expression ParseUnary(Expression lhs = null)
        {
            throw new NotImplementedException();
        }

        private Expression ParseMethod()
        {
            Push();
            PushScopeStack();
            if (!TryReadIdentifier(out var methodName))
            {
                PopScopeStack();
                Pop();
                return null;
            }
            if (!TryReadKeyword('('))
            {
                PopScopeStack();
                Pop();
                return null;
            }

            var varDecl = new List<Variable>();
            while (true)
            {
                var start = currentPosition;
                var par = ParseParameter();
                SkipWhiteSpaceAndComments();
                if (par == null)
                {
                    if (TryReadKeyword(')'))
                    {
                        break;
                    }
                    currentPosition = start;
                    SyntaxError("Expected parameter");
                    Pop();
                    return null;
                }
                varDecl.Add(par);
                scopeStack[scopeStack.Count - 1].Add(par);
                if (TryReadKeyword(','))
                {
                    continue;
                }
                else if (TryReadKeyword(')'))
                {
                    break;
                }
                else
                {
                    PopScopeStack();
                    Pop();
                    return null;
                }
            }

            TypeShim returnType = null;
            if (TryReadKeyword("=>"))
            {
                returnType = ParseType();
            }

            var block = ParseBlock();

            if (block == null)
            {
                PopScopeStack();
                Pop();
                return null;
            }

            return new Method(returnType ?? block.Type, methodName, block, PopScopeStack(), varDecl.ToArray());
        }

        private Expression ParseConstant()
        {
            if (TryReadKeyword("true"))
            {
                return Constant.True;
            }
            else if (TryReadKeyword("false"))
            {
                return Constant.False;
            }
            else if (TryReadKeyword("null"))
            {
                return Constant.Null;
            }
            if (TryReadWhile(c => char.IsDigit(((char)c)), out string value))
            {
                return new Constant(int.Parse(value), Types.Int);
            }
            Push();
            if (TryReadIdentifier(out var id))
            {
                var target = FindVariable(id);
                if (target != null)
                {
                    var trivia = Scope.FindVariableTrivia(target).ToArray();
                    if(!trivia.OfType<VariableDefinatelyAssignedTrivia>().Any())
                    {
                        SyntaxError($"The variable {id} has not been assigned a value.");
                    }
                    return target;
                }
                Pop();
            }
            else
            {
                Pop();
            }
            return null;

        }

        private bool TryReadWhile(Func<int, bool> condition, out string value)
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
                    value = result.ToString();
                    Merge();
                    currentPosition = pos;
                    return true;
                }
            } while (true);
        }

        private bool TryReadIdentifier(out string identifier)
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
                    identifier = result.ToString();
                    Merge();
                    currentPosition = pos;
                    return true;
                }
            } while (true);
        }

        private void SkipWhiteSpaceAndComments()
        {
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
                            continue;
                        }
                        else if(ch == '/')
                        {
                            while(ch != '\n')
                            {
                                ch = ReadChar();
                            }
                            return;
                        }
                        else
                        {
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
                            continue;
                        }
                    }
                }
            }
        }

        private bool TryReadKeyword(char keyword)
        {
            SkipWhiteSpaceAndComments();
            var pos = currentPosition;
            var c = ReadChar();
            if(keyword == c)
            {
                return true;
            }
            currentPosition = pos;
            return false;
        }

        private bool TryReadKeyword(out char result, params char[] keyword)
        {
            SkipWhiteSpaceAndComments();
            var pos = currentPosition;
            var c = (char)ReadChar();
            if (keyword.Contains(c))
            {
                result = (char)c;
                return true;
            }
            currentPosition = pos;
            result = '\0';
            return false;
        }

        private bool TryReadKeyword(string keyword)
        {
            SkipWhiteSpaceAndComments();
            Push();
            int count = 0;
            do
            {
                var c = ReadChar();
                if (c != keyword[count])
                {
                    Pop();
                    return false;
                }
                ++count;
            } while (count < keyword.Length);
            Merge();
            return true;
        }

        private void Push()
        {
            positionStack.Push(currentPosition);
        }

        private void Merge()
        {
            positionStack.Pop();
        }

        private void Pop()
        {
            currentPosition = positionStack.Pop();
        }

        private void Rewind()
        {
            currentPosition = positionStack.Peek();
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
