using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAG_Library
{
    public static class GraphUtils<N, L> 
        where L : IComparable<L>
        where N : IComparable<N>
    {
        public delegate bool CheckDelegate<Node, Link>(Node node, (Node to, Link link)[] outgoingLinks);
        public delegate void ActionDelegate<Node, Link>(Node node, (Node to, Link link)[] outgoingLinks);
        public delegate IGraph<N, L> GraphConstructorDelegate<N, L>() where L : IComparable<L> ;

        public static readonly GraphConstructorDelegate<N, L> ArrayGraphConstructor =
            () => new ArrayGraph<N, L>();
        public static readonly GraphConstructorDelegate<N, L> LinkedGraphConstructor =
            () => new LinkedGraph<N, L>();



        // Реализация стека для DFS
        
        public static bool Exists<Node, Link>(IGraph<Node, Link> graph, CheckDelegate<Node, Link> check)
            where Link : IComparable<Link>
        {
            foreach (var node in graph.Nodes)
            {
                var outgoing = GetOutgoingArray(graph, node);
                if (check(node, outgoing))
                    return true;
            }
            return false;
        }

        public static IGraph<Node, Link> FindAll<Node, Link>(
            IGraph<Node, Link> source,
            CheckDelegate<Node, Link> check,
            GraphConstructorDelegate<Node, Link> constructor)
            where Link : IComparable<Link>
        {
            var result = constructor();
            var nodesToAdd = new List<Node>();

            // Сначала добавляем подходящие узлы
            foreach (var node in source.Nodes)
            {
                var outgoing = GetOutgoingArray(source, node);
                if (check(node, outgoing))
                {
                    result.AddNode(node);
                    nodesToAdd.Add(node);
                }
            }

            // Затем добавляем связи между ними
            foreach (var node in nodesToAdd)
            {
                foreach (var (to, link) in source.GetOutgoingLinks(node))
                {
                    if (result.Contains(to))
                    {
                        try { result.AddLink(node, to, link); }
                        catch { /* Игнорируем ошибки добавления связей */ }
                    }
                }
            }

            return result;
        }

        public static void ForEach<Node, Link>(
            IGraph<Node, Link> graph,
            ActionDelegate<Node, Link> action)
            where Link : IComparable<Link>
        {
            foreach (var node in graph.Nodes)
            {
                var outgoing = GetOutgoingArray(graph, node);
                action(node, outgoing);
            }
        }

        public static bool CheckForAll<Node, Link>(
            IGraph<Node, Link> graph,
            CheckDelegate<Node, Link> check)
            where Link : IComparable<Link>
        {
            foreach (var node in graph.Nodes)
            {
                var outgoing = GetOutgoingArray(graph, node);
                if (!check(node, outgoing))
                    return false;
            }
            return true;
        }

        private static (Node to, Link link)[] GetOutgoingArray<Node, Link>(
            IGraph<Node, Link> graph, Node node)
            where Link : IComparable<Link>
        {
            var links = new List<(Node, Link)>();
            foreach (var edge in graph.GetOutgoingLinks(node))
                links.Add(edge);
            return links.ToArray();
        }

        // Алгоритмы обхода графа

        public static IEnumerable<Node> DepthFirstTraversal<Node, Link>(
            IGraph<Node, Link> graph, Node startNode)
            where Link : IComparable<Link>
        {
            if (!graph.Contains(startNode))
                throw new NodeNotFoundException<Node>(startNode);

            var visited = new bool[graph.Count];
            var stack = new CustomStack<Node>();
            stack.Push(startNode);

            while (!stack.IsEmpty)
            {
                var current = stack.Pop();
                var index = GetNodeIndex(graph, current);

                if (visited[index]) continue;
                visited[index] = true;

                yield return current;

                foreach (var (neighbor, _) in graph.GetOutgoingLinks(current))
                {
                    var neighborIndex = GetNodeIndex(graph, neighbor);
                    if (!visited[neighborIndex])
                        stack.Push(neighbor);
                }
            }
        }

        public static IEnumerable<Node> BreadthFirstTraversal<Node, Link>(
            IGraph<Node, Link> graph, Node startNode)
            where Link : IComparable<Link>
        {
            if (!graph.Contains(startNode))
                throw new NodeNotFoundException<Node>(startNode);

            var visited = new bool[graph.Count];
            var queue = new CustomQueue<Node>();
            queue.Enqueue(startNode);
            visited[GetNodeIndex(graph, startNode)] = true;

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                yield return current;

                foreach (var (neighbor, _) in graph.GetOutgoingLinks(current))
                {
                    var neighborIndex = GetNodeIndex(graph, neighbor);
                    if (!visited[neighborIndex])
                    {
                        visited[neighborIndex] = true;
                        queue.Enqueue(neighbor);
                    }
                }
            }
        }

        private static int GetNodeIndex<Node, Link>(IGraph<Node, Link> graph, Node node)
            where Link : IComparable<Link>
        {
            int index = 0;
            foreach (var n in graph.Nodes)
            {
                if (n.Equals(node)) return index;
                index++;
            }
            return -1;
        }

        // Алгоритм Дейкстры для поиска кратчайшего пути
        public static IEnumerable<Node> FindShortestPath<Node, Link>(
            IGraph<Node, Link> graph,
            Node start,
            Node end,
            Func<Link, double> weightConverter)
            where Link : IComparable<Link>
        {
            if (!graph.Contains(start) || !graph.Contains(end))
                throw new NodeNotFoundException<Node>(!graph.Contains(start) ? start : end);

            int nodeCount = graph.Count;
            var distances = new double[nodeCount];
            var previous = new int[nodeCount];
            var visited = new bool[nodeCount];

            for (int i = 0; i < nodeCount; i++)
            {
                distances[i] = double.PositiveInfinity;
                previous[i] = -1;
            }

            int startIndex = GetNodeIndex(graph, start);
            distances[startIndex] = 0;

            while (true)
            {
                int currentIndex = -1;
                double minDistance = double.PositiveInfinity;

                // Находим непосещенный узел с минимальным расстоянием
                for (int i = 0; i < nodeCount; i++)
                {
                    if (!visited[i] && distances[i] < minDistance)
                    {
                        minDistance = distances[i];
                        currentIndex = i;
                    }
                }

                if (currentIndex == -1) break;
                visited[currentIndex] = true;

                // Получаем текущий узел по индексу
                Node currentNode = GetNodeByIndex(graph, currentIndex);

                foreach (var (neighbor, link) in graph.GetOutgoingLinks(currentNode))
                {
                    int neighborIndex = GetNodeIndex(graph, neighbor);
                    double distance = distances[currentIndex] + weightConverter(link);

                    if (distance < distances[neighborIndex])
                    {
                        distances[neighborIndex] = distance;
                        previous[neighborIndex] = currentIndex;
                    }
                }
            }

            // Восстанавливаем путь
            int endIndex = GetNodeIndex(graph, end);
            if (previous[endIndex] == -1)
                return Enumerable.Empty<Node>();

            var path = new CustomStack<Node>();
            int current = endIndex;
            while (current != -1)
            {
                path.Push(GetNodeByIndex(graph, current));
                current = previous[current];
            }

            var result = new List<Node>();
            while (!path.IsEmpty)
                result.Add(path.Pop());

            return result;
        }

        private static Node GetNodeByIndex<Node, Link>(IGraph<Node, Link> graph, int index)
            where Link : IComparable<Link>
        {
            int i = 0;
            foreach (var node in graph.Nodes)
            {
                if (i++ == index) return node;
            }
            throw new IndexOutOfRangeException();
        }
    }
}
