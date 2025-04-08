using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAG_Library
{
    public class ArrayGraph<N, L> : IGraph<N, L>
     where N : IComparable<N>
     where L : IComparable<L>
    {
        private struct Edge
        {
            public int FromIndex;
            public int ToIndex;
            public L Weight;
        }

        private N[] nodes;
        private Edge[] edges;
        private int nodeCount;
        private int edgeCount;
        private int nodeCapacity;
        private int edgeCapacity;

        public ArrayGraph(int initialCapacity = 4)
        {
            nodeCapacity = initialCapacity;
            edgeCapacity = initialCapacity * 2;
            nodes = new N[nodeCapacity];
            edges = new Edge[edgeCapacity];
            nodeCount = 0;
            edgeCount = 0;
        }

        public int Count => nodeCount;
        public bool IsEmpty => nodeCount == 0;

        public IEnumerable<N> Nodes
        {
            get
            {
                for (int i = 0; i < nodeCount; i++)
                    yield return nodes[i];
            }
        }

        public IEnumerable<(N from, N to, L link)> Links
        {
            get
            {
                for (int i = 0; i < edgeCount; i++)
                {
                    yield return (
                        nodes[edges[i].FromIndex],
                        nodes[edges[i].ToIndex],
                        edges[i].Weight
                    );
                }
            }
        }

        public void AddNode(N node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            if (Contains(node))
                return;

            if (nodeCount == nodeCapacity)
                ResizeNodes(nodeCapacity * 2);

            nodes[nodeCount++] = node;
        }

        public void AddLink(N from, N to, L weight)
        {
            if (from == null || to == null)
                throw new ArgumentNullException();

            int fromIndex = FindNodeIndex(from);
            int toIndex = FindNodeIndex(to);

            if (fromIndex == -1)
                throw new NodeNotFoundException<N>(from);
            if (toIndex == -1)
                throw new NodeNotFoundException<N>(to);

            if (HasLink(from, to))
                throw new LinkAlreadyExistsException<N>(from, to);

            if (WouldCreateCycle(fromIndex, toIndex))
                throw new CyclicGraphException();

            if (edgeCount == edgeCapacity)
                ResizeEdges(edgeCapacity * 2);

            edges[edgeCount++] = new Edge
            {
                FromIndex = fromIndex,
                ToIndex = toIndex,
                Weight = weight
            };
        }

        public bool Contains(N node)
        {
            return FindNodeIndex(node) != -1;
        }

        private int FindNodeIndex(N node)
        {
            for (int i = 0; i < nodeCount; i++)
            {
                if (nodes[i].CompareTo(node) == 0)
                    return i;
            }
            return -1;
        }

        public bool HasLink(N from, N to)
        {
            int fromIndex = FindNodeIndex(from);
            int toIndex = FindNodeIndex(to);

            if (fromIndex == -1 || toIndex == -1)
                return false;

            for (int i = 0; i < edgeCount; i++)
            {
                if (edges[i].FromIndex == fromIndex && edges[i].ToIndex == toIndex)
                    return true;
            }
            return false;
        }

        public L GetLink(N from, N to)
        {
            int fromIndex = FindNodeIndex(from);
            int toIndex = FindNodeIndex(to);

            if (fromIndex == -1)
                throw new NodeNotFoundException<N>(from);
            if (toIndex == -1)
                throw new NodeNotFoundException<N>(to);

            for (int i = 0; i < edgeCount; i++)
            {
                if (edges[i].FromIndex == fromIndex && edges[i].ToIndex == toIndex)
                    return edges[i].Weight;
            }

            throw new LinkNotFoundException<N>(from, to);
        }

        public IEnumerable<N> GetNeighbors(N node)
        {
            int nodeIndex = FindNodeIndex(node);
            if (nodeIndex == -1)
                throw new NodeNotFoundException<N>(node);

            for (int i = 0; i < edgeCount; i++)
            {
                if (edges[i].FromIndex == nodeIndex)
                    yield return nodes[edges[i].ToIndex];
            }
        }

        public IEnumerable<(N to, L link)> GetOutgoingLinks(N node)
        {
            int nodeIndex = FindNodeIndex(node);
            if (nodeIndex == -1)
                throw new NodeNotFoundException<N>(node);

            for (int i = 0; i < edgeCount; i++)
            {
                if (edges[i].FromIndex == nodeIndex)
                    yield return (nodes[edges[i].ToIndex], edges[i].Weight);
            }
        }

        public IEnumerable<(N from, L link)> GetIncomingLinks(N node)
        {
            int nodeIndex = FindNodeIndex(node);
            if (nodeIndex == -1)
                throw new NodeNotFoundException<N>(node);

            for (int i = 0; i < edgeCount; i++)
            {
                if (edges[i].ToIndex == nodeIndex)
                    yield return (nodes[edges[i].FromIndex], edges[i].Weight);
            }
        }

        public void RemoveNode(N node)
        {
            int nodeIndex = FindNodeIndex(node);
            if (nodeIndex == -1)
                throw new NodeNotFoundException<N>(node);

            // Удаляем все связанные рёбра
            for (int i = edgeCount - 1; i >= 0; i--)
            {
                if (edges[i].FromIndex == nodeIndex || edges[i].ToIndex == nodeIndex)
                {
                    RemoveEdge(i);
                }
            }

            // Сдвигаем узлы
            for (int i = nodeIndex; i < nodeCount - 1; i++)
            {
                nodes[i] = nodes[i + 1];
            }

            // Обновляем индексы в рёбрах
            for (int i = 0; i < edgeCount; i++)
            {
                if (edges[i].FromIndex > nodeIndex)
                    edges[i].FromIndex--;

                if (edges[i].ToIndex > nodeIndex)
                    edges[i].ToIndex--;
            }

            nodeCount--;
            nodes[nodeCount] = default;
        }

        public void RemoveLink(N from, N to)
        {
            int fromIndex = FindNodeIndex(from);
            int toIndex = FindNodeIndex(to);

            if (fromIndex == -1)
                throw new NodeNotFoundException<N>(from);
            if (toIndex == -1)
                throw new NodeNotFoundException<N>(to);

            for (int i = 0; i < edgeCount; i++)
            {
                if (edges[i].FromIndex == fromIndex && edges[i].ToIndex == toIndex)
                {
                    RemoveEdge(i);
                    return;
                }
            }

            throw new LinkNotFoundException<N>(from, to);
        }

        private void RemoveEdge(int edgeIndex)
        {
            // Сдвигаем рёбра
            for (int i = edgeIndex; i < edgeCount - 1; i++)
            {
                edges[i] = edges[i + 1];
            }

            edgeCount--;
            edges[edgeCount] = default;
        }

        public void Clear()
        {
            nodes = new N[nodeCapacity];
            edges = new Edge[edgeCapacity];
            nodeCount = 0;
            edgeCount = 0;
        }

        private bool WouldCreateCycle(int fromIndex, int toIndex)
        {
            if (fromIndex == toIndex)
                return true;

            var visited = new bool[nodeCount];
            var queue = new Queue<int>();
            queue.Enqueue(toIndex);
            visited[toIndex] = true;

            while (queue.Count > 0)
            {
                int current = queue.Dequeue();
                for (int i = 0; i < edgeCount; i++)
                {
                    if (edges[i].FromIndex == current)
                    {
                        if (edges[i].ToIndex == fromIndex)
                            return true;

                        if (!visited[edges[i].ToIndex])
                        {
                            visited[edges[i].ToIndex] = true;
                            queue.Enqueue(edges[i].ToIndex);
                        }
                    }
                }
            }
            return false;
        }

        private void ResizeNodes(int newCapacity)
        {
            Array.Resize(ref nodes, newCapacity);
            nodeCapacity = newCapacity;
        }

        private void ResizeEdges(int newCapacity)
        {
            Array.Resize(ref edges, newCapacity);
            edgeCapacity = newCapacity;
        }

        public IEnumerator<N> GetEnumerator()
        {
            for (int i = 0; i < nodeCount; i++)
                yield return nodes[i];
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
