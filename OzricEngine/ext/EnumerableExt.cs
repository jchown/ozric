using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OzricEngine.ext
{
    public static class EnumerableExt
    {
        public static string Join(this IEnumerable<string> enumerable, string joiner)
        {
            var list = enumerable.ToList();
            
            StringBuilder sb = new StringBuilder(list.Sum(s => s.Length) + joiner.Length * (list.Count - 1));
            
            for (int i = 0; i < list.Count; i++)
            {
                if (i > 0)
                    sb.Append(joiner);

                sb.Append(list[i]);
            }

            return sb.ToString();
        }
    }
}