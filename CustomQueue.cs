using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAG_Library
{
    internal class CustomQueue<T>
    {
        private T[] items;
        private int head;
        private int tail;
        private int size;
        private const int DefaultCapacity = 4;

        public CustomQueue()
        {
            items = new T[DefaultCapacity];
        }

        public int Count => size;

        public void Enqueue(T item)
        {
            if (size == items.Length)
                Grow();

            items[tail] = item;
            tail = (tail + 1) % items.Length;
            size++;
        }

        public T Dequeue()
        {
            if (size == 0)
                throw new InvalidOperationException("Queue is empty");

            T item = items[head];
            items[head] = default;
            head = (head + 1) % items.Length;
            size--;
            return item;
        }

        public T Peek()
        {
            if (size == 0)
                throw new InvalidOperationException("Queue is empty");

            return items[head];
        }

        public void Clear()
        {
            Array.Clear(items, 0, items.Length);
            head = 0;
            tail = 0;
            size = 0;
        }

        private void Grow()
        {
            int newCapacity = items.Length * 2;
            T[] newItems = new T[newCapacity];

            if (head < tail)
            {
                Array.Copy(items, head, newItems, 0, size);
            }
            else
            {
                Array.Copy(items, head, newItems, 0, items.Length - head);
                Array.Copy(items, 0, newItems, items.Length - head, tail);
            }

            items = newItems;
            head = 0;
            tail = size;
        }
    }
}
