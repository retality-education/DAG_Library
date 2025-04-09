using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAG_Library
{
    public class UnmutableGraph<N, L> : IGraph<N, L>
       where N : IComparable
       where L : IComparable
    {
        private readonly IGraph<N, L> _originalGraph;

        public UnmutableGraph(IGraph<N, L> graph)
        {
            _originalGraph = graph;
        }

        public int Count => _originalGraph.Count;
        public bool IsEmpty => _originalGraph.IsEmpty;
        public IEnumerable<Vertex<N, L>> Nodes => _originalGraph.Nodes;
        public IEnumerable<Edge<N, L>> Edges => _originalGraph.Edges;

        public void AddNode(N node)
            => throw new GraphExceptions.ImmutableGraphModificationException();

        public void AddEdge(N from, N to, L link)
            => throw new GraphExceptions.ImmutableGraphModificationException();

        public void RemoveNode(N node)
            => throw new GraphExceptions.ImmutableGraphModificationException();

        public void RemoveEdge(N from, N to)
            => throw new GraphExceptions.ImmutableGraphModificationException();

        public void Clear()
            => throw new GraphExceptions.ImmutableGraphModificationException();

        public bool Contains(N node) => _originalGraph.Contains(node);

        public IEnumerator<Vertex<N, L>> GetEnumerator() => _originalGraph.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
