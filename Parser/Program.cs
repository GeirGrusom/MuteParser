using Parser.SyntaxNodes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser
{
    static class Program
    {
        static void PrintSyntaxNodes(IEnumerable<SyntaxNode> nodes)
        {

            int lineNo = 1;
            void IncrementLineNumber(bool includeNewline = true)
            {
                if (includeNewline)
                {
                    Console.WriteLine();
                }
                Console.BackgroundColor = ConsoleColor.Gray;
                Console.ForegroundColor = ConsoleColor.Black;
                Console.Write(lineNo.ToString().PadRight(4, ' '));
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write(' ');
                ++lineNo;
            }

            IncrementLineNumber(false);

            foreach (var node in nodes)
            {
                if (node.Kind == Kind.Keyword)
                {
                    Console.ForegroundColor = ConsoleColor.Blue;
                }
                else if (node.Kind == Kind.Literal || node.Kind == Kind.StringStart || node.Kind == Kind.StringContents || node.Kind == Kind.StringEnd)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else if (node.Kind == Kind.TypeNullable || node.Kind == Kind.ArrayNullable || node.Kind == Kind.TupleNullable || node.Kind == Kind.UnionNullable)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Gray;
                }
                for (int i = 0; i < node.Value.Length; ++i)
                {
                    if(node.Value.Span[i] == '\r')
                    {
                        continue;
                    }
                    if (node.Value.Span[i] == '\n')
                    {
                        IncrementLineNumber();
                    }
                    else
                    {
                        Console.Write(node.Value.Span[i]);
                    }
                }
            }
            Console.WriteLine();
            Console.BackgroundColor = ConsoleColor.Black;
        }

        static void Main(string[] args)
        {

            string c =
@"
test(a: i32, b: i32, c: i32)
{
}

main(args: string[]?)
{
    var def: i32? <- 100;

    let abcd <- (first: 1, second: 2, third: 3);

    if(def != null)
    {
        let ghi : i32 <- def;
    }
    else
    {
        let abc : i32 <- def;
    }
    let foobar <- 100 + -10;

    def.ToString(""Hello \""World!\""\n""!!);
}
";

            var stream = new MemoryStream(Encoding.UTF8.GetBytes(c));
            var references = new References(new[] { typeof(System.String).Assembly });

            var parser = new Parser(references, c);

            var res = parser.Parse();

            PrintSyntaxNodes(parser.GetSyntaxNodes());

            if (parser.SyntaxErrors.Count != 0)
            {
                Console.WriteLine("Parsing failed");
            }

            foreach (var err in parser.SyntaxErrors)
            {
                Console.WriteLine(GetLine(c, err.Position.Value));
                Console.Write(new string('-', err.Position.Column - 1));
                Console.WriteLine('^');
                Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(err.Position.Row);
                Console.Write(", ");
                Console.Write(err.Position.Column);
                Console.Write(": ");
                Console.WriteLine(err.Message);
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.BackgroundColor = ConsoleColor.Black;
            }
        }

        private static string GetLine(string text, int position)
        {
            var start = FindLineStart(text, position);
            var end = FindLineEnd(text, position);
            return text.Substring(start, end - start);
        }

        private static int FindLineStart(string text, int start)
        {
            for (int i = start; i >= 0; --i)
            {
                if (text[i] == '\n')
                {
                    return i + 1;
                }
            }
            return 0;
        }

        private static int FindLineEnd(string text, int start)
        {
            for (int i = start; i < text.Length; ++i)
            {
                if (text[i] == '\n')
                {
                    return i;
                }
            }
            return 0;
        }
    }
}
