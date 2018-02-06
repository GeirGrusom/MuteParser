using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser
{
    class Program
    {
        static void Main(string[] args)
        {

            string c = 
@"module foo

main(args: string?[]) 
{
    let def: i32? <- 100
    if(def != null)
    {
        let ghi : i32 <- def
    }
    else
    {
        let abc : i32 <- def
    }
}
";

            Console.WriteLine(c);
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(c));

            var parser = new Parser(stream);

            var res = parser.Parse();

            foreach(var err in parser.SyntaxErrors)
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
            for(int i = start; i >= 0; --i)
            {
                if(text[i] == '\n')
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
                if (text[i] == '\n' || text[i] == '\r')
                {
                    return i;
                }
            }
            return 0;
        }
    }
}
