using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAG_Library
{
    internal class CustomStack<T>
    {
        private T[] items;
        private int top;

        public CustomStack(int capacity = 4)
        {
            items = new T[capacity];
        }

        public void Push(T item)
        {
            if (top == items.Length)
                Array.Resize(ref items, items.Length * 2);
            items[top++] = item;
        }

        public T Pop()
        {
            if (top == 0) throw new InvalidOperationException();
            return items[--top];
        }

        public bool IsEmpty => top == 0;
    }

}
