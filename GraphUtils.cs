using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyCustomCollections;
namespace DAG_Library
{
    public static class GraphUtils<NODE, LINK>
        where NODE : IComparable
        where LINK : IComparable
    {
        // Делегаты для операций с графами
        public delegate bool CheckDelegate<N, L>(Edge<N, L> edge)
            where N : IComparable
            where L : IComparable;
        public delegate void ActionDelegate<N, L>(Edge<N, L> edge)
            where N : IComparable
            where L : IComparable;

        public delegate IGraph<N, L> GraphConstructor<N, L>(int capacity)
            where N : IComparable
            where L : IComparable;

        // Стандартные конструкторы графов
        public static readonly GraphConstructor<NODE, LINK>  ArrayGraphConstructor =
            capacity => new ArrayGraph<NODE, LINK>(capacity);
        public static readonly GraphConstructor<NODE, LINK> LinkedGraphConstructor =
            capacity => new LinkedGraph<NODE, LINK>();

        /// <summary>Проверяет существование хотя бы одного ребра, удовлетворяющего условию</summary>
        public static bool Exists<N, L>(IGraph<N, L> graph, CheckDelegate<N, L> check)
            where N : IComparable
            where L : IComparable
        {
            foreach (var edge in graph.Edges)
                if (check(edge))
                    return true;
            return false;
        }

        /// <summary>Создает новый граф с ребрами, удовлетворяющими условию</summary>
        public static IGraph<N, L> FindAll<N, L>(
            IGraph<N, L> graph,
            CheckDelegate<N, L> check,
            GraphConstructor<N, L> constructor
        )
            where N : IComparable
            where L : IComparable
        {
            var result = constructor(graph.Count);
            foreach (var node in graph.Nodes)
                result.AddNode(node.Value);

            foreach (var edge in graph.Edges)
                if (check(edge))
                    result.AddEdge(edge.From, edge.To, edge.LinkValue);

            return result;
        }

        /// <summary>Выполняет действие для каждого ребра графа</summary>
        public static void ForEach<N, L>(IGraph<N, L> graph, ActionDelegate<N, L> action)
            where N : IComparable
            where L : IComparable
        {
            foreach (var edge in graph.Edges)
                action(edge);
        }

        /// <summary>Проверяет, что все ребра удовлетворяют условию</summary>
        public static bool CheckForAll<N, L>(IGraph<N, L> graph, CheckDelegate<N, L> check)
            where N : IComparable
            where L : IComparable
        {
            foreach (var edge in graph.Edges)
                if (!check(edge))
                    return false;
            return true;
        }

        /// <summary>Обход в глубину (DFS)</summary>
        public static IEnumerable<N> DepthFirstTraversal<N, L>(IGraph<N, L> graph, N start)
            where N : IComparable
            where L : IComparable
        {
            var visited = new CustomLinkedList<N>();
            var stack = new Stack<N>();
            stack.Push(start);

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                if (Contains(visited, current)) continue;

                visited.AddLast(current);
                yield return current;

                foreach (var edge in graph.Edges)
                    if (edge.From.CompareTo(current) == 0)
                        stack.Push(edge.To);
            }
        }

        /// <summary>Обход в ширину (BFS)</summary>
        public static IEnumerable<N> BreadthFirstTraversal<N, L>(IGraph<N, L> graph, N start)
            where N : IComparable
            where L : IComparable
        {
            var visited = new CustomLinkedList<N>();
            var queue = new Queue<N>();
            queue.Enqueue(start);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (Contains(visited, current)) continue;

                visited.AddLast(current);
                yield return current;

                foreach (var edge in graph.Edges)
                    if (edge.From.CompareTo(current) == 0)
                        queue.Enqueue(edge.To);
            }
        }

        public static (N[] Nodes, int[] Distances) ShortestPaths<N, L>(IGraph<N, L> graph, N start)
            where N : IComparable
            where L : IComparable
        {
            var nodes = graph.Nodes.Select(x => x.Value).ToArray();
            var distances = new int[nodes.Length];
            Array.Fill(distances, -1);

            int startIndex = Array.FindIndex(nodes, n => n.CompareTo(start) == 0);
            if (startIndex == -1) throw new GraphExceptions.NodeNotFoundException<N>(start);

            var queue = new Queue<N>();
            queue.Enqueue(start);
            distances[startIndex] = 0;

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                int currentIndex = Array.FindIndex(nodes, n => n.CompareTo(current) == 0);

                foreach (var edge in graph.Edges)
                {
                    if (edge.From.CompareTo(current) != 0) continue;

                    int neighborIndex = Array.FindIndex(nodes, n => n.CompareTo(edge.To) == 0);
                    if (distances[neighborIndex] == -1)
                    {
                        distances[neighborIndex] = distances[currentIndex] + 1;
                        queue.Enqueue(edge.To);
                    }
                }
            }

            return (nodes, distances);
        }

        /// <summary>Топологическая сортировка (DFS-based)</summary>
        public static CustomLinkedList<N> TopologicalSort<N, L>(IGraph<N, L> graph)
            where N : IComparable
            where L : IComparable
        {
            var visited = new CustomLinkedList<N>();
            var result = new CustomLinkedList<N>();

            foreach (var node in graph.Nodes)
            {
                if (!Contains(visited, node.Value))
                {
                    TopSortUtil(node.Value, graph, visited, result);
                }
            }
            return result;
        }

        private static void TopSortUtil<N, L>(
            N node,
            IGraph<N, L> graph,
            CustomLinkedList<N> visited,
            CustomLinkedList<N> result
        ) 
            where N : IComparable
            where L : IComparable
        {
            visited.AddLast(node);

            foreach (var edge in graph.Edges)
            {
                if (edge.From.CompareTo(node) == 0 && !Contains(visited, edge.To))
                {
                    TopSortUtil(edge.To, graph, visited, result);
                }
            }

            result.AddFirst(node);
        }

        /// <summary>Проверка наличия элемента в CustomLinkedList</summary>
        private static bool Contains<N>(CustomLinkedList<N> list, N value)
            where N : IComparable
        {
            foreach (var item in list)
            {
                if (item.CompareTo(value) == 0) return true;
            }
            return false;
        }
        /// <summary>Проверка достижимости</summary>
        public static bool IsReachable<N, L>(IGraph<N, L> graph, N from, N to)
            where N : IComparable
            where L : IComparable
        {
            var visited = new CustomLinkedList<N>();
            return DfsReachable(from, to, graph, visited);
        }

        private static bool DfsReachable<N, L>(
            N current,
            N target,
            IGraph<N, L> graph,
            CustomLinkedList<N> visited
        )
            where N : IComparable
            where L : IComparable
        {
            if (current.CompareTo(target) == 0) return true;
            visited.AddLast(current);

            foreach (var edge in graph.Edges)
            {
                if (edge.From.CompareTo(current) != 0) continue;
                var neighbor = edge.To;

                if (!Contains(visited, neighbor) && DfsReachable(neighbor, target, graph, visited))
                    return true;
            }
            return false;
        }

    }
}