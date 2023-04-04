using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GDL.Src.Utility
{
    public static class QueueExtends
    {
        public static void AddAll<T>(this Queue<T> queue, IEnumerable<T> values, Predicate<T> term)
        {
            foreach (var val in values)
            {
                if (term.Invoke(val))
                {
                    queue.Enqueue(val);
                }
            }
        }

        public static void AddAll<T>(this Queue<T> queue, IEnumerable<T> values)
        {
            foreach (var val in values)
            {
                queue.Enqueue(val);
            }
        }

        public static void ForEach<T>(this Queue<T> queue, Action<T> action)
        {
            foreach (var i in queue)
            {
                action.Invoke(i);
            }
        }
    }
}
