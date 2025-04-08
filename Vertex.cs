using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyCustomCollections;

namespace DAG_Library
{
    public class Vertex<N, L> where N : IComparable where L : IComparable
    {
        public N Value { get; }
        public CustomLinkedList<Edge<L, N>> OutgoingEdges { get; }

        public Vertex(N value)
        {
            Value = value;
            OutgoingEdges = new CustomLinkedList<Edge<L, N>>();
        }
    }
}
