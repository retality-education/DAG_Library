using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAG_Library
{
    public class UnmutableGraph<N, L> : IGraph<N, L> where L:IComparable<L>
    {
        private readonly IGraph<N, L> wrappedGraph;

        public UnmutableGraph(IGraph<N, L> graph)
        {
            wrappedGraph = graph ?? throw new ArgumentNullException(nameof(graph));
        }

        public int Count => wrappedGraph.Count;
        public bool IsEmpty => wrappedGraph.IsEmpty;
        public IEnumerable<N> Nodes => wrappedGraph.Nodes;
        public IEnumerable<(N from, N to, L link)> Links => wrappedGraph.Links;

        public void AddNode(N node) =>
            throw new InvalidOperationException("Graph is immutable");

        public void AddLink(N from, N to, L link) =>
            throw new InvalidOperationException("Graph is immutable");

        public void Clear() =>
            throw new InvalidOperationException("Graph is immutable");

        public bool Contains(N node) => wrappedGraph.Contains(node);

        public void RemoveNode(N node) =>
            throw new InvalidOperationException("Graph is immutable");

        public void RemoveLink(N from, N to) =>
            throw new InvalidOperationException("Graph is immutable");

        public bool HasLink(N from, N to) => wrappedGraph.HasLink(from, to);

        public L GetLink(N from, N to) => wrappedGraph.GetLink(from, to);

        public IEnumerable<N> GetNeighbors(N node) => wrappedGraph.GetNeighbors(node);

        public IEnumerable<(N to, L link)> GetOutgoingLinks(N node) => wrappedGraph.GetOutgoingLinks(node);

        public IEnumerable<(N from, L link)> GetIncomingLinks(N node) => wrappedGraph.GetIncomingLinks(node);

        public IEnumerator<N> GetEnumerator() => wrappedGraph.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
