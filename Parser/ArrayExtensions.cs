using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser
{
    public static class ArrayExtensions
    {
        public static bool Contains<T>(this T[] arr, T value)
            where T : struct, IEquatable<T>
        {
            for(int i = 0; i < arr.Length; ++i)
            {
                if (arr[i].Equals(value))
                    return true;
            }
            return false;
        }
    }
}
