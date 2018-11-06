using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KimbapHeaven
{
    public static class Extension
    {
        public static string ToStringWithNewLine<T>(this IEnumerable<T> list)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var item in list)
            {
                stringBuilder.Append(item.ToString()).Append(Environment.NewLine);
            }

            return stringBuilder.ToString();
        }

        public static List<T> Clone<T>(this List<T> list) where T : ICloneable
        {
            return list.Select(obj => (T) obj.Clone()).ToList();
        }
    }
}
