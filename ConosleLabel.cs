using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GDL
{
    internal class ConsoleLabels
    {

        private readonly Dictionary<string, string> labels = new();

        public int X { get; private set; } = 0;
        public int Y { get; private set; } = 0;

        public void Add(string labelname, string text)
        {
            labels.Add(labelname, text);
        }

        public void SetText(string labelname, string text)
        {
            if (labels.ContainsKey(labelname))
                labels[labelname] = text;
        }

        public void Remove(string labelname)
        {
            labels.Remove(labelname);
        }

        public void Position(int x, int y)
        {
            X = x;
            Y = y;
        }

        public void WriteText()
        {
            var prevPosX = Console.CursorLeft;
            var prevPosY = Console.CursorTop;
            Console.CursorVisible = false;
            Console.SetCursorPosition(X, Y);
            int count = 0;
            foreach (var label in labels)
            {
                Console.SetCursorPosition(X, Y + count);
                Console.Write(new string(' ', Console.WindowWidth - X));
                Console.SetCursorPosition(X, Y + count);
                Console.Write(label.Value);
                count++;
            }
            Console.CursorVisible = true;
            Console.SetCursorPosition(prevPosX, prevPosY);
        }
    }
}
