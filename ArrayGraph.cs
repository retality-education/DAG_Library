using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyCustomCollections;
namespace DAG_Library
{
    public class ArrayGraph<N, L> : IGraph<N, L>
        where N : IComparable
        where L : IComparable
    {
        private Vertex<N, L>[] vertices;
        private int count;

        public ArrayGraph(int capacity)
        {
            vertices = new Vertex<N, L>[capacity];
            count = 0;
        }

        public int Count => count;
        public bool IsEmpty => count == 0;

        public IEnumerable<Vertex<N, L>> Nodes
        {
            get
            {
                for (int i = 0; i < count; i++)
                    yield return vertices[i];
            }
        }

        public IEnumerable<Edge<N, L>> Edges
        {
            get
            {
                for (int i = 0; i < count; i++)
                    foreach (var edge in vertices[i].OutgoingEdges)
                        yield return edge;
            }
        }

        public void AddNode(N value)
        {
            if (Contains(value))
                throw new GraphExceptions.NodeAlreadyExistsException<N>(value);

            if (count >= vertices.Length)
                throw new InvalidOperationException("Graph capacity exceeded");

            vertices[count++] = new Vertex<N, L>(value);
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
            int index = FindVertexIndex(value);
            if (index == -1)
                throw new GraphExceptions.NodeNotFoundException<N>(value);

            // Удаляем все входящие ребра
            for (int i = 0; i < count; i++)
            {
                bool removed;
                do
                {
                    removed = vertices[i].OutgoingEdges.Remove(
                        default(Edge<N, L>),
                        (edge, _) => edge.To.CompareTo(value) == 0
                    );
                } while (removed);
            }

            // Сдвигаем вершины
            for (int i = index; i < count - 1; i++)
                vertices[i] = vertices[i + 1];

            count--;
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
            count = 0;
            vertices = new Vertex<N, L>[vertices.Length];
        }

        private Vertex<N, L>? FindVertex(N value)
        {
            for (int i = 0; i < count; i++)
                if (vertices[i].Value.CompareTo(value) == 0)
                    return vertices[i];
            return null;
        }

        private int FindVertexIndex(N value)
        {
            for (int i = 0; i < count; i++)
                if (vertices[i].Value.CompareTo(value) == 0)
                    return i;
            return -1;
        }

        public IEnumerator<Vertex<N, L>> GetEnumerator() => Nodes.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }   
}
