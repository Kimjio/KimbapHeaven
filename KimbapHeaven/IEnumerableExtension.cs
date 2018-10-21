using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KimbapHeaven
{
    public static class IEnumerableExtension
    {
        public static string ToStringList<T>(this IEnumerable<T> list)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var item in list)
            {
                stringBuilder.Append(item.ToString()).Append(Environment.NewLine);
            }

            return stringBuilder.ToString();
        }
    }
}
