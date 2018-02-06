using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser
{
    public class RefStack<T>
    {
        private int index;

        private T[] values;

        public RefStack()
        {
            values = new T[1024];
        }

        public ref T Push(in T value)
        {
            if (index >= values.Length - 1)
            {
                Array.Resize<T>(ref values, values.Length + 1024);
            }
            values[index++] = value;
            return ref values[index - 1];
        }

        public void Clear()
        {
            index = 0;
        }

        public ref T Peek()
        {
            return ref values[index - 1];
        }

        public T Pop()
        {
            --index;
            return values[index];
        }

        public ref T this[int index] => ref values[index];

        public int Count => index;
    }
}
