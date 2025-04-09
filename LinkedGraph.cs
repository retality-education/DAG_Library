using System;
using MyCustomCollections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace DAG_Library
{
    public class LinkedGraph<N, L> : IGraph<N, L>
        where N : IComparable
        where L : IComparable
    {
        private CustomLinkedList<Vertex<N, L>> vertices = new CustomLinkedList<Vertex<N, L>>();

        public int Count => vertices.Count;
        public bool IsEmpty => vertices.Count == 0;

        public IEnumerable<Vertex<N, L>> Nodes
        {
            get
            {
                foreach (var vertex in vertices)
                    yield return vertex;
            }
        }

        public IEnumerable<Edge<N, L>> Edges
        {
            get
            {
                foreach (var vertex in vertices)
                    foreach (var edge in vertex.OutgoingEdges)
                        yield return edge;
            }
        }

        public void AddNode(N value)
        {
            if (Contains(value))
                throw new GraphExceptions.NodeAlreadyExistsException<N>(value);

            vertices.AddLast(new Vertex<N, L>(value));
        }

        public void AddEdge(N from, N to, L link)
        {
            var source = FindVertex(from) ?? throw new GraphExceptions.NodeNotFoundException<N>(from);
            var target = FindVertex(to) ?? throw new GraphExceptions.NodeNotFoundException<N>(to);

            // Проверка на существование ребра
            bool edgeExists = source.OutgoingEdges.Any(edge =>
                edge.To.CompareTo(to) == 0 &&
                edge.LinkValue.CompareTo(link) == 0);

            if (edgeExists)
                throw new GraphExceptions.LinkAlreadyExistsException<N>(from, to);

            var newEdge = new Edge<N, L>(from, link, to);
            source.OutgoingEdges.AddLast(newEdge);

            // Проверка на циклы
            if (GraphUtils<N, L>.IsReachable(this, to, from))
            {
                source.OutgoingEdges.Remove(newEdge, (e1, e2) =>
                    e1.LinkValue.CompareTo(e2.LinkValue) == 0 &&
                    e1.To.CompareTo(e2.To) == 0);
                throw new GraphExceptions.CycleDetectedException();
            }
        }

        public bool Contains(N value)
        {
            return FindVertex(value) != null;
        }

        public void RemoveNode(N value)
        {
            var vertexToRemove = FindVertex(value) ??
                throw new GraphExceptions.NodeNotFoundException<N>(value);

            // Удаляем все входящие ребра
            foreach (var vertex in vertices)
            {
                bool removed;
                do
                {
                    removed = vertex.OutgoingEdges.Remove(
                        default(Edge<N, L>),
                        (edge, _) => edge.To.CompareTo(value) == 0
                    );
                } while (removed);
            }

            // Удаляем саму вершину
            vertices.Remove(vertexToRemove, (v1, v2) =>
                v1.Value.CompareTo(v2.Value) == 0);
        }

        public void RemoveEdge(N from, N to)
        {
            var source = FindVertex(from) ?? throw new GraphExceptions.NodeNotFoundException<N>(from);
            _ = FindVertex(to) ?? throw new GraphExceptions.NodeNotFoundException<N>(to);

            bool removed = source.OutgoingEdges.Remove(
                default(Edge<N, L>),
                (edge, _) => edge.To.CompareTo(to) == 0
            );

            if (!removed)
                throw new GraphExceptions.LinkNotFoundException<N>(from, to);
        }

        public void Clear()
        {
            vertices = new CustomLinkedList<Vertex<N, L>>();
        }

        private Vertex<N, L>? FindVertex(N value)
        {
            foreach (var vertex in vertices)
                if (vertex.Value.CompareTo(value) == 0)
                    return vertex;
            return null;
        }

        public IEnumerator<Vertex<N, L>> GetEnumerator() => Nodes.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
